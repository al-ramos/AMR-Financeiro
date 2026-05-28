#!/usr/bin/env bash
# ============================================================
# start-portfolio.sh — Sobe toda a infraestrutura AMR no AWS
# Uso: ./start-portfolio.sh
# ============================================================
set -e

INFRA_DIR="$(cd "$(dirname "$0")/infra/terraform" && pwd)"

echo ""
echo "🚀 AMR SYSTEM — Subindo infraestrutura no AWS..."
echo "================================================="

cd "$INFRA_DIR"

echo ""
echo "📦 Inicializando Terraform..."
terraform init -reconfigure

echo ""
echo "🔨 Aplicando infraestrutura (pode levar ~5 minutos)..."
terraform apply -auto-approve

echo ""
echo "⏳ Aguardando serviços ficarem estáveis..."
CLUSTER=$(terraform output -raw ecs_cluster_name)
API_SVC=$(terraform output -raw ecs_api_service_name)
WEB_SVC=$(terraform output -raw ecs_web_service_name)
REGION="sa-east-1"

aws ecs wait services-stable \
  --region "$REGION" \
  --cluster "$CLUSTER" \
  --services "$API_SVC" "$WEB_SVC"

ALB_URL=$(terraform output -raw alb_url)

echo ""
echo "================================================="
echo "✅ AMR SYSTEM no ar!"
echo ""
echo "  🌐 Frontend:  $ALB_URL"
echo "  📡 Swagger:   $ALB_URL/api/swagger"
echo ""
echo "  Para desligar: ./stop-portfolio.sh"
echo "================================================="
