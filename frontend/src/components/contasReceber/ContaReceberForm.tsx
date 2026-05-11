import { useState } from 'react';

interface Props {
  onSalvar: (dados: { descricao: string; valor: number; vencimento: string; documentoOrigem?: string }) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function ContaReceberForm({ onSalvar, onCancelar, carregando }: Props) {
  const [descricao, setDescricao]         = useState('');
  const [valor, setValor]                 = useState('');
  const [vencimento, setVencimento]       = useState('');
  const [documentoOrigem, setDocumentoOrigem] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!descricao.trim() || !valor || !vencimento) return;
    onSalvar({ descricao: descricao.trim(), valor: parseFloat(valor), vencimento, documentoOrigem: documentoOrigem.trim() || undefined });
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Descrição *</label>
        <input type="text" value={descricao} onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Fatura NF-e 001" className="form-control form-control-sm" required />
      </div>
      <div className="row g-3 mb-3">
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Valor (R$) *</label>
          <input type="number" value={valor} onChange={e => setValor(e.target.value)}
            placeholder="0,00" min="0.01" step="0.01" className="form-control form-control-sm" required />
        </div>
        <div className="col-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Vencimento *</label>
          <input type="date" value={vencimento} onChange={e => setVencimento(e.target.value)} className="form-control form-control-sm" required />
        </div>
      </div>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Documento de Origem</label>
        <input type="text" value={documentoOrigem} onChange={e => setDocumentoOrigem(e.target.value)}
          placeholder="NF, pedido, contrato... (opcional)" className="form-control form-control-sm" />
      </div>
      <div className="d-flex justify-content-end gap-2 mt-4">
        <button type="button" onClick={onCancelar} className="btn btn-sm btn-outline-secondary">Cancelar</button>
        <button type="submit" disabled={carregando} className="btn btn-sm btn-primary">
          {carregando ? <><span className="spinner-border spinner-border-sm me-1"></span>Salvando...</> : 'Salvar'}
        </button>
      </div>
    </form>
  );
}
