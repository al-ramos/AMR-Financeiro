#!/usr/bin/env bash
# ============================================================
# stop-portfolio.sh — Destroi toda a infraestrutura AMR no AWS
# Custo zero após execução (exceto ECR ~$0.50/mês)
# Uso: ./stop-portfolio.sh
# ============================================================
set -e

INFRA_DIR="$(cd "$(dirname "$0")/infra/terraform" && pwd)"

echo ""
echo "🛑 AMR SYSTEM — Destruindo infraestrutura AWS..."
echo "================================================="
echo "  ⚠️  Isso removerá: ECS, ALB, EFS e todos os dados."
echo "  ✅  Imagens ECR são preservadas para o próximo deploy."
echo ""
read -p "Confirma? (s/N): " CONFIRM
if [[ "$CONFIRM" != "s" && "$CONFIRM" != "S" ]]; then
  echo "Cancelado."
  exit 0
fi

cd "$INFRA_DIR"

echo ""
echo "💣 Destruindo infraestrutura..."
terraform destroy -auto-approve

echo ""
echo "================================================="
echo "✅ Infraestrutura destruída. Custo AWS = ~\$0"
echo ""
echo "  Para subir novamente: ./start-portfolio.sh"
echo "================================================="
