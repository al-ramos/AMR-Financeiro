# =============================================================================
# secrets.tf — AWS Secrets Manager
# =============================================================================
# Armazena segredos da aplicação fora do código-fonte.
# O ECS injeta os valores como variáveis de ambiente nos containers,
# sem que o valor apareça em logs, task definitions ou repositórios git.
#
# Fluxo:
#   GitHub Secret (JWT_KEY) → terraform -var → Secrets Manager → ECS env var
#   → ASP.NET Core lê Jwt__Key como Jwt:Key (__ = separador de seção)
# =============================================================================

resource "aws_secretsmanager_secret" "jwt_key" {
  name        = "${var.app_prefix}/jwt-key"
  description = "JWT signing key para a API AMR Financeiro"

  # Aguarda 7 dias antes de deletar permanentemente (proteção contra exclusão acidental)
  recovery_window_in_days = 7
}

resource "aws_secretsmanager_secret_version" "jwt_key" {
  secret_id     = aws_secretsmanager_secret.jwt_key.id
  secret_string = var.jwt_key
}
