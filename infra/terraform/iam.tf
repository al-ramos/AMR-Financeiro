# =============================================================================
# iam.tf — Papéis IAM para o ECS Fargate
# =============================================================================
# O ECS usa dois papéis IAM distintos com propósitos diferentes:
#
#   ecsTaskExecutionRole — assumido pelo AGENTE do ECS (a plataforma):
#     • Baixar imagens do ECR (docker pull)
#     • Escrever logs no CloudWatch
#     • Ler secrets do SSM Parameter Store (se necessário no futuro)
#
#   ecsTaskRole — assumido pelo CÓDIGO dentro do container:
#     • Ler/escrever no EFS (necessário para o SQLite funcionar)
#     • Qualquer outra permissão que a aplicação precisar
#
# Regra de ouro: TaskExecutionRole = "o ECS pode operar";
#                TaskRole = "a aplicação pode fazer X na AWS".
# =============================================================================

# Define quem pode assumir os papéis abaixo (trust policy)
data "aws_iam_policy_document" "ecs_trust" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

# ─── ecsTaskExecutionRole ────────────────────────────────────────────────────

resource "aws_iam_role" "ecs_task_execution" {
  name               = "${var.app_prefix}-ecsTaskExecutionRole"
  assume_role_policy = data.aws_iam_policy_document.ecs_trust.json
}

# Política gerenciada pela AWS — concede permissões mínimas para o agente ECS:
# ECR pull, CloudWatch Logs, SSM GetParameters (para secrets).
resource "aws_iam_role_policy_attachment" "ecs_task_execution" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# ─── ecsTaskRole (papel da aplicação) ────────────────────────────────────────

resource "aws_iam_role" "ecs_task" {
  name               = "${var.app_prefix}-ecsTaskRole"
  assume_role_policy = data.aws_iam_policy_document.ecs_trust.json
}

# Permissões inline para montar e usar o EFS com o arquivo SQLite
data "aws_iam_policy_document" "efs_access" {
  statement {
    sid = "AllowEFSAccess"
    actions = [
      "elasticfilesystem:ClientMount",
      "elasticfilesystem:ClientWrite",
      "elasticfilesystem:ClientRootAccess",
    ]
    resources = [aws_efs_file_system.sqlite.arn]
  }
}

resource "aws_iam_role_policy" "ecs_task_efs" {
  name   = "${var.app_prefix}-efs-access"
  role   = aws_iam_role.ecs_task.id
  policy = data.aws_iam_policy_document.efs_access.json
}

# ─── Permissão: ecsTaskExecutionRole → Secrets Manager ───────────────────────
# O agente ECS (execution role) precisa buscar o secret antes de iniciar
# o container, para injetar o valor como variável de ambiente.

data "aws_iam_policy_document" "secrets_access" {
  statement {
    sid     = "AllowSecretsManagerRead"
    actions = ["secretsmanager:GetSecretValue"]
    resources = [aws_secretsmanager_secret.jwt_key.arn]
  }
}

resource "aws_iam_role_policy" "ecs_task_execution_secrets" {
  name   = "${var.app_prefix}-secrets-access"
  role   = aws_iam_role.ecs_task_execution.id
  policy = data.aws_iam_policy_document.secrets_access.json
}
