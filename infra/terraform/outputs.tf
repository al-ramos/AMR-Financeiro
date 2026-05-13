# =============================================================================
# outputs.tf — Valores exportados após o terraform apply
# =============================================================================
# Outputs aparecem no terminal ao final do apply e podem ser lidos
# por outros módulos ou pelo CI/CD com:
#   terraform output -raw <nome>
# =============================================================================

output "alb_url" {
  description = "URL pública do ALB — acesse no browser para ver a aplicação"
  value       = "http://${aws_lb.main.dns_name}"
}

output "ecr_api_url" {
  description = "URL do repositório ECR da API (use no docker push)"
  value       = aws_ecr_repository.api.repository_url
}

output "ecr_web_url" {
  description = "URL do repositório ECR do frontend (use no docker push)"
  value       = aws_ecr_repository.web.repository_url
}

output "ecs_cluster_name" {
  description = "Nome do cluster ECS (usado no aws ecs update-service)"
  value       = aws_ecs_cluster.main.name
}

output "ecs_api_service_name" {
  description = "Nome do ECS Service da API"
  value       = aws_ecs_service.api.name
}

output "ecs_web_service_name" {
  description = "Nome do ECS Service do frontend"
  value       = aws_ecs_service.web.name
}

output "efs_id" {
  description = "ID do EFS que persiste o banco SQLite"
  value       = aws_efs_file_system.sqlite.id
}
