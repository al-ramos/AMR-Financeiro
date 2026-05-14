# =============================================================================
# networking.tf — Security Groups
# =============================================================================
# Security Groups funcionam como firewalls virtuais. Cada recurso (ALB,
# containers ECS, EFS) fica num SG próprio, e só liberamos o tráfego
# estritamente necessário entre eles.
#
# Fluxo de tráfego:
#   Internet ──► ALB (80/443) ──► ECS containers (8080 ou 80) ──► EFS (2049)
#
# Princípio do menor privilégio: ECS só aceita do ALB; EFS só aceita do ECS.
# =============================================================================

# ─── ALB Security Group ──────────────────────────────────────────────────────
# O ALB é o único recurso exposto à internet. Recebe HTTP e HTTPS de qualquer IP.
resource "aws_security_group" "alb" {
  name        = "${var.app_prefix}-alb-sg"
  description = "Trafego publico para o Application Load Balancer"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    description = "HTTP de qualquer IP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS (reservado para futuro certificado SSL)"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Todo tráfego de saída liberado (para encaminhar requisições aos containers)
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# ─── ECS Security Group ──────────────────────────────────────────────────────
# Os containers só aceitam tráfego do ALB — não ficam expostos diretamente.
resource "aws_security_group" "ecs" {
  name        = "${var.app_prefix}-ecs-sg"
  description = "Trafego do ALB para os containers ECS Fargate"
  vpc_id      = data.aws_vpc.default.id

  # Container da API (ASP.NET Core) escuta na porta 8080
  ingress {
    description     = "API do ALB"
    from_port       = 8080
    to_port         = 8080
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  # Container do frontend (nginx) escuta na porta 80
  ingress {
    description     = "Web do ALB"
    from_port       = 80
    to_port         = 80
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  # Saída liberada: pull de imagens ECR, escrita no EFS, logs no CloudWatch
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# ─── EFS Security Group ──────────────────────────────────────────────────────
# O EFS (sistema de arquivos) só aceita conexões NFS dos containers ECS.
resource "aws_security_group" "efs" {
  name        = "${var.app_prefix}-efs-sg"
  description = "Acesso NFS ao EFS somente pelos containers ECS"
  vpc_id      = data.aws_vpc.default.id

  # NFS usa porta 2049; aceita exclusivamente do security group dos containers
  ingress {
    description     = "NFS dos containers ECS"
    from_port       = 2049
    to_port         = 2049
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}
