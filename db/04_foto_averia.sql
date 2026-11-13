-- 04_foto_averia.sql
-- Migración del cambio CR-01 (RF-04): permite guardar una foto por avería.
-- Ejecútelo una sola vez, después de los scripts 01 a 03.

SET NAMES utf8mb4;
USE averias;

ALTER TABLE averia ADD COLUMN foto_url VARCHAR(255) NULL AFTER descripcion;
