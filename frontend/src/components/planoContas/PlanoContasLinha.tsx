import { useState } from 'react';
import type { PlanoContasArvoreDto } from '../../api/planoContasApi';

interface Props {
  node: PlanoContasArvoreDto;
  onEditar: (id: number) => void;
  onToggleAtivo: (id: number, ativo: boolean) => void;
}

export function PlanoContasLinha({ node, onEditar, onToggleAtivo }: Props) {
  const [aberto, setAberto] = useState(node.nivel <= 1);
  const temFilhos = node.filhos.length > 0;

  return (
    <>
      <tr style={{ opacity: node.ativo ? 1 : 0.5 }}>
        <td className="text-nowrap">
          <div className="d-flex align-items-center gap-1" style={{ paddingLeft: node.nivel * 20 }}>
            {temFilhos ? (
              <button
                onClick={() => setAberto(a => !a)}
                className="btn btn-sm p-0"
                style={{ fontSize: 11, lineHeight: 1, color: '#6c757d', minWidth: 16 }}
              >
                <i className={`bi ${aberto ? 'bi-chevron-down' : 'bi-chevron-right'}`}></i>
              </button>
            ) : (
              <span style={{ minWidth: 16, display: 'inline-block' }}></span>
            )}
            <span className="font-monospace text-muted" style={{ fontSize: 12 }}>{node.codigo}</span>
          </div>
        </td>
        <td className={node.tipo === 'Sintetica' ? 'fw-semibold' : ''}>{node.descricao}</td>
        <td>
          {node.tipo === 'Sintetica'
            ? <span className="badge rounded-pill" style={{ background: '#ede9fe', color: '#5b21b6', fontSize: 11, padding: '3px 9px' }}>Sintética</span>
            : <span className="badge rounded-pill" style={{ background: '#d1fae5', color: '#065f46', fontSize: 11, padding: '3px 9px' }}>Analítica</span>
          }
        </td>
        <td>
          {node.ativo
            ? <span className="badge badge-paga rounded-pill" style={{ fontSize: 11, padding: '3px 9px' }}>Ativo</span>
            : <span className="badge badge-cancelada rounded-pill" style={{ fontSize: 11, padding: '3px 9px' }}>Inativo</span>
          }
        </td>
        <td className="text-end text-nowrap">
          <button onClick={() => onEditar(node.id)} className="btn btn-sm btn-outline-secondary me-1" style={{ fontSize: 11 }}>
            <i className="bi bi-pencil me-1"></i>Editar
          </button>
          <button
            onClick={() => onToggleAtivo(node.id, node.ativo)}
            className={`btn btn-sm ${node.ativo ? 'btn-outline-danger' : 'btn-outline-success'}`}
            style={{ fontSize: 11 }}
          >
            {node.ativo ? 'Inativar' : 'Ativar'}
          </button>
        </td>
      </tr>

      {aberto && node.filhos.map(filho => (
        <PlanoContasLinha key={filho.id} node={filho} onEditar={onEditar} onToggleAtivo={onToggleAtivo} />
      ))}
    </>
  );
}
