-- 01_schema.sql
-- Crea la base de datos "averias" y sus tablas.
-- Advertencia: borra las tablas si ya existen. Úselo solo en desarrollo.

SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS averias
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE averias;

DROP TABLE IF EXISTS averia;
DROP TABLE IF EXISTS tecnico;
DROP TABLE IF EXISTS tipo_averia;
DROP TABLE IF EXISTS aula;

CREATE TABLE aula (
  id INT AUTO_INCREMENT PRIMARY KEY,
  edificio VARCHAR(60) NOT NULL,
  nombre VARCHAR(60) NOT NULL,
  UNIQUE KEY uq_aula (edificio, nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE tipo_averia (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(60) NOT NULL UNIQUE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE tecnico (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE averia (
  id INT AUTO_INCREMENT PRIMARY KEY,
  aula_id INT NOT NULL,
  tipo_id INT NOT NULL,
  descripcion VARCHAR(500) NOT NULL,
  reportado_por VARCHAR(100) NULL,
  estado ENUM('Reportada', 'Asignada', 'En reparación', 'Cerrada') NOT NULL DEFAULT 'Reportada',
  tecnico_id INT NULL,
  creada_en DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizada_en DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT fk_averia_aula FOREIGN KEY (aula_id) REFERENCES aula (id),
  CONSTRAINT fk_averia_tipo FOREIGN KEY (tipo_id) REFERENCES tipo_averia (id),
  CONSTRAINT fk_averia_tecnico FOREIGN KEY (tecnico_id) REFERENCES tecnico (id),
  KEY idx_averia_estado (estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
