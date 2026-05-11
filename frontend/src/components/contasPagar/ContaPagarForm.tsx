import { useState } from 'react';
import type { CriarContaPagarPayload } from '../../api/contasPagarApi';

interface Props {
  onSalvar: (dados: Omit<CriarContaPagarPayload, 'cdFilial'>) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function ContaPagarForm({ onSalvar, onCancelar, carregando }: Props) {
  const [descricao, setDescricao] = useState('');
  const [valor, setValor]         = useState('');
  const [vencimento, setVencimento] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!descricao.trim() || !valor || !vencimento) return;
    const valorNum = parseFloat(valor.replace(',', '.'));
    if (isNaN(valorNum) || valorNum <= 0) return;
    onSalvar({ descricao: descricao.trim(), valor: valorNum, vencimento });
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Descrição *</label>
        <input type="text" value={descricao} onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Aluguel maio/2026" className="form-control form-control-sm" required maxLength={200} />
      </div>
      <div className="row g-3 mb-3">
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Valor *</label>
          <input type="number" step="0.01" min="0.01" value={valor} onChange={e => setValor(e.target.value)}
            placeholder="0,00" className="form-control form-control-sm" required />
        </div>
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Vencimento *</label>
          <input type="date" value={vencimento} onChange={e => setVencimento(e.target.value)} className="form-control form-control-sm" required />
        </div>
      </div>
      <div className="d-flex justify-content-end gap-2 mt-4">
        <button type="button" onClick={onCancelar} className="btn btn-sm btn-outline-secondary">Cancelar</button>
        <button type="submit" disabled={carregando} className="btn btn-sm btn-primary">
          {carregando ? <><span className="spinner-border spinner-border-sm me-1"></span>Salvando...</> : 'Registrar conta'}
        </button>
      </div>
    </form>
  );
}
