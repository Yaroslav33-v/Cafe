INSERT INTO public.cards (card_token, last_four, expiry_month, expiry_year, card_type) 
VALUES 
    ('pm_4242424242424242_123', '4242', 9, 2026, 'Visa'),
    ('pm_5555555555554444_001', '4444', 12, 2027, 'MasterCard'),
    ('pm_4000000000009995_567', '9995', 12, 2028, 'American Express');

-- Депозит на карту 1 (+1000 USD)
INSERT INTO operations (card_id, operation_type, amount, description, status, posted_at)
VALUES (1, 'DEPOSIT', 1000.00, 'Пополнение счета', 'POSTED', NOW());

-- Снятие с карты 1 (-200 USD)
INSERT INTO operations (card_id, operation_type, amount, description, status, posted_at)
VALUES (1, 'WITHDRAWAL', -200.00, 'Снятие наличных', 'POSTED', NOW());

-- Комиссия на карту 1 (-10 USD)
INSERT INTO operations (card_id, operation_type, amount, description, status, posted_at)
VALUES (1, 'FEE', -10.00, 'Ежемесячная комиссия', 'POSTED', NOW());

-- Проценты на карту 2 (+50 USD)
INSERT INTO operations (card_id, operation_type, amount, description, status, posted_at)
VALUES (2, 'INTEREST', 50.00, 'Начисление процентов', 'POSTED', NOW());

-- Перевод 500 USD с карты 1 на карту 2
INSERT INTO operations (
    card_id,
    transfer_card_id,
    operation_type,
    amount,
    description,
    status,
    posted_at
) VALUES (
    1,              -- card_id (откуда)
    2,              -- transfer_card_id (куда)
    'TRANSFER_OUT', -- тип операции
    -500.00,        -- сумма (отрицательная для OUT)
    'Перевод на карту друга',
    'POSTED',
    NOW()
);
