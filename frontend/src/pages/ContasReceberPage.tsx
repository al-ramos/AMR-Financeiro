import { useState } from 'react';
import { useContasReceber, useCriarContaReceber, useReceberConta, useCancelarContaReceber } from '../hooks/useContasReceber';
import type { ContaReceberDto } from '../api/contasReceberApi';
import { ContaReceberForm } from '../components/contasReceber/ContaReceberForm';
import { Modal } from '../components/ui/Modal';
import { AlertaErro } from '../components/ui/AlertaErro';

const CD_FILIAL = 1;

function fmt(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

function brl(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function StatusBadge({ status }: { status: ContaReceberDto['status'] }) {
  const cls: Record<string, string> = {
    Aberta: 'badge-aberta', Recebida: 'badge-paga',
    Vencida: 'badge-vencida', Cancelada: 'badge-cancelada',
  };
  const icon: Record<string, string> = {
    Aberta: 'bi-hourglass-split', Recebida: 'bi-check-circle',
    Vencida: 'bi-exclamation-triangle', Cancelada: 'bi-slash-circle',
  };
  return (
    <span className={`badge rounded-pill fw-semibold ${cls[status] ?? ''}`} style={{ fontSize: 11, padding: '4px 10px' }}>
      <i className={`bi ${icon[status]} me-1`}></i>{status}
    </span>
  );
}

export function ContasReceberPage() {
  const [modalAberto, setModalAberto]     = useState(false);
  const [erroForm, setErroForm]           = useState<string | null>(null);
  const [recebendoId, setRecebendoId]     = useState<number | null>(null);
  const [dataRecebimento, setDataRecebimento] = useState(new Date().toISOString().slice(0, 10));

  const { data: contas = [], isLoading, isError, error } = useContasReceber(CD_FILIAL);
  const criar   = useCriarContaReceber();
  const receber  = useReceberConta();
  const cancelar = useCancelarContaReceber();

  const totalAberto   = contas.filter(c => c.status === 'Aberta').reduce((s, c) => s + c.valor, 0);
  const totalVencido  = contas.filter(c => c.status === 'Vencida').reduce((s, c) => s + c.valor, 0);
  const totalRecebido = contas.filter(c => c.status === 'Recebida').reduce((s, c) => s + (c.valorRecebido ?? c.valor), 0);

  const contasOrdenadas = [...contas].sort((a, b) => {
    const ord: Record<string, number> = { Vencida: 0, Aberta: 1, Recebida: 2, Cancelada: 3 };
    if (ord[a.status] !== ord[b.status]) return ord[a.status] - ord[b.status];
    return a.vencimento.localeCompare(b.vencimento);
  });

  const handleSalvar = async (dados: { descricao: string; valor: number; vencimento: string; documentoOrigem?: string }) => {
    setErroForm(null);
    try {
      await criar.mutateAsync({ ...dados, cdFilial: CD_FILIAL });
      setModalAberto(false);
    } catch {
      setErroForm('Erro ao registrar a conta. Verifique os dados e tente novamente.');
    }
  };

  const handleReceber = async (id: number) => {
    try { await receber.mutateAsync({ id, dataRecebimento }); setRecebendoId(null); }
    catch {/* toast */}
  };

  const handleCancelar = async (id: number) => {
    if (!confirm('Cancelar esta conta a receber?')) return;
    try { await cancelar.mutateAsync(id); } catch {/* toast */}
  };

  return (
    <>
      <div className="row g-3 mb-4">
        {[
          { label: 'A receber',  value: brl(totalAberto),   color: '#1565c0' },
          { label: 'Vencidas',   value: brl(totalVencido),  color: '#c62828' },
          { label: 'Recebidas',  value: brl(totalRecebido), color: '#2e7d32' },
        ].map(m => (
          <div key={m.label} className="col-md-4">
            <div className="amr-metric-card">
              <p className="amr-metric-label">{m.label}</p>
              <p className="amr-metric-value" style={{ color: m.color }}>{m.value}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="amr-table-card">
        <div className="d-flex align-items-center justify-content-between px-3 py-3 border-bottom">
          <span style={{ fontSize: 13, fontWeight: 600, color: '#495057' }}>
            <i className="bi bi-list-ul me-2"></i>Contas · Filial {CD_FILIAL}
          </span>
          <button
            className="btn btn-sm btn-primary"
            onClick={() => { setErroForm(null); setModalAberto(true); }}
          >
            <i className="bi bi-plus-lg me-1"></i>Nova Conta
          </button>
        </div>

        {isLoading && (
          <div className="amr-empty">
            <div className="spinner-border spinner-border-sm text-primary mb-2" role="status"></div>
            <span style={{ fontSize: 13 }}>Carregando...</span>
          </div>
        )}

        {isError && (
          <div className="p-3">
            <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar contas a receber.'} />
          </div>
        )}

        {!isLoading && !isError && contasOrdenadas.length === 0 && (
          <div className="amr-empty">
            <i className="bi bi-inbox"></i>
            <div style={{ fontSize: 14, fontWeight: 500 }}>Nenhuma conta registrada</div>
          </div>
        )}

        {!isLoading && contasOrdenadas.length > 0 && (
          <div className="table-responsive">
            <table className="table table-hover table-sm mb-0" style={{ fontSize: 13 }}>
              <thead className="table-light">
                <tr>
                  <th>Descrição</th>
                  <th>Doc. Origem</th>
                  <th>Vencimento</th>
                  <th>Recebimento</th>
                  <th>Status</th>
                  <th className="text-end">Valor</th>
                  <th className="text-end">Ações</th>
                </tr>
              </thead>
              <tbody>
                {contasOrdenadas.map(c => (
                  <tr key={c.id}>
                    <td>{c.descricao}</td>
                    <td className="text-muted" style={{ fontSize: 12 }}>{c.documentoOrigem ?? '—'}</td>
                    <td className="text-nowrap">{fmt(c.vencimento)}</td>
                    <td className="text-nowrap text-muted">{c.dataRecebimento ? fmt(c.dataRecebimento) : '—'}</td>
                    <td><StatusBadge status={c.status} /></td>
                    <td className="text-end fw-semibold">{brl(c.valorRecebido ?? c.valor)}</td>
                    <td className="text-end text-nowrap">
                      {(c.status === 'Aberta' || c.status === 'Vencida') && (
                        recebendoId === c.id ? (
                          <div className="d-flex align-items-center justify-content-end gap-1">
                            <input
                              type="date"
                              value={dataRecebimento}
                              onChange={e => setDataRecebimento(e.target.value)}
                              className="form-control form-control-sm"
                              style={{ width: 130 }}
                            />
                            <button onClick={() => handleReceber(c.id)} className="btn btn-sm btn-success">✓</button>
                            <button onClick={() => setRecebendoId(null)} className="btn btn-sm btn-outline-secondary">✕</button>
                          </div>
                        ) : (
                          <>
                            <button onClick={() => setRecebendoId(c.id)} className="btn btn-sm btn-outline-success me-1">Receber</button>
                            <button onClick={() => handleCancelar(c.id)} className="btn btn-sm btn-outline-danger">Cancelar</button>
                          </>
                        )
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal titulo="Nova Conta a Receber" aberto={modalAberto} onFechar={() => setModalAberto(false)}>
        {erroForm && <div className="mb-3"><AlertaErro mensagem={erroForm} /></div>}
        <ContaReceberForm
          onSalvar={handleSalvar}
          onCancelar={() => setModalAberto(false)}
          carregando={criar.isPending}
        />
      </Modal>
    </>
  );
}
