# =============================================================================
# variables.tf — Variáveis reutilizáveis em todo o módulo
# =============================================================================
# Centralizar valores aqui facilita manutenção: para trocar região, conta
# ou prefixo basta editar este único arquivo (ou passar -var no CLI).
#
# Uso no CLI:
#   terraform apply -var="api_image_tag=abc12345"
# =============================================================================

variable "aws_region" {
  description = "Região AWS onde todos os recursos serão provisionados"
  type        = string
  default     = "sa-east-1" # São Paulo
}

variable "aws_account_id" {
  description = "ID numérico da conta AWS (montado nas URLs do ECR)"
  type        = string
  default     = "474874558993"
}

variable "app_prefix" {
  description = "Prefixo aplicado ao nome de todos os recursos (SGs, ECS, ECR, ALB...)"
  type        = string
  default     = "amr-financeiro"
}

variable "api_image_tag" {
  description = "Tag da imagem Docker da API; sobrescrita pelo CI/CD via -var"
  type        = string
  default     = "latest"
}

variable "web_image_tag" {
  description = "Tag da imagem Docker do frontend; sobrescrita pelo CI/CD via -var"
  type        = string
  default     = "latest"
}

variable "jwt_key" {
  description = "JWT signing key injetada via Secrets Manager no container da API. Nunca hardcode — passe via -var ou GitHub Secrets."
  type        = string
  sensitive   = true
}

variable "ecs_task_cpu" {
  description = "CPU alocada por task Fargate em unidades (256 = 0,25 vCPU)"
  type        = number
  default     = 256
}

variable "ecs_task_memory" {
  description = "Memória alocada por task Fargate em MB"
  type        = number
  default     = 1024   # mínimo seguro para .NET 8 (segfault com 512 MB)
}
