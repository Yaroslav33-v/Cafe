CREATE TABLE public.categories(
	category_id SERIAL PRIMARY KEY,
	name VARCHAR(25) NOT NULL UNIQUE
);

CREATE TABLE public.food(
	food_id SERIAL PRIMARY KEY,
	food_name VARCHAR(100) NOT NULL UNIQUE,
	price DECIMAL(7, 2) NOT NULL,
	calories DECIMAL(7, 2) NOT NULL,
	weight DECIMAL(7, 2) NOT NULL,
	ingredients TEXT NOT NULL,
	description TEXT NOT NULL,
	front_image_address VARCHAR(255) NOT NULL,
	back_image_address VARCHAR(255) NOT NULL, -- Адрес изображения в файловой системе программы
	category_id INTEGER REFERENCES public.categories(category_id)
);

CREATE TABLE public.offers(
	offer_id SERIAL PRIMARY KEY,
	offer_name VARCHAR(100) NOT NULL,
	description TEXT NOT NULL,
	discount DECIMAL(3, 2) NOT NULL, -- Скидка в процентах
	starts_at DATE NOT NULL DEFAULT CURRENT_DATE,
	ends_at DATE NOT NULL DEFAULT CURRENT_DATE + 7,
	is_notificated BOOLEAN DEFAULT false -- Для тг-бота
		
	CONSTRAINT chk_starts_earlier_ends CHECK (
		starts_at < ends_at
	)
);

CREATE TABLE public.users(
	user_id SERIAL PRIMARY KEY,
	login VARCHAR(25) NOT NULL UNIQUE,
	password VARCHAR(255) NOT NULL,
	is_admin BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE public.orders(
	order_id SERIAL PRIMARY KEY,
	order_number VARCHAR(5) NOT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	status VARCHAR(20) NOT NULL CHECK (status IN (
		'Ожидает оплаты',
		'Ошибка оплаты',
		'Готовится',
		'Готов'
	)),
	done_at TIMESTAMP,
	user_id INTEGER REFERENCES users(user_id),
	
	CONSTRAINT chk_done_at_required CHECK (
        (status = 'Готов' AND done_at IS NOT NULL) OR
        (status != 'Готов' AND done_at IS NULL)
    ),
    
    CONSTRAINT chk_done_at_valid CHECK (
        done_at IS NULL OR done_at >= created_at
    )
);

CREATE TABLE public.offers_food(
	id BIGSERIAL PRIMARY KEY,
	food_id INTEGER REFERENCES public.food(food_id),
	offer_id INTEGER REFERENCES public.offers(offer_id)
);

CREATE TABLE public.food_orders(
	id BIGSERIAL PRIMARY KEY,
	food_id INTEGER REFERENCES public.food(food_id),
	order_id INTEGER REFERENCES public.orders(order_id),
	food_quantity INTEGER NOT NULL CHECK (food_quantity > 0)
);

CREATE TABLE public.favourite_food(
	id BIGSERIAL PRIMARY KEY,
	food_id INTEGER REFERENCES public.food(food_id),
	user_id INTEGER REFERENCES public.users(user_id)
);

CREATE TABLE public.promocodes(
	promocode_id SERIAL PRIMARY KEY,
	code VARCHAR(20) NOT NULL UNIQUE,
	from_sum DECIMAL(7, 2) NOT NULL,
	discount DECIMAL(7, 2) NOT NULL, -- Скидка( -100 рублей, -200 рублей и т.д.)
	expires_at DATE NOT NULL DEFAULT CURRENT_DATE + 7
);

CREATE TABLE public.telegram_chats(
	id SERIAL PRIMARY KEY,
    chat_id BIGINT NOT NULL UNIQUE, 
    is_active BOOLEAN DEFAULT true
);

CREATE INDEX idx_telegram_chats_chat_id ON public.telegram_chats(chat_id);
CREATE INDEX idx_offers_food ON offers_food(food_id, offer_id);
CREATE INDEX idx_food_orders ON food_orders(food_id, order_id);
CREATE INDEX idx_favourite_food ON favourite_food(user_id, food_id);
