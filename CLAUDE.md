# AgendeX — Contexto do Projeto para Claude Code

## Visão Geral

Sistema web de gerenciamento de agendamentos de atendimentos entre clientes e atendentes/especialistas. Desenvolvido como prova prática para processo seletivo SENAI/FIESC (01064/2026).

## Stack

### Backend
- .NET 8 — ASP.NET Core Web API
- Clean Architecture (Domain / Application / Infrastructure / WebAPI)
- CQRS com MediatR
- Entity Framework Core + PostgreSQL
- FluentValidation
- AutoMapper
- JWT Bearer Authentication
- Swagger/OpenAPI (Swashbuckle)
- ClosedXML (exportação XLSX)
- xUnit + Moq + FluentAssertions (testes)

### Frontend
- React 18 + TypeScript + Vite
- React Query (TanStack Query)
- React Hook Form + Zod
- Axios
- Tailwind CSS + shadcn/ui
- Zustand

### Infraestrutura
- Docker + Docker Compose
- PostgreSQL 16

## Estrutura de Pastas

```
prova-dotnet-react-senior-01064-2026/
├── backend/
│   ├── AgendeX.sln
│   ├── src/
│   │   ├── AgendeX.Domain/          # Entidades, enums, interfaces
│   │   ├── AgendeX.Application/     # Use Cases, DTOs, handlers, validators
│   │   ├── AgendeX.Infrastructure/  # EF Core, repositórios, migrations, JWT
│   │   └── AgendeX.WebAPI/          # Controllers, Swagger, middlewares, Program.cs
│   └── tests/
│       └── AgendeX.Tests/           # Testes unitários (xUnit + Moq)
├── frontend/
│   └── src/
│       ├── components/              # Componentes reutilizáveis
│       ├── pages/                   # Páginas por módulo
│       ├── hooks/                   # Custom hooks
│       ├── services/                # Chamadas à API (Axios)
│       ├── store/                   # Estado global (Zustand)
│       ├── types/                   # Interfaces TypeScript
│       └── utils/                   # Helpers e formatadores
├── docker-compose.yml
└── README.md
```

## Modelo de Domínio

### Entidades principais

```
User
  - Id (Guid)
  - Nome (string)
  - Email (string, único)
  - SenhaHash (string)
  - Perfil (enum: Administrador | Atendente | Cliente)
  - Ativo (bool)
  - CriadoEm (DateTime)

ClienteDetalhe  [somente se Perfil == Cliente]
  - Id (Guid)
  - UserId (Guid, FK)
  - CPF (string, único)
  - DataNascimento (DateOnly)
  - Telefone (string)
  - Observacoes (string?)

TipoAtendimento  [tabela de apoio]
  - Id (int)
  - Descricao (string)
  - Exemplos: Consultoria, Suporte Técnico, Atendimento Comercial, Entrevista

DisponibilidadeAtendente
  - Id (Guid)
  - AtendenteId (Guid, FK → User)
  - DiaSemana (enum: Segunda ... Domingo)
  - HoraInicial (TimeOnly)
  - HoraFinal (TimeOnly)
  - Ativo (bool)
  - Regra: HoraFinal > HoraInicial
  - Regra: sem sobreposição de intervalos para mesmo atendente + dia

Agendamento
  - Id (Guid)
  - Titulo (string)
  - Descricao (string?)
  - TipoAtendimentoId (int, FK)
  - ClienteId (Guid, FK → User)
  - AtendenteId (Guid, FK → User)
  - Data (DateOnly)
  - Horario (TimeOnly)
  - Status (enum — ver abaixo)
  - JustificativaRecusa (string?)
  - ResumoAtendimento (string?)
  - CriadoEm (DateTime)
  - ConfirmadoEm (DateTime?)
  - CanceladoEm (DateTime?)
  - Observacoes (string?)
```

### Enum Status do Agendamento
```
PendenteConfirmacao
Confirmado
Recusado
Cancelado
Realizado
```

## Regras de Negócio Críticas

### Perfis e Permissões

| Ação | Administrador | Atendente | Cliente |
|------|:---:|:---:|:---:|
| Ver todos usuários | ✅ | ❌ | ❌ |
| Criar usuário | ✅ | ❌ | ❌ |
| Editar qualquer usuário | ✅ | ❌ | ❌ |
| Editar próprio usuário | ✅ | ✅ | ✅ |
| Excluir usuário | ✅ | ❌ | ❌ |
| Criar agendamento | ❌ | ❌ | ✅ |
| Ver todos agendamentos | ✅ | ❌ | ❌ |
| Ver próprios agendamentos | ✅ | ✅ (atribuídos) | ✅ |
| Confirmar/Recusar agendamento | ❌ | ✅ | ❌ |
| Cancelar agendamento | ✅ (qualquer) | ❌ | ✅ (com restrições) |
| Marcar como Realizado | ❌ | ✅ | ❌ |
| Reatribuir atendente | ✅ | ❌ | ❌ |
| Cadastrar disponibilidade | ✅ | ❌ | ❌ |
| Ver relatórios | ✅ | ✅ (restrito) | ❌ |

### Agendamento — Regras de Transição de Status

```
PendenteConfirmacao
  → Confirmado       (ação: Atendente confirma)
  → Recusado         (ação: Atendente recusa — JustificativaRecusa obrigatória)
  → Cancelado        (ação: Cliente ou Administrador cancela)

Confirmado
  → Cancelado        (ação: Cliente [somente se ainda não ocorreu] ou Administrador)
  → Realizado        (ação: Atendente [somente se data/hora já foi atingida])
```

### Cancelamento pelo Cliente
- Somente se status for `PendenteConfirmacao` ou `Confirmado`
- Somente se a data/hora do agendamento ainda não ocorreu

### Criação de Agendamento
- Data não pode ser anterior à data atual
- Horário deve estar dentro de uma janela de disponibilidade do atendente
- Não pode haver conflito com outro agendamento `Confirmado` ou `PendenteConfirmacao` do mesmo atendente no mesmo horário

### Disponibilidade
- HoraFinal > HoraInicial (obrigatório)
- Intervalos não podem se sobrepor para o mesmo atendente e dia da semana
- Ao consultar horários disponíveis: descontar horários já ocupados por agendamentos ativos

## Requisitos Não Funcionais

- **RQNF1** — Backend em C#/.NET 8+, banco PostgreSQL ou SQL Server, frontend React + TypeScript
- **RQNF2** — Frontend e backend em containers Docker distintos
- **RQNF3** — Mínimo 70% cobertura de testes unitários nas classes de regra de negócio, sem falhas
- **RQNF4** — Toda rota protegida por JWT; controle de acesso por perfil
- **RQNF5** — Retornos HTTP semânticos: 200, 201, 400, 401, 403, 404, 500
- **RQNF6** — Frontend exibe mensagens de erro amigáveis para todos os erros da API
- **RQNF7** — Estrutura de banco via EF Core Migrations (sem SQL manual)
- **RQNF8** — Campos com asterisco (*) são obrigatórios — validar no frontend e backend
- **RQNF9** — Swagger completo com exemplos de request/response em todos os endpoints
- **RQNF10** — Documentação técnica com diagramas de arquitetura, decisões de design e guia de instalação
- **RQNF11** — Microsserviços são opcionais (não priorizar no prazo atual)

## Módulos Funcionais

### RQF1 — Usuários
- 1.1 Listagem (filtrada por perfil)
- 1.2 Inserção (somente Admin; campos extras para Cliente)
- 1.3 Edição (sem alterar email e senha)

### RQF2 — Agendamentos
- 2.1 Criação (somente Cliente)
- 2.2 Listagem com filtros (Cliente, Atendente, Tipo, Status, Período)
- 2.3 Detalhes e ações por perfil
- 2.4 Cancelamento
- 2.5 Conclusão (marcar como Realizado)

### RQF3 — Disponibilidade de Agenda
- 3.1 Cadastro de janelas por dia da semana (somente Admin)
- 3.2 Consulta de horários disponíveis ao selecionar atendente + data

### RQF4 — Relatórios
- Filtros: Cliente(s), Atendente(s), Período, Tipo de Atendimento, Status
- Tipos de relatório: por atendente, por cliente, por status, taxa realizado vs cancelado, por tipo
- Exportação CSV e XLSX
- Tabela ordenável por qualquer coluna
- Acesso: Administrador (completo) e Atendente (restrito aos seus dados)

## Padrões de Código

### Backend
- Um handler MediatR por use case (nunca lógica de negócio no Controller)
- Controllers finos: recebem request → disparam comando/query → retornam resultado
- Validações com FluentValidation em classes separadas (`*Validator.cs`)
- Repositórios via interface no Domain, implementação na Infrastructure
- Nunca usar `var` onde o tipo não é óbvio
- Métodos com no máximo 20 linhas — extrair se necessário
- Nomes em português para entidades de domínio, inglês para infraestrutura técnica

### Testes
- Um arquivo de teste por handler
- Nomenclatura: `NomeDoMetodo_Cenario_ResultadoEsperado`
- Sempre mockar repositórios com Moq
- Usar FluentAssertions para assertions legíveis

### Frontend
- Componentes funcionais com TypeScript estrito
- Custom hooks para lógica de negócio (nunca direto no componente)
- React Query para cache e estados de loading/error
- Zod para validação de formulários
- Nunca usar `any` — sempre tipar

## Critérios de Avaliação (pesos)

1. Conhecimento técnico (peso 2)
2. Planejamento e organização
3. Comunicação e interação
4. Trabalho colaborativo
5. Análise e síntese

## Prazo

**19/04/2026 às 23h59** — envio obrigatório via repositório + Pandapé

## Ordem de Implementação Recomendada

1. Estrutura base + Docker Compose + migrations iniciais
2. Autenticação JWT (login, geração de token, middleware)
3. Módulo de Usuários (CRUD completo)
4. Módulo de Disponibilidade (pré-requisito para agendamentos)
5. Módulo de Agendamentos (criação, listagem, ações, cancelamento, conclusão)
6. Módulo de Relatórios (queries + exportação)
7. Testes unitários dos handlers
8. Documentação técnica (README, diagramas Mermaid)
