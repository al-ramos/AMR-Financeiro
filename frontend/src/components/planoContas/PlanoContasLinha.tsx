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

  const indentPx = node.nivel * 20;

  return (
    <>
      <tr className={`group border-b border-gray-100 hover:bg-blue-50/40 transition-colors ${!node.ativo ? 'opacity-50' : ''}`}>
        {/* Código */}
        <td className="px-4 py-2 whitespace-nowrap">
          <div className="flex items-center gap-1" style={{ paddingLeft: indentPx }}>
            {temFilhos ? (
              <button
                onClick={() => setAberto(a => !a)}
                className="w-5 h-5 flex items-center justify-center text-gray-400 hover:text-blue-600 transition-colors shrink-0"
                aria-label={aberto ? 'Recolher' : 'Expandir'}
              >
                {aberto ? '▾' : '▸'}
              </button>
            ) : (
              <span className="w-5 shrink-0" />
            )}
            <span className="font-mono text-sm text-gray-600">{node.codigo}</span>
          </div>
        </td>

        {/* Descrição */}
        <td className="px-4 py-2 text-sm text-gray-800">
          <span className={node.tipo === 'Sintetica' ? 'font-semibold' : ''}>
            {node.descricao}
          </span>
        </td>

        {/* Tipo */}
        <td className="px-4 py-2 text-sm">
          {node.tipo === 'Sintetica' ? (
            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
              Sintética
            </span>
          ) : (
            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-700">
              Analítica
            </span>
          )}
        </td>

        {/* Status */}
        <td className="px-4 py-2 text-sm">
          {node.ativo ? (
            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-700">
              Ativo
            </span>
          ) : (
            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-500">
              Inativo
            </span>
          )}
        </td>

        {/* Ações */}
        <td className="px-4 py-2 text-right whitespace-nowrap">
          <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
            <button
              onClick={() => onEditar(node.id)}
              className="px-2 py-1 text-xs rounded-md border border-gray-200 hover:border-blue-300 hover:bg-blue-50 hover:text-blue-700 transition-colors"
            >
              Editar
            </button>
            <button
              onClick={() => onToggleAtivo(node.id, node.ativo)}
              className={`px-2 py-1 text-xs rounded-md border transition-colors ${
                node.ativo
                  ? 'border-gray-200 hover:border-red-300 hover:bg-red-50 hover:text-red-600'
                  : 'border-gray-200 hover:border-emerald-300 hover:bg-emerald-50 hover:text-emerald-700'
              }`}
            >
              {node.ativo ? 'Inativar' : 'Ativar'}
            </button>
          </div>
        </td>
      </tr>

      {/* Filhos (recursivo) */}
      {aberto &&
        node.filhos.map(filho => (
          <PlanoContasLinha
            key={filho.id}
            node={filho}
            onEditar={onEditar}
            onToggleAtivo={onToggleAtivo}
          />
        ))}
    </>
  );
}
