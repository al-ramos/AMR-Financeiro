import { useState } from 'react';
import type { PlanoContasDto } from '../../api/planoContasApi';
import type { CriarLancamentoPayload } from '../../api/lancamentosApi';

interface Props {
  contas: PlanoContasDto[];
  onSalvar: (dados: Omit<CriarLancamentoPayload, 'cdFilial'>) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function LancamentoForm({ contas, onSalvar, onCancelar, carregando }: Props) {
  const today = new Date().toISOString().slice(0, 10);
  const [planoContasId, setPlanoContasId] = useState<number | ''>('');
  const [tipo, setTipo]                   = useState<'Debito' | 'Credito'>('Debito');
  const [valor, setValor]                 = useState('');
  const [dataLancamento, setDataLancamento] = useState(today);
  const [historico, setHistorico]         = useState('');

  const analiticas = contas.filter(c => c.tipo === 'Analitica' && c.ativo);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!planoContasId || !valor || !dataLancamento || !historico.trim()) return;
    const valorNum = parseFloat(valor.replace(',', '.'));
    if (isNaN(valorNum) || valorNum <= 0) return;
    onSalvar({ planoContasId: Number(planoContasId), tipo, valor: valorNum, dataLancamento, historico: historico.trim() });
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Conta *</label>
        <select
          value={planoContasId}
          onChange={e => setPlanoContasId(e.target.value ? Number(e.target.value) : '')}
          className="form-select form-select-sm"
          required
        >
          <option value="">— Selecione uma conta —</option>
          {analiticas.map(c => (
            <option key={c.id} value={c.id}>{c.codigo} — {c.descricao}</option>
          ))}
        </select>
        <div className="form-text">Apenas contas analíticas aceitam lançamentos.</div>
      </div>

      <div className="row g-3 mb-3">
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Tipo *</label>
          <div className="btn-group w-100">
            <button type="button" onClick={() => setTipo('Debito')} className={`btn btn-sm ${tipo === 'Debito' ? 'btn-danger' : 'btn-outline-secondary'}`}>
              <i className="bi bi-dash me-1"></i>Débito
            </button>
            <button type="button" onClick={() => setTipo('Credito')} className={`btn btn-sm ${tipo === 'Credito' ? 'btn-success' : 'btn-outline-secondary'}`}>
              <i className="bi bi-plus me-1"></i>Crédito
            </button>
          </div>
        </div>
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Valor *</label>
          <input type="number" step="0.01" min="0.01" value={valor} onChange={e => setValor(e.target.value)} placeholder="0,00" className="form-control form-control-sm" required />
        </div>
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Data *</label>
        <input type="date" value={dataLancamento} onChange={e => setDataLancamento(e.target.value)} className="form-control form-control-sm" required />
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Histórico *</label>
        <input type="text" value={historico} onChange={e => setHistorico(e.target.value)} placeholder="Ex: Pagamento fornecedor" className="form-control form-control-sm" required maxLength={200} />
      </div>

      <div className="d-flex justify-content-end gap-2 mt-4">
        <button type="button" onClick={onCancelar} className="btn btn-sm btn-outline-secondary">Cancelar</button>
        <button type="submit" disabled={carregando} className="btn btn-sm btn-primary">
          {carregando ? <><span className="spinner-border spinner-border-sm me-1"></span>Salvando...</> : 'Registrar lançamento'}
        </button>
      </div>
    </form>
  );
}
