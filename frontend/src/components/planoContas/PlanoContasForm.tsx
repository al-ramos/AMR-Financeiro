import { useEffect, useState } from 'react';
import type { PlanoContasDto } from '../../api/planoContasApi';

interface Props {
  contas: PlanoContasDto[];
  editando?: PlanoContasDto | null;
  onSalvar: (dados: {
    codigo: string;
    descricao: string;
    tipo: 'Sintetica' | 'Analitica';
    paiId: number | null;
  }) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function PlanoContasForm({ contas, editando, onSalvar, onCancelar, carregando }: Props) {
  const [codigo, setCodigo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [tipo, setTipo] = useState<'Sintetica' | 'Analitica'>('Analitica');
  const [paiId, setPaiId] = useState<number | null>(null);

  useEffect(() => {
    if (editando) {
      setCodigo(editando.codigo);
      setDescricao(editando.descricao);
      setTipo(editando.tipo);
      setPaiId(editando.paiId);
    } else {
      setCodigo('');
      setDescricao('');
      setTipo('Analitica');
      setPaiId(null);
    }
  }, [editando]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!codigo.trim() || !descricao.trim()) return;
    onSalvar({ codigo: codigo.trim(), descricao: descricao.trim(), tipo, paiId });
  };

  const sinteticas = contas.filter(c => c.tipo === 'Sintetica' && c.ativo);

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Codigo *
        </label>
        <input
          type="text"
          value={codigo}
          onChange={e => setCodigo(e.target.value)}
          disabled={!!editando}
          placeholder="Ex: 1.1.01"
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:text-gray-500"
          required
        />
        {editando && (
          <p className="mt-1 text-xs text-gray-400">O codigo nao pode ser alterado.</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Descricao *
        </label>
        <input
          type="text"
          value={descricao}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Caixa e Equivalentes"
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Tipo</label>
        <select
          value={tipo}
          onChange={e => setTipo(e.target.value as 'Sintetica' | 'Analitica')}
          disabled={!!editando}
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
        >
          <option value="Sintetica">Sintetica (agrupador)</option>
          <option value="Analitica">Analitica (aceita lancamentos)</option>
        </select>
      </div>

      {!editando && (
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Conta Pai</label>
          <select
            value={paiId ?? ''}
            onChange={e => setPaiId(e.target.value ? Number(e.target.value) : null)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">-- Raiz (sem pai) --</option>
            {sinteticas.map(c => (
              <option key={c.id} value={c.id}>
                {c.codigo} -- {c.descricao}
              </option>
            ))}
          </select>
        </div>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <button
          type="button"
          onClick={onCancelar}
          className="px-4 py-2 text-sm rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={carregando}
          className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {carregando ? 'Salvando...' : editando ? 'Atualizar' : 'Criar conta'}
        </button>
      </div>
    </form>
  );
}
