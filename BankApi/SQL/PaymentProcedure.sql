CREATE OR REPLACE PROCEDURE public.do_payment_attempt(
    p_card_token VARCHAR(255),
    p_last_four CHAR(4),
    p_order_total DECIMAL(15,2),
    p_description TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_card_id INTEGER;
    v_current_balance DECIMAL(15,2);
BEGIN
    -- Получаем card_id и текущий баланс
    SELECT 
        c.card_id,
        COALESCE(SUM(o.amount), 0) INTO v_card_id, v_current_balance
    FROM public.cards c
    LEFT JOIN public.operations o ON c.card_id = o.card_id
    WHERE c.card_token = p_card_token 
      AND c.last_four = p_last_four
    GROUP BY c.card_id;
    
    -- Проверяем, что карта найдена
    IF v_card_id IS NULL THEN
        RAISE EXCEPTION 'Карта с токеном % и последними цифрами % не найдена', 
            p_card_token, p_last_four;
    END IF;
    
    -- Проверяем достаточно ли средств
    IF v_current_balance < p_order_total THEN
        RAISE EXCEPTION 'Недостаточно средств. Доступно: %, требуется: %', 
            v_current_balance, p_order_total;
    END IF;
    
    -- Вставляем запись об операции списания
    INSERT INTO public.operations (
        card_id,
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
        v_card_id,
        'WITHDRAWAL',
        -p_order_total,  -- Отрицательная сумма для списания
        'USD',
        'POSTED',
        p_description,
        CURRENT_DATE,
        CURRENT_DATE,
        NOW(),
        NOW()
    );
    
    RAISE NOTICE 'Платеж успешно выполнен. Карта: •••• %, сумма: %', 
        p_last_four, p_order_total;
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Ошибка платежа: %', SQLERRM;
        RAISE;
END;
$$;