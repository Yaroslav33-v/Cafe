CREATE TABLE public.cards(
    card_id SERIAL PRIMARY KEY,
    card_token VARCHAR(255) NOT NULL UNIQUE,  -- Токен от платежного шлюза
    last_four CHAR(4) NOT NULL CHECK (last_four ~ '^[0-9]{4}$'), -- Только цифры
    expiry_month SMALLINT NOT NULL CHECK (expiry_month BETWEEN 1 AND 12), -- Месяц конца работы карты
    expiry_year SMALLINT NOT NULL CHECK (expiry_year >= EXTRACT(YEAR FROM CURRENT_DATE)), -- Год конца работы карты
    card_type VARCHAR(20),                     -- Visa/MasterCard/etc
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT chk_expiry_not_past CHECK (
        MAKE_DATE(expiry_year, expiry_month, 1) >= DATE_TRUNC('month', CURRENT_DATE)
    ),
    CONSTRAINT chk_last_four_length CHECK (CHAR_LENGTH(last_four) = 4)
);

CREATE TABLE public.operations(
    operation_id BIGSERIAL PRIMARY KEY,
    card_id INTEGER NOT NULL REFERENCES cards(card_id),
    
    -- Тип операции
    operation_type VARCHAR(20) NOT NULL CHECK (operation_type IN (
        'DEPOSIT',      -- Зачисление
        'WITHDRAWAL',   -- Списание  
        'TRANSFER_IN', -- Перевод (+)
        'TRANSFER_OUT', -- Перевод (-)
        'FEE',          -- Комиссия
        'INTEREST',     -- Проценты
        'ADJUSTMENT'    -- Корректировка
    )),
    
    -- Сумма (МОЖЕТ БЫТЬ ОТРИЦАТЕЛЬНОЙ!)
    amount DECIMAL(15,2) NOT NULL, 
    
    -- Валюта
    currency CHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Статус (только завершенные операции влияют на баланс)
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (status IN (
        'PENDING',      -- В обработке
        'POSTED',       -- Проведена (уже в балансе)
        'CANCELLED',    -- Отменена
        'REVERSED'      -- Сторнирована
    )),
    
    -- Для переводов (вторая карта-участница)
    transfer_card_id INTEGER REFERENCES cards(card_id),
    
    -- Детали
    description TEXT NOT NULL,
    
    -- Ключевые метки времени
    transaction_date DATE NOT NULL DEFAULT CURRENT_DATE,  -- Дата операции
    value_date DATE NOT NULL DEFAULT CURRENT_DATE,        -- Дата валютирования
    posted_at TIMESTAMPTZ,                                -- Когда проведена
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    -- Внешние ключи
    CONSTRAINT fk_operations_card FOREIGN KEY (card_id) 
        REFERENCES cards(card_id),
    
    -- Ограничения
    CONSTRAINT chk_amount_not_zero CHECK (amount != 0),
    CONSTRAINT chk_dates_valid CHECK (value_date >= transaction_date),
    CONSTRAINT chk_posted_completed CHECK (
        (status = 'POSTED' AND posted_at IS NOT NULL) OR
        (status != 'POSTED' AND posted_at IS NULL)
    )
);

-- Создаем процедуру/триггер для автоматического создания второй операции
CREATE OR REPLACE FUNCTION process_transfer_operation()
RETURNS TRIGGER AS $$
DECLARE
    v_other_card_id INTEGER;
    v_other_card_last_four CHAR(4);
    v_current_card_last_four CHAR(4);
    v_other_operation_type VARCHAR(20);
    v_other_amount DECIMAL(15,2);
    v_other_description TEXT;
	v_already_exists BOOLEAN;
BEGIN
    -- Срабатывает только для операций перевода
    IF NEW.operation_type IN ('TRANSFER_OUT', 'TRANSFER_IN') THEN

		SELECT EXISTS (
            SELECT 1 FROM operations o2
            WHERE o2.card_id = NEW.transfer_card_id
              AND o2.transfer_card_id = NEW.card_id
              AND o2.operation_type IN ('TRANSFER_OUT', 'TRANSFER_IN')
              AND ABS(o2.amount) = ABS(NEW.amount)
              AND o2.transaction_date = NEW.transaction_date
              AND o2.created_at > NOW() - INTERVAL '1 minute'  -- Недавно созданная
        ) INTO v_already_exists;
        
        IF v_already_exists THEN
            RETURN NEW;
        END IF;
        
        -- Проверяем, что указана вторая карта
        IF NEW.transfer_card_id IS NULL THEN
            RAISE EXCEPTION 'Для операции перевода должен быть указан transfer_card_id';
        END IF;
        
        -- Проверяем, что карты разные
        IF NEW.card_id = NEW.transfer_card_id THEN
            RAISE EXCEPTION 'Нельзя переводить на ту же карту';
        END IF;
        
        -- Получаем данные карт для описания
        SELECT last_four INTO v_current_card_last_four
        FROM cards WHERE card_id = NEW.card_id;
        
        SELECT last_four INTO v_other_card_last_four
        FROM cards WHERE card_id = NEW.transfer_card_id;
        
        -- Определяем параметры для второй операции
        IF NEW.operation_type = 'TRANSFER_OUT' THEN
            v_other_operation_type := 'TRANSFER_IN';
            v_other_amount := ABS(NEW.amount);  -- Положительная сумма
            v_other_description := FORMAT('Перевод с карты •••• %s', v_current_card_last_four);
        ELSE -- TRANSFER_IN
            v_other_operation_type := 'TRANSFER_OUT';
            v_other_amount := -ABS(NEW.amount);  -- Отрицательная сумма
            v_other_description := FORMAT('Перевод на карту •••• %s', v_current_card_last_four);
        END IF;
        
        -- Создаем вторую операцию перевода
        INSERT INTO operations (
            card_id,
            transfer_card_id,
            operation_type,
            amount,
            currency,
            status,
            description,
            transaction_date,
            value_date,
            posted_at,
            created_at
        ) VALUES (
            NEW.transfer_card_id,          -- Вторая карта
            NEW.card_id,                   -- Первая карта как transfer_card_id
            v_other_operation_type,        -- Противоположный тип
            v_other_amount,                -- Противоположный знак суммы
            NEW.currency,
            NEW.status,
            v_other_description,
            NEW.transaction_date,
            NEW.value_date,
            NEW.posted_at,
            NOW()
        );
        
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Создаем триггер (AFTER INSERT, чтобы operation_id уже был сгенерирован)
CREATE TRIGGER trg_process_transfer
    AFTER INSERT ON operations
    FOR EACH ROW
    WHEN (NEW.operation_type IN ('TRANSFER_OUT', 'TRANSFER_IN'))
    EXECUTE FUNCTION process_transfer_operation();