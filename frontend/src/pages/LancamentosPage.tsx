import { useState } from 'react';
import { useLancamentosPorPeriodo, useCriarLancamento } from '../hooks/useLancamentos';
import { usePlanoContasLista } from '../hooks/usePlanoContas';
import type { LancamentoFinanceiroDto } from '../api/lancamentosApi';
import { LancamentoForm } from '../components/lancamentos/LancamentoForm';
import { Modal } from '../components/ui/Modal';
import { AlertaErro } from '../components/ui/AlertaErro';

const CD_FILIAL = 1;

function getMesAtual() {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  return { inicio: `${y}-${m}-01`, fim: `${y}-${m}-${new Date(y, now.getMonth() + 1, 0).getDate()}` };
}

function fmt(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

function brl(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function TipoBadge({ tipo }: { tipo: LancamentoFinanceiroDto['tipo'] }) {
  return tipo === 'Credito'
    ? <span className="badge badge-paga rounded-pill" style={{ fontSize: 11, padding: '4px 10px' }}><i className="bi bi-plus me-1"></i>Crédito</span>
    : <span className="badge badge-vencida rounded-pill" style={{ fontSize: 11, padding: '4px 10px' }}><i className="bi bi-dash me-1"></i>Débito</span>;
}

function OrigemBadge({ origem }: { origem: LancamentoFinanceiroDto['origem'] }) {
  const cls: Record<string, string> = {
    Manual: 'badge-cancelada',
    ContaPagar: 'badge-pendente',
    ContaReceber: 'badge-aberta',
  };
  const label: Record<string, string> = {
    Manual: 'Manual', ContaPagar: 'Ct. Pagar', ContaReceber: 'Ct. Receber',
  };
  return (
    <span className={`badge rounded-pill ${cls[origem] ?? 'badge-cancelada'}`} style={{ fontSize: 11, padding: '4px 10px' }}>
      {label[origem] ?? origem}
    </span>
  );
}

export function LancamentosPage() {
  const mesAtual = getMesAtual();
  const [inicio, setInicio] = useState(mesAtual.inicio);
  const [fim, setFim]       = useState(mesAtual.fim);
  const [modalAberto, setModalAberto] = useState(false);
  const [erroForm, setErroForm]       = useState<string | null>(null);

  const { data: lancamentos = [], isLoading, isError, error } = useLancamentosPorPeriodo(CD_FILIAL, inicio, fim);
  const { data: contas = [] } = usePlanoContasLista();
  const criar = useCriarLancamento();

  const totalCredito = lancamentos.filter(l => l.tipo === 'Credito').reduce((s, l) => s + l.valor, 0);
  const totalDebito  = lancamentos.filter(l => l.tipo === 'Debito').reduce((s, l) => s + l.valor, 0);
  const saldo = totalCredito - totalDebito;

  const ordenados = [...lancamentos].sort(
    (a, b) => new Date(b.dataLancamento).getTime() - new Date(a.dataLancamento).getTime()
  );

  const handleSalvar = async (dados: {
    planoContasId: number; tipo: 'Debito' | 'Credito';
    valor: number; dataLancamento: string; historico: string;
  }) => {
    setErroForm(null);
    try {
      await criar.mutateAsync({ ...dados, cdFilial: CD_FILIAL });
      setModalAberto(false);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { erro?: string } } })?.response?.data?.erro
        ?? 'Erro ao registrar o lançamento.';
      setErroForm(msg);
    }
  };

  return (
    <>
      {/* Filtro de período */}
      <div className="amr-metric-card d-flex flex-wrap align-items-end gap-3 mb-4">
        <div>
          <label className="form-label form-label-sm mb-1" style={{ fontSize: 11, color: '#6c757d' }}>De</label>
          <input type="date" value={inicio} onChange={e => setInicio(e.target.value)} className="form-control form-control-sm" />
        </div>
        <div>
          <label className="form-label form-label-sm mb-1" style={{ fontSize: 11, color: '#6c757d' }}>Até</label>
          <input type="date" value={fim} onChange={e => setFim(e.target.value)} className="form-control form-control-sm" />
        </div>
        <span style={{ fontSize: 12, color: '#adb5bd' }}>
          {lancamentos.length} lançamento{lancamentos.length !== 1 ? 's' : ''} no período
        </span>
      </div>

      {/* Cards resumo */}
      <div className="row g-3 mb-4">
        {[
          { label: 'Total créditos', value: brl(totalCredito), color: '#2e7d32' },
          { label: 'Total débitos',  value: brl(totalDebito),  color: '#c62828' },
          { label: 'Saldo do período', value: brl(saldo),      color: saldo >= 0 ? '#2e7d32' : '#c62828' },
        ].map(m => (
          <div key={m.label} className="col-md-4">
            <div className="amr-metric-card">
              <p className="amr-metric-label">{m.label}</p>
              <p className="amr-metric-value" style={{ color: m.color }}>{m.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Tabela */}
      <div className="amr-table-card">
        <div className="d-flex align-items-center justify-content-between px-3 py-3 border-bottom">
          <span style={{ fontSize: 13, fontWeight: 600, color: '#495057' }}>
            <i className="bi bi-journal-text me-2"></i>Movimentos · Filial {CD_FILIAL}
          </span>
          <button className="btn btn-sm btn-primary" onClick={() => { setErroForm(null); setModalAberto(true); }}>
            <i className="bi bi-plus-lg me-1"></i>Novo Lançamento
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
            <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar lançamentos.'} />
          </div>
        )}

        {!isLoading && !isError && ordenados.length === 0 && (
          <div className="amr-empty">
            <i className="bi bi-inbox"></i>
            <div style={{ fontSize: 14, fontWeight: 500 }}>Nenhum lançamento no período</div>
          </div>
        )}

        {!isLoading && ordenados.length > 0 && (
          <div className="table-responsive">
            <table className="table table-hover table-sm mb-0" style={{ fontSize: 13 }}>
              <thead className="table-light">
                <tr>
                  <th>Data</th>
                  <th>Conta</th>
                  <th>Histórico</th>
                  <th>Tipo</th>
                  <th>Origem</th>
                  <th className="text-end">Valor</th>
                </tr>
              </thead>
              <tbody>
                {ordenados.map(l => (
                  <tr key={l.id}>
                    <td className="text-nowrap">{fmt(l.dataLancamento)}</td>
                    <td>
                      <span className="font-monospace text-muted" style={{ fontSize: 11 }}>{l.planoContasCodigo}</span>
                      <span className="ms-1" style={{ fontSize: 12 }}>{l.planoContasDescricao}</span>
                    </td>
                    <td>{l.historico}</td>
                    <td><TipoBadge tipo={l.tipo} /></td>
                    <td><OrigemBadge origem={l.origem} /></td>
                    <td className={`text-end fw-semibold ${l.tipo === 'Credito' ? 'text-success' : 'text-danger'}`}>
                      {l.tipo === 'Credito' ? '+' : '−'} {brl(l.valor)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal titulo="Novo Lançamento" aberto={modalAberto} onFechar={() => { setModalAberto(false); setErroForm(null); }}>
        {erroForm && <div className="mb-3"><AlertaErro mensagem={erroForm} /></div>}
        <LancamentoForm
          contas={contas}
          onSalvar={handleSalvar}
          onCancelar={() => { setModalAberto(false); setErroForm(null); }}
          carregando={criar.isPending}
        />
      </Modal>
    </>
  );
}
