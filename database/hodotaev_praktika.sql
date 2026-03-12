\c postgres

DROP ROLE IF EXISTS app;
CREATE ROLE app WITH LOGIN PASSWORD '123456789';

DROP DATABASE IF EXISTS hodotaev_praktika;
CREATE DATABASE hodotaev_praktika OWNER app;

\c hodotaev_praktika

DROP SCHEMA IF EXISTS app CASCADE;
CREATE SCHEMA app AUTHORIZATION app;

ALTER ROLE app SET search_path TO app, public;

CREATE TABLE app.hodotaev_partner_types (
    partner_type_id SERIAL PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500)
);

CREATE TABLE app.hodotaev_partners (
    partner_id SERIAL PRIMARY KEY,
    partner_type_id INTEGER NOT NULL,
    company_name VARCHAR(255) NOT NULL,
    legal_address VARCHAR(500),
    inn VARCHAR(20),
    director_full_name VARCHAR(255),
    phone VARCHAR(50),
    email VARCHAR(100),
    rating INTEGER NOT NULL DEFAULT 0 CHECK (rating >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_partners_partner_type 
        FOREIGN KEY (partner_type_id) 
        REFERENCES app.hodotaev_partner_types(partner_type_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE INDEX idx_partners_partner_type ON app.hodotaev_partners(partner_type_id);

CREATE TABLE app.hodotaev_products (
    product_id SERIAL PRIMARY KEY,
    product_name VARCHAR(255) NOT NULL,
    article VARCHAR(50) UNIQUE,
    product_type VARCHAR(100),
    description VARCHAR(1000),
    min_price DECIMAL(10, 2) NOT NULL CHECK (min_price >= 0),
    unit VARCHAR(50) DEFAULT 'шт.',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE app.hodotaev_sales (
    sale_id SERIAL PRIMARY KEY,
    partner_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    sale_price DECIMAL(10, 2) NOT NULL CHECK (sale_price >= 0),
    sale_date DATE NOT NULL DEFAULT CURRENT_DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_sales_partner 
        FOREIGN KEY (partner_id) 
        REFERENCES app.hodotaev_partners(partner_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_sales_product 
        FOREIGN KEY (product_id) 
        REFERENCES app.hodotaev_products(product_id)
        ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE INDEX idx_sales_partner ON app.hodotaev_sales(partner_id);
CREATE INDEX idx_sales_product ON app.hodotaev_sales(product_id);
CREATE INDEX idx_sales_date ON app.hodotaev_sales(sale_date);

CREATE TABLE app.hodotaev_partner_rating_history (
    history_id SERIAL PRIMARY KEY,
    partner_id INTEGER NOT NULL,
    old_rating INTEGER,
    new_rating INTEGER NOT NULL,
    change_reason VARCHAR(500),
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    changed_by VARCHAR(100),
    CONSTRAINT fk_rating_history_partner 
        FOREIGN KEY (partner_id) 
        REFERENCES app.hodotaev_partners(partner_id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX idx_rating_history_partner ON app.hodotaev_partner_rating_history(partner_id);

CREATE OR REPLACE VIEW app.hodotaev_partners_summary AS
SELECT 
    p.partner_id,
    p.company_name,
    pt.type_name AS partner_type,
    p.rating,
    p.phone,
    p.email,
    COALESCE(SUM(s.quantity * s.sale_price), 0) AS total_sales_amount
FROM app.hodotaev_partners p
LEFT JOIN app.hodotaev_partner_types pt ON p.partner_type_id = pt.partner_type_id
LEFT JOIN app.hodotaev_sales s ON p.partner_id = s.partner_id
GROUP BY p.partner_id, p.company_name, pt.type_name, p.rating, p.phone, p.email;

CREATE OR REPLACE FUNCTION app.hodotaev_calculate_discount(p_partner_id INTEGER)
RETURNS DECIMAL(5, 2) AS $$
DECLARE
    total_sales DECIMAL(10, 2);
    discount DECIMAL(5, 2);
BEGIN
    SELECT COALESCE(SUM(quantity * sale_price), 0)
    INTO total_sales
    FROM app.hodotaev_sales
    WHERE partner_id = p_partner_id;
    
    IF total_sales < 10000 THEN
        discount := 0;
    ELSIF total_sales >= 10000 AND total_sales < 50000 THEN
        discount := 5;
    ELSIF total_sales >= 50000 AND total_sales < 300000 THEN
        discount := 10;
    ELSE
        discount := 15;
    END IF;
    
    RETURN discount;
END;
$$ LANGUAGE plpgsql STABLE;

CREATE OR REPLACE FUNCTION app.hodotaev_update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_partners_update_updated_at
    BEFORE UPDATE ON app.hodotaev_partners
    FOR EACH ROW
    EXECUTE FUNCTION app.hodotaev_update_updated_at_column();

INSERT INTO app.hodotaev_partner_types (type_name, description) VALUES
('Оптовый магазин', 'Продажа продукции крупными партиями'),
('Розничный магазин', 'Продажа продукции в розницу'),
('Интернет-магазин', 'Дистанционная продажа через веб-сайт'),
('Дистрибьютор', 'Оптовая продажа с доставкой до конечных потребителей'),
('Корпоративный клиент', 'Продажа юридическим лицам для собственных нужд');

INSERT INTO app.hodotaev_products (product_name, article, product_type, description, min_price, unit) VALUES
('Ламинат Дуб Классик', 'LAM-001', 'Ламинат', 'Ламинированное напольное покрытие, 33 класс', 850.00, 'кв.м'),
('Ламинат Орех Премиум', 'LAM-002', 'Ламинат', 'Ламинированное напольное покрытие, 34 класс', 1200.00, 'кв.м'),
('Паркетная доска Ясень', 'PAR-001', 'Паркетная доска', 'Натуральная паркетная доска', 2500.00, 'кв.м'),
('Паркетная доска Дуб', 'PAR-002', 'Паркетная доска', 'Натуральная паркетная доска', 3200.00, 'кв.м'),
('Виниловый пол Модерн', 'VIN-001', 'Виниловое покрытие', 'Влагостойкое виниловое покрытие', 1500.00, 'кв.м'),
('Плинтус деревянный', 'PLI-001', 'Плинтус', 'Плинтус из натурального дерева', 350.00, 'м.п'),
('Подложка пробковая', 'SUB-001', 'Подложка', 'Подложка под ламинат 3мм', 450.00, 'кв.м');

INSERT INTO app.hodotaev_partners (partner_type_id, company_name, legal_address, inn, director_full_name, phone, email, rating) VALUES
(1, 'ООО "СтройМастер"', 'г. Москва, ул. Ленина, д. 10', '7701234567', 'Иванов Иван Иванович', '+7 (495) 123-45-67', 'info@stroymaster.ru', 5),
(2, 'ИП Петров А.С.', 'г. Санкт-Петербург, Невский пр., д. 25', '7801234568', 'Петров Александр Сергеевич', '+7 (812) 234-56-78', 'petrov@mail.ru', 3),
(3, 'ООО "ОнлайнПол"', 'г. Екатеринбург, ул. Мира, д. 5', '6601234569', 'Сидорова Мария Петровна', '+7 (343) 345-67-89', 'sales@onlinefloor.ru', 4),
(4, 'ЗАО "Торговый Дом Пол"', 'г. Казань, пр. Победы, д. 100', '1601234570', 'Кузнецов Петр Иванович', '+7 (843) 456-78-90', 'td@pol-kazan.ru', 5),
(1, 'ООО "РегионСтрой"', 'г. Новосибирск, ул. Советская, д. 15', '5401234571', 'Смирнов Андрей Владимирович', '+7 (383) 567-89-01', 'region@stroy-nsk.ru', 2);

INSERT INTO app.hodotaev_sales (partner_id, product_id, quantity, sale_price, sale_date) VALUES
(1, 1, 500, 850.00, '2024-01-15'),
(1, 2, 300, 1200.00, '2024-02-20'),
(1, 3, 200, 2500.00, '2024-03-10'),
(1, 4, 150, 3200.00, '2024-04-05'),
(2, 1, 50, 850.00, '2024-01-20'),
(2, 5, 30, 1500.00, '2024-02-25'),
(3, 2, 100, 1200.00, '2024-01-25'),
(3, 3, 50, 2500.00, '2024-03-15'),
(3, 4, 40, 3200.00, '2024-04-10'),
(4, 1, 1000, 850.00, '2024-02-01'),
(4, 2, 500, 1200.00, '2024-03-01'),
(4, 5, 300, 1500.00, '2024-04-01'),
(5, 6, 20, 350.00, '2024-03-05'),
(5, 7, 10, 450.00, '2024-04-15');

GRANT ALL PRIVILEGES ON SCHEMA app TO app;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA app TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app TO app;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app;

ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT ALL ON TABLES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT ALL ON SEQUENCES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT ALL ON FUNCTIONS TO app;
