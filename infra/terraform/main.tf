# =============================================================================
# main.tf — Provider AWS e data sources globais
# =============================================================================
# Este arquivo é o ponto de entrada do Terraform. Define:
#   - A versão mínima do Terraform e do provider AWS
#   - O backend S3 (onde o .tfstate é guardado entre execuções)
#   - Data sources reutilizados pelos demais arquivos do módulo
#
# Antes do primeiro "terraform init", crie o bucket S3 de estado:
#   aws s3api create-bucket \
#     --bucket amr-financeiro-tfstate-474874558993 \
#     --region sa-east-1 \
#     --create-bucket-configuration LocationConstraint=sa-east-1
#   aws s3api put-bucket-versioning \
#     --bucket amr-financeiro-tfstate-474874558993 \
#     --versioning-configuration Status=Enabled
# =============================================================================

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Backend S3 — persiste o estado remotamente para que CI/CD e dev local
  # compartilhem exatamente o mesmo estado da infraestrutura.
  backend "s3" {
    bucket  = "amr-financeiro-tfstate-474874558993"
    key     = "amr-financeiro/terraform.tfstate"
    region  = "sa-east-1"
    encrypt = true
  }
}

provider "aws" {
  region = var.aws_region

  # Tags padrão aplicadas automaticamente a todos os recursos.
  # Facilita rastreamento de custos no AWS Cost Explorer.
  default_tags {
    tags = {
      Project     = var.app_prefix
      Environment = "production"
      ManagedBy   = "terraform"
    }
  }
}

# =============================================================================
# Data sources globais (reutilizados por networking.tf, ecs.tf, efs.tf)
# =============================================================================

# VPC padrão da conta — criada automaticamente pela AWS em cada região.
# Usar a VPC default evita criar sub-redes, route tables e internet gateway.
data "aws_vpc" "default" {
  default = true
}

# Sub-redes da VPC default (uma por zona de disponibilidade).
# ALB exige ao menos 2 sub-redes em AZs distintas; EFS precisa de um
# mount target por AZ para alta disponibilidade.
data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}
