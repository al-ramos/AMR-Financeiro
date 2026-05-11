import { useEffect, useState } from 'react';
import type { PlanoContasDto } from '../../api/planoContasApi';

interface Props {
  contas: PlanoContasDto[];
  editando?: PlanoContasDto | null;
  onSalvar: (dados: { codigo: string; descricao: string; tipo: 'Sintetica' | 'Analitica'; paiId: number | null }) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function PlanoContasForm({ contas, editando, onSalvar, onCancelar, carregando }: Props) {
  const [codigo, setCodigo]   = useState('');
  const [descricao, setDescricao] = useState('');
  const [tipo, setTipo]       = useState<'Sintetica' | 'Analitica'>('Analitica');
  const [paiId, setPaiId]     = useState<number | null>(null);

  useEffect(() => {
    if (editando) {
      setCodigo(editando.codigo); setDescricao(editando.descricao);
      setTipo(editando.tipo); setPaiId(editando.paiId);
    } else {
      setCodigo(''); setDescricao(''); setTipo('Analitica'); setPaiId(null);
    }
  }, [editando]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!codigo.trim() || !descricao.trim()) return;
    onSalvar({ codigo: codigo.trim(), descricao: descricao.trim(), tipo, paiId });
  };

  const sinteticas = contas.filter(c => c.tipo === 'Sintetica' && c.ativo);

  return (
    <form onSubmit={handleSubmit}>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Código *</label>
        <input
          type="text"
          value={codigo}
          onChange={e => setCodigo(e.target.value)}
          disabled={!!editando}
          placeholder="Ex: 1.1.01"
          className="form-control form-control-sm"
          required
        />
        {editando && <div className="form-text">O código não pode ser alterado.</div>}
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Descrição *</label>
        <input
          type="text"
          value={descricao}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Caixa e Equivalentes"
          className="form-control form-control-sm"
          required
        />
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Tipo</label>
        <select
          value={tipo}
          onChange={e => setTipo(e.target.value as 'Sintetica' | 'Analitica')}
          disabled={!!editando}
          className="form-select form-select-sm"
        >
          <option value="Sintetica">Sintética (agrupador)</option>
          <option value="Analitica">Analítica (aceita lançamentos)</option>
        </select>
      </div>

      {!editando && (
        <div className="mb-3">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Conta Pai</label>
          <select
            value={paiId ?? ''}
            onChange={e => setPaiId(e.target.value ? Number(e.target.value) : null)}
            className="form-select form-select-sm"
          >
            <option value="">— Raiz (sem pai) —</option>
            {sinteticas.map(c => (
              <option key={c.id} value={c.id}>{c.codigo} — {c.descricao}</option>
            ))}
          </select>
        </div>
      )}

      <div className="d-flex justify-content-end gap-2 mt-4">
        <button type="button" onClick={onCancelar} className="btn btn-sm btn-outline-secondary">
          Cancelar
        </button>
        <button type="submit" disabled={carregando} className="btn btn-sm btn-primary">
          {carregando ? <><span className="spinner-border spinner-border-sm me-1"></span>Salvando...</> : editando ? 'Atualizar' : 'Criar conta'}
        </button>
      </div>
    </form>
  );
}
