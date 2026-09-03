# AMR-Financeiro — Contexto para Claude Code

## Identidade do módulo
Módulo financeiro do **AMR SYSTEM** — gestão de Contas a Pagar, Contas a Receber, Lançamentos e Plano de Contas.
- **API local**: http://localhost:5015/swagger
- **Web local**: http://localhost:5173
- **ALB (AWS)**: listener `:80` · serviços ECS `amr-financeiro-api` + `amr-financeiro-web`
- **Banco**: SQL Server (dev) / SQLite via EF Core 8 (EFS `/data` em produção)

## Ecossistema
AMR-Financeiro é o módulo financeiro do **AMR SYSTEM** — ERP corporativo composto por 3 sistemas ativos:
- **AMR-Financeiro** (este repo) — SQL Server/SQLite, porta API :5015, web :5173
- **AMR-Core** — SQLite, porta API :5001, web :5175
- **AMR-Forms-Fábrica** — SQLite, porta API :5186, web :5174

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 8 + Clean Architecture + CQRS (MediatR) |
| ORM | EF Core 8 + SQL Server (dev) / SQLite (prod AWS) |
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS |
| Auth | JWT Bearer (HS256, 8h, PBKDF2/SHA256 password hash) |
| Mensageria | RabbitMQ + MassTransit (Worker Background Service) |
| Testes | xUnit + Coverlet (54 testes) |
| Infra | AWS ECS Fargate + ALB + EFS + ECR (Terraform) |
| CI/CD | GitHub Actions (ci.yml + deploy-aws.yml) |

Credenciais dev: `admin` / `admin123` (seed automático no startup).

## Arquitetura

```
src/
├── AMR.Financeiro.Domain/          # Entidades, enums, eventos, interfaces
├── AMR.Financeiro.Application/     # CQRS handlers, DTOs, validators (MediatR)
├── AMR.Financeiro.Infrastructure/  # EF Core, repositórios, RabbitMQ, seed
├── AMR.Financeiro.API/             # Controllers, Program.cs, DI, JWT, Swagger
├── AMR.Financeiro.Worker/          # Consumer RabbitMQ (LancamentoCriadoConsumer)
└── AMR.Financeiro.Shared/          # Utilitários cross-cutting
frontend/                           # React 18 + Vite + Tailwind
tests/AMR.Financeiro.Tests/         # xUnit — 15 testes (handlers)
infra/terraform/                    # IaC AWS (ECS, ALB, EFS, ECR, IAM, Secrets)
```

Padrões: Clean Architecture, CQRS+MediatR, Repository Pattern, Unit of Work, DI.

## Entidades do Domínio

- **PlanoContas** — hierárquico (código 1/1.1/1.1.1, Sintética/Analítica, paiId)
- **LancamentoFinanceiro** — lançamento contábil (Débito/Crédito, ligado ao PlanoContas)
- **ContaPagar** — Aberta → Paga/Cancelada/Vencida; baixa gera Lançamento de Débito
- **ContaReceber** — Aberta → Recebida/Cancelada/Vencida; baixa gera Lançamento de Crédito
- **Usuario** — Autenticação JWT (Role: Admin/User)

### Enums

```csharp
TipoLancamento:    Débito | Crédito
StatusConta:       Aberta | Paga | Cancelada | Vencida
StatusContaReceber:Aberta | Recebida | Cancelada | Vencida
TipoContaPlano:    Sintética | Analítica
OrigemLancamento:  ContaPagar | ContaReceber | Manual
```

### Eventos de domínio

- `LancamentoCriadoEvent` — publicado no RabbitMQ (exchange `lancamentos`, queue `lancamento.criado`) ao criar lançamento

## CQRS — Features

### Auth
- `LoginCommand` → `LoginHandler` (valida credenciais, emite JWT)

### PlanoContas
- Queries: `GetPlanoContasQuery` (árvore hierárquica)
- Commands: `CriarPlanoContasCommand`, `AtualizarPlanoContasCommand`

### Lancamentos
- Queries: `GetLancamentosQuery`
- Commands: `CriarLancamentoCommand` (valida PlanoContas analítica, publica evento)

### ContasPagar
- Queries: `GetContasPagarQuery`, `GetContaPagarByIdQuery`
- Commands: `CriarContaPagarCommand`, `PagarContaCommand` (cria Lançamento Débito), `CancelarContaCommand`

### ContasReceber
- Queries: `GetContasReceberQuery`, `GetContaReceberByIdQuery`
- Commands: `CriarContaReceberCommand`, `ReceberContaCommand` (cria Lançamento Crédito), `CancelarContaReceberCommand`

## Comandos Principais

```bash
# Backend
cd src/AMR.Financeiro.API && dotnet run
# → http://localhost:5015/swagger

# Frontend
cd frontend && npm install && npm run dev
# → http://localhost:5173

# RabbitMQ (necessário para Worker)
docker-compose up -d

# Testes
dotnet test tests/AMR.Financeiro.Tests/
dotnet test --settings coverlet.runsettings   # com cobertura

# Migrations
dotnet ef migrations add <Nome> --project ../AMR.Financeiro.Infrastructure
dotnet ef database update
```

## Deploy AWS

Push para `main` dispara `deploy-aws.yml`:
1. Build Docker → push ECR (`amr-financeiro-api`, `amr-financeiro-web`)
2. Update ECS task definition + force deploy no cluster `amr-system`
3. Health check no ALB

- **Produção:** `amr-system-1908797477.sa-east-1.elb.amazonaws.com`
- **Cluster:** `amr-system` | **Region:** `sa-east-1` | **Account:** `474874558993`
- **ECR:** `amr-financeiro-api`, `amr-financeiro-web`
- **EFS:** montado em `/data` para persistência do SQLite

## Convenções

- **Clean Architecture**: dependências apontam sempre para dentro (Domain → Application → Infrastructure/API)
- **Handlers** não referenciam Infrastructure diretamente — usam interfaces do Domain
- **Repositórios** ficam em Infrastructure, registrados via `DependencyInjection.cs`
- **DTOs** ficam junto ao Feature em Application (não no Domain)
- Serialize enums como string (`JsonStringEnumConverter` no Program.cs)
- Swagger disponível em produção via `/api/swagger` (ALB-compatible RoutePrefix)

## Integração com AMR SYSTEM

- **AMR-Fábrica → AMR-Financeiro**: saída de ficha gera `ContaPagar`; NF transmitida gera `ContaReceber` (integração fail-safe via RabbitMQ)
- **AMR-Core**: repositório separado com Produtos, Pedidos de Compra/Venda
- ALB compartilhado (cluster `amr-system`) roteia tráfego entre os 3 sistemas

## Páginas frontend

| Rota | Página | Funcionalidades |
|------|--------|-----------------|
| `/login` | `LoginPage` | Autenticação JWT, redirect automático |
| `/dashboard` | `DashboardPage` | Cards KPIs + gráfico lançamentos por período |
| `/contas-pagar` | `ContasPagarPage` | CRUD, baixa manual, status, vencimento |
| `/contas-receber` | `ContasReceberPage` | CRUD, baixa com lançamento, alertas vencimento |
| `/lancamentos` | `LancamentosPage` | Listagem lançamentos com categorização |
| `/plano-contas` | `PlanoContasPage` | Árvore hierárquica do plano de contas |

## Estado do Projeto — Sprint 6 ativo (02/06–24/06/2026)

- 54 testes unitários passando — todos os handlers cobertos (ContasPagar, ContasReceber, Lancamentos, PlanoContas)
- Infra Terraform unificada provisionada na AWS (cluster `amr-system`)
- CI/CD GitHub Actions funcionando
- **Sprint 6 entregues no AMR-Financeiro:**
  - CLAUDE.md criado na raiz do repo (03/06/2026 · commit `9136720`)
  - ErrorHandling ProblemDetails RFC 7807: ExceptionHandlingMiddleware + ResultExtensions + Result\<T\> (04/06/2026 · commit `8c461a1`)
  - FluentValidation reusável: ValidationBehavior + 5 validators + ValidationException → 400 no middleware (04/06/2026 · commit `158cb84`)
  - Security Hardening: AWS Account ID removido do deploy-aws.yml → `${{ secrets.AWS_ACCOUNT_ID }}`; .gitignore reforçado; LICENSE BSL 1.1 adicionado (04/06/2026 · commit `16309ba`)

## Protocolo de Encerramento de Card

Ao concluir qualquer card/tarefa, executar nesta ordem:

1. **Git** — commit descritivo + `git push -u origin <branch>`
2. **Notion card** — atualizar `Entrega` para a data real e adicionar referência do commit no conteúdo da página
3. **Kanban** — atualizar a propriedade `Status` do card no Notion para `✅ Concluído` (via `update_properties`)
4. **CLAUDE.md** — atualizar seção `Estado do Projeto` se houve mudança relevante de contexto
5. **Próximo Card** — identificar o próximo card no Backlog, atualizar `Status` para `▶️ Em andamento` no Notion e atualizar a seção `## Próximo Card` abaixo
6. **Merge para main** — fazer merge do branch de feature para `main` e push, garantindo que o CLAUDE.md atualizado esteja disponível para a próxima sessão

---

## Protocolo de Encerramento de Sessão

Disparado quando o usuário disser **"encerrar sessão"** (ou "fechar sessão", "fim do dia", "encerrando").

Executar em ordem:

1. Consolidar todos os cards concluídos na sessão (título, commit, link Notion)
2. **Kanban** — atualizar `Status` de todos os cards trabalhados na sessão (concluídos → `✅ Concluído`, próximo → `▶️ Em andamento`)
3. Confirmar o Próximo Card atualizado no `CLAUDE.md`
4. Criar **1 evento no Google Calendar** com:
   - Título: `AMR-Financeiro ✅ Sessão DD/MM/YYYY`
   - Data/hora: agora (duração 30 min)
   - Descrição: cards entregues + commits + próximo card
   - Reminder: e-mail 0 minutos antes (notificação imediata)

> Apenas 1 chamada de Calendar por sessão — independente de quantos cards foram feitos.

---

## Próximo Card

**Cards AMR-Financeiro Sprint 6 concluídos** (CLAUDE.md, ErrorHandling, Repository Pattern, FluentValidation).

> **Card pendente em outro repo (requer sessão separada):**
> 🔧 CLAUDE.md em AMR-Forms-Fábrica
> - Notion: https://www.notion.so/374d35f21de58191939acf6c08a6e3e5
> - Requer sessão Claude Code web com o repo `al-ramos/AMR-Forms-Fabrica`

---

## Troubleshooting Frequente

| Problema | Solução |
|----------|---------|
| Porta errada no backend | Verificar `launchSettings.json` → `applicationUrl: http://localhost:5015` |
| CORS bloqueando frontend | `appsettings.Development.json` → `AllowedOrigins: ["http://localhost:5173"]` |
| JWT inválido/expirado | Token expira em 8h; seed cria `admin`/`admin123` automaticamente no startup |
| Worker sem conexão RabbitMQ | Executar `docker-compose up -d` antes de iniciar o Worker |
| MediatR não resolve handlers | Verificar registro em `Application/DependencyInjection.cs` |
| EF migration falha | Usar `--project ../AMR.Financeiro.Infrastructure` a partir de `src/AMR.Financeiro.API` |
| AWS CLI no Git Bash | Prefixar com `MSYS_NO_PATHCONV=1 aws ecs ...` |
