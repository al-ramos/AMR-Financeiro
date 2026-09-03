import { useState, useEffect } from 'react';
import { parcelamentosApi } from '../api/parcelamentosApi';
import type { ParcelamentoDto, ParcelaDto, CriarParcelamentoPayload } from '../api/parcelamentosApi';

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const fmtDate = (d: string) =>
  new Date(d).toLocaleDateString('pt-BR');

const STATUS_CONFIG = {
  Pendente: { label: 'Pendente', color: '#f57f17', bg: '#fff9c4' },
  Pago:     { label: 'Pago',     color: '#2e7d32', bg: '#e8f5e9' },
  Vencido:  { label: 'Vencido',  color: '#b71c1c', bg: '#ffebee' },
  Cancelado:{ label: 'Cancelado',color: '#757575', bg: '#f5f5f5' },
};

const EMPTY: CriarParcelamentoPayload = {
  descricao: '',
  valorTotal: 0,
  numeroParcelas: 3,
  tipoVinculo: 'Avulso',
  vinculoId: null,
  primeiroVencimento: new Date().toISOString().slice(0, 10),
};

interface PagarModalState {
  parcelamentoId: number;
  parcela: ParcelaDto;
}

export function ParcelamentosPage() {
  const [lista, setLista] = useState<ParcelamentoDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState<CriarParcelamentoPayload>(EMPTY);
  const [saving, setSaving] = useState(false);
  const [erro, setErro] = useState('');
  const [pagarModal, setPagarModal] = useState<PagarModalState | null>(null);
  const [dataPagamento, setDataPagamento] = useState(new Date().toISOString().slice(0, 10));
  const [expanded, setExpanded] = useState<number | null>(null);

  const load = () => {
    setLoading(true);
    parcelamentosApi.getAll().then(setLista).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const salvar = async () => {
    if (!form.descricao.trim()) { setErro('Descrição é obrigatória.'); return; }
    if (form.valorTotal <= 0) { setErro('Valor total deve ser maior que zero.'); return; }
    setSaving(true);
    try {
      await parcelamentosApi.criar(form);
      setShowModal(false);
      load();
    } catch {
      setErro('Erro ao criar parcelamento.');
    } finally {
      setSaving(false);
    }
  };

  const pagar = async () => {
    if (!pagarModal) return;
    setSaving(true);
    try {
      await parcelamentosApi.pagarParcela(
        pagarModal.parcelamentoId, pagarModal.parcela.id, dataPagamento);
      setPagarModal(null);
      load();
    } catch {
      setErro('Erro ao registrar pagamento.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 20 }}>
        <button className="amr-btn amr-btn-primary" onClick={() => { setForm(EMPTY); setErro(''); setShowModal(true); }}>
          <i className="bi bi-plus-lg" /> Novo Parcelamento
        </button>
      </div>

      {loading ? (
        <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Carregando...</div></div>
      ) : lista.length === 0 ? (
        <div className="amr-empty">
          <i className="bi bi-credit-card-2-front" />
          <div>Nenhum parcelamento cadastrado</div>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {lista.map(p => {
            const isOpen = expanded === p.id;
            const pct = p.numeroParcelas > 0 ? (p.totalPagas / p.numeroParcelas) * 100 : 0;
            return (
              <div key={p.id} className="amr-card">
                <div
                  style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer' }}
                  onClick={() => setExpanded(isOpen ? null : p.id)}
                >
                  <div>
                    <div style={{ fontWeight: 600, fontSize: 15, color: '#212121' }}>{p.descricao}</div>
                    <div style={{ fontSize: 12, color: '#78909c', marginTop: 2 }}>
                      {p.numeroParcelas}x de {fmt(p.valorTotal / p.numeroParcelas)} · Total: {fmt(p.valorTotal)}
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
                    <div style={{ textAlign: 'right' }}>
                      <div style={{ fontSize: 12, color: '#78909c' }}>
                        {p.totalPagas}/{p.numeroParcelas} pagas
                      </div>
                      <div style={{ width: 120, height: 6, background: '#e0e0e0', borderRadius: 3, marginTop: 4 }}>
                        <div style={{
                          width: `${pct}%`, height: '100%',
                          background: pct === 100 ? '#2e7d32' : '#1976d2',
                          borderRadius: 3, transition: 'width 0.4s',
                        }} />
                      </div>
                    </div>
                    <i className={`bi bi-chevron-${isOpen ? 'up' : 'down'}`} style={{ color: '#90a4ae' }} />
                  </div>
                </div>

                {isOpen && (
                  <div style={{ marginTop: 16, borderTop: '1px solid #f0f4f8', paddingTop: 16 }}>
                    <table className="amr-table">
                      <thead>
                        <tr>
                          <th>#</th>
                          <th>Vencimento</th>
                          <th style={{ textAlign: 'right' }}>Valor</th>
                          <th>Status</th>
                          <th>Pagamento</th>
                          <th />
                        </tr>
                      </thead>
                      <tbody>
                        {p.parcelas.map(parc => {
                          const sc = STATUS_CONFIG[parc.status];
                          return (
                            <tr key={parc.id}>
                              <td>{parc.numeroParcela}</td>
                              <td>{fmtDate(parc.dataVencimento)}</td>
                              <td style={{ textAlign: 'right' }}>{fmt(parc.valorParcela)}</td>
                              <td>
                                <span style={{
                                  background: sc.bg, color: sc.color,
                                  fontSize: 11, fontWeight: 600, borderRadius: 4, padding: '2px 8px',
                                }}>
                                  {sc.label}
                                </span>
                              </td>
                              <td style={{ color: '#78909c', fontSize: 13 }}>
                                {parc.dataPagamento ? fmtDate(parc.dataPagamento) : '—'}
                              </td>
                              <td>
                                {(parc.status === 'Pendente' || parc.status === 'Vencido') && (
                                  <button
                                    className="amr-btn amr-btn-sm amr-btn-primary"
                                    onClick={() => {
                                      setDataPagamento(new Date().toISOString().slice(0, 10));
                                      setPagarModal({ parcelamentoId: p.id, parcela: parc });
                                    }}
                                  >
                                    Pagar
                                  </button>
                                )}
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Modal Novo Parcelamento */}
      {showModal && (
        <div className="amr-modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="amr-modal" onClick={e => e.stopPropagation()} style={{ maxWidth: 460 }}>
            <div className="amr-modal-header">
              <span>Novo Parcelamento</span>
              <button className="amr-modal-close" onClick={() => setShowModal(false)}>&times;</button>
            </div>
            <div className="amr-modal-body">
              {erro && <div className="amr-alert amr-alert-danger">{erro}</div>}
              <div className="amr-form-group">
                <label>Descrição *</label>
                <input className="amr-input" value={form.descricao}
                  onChange={e => setForm(f => ({ ...f, descricao: e.target.value }))} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="amr-form-group">
                  <label>Valor Total (R$) *</label>
                  <input className="amr-input" type="number" step="0.01" value={form.valorTotal}
                    onChange={e => setForm(f => ({ ...f, valorTotal: parseFloat(e.target.value) || 0 }))} />
                </div>
                <div className="amr-form-group">
                  <label>Nº Parcelas *</label>
                  <input className="amr-input" type="number" min="1" max="360" value={form.numeroParcelas}
                    onChange={e => setForm(f => ({ ...f, numeroParcelas: parseInt(e.target.value) || 1 }))} />
                </div>
                <div className="amr-form-group">
                  <label>1º Vencimento</label>
                  <input className="amr-input" type="date" value={form.primeiroVencimento}
                    onChange={e => setForm(f => ({ ...f, primeiroVencimento: e.target.value }))} />
                </div>
                <div className="amr-form-group">
                  <label>Tipo Vínculo</label>
                  <select className="amr-input" value={form.tipoVinculo}
                    onChange={e => setForm(f => ({ ...f, tipoVinculo: e.target.value as CriarParcelamentoPayload['tipoVinculo'] }))}>
                    <option value="Avulso">Avulso</option>
                    <option value="Lancamento">Lançamento</option>
                    <option value="Boleto">Boleto</option>
                  </select>
                </div>
              </div>
              {form.numeroParcelas > 0 && form.valorTotal > 0 && (
                <div style={{ background: '#e3f2fd', borderRadius: 6, padding: '10px 14px', fontSize: 13, color: '#1565c0' }}>
                  {form.numeroParcelas}x de {fmt(form.valorTotal / form.numeroParcelas)}
                  {' · A última parcela absorve os centavos restantes.'}
                </div>
              )}
            </div>
            <div className="amr-modal-footer">
              <button className="amr-btn" onClick={() => setShowModal(false)}>Cancelar</button>
              <button className="amr-btn amr-btn-primary" onClick={salvar} disabled={saving}>
                {saving ? 'Gerando...' : 'Criar Parcelamento'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Pagar Parcela */}
      {pagarModal && (
        <div className="amr-modal-backdrop" onClick={() => setPagarModal(null)}>
          <div className="amr-modal" onClick={e => e.stopPropagation()} style={{ maxWidth: 380 }}>
            <div className="amr-modal-header">
              <span>Registrar Pagamento</span>
              <button className="amr-modal-close" onClick={() => setPagarModal(null)}>&times;</button>
            </div>
            <div className="amr-modal-body">
              <p style={{ fontSize: 14, color: '#546e7a', marginBottom: 16 }}>
                Parcela {pagarModal.parcela.numeroParcela} · {fmt(pagarModal.parcela.valorParcela)}
              </p>
              <div className="amr-form-group">
                <label>Data do Pagamento</label>
                <input className="amr-input" type="date" value={dataPagamento}
                  onChange={e => setDataPagamento(e.target.value)} />
              </div>
            </div>
            <div className="amr-modal-footer">
              <button className="amr-btn" onClick={() => setPagarModal(null)}>Cancelar</button>
              <button className="amr-btn amr-btn-primary" onClick={pagar} disabled={saving}>
                {saving ? 'Registrando...' : 'Confirmar Pagamento'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
