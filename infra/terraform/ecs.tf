# =============================================================================
# ecs.tf — Cluster ECS, Task Definitions e Services Fargate
# =============================================================================
# O ECS (Elastic Container Service) com Fargate executa containers sem que
# você precise gerenciar servidores EC2. A hierarquia é:
#
#   Cluster  →  Service  →  Task  →  Container(s)
#   └─ agrupamento lógico
#              └─ garante N tasks sempre rodando + integra com ALB
#                        └─ instância em execução (runtime)
#                                   └─ processo dentro da task
#
# Cada serviço tem sua própria task definition (API e Web separados),
# permitindo escalá-los de forma independente no futuro.
# =============================================================================

# ─── CloudWatch Log Groups ───────────────────────────────────────────────────
# Os logs de cada container ficam em grupos separados para facilitar busca.
resource "aws_cloudwatch_log_group" "api" {
  name              = "/ecs/${var.app_prefix}/api"
  retention_in_days = 30
}

resource "aws_cloudwatch_log_group" "web" {
  name              = "/ecs/${var.app_prefix}/web"
  retention_in_days = 30
}

# ─── Cluster ─────────────────────────────────────────────────────────────────
resource "aws_ecs_cluster" "main" {
  name = var.app_prefix

  setting {
    name  = "containerInsights"
    value = "enabled" # métricas detalhadas de CPU/memória no CloudWatch
  }
}

# ─── Task Definition: API ────────────────────────────────────────────────────
# Uma "task definition" é uma receita imutável: imagem, portas, env vars,
# volumes e recursos. Cada nova versão recebe um número de revisão.
resource "aws_ecs_task_definition" "api" {
  family                   = "${var.app_prefix}-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc" # obrigatório no Fargate
  cpu                      = var.ecs_task_cpu
  memory                   = var.ecs_task_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn # agente ECS
  task_role_arn            = aws_iam_role.ecs_task.arn           # aplicação

  # Volume EFS: declarado aqui e referenciado como mountPoint no container
  volume {
    name = "sqlite-data"

    efs_volume_configuration {
      file_system_id     = aws_efs_file_system.sqlite.id
      transit_encryption = "ENABLED" # dados criptografados em trânsito (TLS)

      authorization_config {
        access_point_id = aws_efs_access_point.sqlite.id
        iam             = "ENABLED" # usa o ecsTaskRole para autorizar o mount
      }
    }
  }

  container_definitions = jsonencode([{
    name  = "api"
    image = "${var.aws_account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.app_prefix}/api:${var.api_image_tag}"

    portMappings = [{
      containerPort = 8080
      protocol      = "tcp"
    }]

    environment = [
      # Caminho do arquivo SQLite persistido no EFS
      {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Data Source=/app/data/amr_v2.db"
      },
      # Garante que o ASP.NET Core escuta na porta correta
      {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      },
      {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
    ]

    # Secrets injetados pelo ECS via Secrets Manager — valor nunca aparece em
    # logs nem na task definition. O ASP.NET Core lê Jwt__Key como Jwt:Key.
    secrets = [
      {
        name      = "Jwt__Key"
        valueFrom = aws_secretsmanager_secret.jwt_key.arn
      }
    ]

    # Monta o volume EFS declarado acima no caminho /app/data
    mountPoints = [{
      sourceVolume  = "sqlite-data"
      containerPath = "/app/data"
      readOnly      = false
    }]

    # Logs enviados ao CloudWatch (visíveis no console AWS → CloudWatch → Log groups)
    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group"         = "/ecs/${var.app_prefix}/api"
        "awslogs-region"        = var.aws_region
        "awslogs-stream-prefix" = "api"
      }
    }

    essential = true
  }])
}

# ─── Task Definition: Web ────────────────────────────────────────────────────
resource "aws_ecs_task_definition" "web" {
  family                   = "${var.app_prefix}-web"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = var.ecs_task_cpu
  memory                   = var.ecs_task_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn

  container_definitions = jsonencode([{
    name  = "web"
    image = "${var.aws_account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.app_prefix}/web:${var.web_image_tag}"

    portMappings = [{
      containerPort = 80
      protocol      = "tcp"
    }]

    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group"         = "/ecs/${var.app_prefix}/web"
        "awslogs-region"        = var.aws_region
        "awslogs-stream-prefix" = "web"
      }
    }

    essential = true
  }])
}

# ─── ECS Service: API ────────────────────────────────────────────────────────
# O Service mantém 1 task da API em execução e a registra no ALB.
# Se a task falhar, o Service inicia uma nova automaticamente.
resource "aws_ecs_service" "api" {
  name            = "${var.app_prefix}-api"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  # Aguarda os mount targets do EFS e a regra do ALB estarem prontos
  depends_on = [
    aws_efs_mount_target.sqlite,
    aws_lb_listener_rule.api,
  ]

  network_configuration {
    subnets          = data.aws_subnets.default.ids
    security_groups  = [aws_security_group.ecs.id]
    # assign_public_ip = true é necessário para que as tasks Fargate consigam
    # fazer pull de imagens do ECR sem um NAT Gateway (economia de custo).
    assign_public_ip = true
  }

  # Conecta o serviço ao target group do ALB para receber tráfego
  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "api"
    container_port   = 8080
  }
}

# ─── ECS Service: Web ────────────────────────────────────────────────────────
resource "aws_ecs_service" "web" {
  name            = "${var.app_prefix}-web"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.web.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  depends_on = [aws_lb_listener.http]

  network_configuration {
    subnets          = data.aws_subnets.default.ids
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.web.arn
    container_name   = "web"
    container_port   = 80
  }
}
