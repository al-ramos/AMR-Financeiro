# =============================================================================
# efs.tf — Elastic File System para persistência do banco SQLite
# =============================================================================
# Containers Fargate são efêmeros: ao reiniciar, perdem tudo que foi escrito
# no disco local. Para persistir o arquivo amr.db do SQLite, montamos um
# volume externo NFS — o EFS — dentro do container em /app/data.
#
# Fluxo:
#   Container API (/app/data/amr.db)
#       ↕  NFS (porta 2049)
#   EFS Mount Target (por AZ)
#       ↕
#   EFS File System  ←→  Access Point (/amr-data)
#
# O Access Point garante que o container sempre acesse o mesmo subdiretório
# e usa UID/GID 1000 (usuário sem privilégios dentro do container .NET).
# =============================================================================

# Sistema de arquivos EFS — equivale a um "HD em nuvem" compartilhável
resource "aws_efs_file_system" "sqlite" {
  creation_token = "${var.app_prefix}-sqlite"

  # Arquivos não acessados há 30 dias são movidos para Infrequent Access
  # (armazenamento mais barato); retornam automaticamente ao acesso.
  lifecycle_policy {
    transition_to_ia = "AFTER_30_DAYS"
  }

  tags = {
    Name = "${var.app_prefix}-sqlite-efs"
  }
}

# Mount targets — um por sub-rede da VPC default (uma sub-rede por AZ).
# O mount target é o endpoint NFS dentro de cada zona de disponibilidade.
# Usar for_each garante que todos os targets sejam criados sem repetir código.
resource "aws_efs_mount_target" "sqlite" {
  for_each = toset(data.aws_subnets.default.ids)

  file_system_id  = aws_efs_file_system.sqlite.id
  subnet_id       = each.value
  security_groups = [aws_security_group.efs.id]
}

# Access Point — define o diretório raiz e as permissões POSIX.
# O container monta este access point; o EFS cria /amr-data automaticamente
# na primeira montagem com os metadados de dono (UID/GID 1000).
resource "aws_efs_access_point" "sqlite" {
  file_system_id = aws_efs_file_system.sqlite.id

  # O container enxerga este subdiretório como sua raiz "/"
  root_directory {
    path = "/amr-data"
    creation_info {
      owner_uid   = 1000
      owner_gid   = 1000
      permissions = "755"
    }
  }

  # Usuário POSIX com que o container acessa os arquivos
  posix_user {
    uid = 1000
    gid = 1000
  }

  tags = {
    Name = "${var.app_prefix}-sqlite-ap"
  }
}
