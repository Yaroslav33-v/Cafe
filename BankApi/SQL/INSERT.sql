INSERT INTO public.cards (card_token, last_four, expiry_month, expiry_year, card_type) 
VALUES 
    ('tok_visa_1111', '1111', 12, 2026, 'Visa'),
    ('tok_mc_2222', '2222', 11, 2026, 'MasterCard'),
    ('tok_amex_3333', '3333', 10, 2027, 'American Express'),
    ('tok_visa_4444', '4444', 9, 2027, 'Visa');

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
