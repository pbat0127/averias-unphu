-- 02_catalogos.sql
-- Datos fijos: aulas, tipos de avería y técnicos (ficticios).

SET NAMES utf8mb4;
USE averias;

INSERT INTO aula (edificio, nombre) VALUES
  ('Edificio A', 'Aula A-101'),
  ('Edificio A', 'Aula A-102'),
  ('Edificio A', 'Aula A-203'),
  ('Edificio B', 'Laboratorio B-204'),
  ('Edificio B', 'Laboratorio B-205'),
  ('Edificio C', 'Auditorio C-001');

INSERT INTO tipo_averia (nombre) VALUES
  ('Aire acondicionado'),
  ('Electricidad'),
  ('Internet / Wi-Fi'),
  ('Mobiliario'),
  ('Pizarra'),
  ('Proyector');

INSERT INTO tecnico (nombre) VALUES
  ('Técnico de turno A'),
  ('Técnico de turno B'),
  ('Técnico de redes');
