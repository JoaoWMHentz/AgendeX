-- SQL simples para criar 1 admin de teste
-- Login: admin@agendex.local
-- Senha: Senha@123

INSERT INTO public.users ("Id","Name","Email","PasswordHash","Role","IsActive","CreatedAt") VALUES
	 ('feec6de4-ebe0-47ae-bee6-436da009950e'::uuid,'Admin Teste','admin@agendex.local','$2a$12$8De3HJk4i3C996mGXF/sAefCFSHI80oJjA0KL66Y1T8fl9tXONl9q',0,true,'2026-04-17 21:36:54.778126-03');