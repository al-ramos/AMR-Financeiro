# Infraestrutura AWS — AMR.Financeiro

Terraform que provisiona toda a infraestrutura de produção na AWS (região `sa-east-1`).

## Arquitetura

```
Internet
    │
    ▼
ALB  amr-financeiro  (porta 80)
    ├── /api/*  ──►  ECS Service API   ──►  Fargate (ASP.NET 8, porta 8080)
    │                                            │
    │                                       EFS /app/data/amr.db
    │
    └── /*  ────►  ECS Service Web  ──►  Fargate (nginx + React, porta 80)

ECR:  474874558993.dkr.ecr.sa-east-1.amazonaws.com/amr-financeiro/{api,web}
```

## Pré-requisitos

- [Terraform >= 1.5](https://developer.hashicorp.com/terraform/install)
- [AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html) configurado (`aws configure`)
- Permissões IAM: AdministratorAccess (ou política customizada com ECS, ECR, EFS, ALB, IAM, S3)

## Primeiro uso (bootstrap)

O estado do Terraform é guardado num bucket S3. Crie-o **uma única vez** antes de qualquer `terraform init`:

```bash
# 1. Criar o bucket de estado
aws s3api create-bucket \
  --bucket amr-financeiro-tfstate-474874558993 \
  --region sa-east-1 \
  --create-bucket-configuration LocationConstraint=sa-east-1

# 2. Habilitar versionamento (permite rollback do estado)
aws s3api put-bucket-versioning \
  --bucket amr-financeiro-tfstate-474874558993 \
  --versioning-configuration Status=Enabled
```

## Comandos Terraform

```bash
cd infra/terraform

# Baixa providers e inicializa o backend S3
terraform init

# Mostra o que será criado/alterado/destruído (não aplica nada)
terraform plan

# Aplica as mudanças na AWS (~5-10 min no primeiro apply)
terraform apply

# Confirma com 'yes' quando solicitado, ou use -auto-approve para CI/CD:
terraform apply -auto-approve

# Destrói TODA a infraestrutura (cuidado: remove dados do EFS/SQLite)
terraform destroy
```

## Secrets necessários no GitHub

Configure em **Settings → Secrets and variables → Actions**:

| Secret | Descrição |
|--------|-----------|
| `AWS_ACCESS_KEY_ID` | Access key de um usuário IAM com permissões de deploy |
| `AWS_SECRET_ACCESS_KEY` | Secret key correspondente |

Crie o usuário IAM com a política mínima necessária:
```bash
aws iam create-user --user-name amr-financeiro-deploy
aws iam attach-user-policy \
  --user-name amr-financeiro-deploy \
  --policy-arn arn:aws:iam::aws:policy/AdministratorAccess
aws iam create-access-key --user-name amr-financeiro-deploy
```

## Estimativa de custos (sa-east-1)

| Recurso | Custo estimado/mês |
|---------|-------------------|
| ECS Fargate (2 tasks × 0,25 vCPU × 512 MB, 24h) | ~USD 15 |
| ALB | ~USD 20 |
| ECR (armazenamento) | ~USD 1 |
| EFS (< 1 GB) | ~USD 1 |
| CloudWatch Logs | ~USD 1 |
| **Total estimado** | **~USD 38/mês** |

> Para economizar fora do horário comercial, reduza `desired_count = 0` nos serviços ECS e suba para `1` quando precisar.

## Arquivos

| Arquivo | Responsabilidade |
|---------|-----------------|
| `main.tf` | Provider AWS, backend S3, data sources globais |
| `variables.tf` | Variáveis reutilizáveis (região, prefixo, tags de imagem) |
| `ecr.tf` | Repositórios Docker privados |
| `networking.tf` | Security Groups (ALB, ECS, EFS) |
| `iam.tf` | Papéis IAM (TaskExecutionRole e TaskRole) |
| `efs.tf` | Sistema de arquivos EFS + mount targets + access point |
| `alb.tf` | Load Balancer, target groups, listener e regras de roteamento |
| `ecs.tf` | Cluster, task definitions e serviços Fargate |
| `outputs.tf` | URL do ALB, URLs dos ECRs, nomes dos serviços |
