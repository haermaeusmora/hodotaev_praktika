-- Скрипт для обновления существующей базы данных
-- Добавить каскадное удаление для продаж
-- Запускать через pgAdmin или любой SQL-клиент

ALTER TABLE app.hodotaev_sales 
DROP CONSTRAINT IF EXISTS fk_sales_partner;

ALTER TABLE app.hodotaev_sales
ADD CONSTRAINT fk_sales_partner
FOREIGN KEY (partner_id)
REFERENCES app.hodotaev_partners(partner_id)
ON UPDATE CASCADE ON DELETE CASCADE;
