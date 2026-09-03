import { useState, useEffect } from 'react';
import { contasBancariasApi } from '../api/contasBancariasApi';
import type { ContaBancariaDto, CriarContaBancariaPayload, TipoContaBancaria } from '../api/contasBancariasApi';

const TIPO_LABELS: Record<TipoContaBancaria, string> = {
  ContaCorrente: 'Conta Corrente',
  Poupanca: 'Poupança',
  Investimento: 'Investimento',
};

const TIPO_COLORS: Record<TipoContaBancaria, string> = {
  ContaCorrente: '#1976d2',
  Poupanca: '#388e3c',
  Investimento: '#f57c00',
};

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const EMPTY: CriarContaBancariaPayload = {
  nome: '', banco: '', agencia: '', conta: '',
  tipoConta: 'ContaCorrente',
  saldoInicial: 0,
  dataSaldoInicial: new Date().toISOString().slice(0, 10),
};

export function ContasBancariasPage() {
  const [contas, setContas] = useState<ContaBancariaDto[]>([]);
  const [incluirInativas, setIncluirInativas] = useState(false);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState<CriarContaBancariaPayload>(EMPTY);
  const [editId, setEditId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);
  const [erro, setErro] = useState('');

  const load = () => {
    setLoading(true);
    contasBancariasApi.getAll(incluirInativas)
      .then(setContas)
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, [incluirInativas]);

  const openNova = () => { setForm(EMPTY); setEditId(null); setErro(''); setShowModal(true); };
  const openEdit = (c: ContaBancariaDto) => {
    setForm({
      nome: c.nome, banco: c.banco, agencia: c.agencia, conta: c.conta,
      tipoConta: c.tipoConta, saldoInicial: c.saldoInicial,
      dataSaldoInicial: c.dataSaldoInicial.slice(0, 10),
    });
    setEditId(c.id);
    setErro('');
    setShowModal(true);
  };

  const salvar = async () => {
    if (!form.nome.trim()) { setErro('Nome é obrigatório.'); return; }
    setSaving(true);
    try {
      if (editId) {
        await contasBancariasApi.atualizar(editId, form);
      } else {
        await contasBancariasApi.criar(form);
      }
      setShowModal(false);
      load();
    } catch {
      setErro('Erro ao salvar. Tente novamente.');
    } finally {
      setSaving(false);
    }
  };

  const desativar = async (id: number) => {
    if (!confirm('Desativar esta conta bancária?')) return;
    await contasBancariasApi.desativar(id);
    load();
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <label style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, color: '#546e7a' }}>
          <input type="checkbox" checked={incluirInativas} onChange={e => setIncluirInativas(e.target.checked)} />
          Incluir inativas
        </label>
        <button className="amr-btn amr-btn-primary" onClick={openNova}>
          <i className="bi bi-plus-lg" /> Nova Conta
        </button>
      </div>

      {loading ? (
        <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Carregando...</div></div>
      ) : contas.length === 0 ? (
        <div className="amr-empty">
          <i className="bi bi-bank" />
          <div>Nenhuma conta bancária cadastrada</div>
          <button className="amr-btn amr-btn-primary" style={{ marginTop: 12 }} onClick={openNova}>
            Cadastrar primeira conta
          </button>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
          {contas.map(c => (
            <div key={c.id} className="amr-card" style={{ opacity: c.ativa ? 1 : 0.6 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 12 }}>
                <span style={{
                  background: TIPO_COLORS[c.tipoConta] + '22',
                  color: TIPO_COLORS[c.tipoConta],
                  fontSize: 11, fontWeight: 600, borderRadius: 4, padding: '2px 8px',
                }}>
                  {TIPO_LABELS[c.tipoConta]}
                </span>
                {!c.ativa && (
                  <span style={{ fontSize: 11, color: '#9e9e9e', fontWeight: 500 }}>Inativa</span>
                )}
              </div>
              <div style={{ fontWeight: 700, fontSize: 16, color: '#212121', marginBottom: 4 }}>{c.nome}</div>
              <div style={{ fontSize: 12, color: '#78909c', marginBottom: 12 }}>
                {c.banco} {c.agencia && `| Ag. ${c.agencia}`} {c.conta && `| Cta. ${c.conta}`}
              </div>
              <div style={{ fontSize: 22, fontWeight: 700, color: c.saldoAtual >= 0 ? '#2e7d32' : '#c62828', marginBottom: 16 }}>
                {fmt(c.saldoAtual)}
              </div>
              <div style={{ display: 'flex', gap: 8 }}>
                <button className="amr-btn amr-btn-sm" onClick={() => openEdit(c)}>
                  <i className="bi bi-pencil" /> Editar
                </button>
                {c.ativa && (
                  <button className="amr-btn amr-btn-sm amr-btn-danger" onClick={() => desativar(c.id)}>
                    <i className="bi bi-archive" /> Desativar
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="amr-modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="amr-modal" onClick={e => e.stopPropagation()} style={{ maxWidth: 480 }}>
            <div className="amr-modal-header">
              <span>{editId ? 'Editar Conta Bancária' : 'Nova Conta Bancária'}</span>
              <button className="amr-modal-close" onClick={() => setShowModal(false)}>&times;</button>
            </div>
            <div className="amr-modal-body">
              {erro && <div className="amr-alert amr-alert-danger">{erro}</div>}
              <div className="amr-form-group">
                <label>Nome *</label>
                <input className="amr-input" value={form.nome}
                  onChange={e => setForm(f => ({ ...f, nome: e.target.value }))} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="amr-form-group">
                  <label>Banco</label>
                  <input className="amr-input" value={form.banco}
                    onChange={e => setForm(f => ({ ...f, banco: e.target.value }))} />
                </div>
                <div className="amr-form-group">
                  <label>Tipo *</label>
                  <select className="amr-input" value={form.tipoConta}
                    onChange={e => setForm(f => ({ ...f, tipoConta: e.target.value as TipoContaBancaria }))}>
                    <option value="ContaCorrente">Conta Corrente</option>
                    <option value="Poupanca">Poupança</option>
                    <option value="Investimento">Investimento</option>
                  </select>
                </div>
                <div className="amr-form-group">
                  <label>Agência</label>
                  <input className="amr-input" value={form.agencia}
                    onChange={e => setForm(f => ({ ...f, agencia: e.target.value }))} />
                </div>
                <div className="amr-form-group">
                  <label>Conta</label>
                  <input className="amr-input" value={form.conta}
                    onChange={e => setForm(f => ({ ...f, conta: e.target.value }))} />
                </div>
                <div className="amr-form-group">
                  <label>Saldo Inicial (R$)</label>
                  <input className="amr-input" type="number" step="0.01" value={form.saldoInicial}
                    onChange={e => setForm(f => ({ ...f, saldoInicial: parseFloat(e.target.value) || 0 }))} />
                </div>
                <div className="amr-form-group">
                  <label>Data Saldo Inicial</label>
                  <input className="amr-input" type="date" value={form.dataSaldoInicial}
                    onChange={e => setForm(f => ({ ...f, dataSaldoInicial: e.target.value }))} />
                </div>
              </div>
            </div>
            <div className="amr-modal-footer">
              <button className="amr-btn" onClick={() => setShowModal(false)}>Cancelar</button>
              <button className="amr-btn amr-btn-primary" onClick={salvar} disabled={saving}>
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
