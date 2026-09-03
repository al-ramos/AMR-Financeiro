import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { Modal } from '../components/ui/Modal';
import { centrosCustoApi } from '../api/centrosCustoApi';
import type { CentroCustoDto, AlertaDto, OrcamentoAnualDto } from '../api/centrosCustoApi';

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const TIPOS = ['Produtivo', 'Auxiliar', 'Administrativo', 'Comercial'];

interface ResumoOrcamento {
  totalOrcado: number;
  totalRealizado: number;
}

function corDoPercentual(pct: number): string {
  if (pct > 100) return '#b71c1c';
  if (pct > 80) return '#e65100';
  return '#2e7d32';
}

function BarraProgresso({ orcado, realizado }: { orcado: number; realizado: number }) {
  const pct = orcado > 0 ? (realizado / orcado) * 100 : 0;
  const cor = corDoPercentual(pct);
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
      <div style={{ flex: 1, height: 8, background: '#eceff1', borderRadius: 4, overflow: 'hidden' }}>
        <div style={{
          width: `${Math.min(pct, 100)}%`, height: '100%',
          background: cor, transition: 'width 0.4s',
        }} />
      </div>
      <span style={{ fontSize: 11, fontWeight: 600, color: cor, minWidth: 42, textAlign: 'right' }}>
        {orcado > 0 ? `${pct.toFixed(0)}%` : '—'}
      </span>
    </div>
  );
}

export function CentrosCustoPage() {
  const anoAtual = new Date().getFullYear();
  const [centros, setCentros] = useState<CentroCustoDto[]>([]);
  const [alertas, setAlertas] = useState<AlertaDto[]>([]);
  const [resumos, setResumos] = useState<Record<number, ResumoOrcamento>>({});
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  // Modal de criação
  const [modalAberto, setModalAberto] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [form, setForm] = useState({
    codigo: '', descricao: '', tipo: 'Administrativo',
    paiId: '' as string, responsavelNome: '',
  });

  const carregar = useCallback(async () => {
    setLoading(true);
    setErro(null);
    try {
      const [ccs, als] = await Promise.all([
        centrosCustoApi.listar(),
        centrosCustoApi.getAlertas(),
      ]);
      setCentros(ccs);
      setAlertas(als);

      const orcamentos = await Promise.all(
        ccs.map(cc =>
          centrosCustoApi.getOrcamento(cc.id, anoAtual)
            .then(o => [cc.id, o] as [number, OrcamentoAnualDto])
            .catch(() => null)
        )
      );
      const mapa: Record<number, ResumoOrcamento> = {};
      for (const item of orcamentos) {
        if (item) mapa[item[0]] = { totalOrcado: item[1].totalOrcado, totalRealizado: item[1].totalRealizado };
      }
      setResumos(mapa);
    } catch {
      setErro('Erro ao carregar centros de custo');
    } finally {
      setLoading(false);
    }
  }, [anoAtual]);

  useEffect(() => { carregar(); }, [carregar]);

  const criar = async (e: React.FormEvent) => {
    e.preventDefault();
    setSalvando(true);
    try {
      const pai = form.paiId ? centros.find(c => c.id === Number(form.paiId)) : undefined;
      await centrosCustoApi.criar({
        cdFilial: 1,
        codigo: form.codigo,
        descricao: form.descricao,
        tipo: form.tipo,
        paiId: pai ? pai.id : null,
        nivel: pai ? Math.min(pai.nivel + 1, 3) : 1,
        responsavelNome: form.responsavelNome || 'A definir',
      });
      setModalAberto(false);
      setForm({ codigo: '', descricao: '', tipo: 'Administrativo', paiId: '', responsavelNome: '' });
      await carregar();
    } catch {
      setErro('Erro ao criar centro de custo — verifique se o código já existe');
    } finally {
      setSalvando(false);
    }
  };

  if (loading) return (
    <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Carregando centros de custo...</div></div>
  );

  const ccsComAlerta = new Set(alertas.map(a => a.centroCustoId));
  // A lista já vem ordenada por código — a hierarquia é exibida por indentação de nível
  const totalOrcadoGeral = Object.values(resumos).reduce((s, r) => s + r.totalOrcado, 0);
  const totalRealizadoGeral = Object.values(resumos).reduce((s, r) => s + r.totalRealizado, 0);

  return (
    <div>
      {erro && (
        <div style={{
          background: '#ffebee', color: '#b71c1c', borderRadius: 8,
          padding: '10px 16px', marginBottom: 16, fontSize: 13,
        }}>
          <i className="bi bi-exclamation-triangle" style={{ marginRight: 8 }} />{erro}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 16, marginBottom: 24 }}>
        <div className="amr-card">
          <div style={{ fontSize: 12, color: '#78909c', fontWeight: 500 }}>Orçado {anoAtual}</div>
          <div style={{ fontSize: 26, fontWeight: 700, color: '#1565c0' }}>{fmt(totalOrcadoGeral)}</div>
        </div>
        <div className="amr-card">
          <div style={{ fontSize: 12, color: '#78909c', fontWeight: 500 }}>Realizado {anoAtual}</div>
          <div style={{ fontSize: 26, fontWeight: 700, color: corDoPercentual(totalOrcadoGeral > 0 ? totalRealizadoGeral / totalOrcadoGeral * 100 : 0) }}>
            {fmt(totalRealizadoGeral)}
          </div>
        </div>
        <div className="amr-card">
          <div style={{ fontSize: 12, color: '#78909c', fontWeight: 500 }}>Alertas de Orçamento</div>
          <div style={{ fontSize: 26, fontWeight: 700, color: alertas.length > 0 ? '#e65100' : '#2e7d32' }}>
            {alertas.length}
          </div>
        </div>
      </div>

      <div className="amr-card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <div style={{ fontSize: 14, fontWeight: 600, color: '#37474f' }}>
            Hierarquia de Centros de Custo
          </div>
          <button className="btn btn-primary btn-sm" onClick={() => setModalAberto(true)}>
            <i className="bi bi-plus-lg" style={{ marginRight: 6 }} />Novo Centro de Custo
          </button>
        </div>

        <table className="amr-table">
          <thead>
            <tr>
              <th>Código / Descrição</th>
              <th>Tipo</th>
              <th>Responsável</th>
              <th style={{ width: 220 }}>Orçado vs Realizado ({anoAtual})</th>
              <th style={{ textAlign: 'right' }}>Orçado</th>
              <th style={{ textAlign: 'right' }}>Realizado</th>
              <th style={{ width: 90 }}></th>
            </tr>
          </thead>
          <tbody>
            {centros.map(cc => {
              const resumo = resumos[cc.id] ?? { totalOrcado: 0, totalRealizado: 0 };
              const pct = resumo.totalOrcado > 0 ? resumo.totalRealizado / resumo.totalOrcado * 100 : 0;
              return (
                <tr key={cc.id} style={{ opacity: cc.ativo ? 1 : 0.5 }}>
                  <td>
                    <div style={{ paddingLeft: (cc.nivel - 1) * 22, display: 'flex', alignItems: 'center', gap: 8 }}>
                      <i className={`bi ${cc.nivel === 1 ? 'bi-folder-fill' : cc.nivel === 2 ? 'bi-folder' : 'bi-file-earmark'}`}
                         style={{ color: cc.nivel === 1 ? '#1565c0' : '#78909c', fontSize: 13 }} />
                      <span style={{ fontFamily: 'monospace', fontSize: 12, color: '#78909c' }}>{cc.codigo}</span>
                      <Link to={`/financeiro/centros-custo/${cc.id}`}
                            style={{ fontWeight: cc.nivel === 1 ? 600 : 400, color: '#263238', textDecoration: 'none' }}>
                        {cc.descricao}
                      </Link>
                      {pct > 80 && (
                        <span style={{
                          background: pct > 100 ? '#ffebee' : '#fff3e0',
                          color: pct > 100 ? '#b71c1c' : '#e65100',
                          fontSize: 10, fontWeight: 700, borderRadius: 10, padding: '2px 8px',
                        }}>
                          <i className="bi bi-exclamation-triangle-fill" style={{ marginRight: 4 }} />
                          {pct > 100 ? 'ESTOURADO' : `${pct.toFixed(0)}% DO ORÇADO`}
                        </span>
                      )}
                      {ccsComAlerta.has(cc.id) && pct <= 80 && (
                        <span style={{
                          background: '#fff3e0', color: '#e65100',
                          fontSize: 10, fontWeight: 700, borderRadius: 10, padding: '2px 8px',
                        }}>
                          <i className="bi bi-bell-fill" style={{ marginRight: 4 }} />ALERTA MENSAL
                        </span>
                      )}
                    </div>
                  </td>
                  <td style={{ fontSize: 12, color: '#546e7a' }}>{cc.tipo}</td>
                  <td style={{ fontSize: 12, color: '#546e7a' }}>{cc.responsavelNome}</td>
                  <td><BarraProgresso orcado={resumo.totalOrcado} realizado={resumo.totalRealizado} /></td>
                  <td style={{ textAlign: 'right', fontSize: 12 }}>{resumo.totalOrcado > 0 ? fmt(resumo.totalOrcado) : '—'}</td>
                  <td style={{ textAlign: 'right', fontSize: 12, fontWeight: 600, color: corDoPercentual(pct) }}>
                    {resumo.totalRealizado > 0 ? fmt(resumo.totalRealizado) : '—'}
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <Link to={`/financeiro/centros-custo/${cc.id}`} className="btn btn-outline-secondary btn-sm"
                          style={{ fontSize: 11 }}>
                      Detalhes
                    </Link>
                  </td>
                </tr>
              );
            })}
            {centros.length === 0 && (
              <tr><td colSpan={7} style={{ textAlign: 'center', color: '#78909c', padding: 24 }}>
                Nenhum centro de custo cadastrado
              </td></tr>
            )}
          </tbody>
        </table>
      </div>

      <Modal titulo="Novo Centro de Custo" aberto={modalAberto} onFechar={() => setModalAberto(false)}>
        <form onSubmit={criar}>
          <div className="mb-3">
            <label className="form-label" style={{ fontSize: 13 }}>Código *</label>
            <input className="form-control form-control-sm" required maxLength={20}
                   placeholder="Ex.: 01.03" value={form.codigo}
                   onChange={e => setForm(f => ({ ...f, codigo: e.target.value }))} />
          </div>
          <div className="mb-3">
            <label className="form-label" style={{ fontSize: 13 }}>Descrição *</label>
            <input className="form-control form-control-sm" required maxLength={200}
                   value={form.descricao}
                   onChange={e => setForm(f => ({ ...f, descricao: e.target.value }))} />
          </div>
          <div className="mb-3">
            <label className="form-label" style={{ fontSize: 13 }}>Tipo *</label>
            <select className="form-select form-select-sm" value={form.tipo}
                    onChange={e => setForm(f => ({ ...f, tipo: e.target.value }))}>
              {TIPOS.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div className="mb-3">
            <label className="form-label" style={{ fontSize: 13 }}>Centro de custo pai (opcional)</label>
            <select className="form-select form-select-sm" value={form.paiId}
                    onChange={e => setForm(f => ({ ...f, paiId: e.target.value }))}>
              <option value="">— Nenhum (nível 1) —</option>
              {centros.filter(c => c.nivel < 3 && c.ativo).map(c => (
                <option key={c.id} value={c.id}>{c.codigo} — {c.descricao}</option>
              ))}
            </select>
          </div>
          <div className="mb-4">
            <label className="form-label" style={{ fontSize: 13 }}>Responsável</label>
            <input className="form-control form-control-sm" maxLength={200}
                   value={form.responsavelNome}
                   onChange={e => setForm(f => ({ ...f, responsavelNome: e.target.value }))} />
          </div>
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
            <button type="button" className="btn btn-outline-secondary btn-sm" onClick={() => setModalAberto(false)}>
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary btn-sm" disabled={salvando}>
              {salvando ? 'Salvando...' : 'Criar'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
