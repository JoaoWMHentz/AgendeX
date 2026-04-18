CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO users ("Id", "Name", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
    gen_random_uuid(),
    'Admin Teste',
    'admin@agendex.local',
    crypt('Senha@123', gen_salt('bf', 12)),
    0,
    true,
    timezone('utc', now())
)
ON CONFLICT ("Email")
DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "PasswordHash" = EXCLUDED."PasswordHash",
    "Role" = EXCLUDED."Role",
    "IsActive" = EXCLUDED."IsActive";
