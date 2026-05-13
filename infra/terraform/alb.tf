# =============================================================================
# alb.tf — Application Load Balancer, Target Groups e Listener
# =============================================================================
# O ALB é o ponto único de entrada da aplicação na internet.
# Recebe requisições HTTP e as encaminha ao container correto baseado
# no caminho da URL (path-based routing):
#
#   GET /api/*   →  Target Group API  →  container ASP.NET Core (porta 8080)
#   GET /*       →  Target Group Web  →  container nginx/React  (porta 80)
#
# Com esse roteamento no ALB, o frontend React chama /api/* diretamente
# e o ALB redireciona para a API — sem necessidade de proxy nginx em produção.
# =============================================================================

# ─── Application Load Balancer ───────────────────────────────────────────────
resource "aws_lb" "main" {
  name               = var.app_prefix
  internal           = false               # exposto à internet (IP público)
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = data.aws_subnets.default.ids # 1 sub-rede por AZ (mín. 2)

  # Desabilitado para facilitar destruição no terraform destroy
  enable_deletion_protection = false
}

# ─── Target Groups ───────────────────────────────────────────────────────────
# Target group = lista de endpoints (IPs de containers) que recebem tráfego.
# O ALB usa health checks para remover containers não-saudáveis da lista.

# Target group da API (.NET, porta 8080)
resource "aws_lb_target_group" "api" {
  name        = "${var.app_prefix}-api"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = data.aws_vpc.default.id
  target_type = "ip" # obrigatório no Fargate: o ALB registra o IP da task, não instância

  health_check {
    path                = "/health"  # endpoint padrão do ASP.NET Core health checks
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    matcher             = "200-404"  # aceita 404 caso o /health ainda não exista
  }
}

# Target group do frontend (nginx, porta 80)
resource "aws_lb_target_group" "web" {
  name        = "${var.app_prefix}-web"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = data.aws_vpc.default.id
  target_type = "ip"

  health_check {
    path                = "/"
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    matcher             = "200"
  }
}

# ─── Listener HTTP ───────────────────────────────────────────────────────────
# O listener "escuta" na porta 80 do ALB e aplica as regras de roteamento.

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.main.arn
  port              = 80
  protocol          = "HTTP"

  # Ação padrão: encaminha para o frontend React (serve o SPA)
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web.arn
  }
}

# Regra de roteamento: /api/* vai para o target group da API.
# Prioridade 10 (quanto menor, maior a prioridade; avaliada antes do default).
resource "aws_lb_listener_rule" "api" {
  listener_arn = aws_lb_listener.http.arn
  priority     = 10

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }
}
