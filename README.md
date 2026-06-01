# 💰 AMR-Financeiro

> Módulo financeiro do ecossistema AMR SYSTEM — gestão de contas a pagar, contas a receber, lançamentos e notas fiscais.

[![CI](https://github.com/alexsandro-ramos/AMR.Financeiro/actions/workflows/ci.yml/badge.svg)](https://github.com/alexsandro-ramos/AMR.Financeiro/actions/workflows/ci.yml)
[![Deploy AWS](https://github.com/alexsandro-ramos/AMR.Financeiro/actions/workflows/deploy-aws.yml/badge.svg)](https://github.com/alexsandro-ramos/AMR.Financeiro/actions/workflows/deploy-aws.yml)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)

Parte do [AMR SYSTEM](../README.md) — veja a documentação do ecossistema completo.

---

## ✨ Funcionalidades

- **Dashboard financeiro** — cards de KPIs + gráfico de lançamentos por período
- **Contas a Pagar** — CRUD completo com status, vencimento e fornecedor
- **Contas a Receber** — controle de recebíveis com status de liquidação
- **Lançamentos** — registro de entradas e saídas com categorização
- **Notas Fiscais** — emissão e controle com vinculação a pedidos
- **Autenticação** — JWT com refresh token

---

## 🛠️ Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 8 + Clean Architecture + CQRS (MediatR) |
| ORM | EF Core 8 + SQL Server |
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS |
| Auth | JWT Bearer + Secrets Manager (AWS) |
| Testes | xUnit + Coverlet |

---

## 🚀 Rodando localmente

```powershell
# Backend
cd src/AMR.Financeiro.API
dotnet run
# → http://localhost:5015/swagger

# Frontend
cd frontend
npm install
npm run dev
# → http://localhost:5173
```

Ou suba o ecossistema completo com `.\automation\start-amr-dev.ps1` na raiz do AMR SYSTEM.

---

## 🏗️ Estrutura

```
src/
├── AMR.Financeiro.Domain/          # Entidades, value objects, regras
├── AMR.Financeiro.Application/     # CQRS handlers, DTOs, validadores
├── AMR.Financeiro.Infrastructure/  # EF Core, SQL Server, repositórios
├── AMR.Financeiro.Shared/          # Contratos compartilhados
└── AMR.Financeiro.API/             # Controllers, Program.cs, DI
frontend/                            # React + Vite + TypeScript
tests/AMR.Financeiro.Tests/          # xUnit + Coverlet
```

---

## ☁️ Deploy

Push para `main` dispara o workflow `.github/workflows/deploy-aws.yml`:

1. **Terraform** — provisiona/atualiza ECR, ECS, ALB, EFS
2. **Build & Push** — imagem Docker para ECR com tag do commit
3. **Deploy ECS** — atualiza task definition + force new deployment
4. **Health check** — aguarda ALB responder na porta 80

**AWS Secrets necessários:** `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `JWT_KEY`
