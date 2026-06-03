# AMR-Financeiro — Contexto para Claude Code

Módulo financeiro do **AMR SYSTEM** — gestão de Contas a Pagar, Contas a Receber, Lançamentos e Plano de Contas.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 8 + Clean Architecture + CQRS (MediatR) |
| ORM | EF Core 8 + SQL Server (dev) / SQLite (prod AWS) |
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS |
| Auth | JWT Bearer (HS256, 8h, PBKDF2/SHA256 password hash) |
| Mensageria | RabbitMQ + MassTransit (Worker Background Service) |
| Testes | xUnit + Coverlet (15 testes) |
| Infra | AWS ECS Fargate + ALB + EFS + ECR (Terraform) |
| CI/CD | GitHub Actions (ci.yml + deploy-aws.yml) |

## Portas e URLs

| Serviço | Dev | Prod |
|---------|-----|------|
| API | http://localhost:5015 | http://amr-system-1908797477.sa-east-1.elb.amazonaws.com |
| Swagger | http://localhost:5015/swagger | mesma URL base /api/swagger |
| Frontend | http://localhost:5173 | ALB porta 80 |
| RabbitMQ AMQP | localhost:5672 | — |
| RabbitMQ UI | http://localhost:15672 | — |

Credenciais dev: `admin` / `admin123` (seed automático no startup).

## Estrutura de Projetos

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

## Domínio

### Entidades principais

- **PlanoContas** — Plano de contas hierárquico (código 1/1.1/1.1.1, Sintética/Analítica, paiId)
- **LancamentoFinanceiro** — Lançamento contábil (Débito/Crédito, ligado ao PlanoContas)
- **ContaPagar** — Conta a pagar (Aberta → Paga/Cancelada/Vencida), baixa gera Lançamento de Débito
- **ContaReceber** — Conta a receber (Aberta → Recebida/Cancelada/Vencida), baixa gera Lançamento de Crédito
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

## Testes

```bash
# Rodar todos os testes
dotnet test tests/AMR.Financeiro.Tests/

# Com cobertura
dotnet test --settings coverlet.runsettings
```

Testes existentes (15):
- `PagarContaHandlerTests` — valida fluxo de baixa de ContaPagar
- `CriarLancamentoHandlerTests` — valida criação de lançamento
- `CriarPlanoContasHandlerTests` — valida criação no plano de contas

Padrão: mocks com `Moq`, asserts com `FluentAssertions`.

## Dev local

```bash
# Backend
cd src/AMR.Financeiro.API
dotnet run
# → http://localhost:5015/swagger

# Frontend
cd frontend
npm install
npm run dev
# → http://localhost:5173

# RabbitMQ (necessário para Worker)
docker-compose up -d
```

## Database

EF Core com migrations automáticas no startup. Para criar nova migration:

```bash
cd src/AMR.Financeiro.API
dotnet ef migrations add NomeDaMigration --project ../AMR.Financeiro.Infrastructure
dotnet ef database update
```

Seed executado no startup: PlanoContas padrão + lançamentos demo + usuário admin.

## CI/CD

- **ci.yml** — build + testes + cobertura (push para main/develop/feat/fix + PRs)
- **deploy-aws.yml** — build Docker → push ECR → force ECS deploy → health check (push para main)

AWS Region: `sa-east-1` | Cluster: `amr-system` | Conta: `474874558993`

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
