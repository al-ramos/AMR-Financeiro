# =============================================================================
# ecr.tf — Elastic Container Registry (repositórios de imagem Docker)
# =============================================================================
# O ECR é o "Docker Hub privado" da AWS. Cada repositório armazena as
# versões (tags) de uma imagem. O CI/CD faz push aqui; o ECS faz pull
# na hora de iniciar os containers.
#
# URLs geradas (usadas no docker push e no ECS):
#   426222909134.dkr.ecr.sa-east-1.amazonaws.com/amr-financeiro/api
#   426222909134.dkr.ecr.sa-east-1.amazonaws.com/amr-financeiro/web
# =============================================================================

# Repositório para a imagem da API (.NET 8)
resource "aws_ecr_repository" "api" {
  name                 = "${var.app_prefix}/api"
  image_tag_mutability = "MUTABLE" # permite sobrescrever a tag "latest"

  # Escaneia vulnerabilidades conhecidas a cada push (sem custo adicional)
  image_scanning_configuration {
    scan_on_push = true
  }
}

# Repositório para a imagem do frontend (nginx + React)
resource "aws_ecr_repository" "web" {
  name                 = "${var.app_prefix}/web"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }
}

# Política de ciclo de vida: mantém apenas as 10 imagens mais recentes
# para evitar acúmulo e cobranças desnecessárias de armazenamento.
resource "aws_ecr_lifecycle_policy" "api" {
  repository = aws_ecr_repository.api.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Manter as últimas 10 imagens"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 10
      }
      action = { type = "expire" }
    }]
  })
}

resource "aws_ecr_lifecycle_policy" "web" {
  repository = aws_ecr_repository.web.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Manter as últimas 10 imagens"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 10
      }
      action = { type = "expire" }
    }]
  })
}
