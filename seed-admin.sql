-- SQL simples para criar 1 admin de teste
-- Login: admin@agendex.local
-- Senha: Teste_123

CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO users ("Id", "Name", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
  gen_random_uuid(),
  'Admin Teste',
  'admin@agendex.local',
  crypt('Teste_123', gen_salt('bf', 12)),
  0,
  true,
  now()
)
ON CONFLICT ("Email") DO NOTHING;
