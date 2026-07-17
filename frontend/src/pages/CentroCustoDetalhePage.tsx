import { useState, useEffect, useCallback } from 'react';
import { Link, useParams } from 'react-router-dom';
import { centrosCustoApi } from '../api/centrosCustoApi';
import type {
  CentroCustoDto, OrcamentoAnualDto, DreCentroCustoDto,
} from '../api/centrosCustoApi';

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const fmtData = (iso: string) => {
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
};

function corDoPercentual(pct: number): string {
  if (pct > 100) return '#b71c1c';
  if (pct > 80) return '#e65100';
  return '#2e7d32';
}

/** Gráfico de evolução mensal — barras orçado (cinza) vs realizado (colorido). */
function GraficoEvolucao({ orcamento }: { orcamento: OrcamentoAnualDto }) {
  const max = Math.max(...orcamento.meses.map(m => Math.max(m.orcado, m.realizado)), 1);
  return (
    <div style={{ display: 'flex', alignItems: 'flex-end', gap: 10, height: 160, padding: '8px 4px' }}>
      {orcamento.meses.map(m => {
        const pct = m.orcado > 0 ? m.realizado / m.orcado * 100 : 0;
        return (
          <div key={m.mes} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4, height: '100%' }}>
            <div style={{ flex: 1, display: 'flex', alignItems: 'flex-end', gap: 3, width: '100%', justifyContent: 'center' }}>
              <div title={`Orçado: ${fmt(m.orcado)}`} style={{
                width: '38%', maxWidth: 18, borderRadius: '3px 3px 0 0',
                height: `${m.orcado / max * 100}%`, minHeight: m.orcado > 0 ? 3 : 0,
                background: '#cfd8dc',
              }} />
              <div title={`Realizado: ${fmt(m.realizado)}`} style={{
                width: '38%', maxWidth: 18, borderRadius: '3px 3px 0 0',
                height: `${m.realizado / max * 100}%`, minHeight: m.realizado > 0 ? 3 : 0,
                background: corDoPercentual(pct), transition: 'height 0.4s',
              }} />
            </div>
            <span style={{ fontSize: 10, color: '#78909c' }}>{m.nomeMes.slice(0, 3)}</span>
          </div>
        );
      })}
    </div>
  );
}

export function CentroCustoDetalhePage() {
  const { id } = useParams<{ id: string }>();
  const ccId = Number(id);
  const anoAtual = new Date().getFullYear();

  const [cc, setCc] = useState<CentroCustoDto | null>(null);
  const [pai, setPai] = useState<CentroCustoDto | null>(null);
  const [ano, setAno] = useState(anoAtual);
  const [orcamento, setOrcamento] = useState<OrcamentoAnualDto | null>(null);
  const [dre, setDre] = useState<DreCentroCustoDto | null>(null);
  const [dataInicio, setDataInicio] = useState(`${anoAtual}-01-01`);
  const [dataFim, setDataFim] = useState(`${anoAtual}-12-31`);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  // Formulário de orçamento
  const [formOrc, setFormOrc] = useState({ contaDescricao: '', mes: 1, valorOrcado: '' });
  const [salvandoOrc, setSalvandoOrc] = useState(false);

  const carregarDre = useCallback(async () => {
    try {
      setDre(await centrosCustoApi.getDre(ccId, dataInicio, dataFim));
    } catch {
      setDre(null);
    }
  }, [ccId, dataInicio, dataFim]);

  useEffect(() => {
    let ativo = true;
    setLoading(true);
    Promise.all([
      centrosCustoApi.listar(),
      centrosCustoApi.getOrcamento(ccId, ano),
      centrosCustoApi.getDre(ccId, dataInicio, dataFim).catch(() => null),
    ])
      .then(([ccs, orc, dreResult]) => {
        if (!ativo) return;
        const atual = ccs.find(c => c.id === ccId) ?? null;
        setCc(atual);
        setPai(atual?.paiId ? ccs.find(c => c.id === atual.paiId) ?? null : null);
        setOrcamento(orc);
        setDre(dreResult);
        if (!atual) setErro('Centro de custo não encontrado');
      })
      .catch(() => { if (ativo) setErro('Erro ao carregar dados do centro de custo'); })
      .finally(() => { if (ativo) setLoading(false); });
    return () => { ativo = false; };
    // dataInicio/dataFim têm botão próprio de atualização — só o carregamento inicial usa os valores default
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ccId, ano]);

  const salvarOrcamento = async (e: React.FormEvent) => {
    e.preventDefault();
    setSalvandoOrc(true);
    setErro(null);
    try {
      await centrosCustoApi.atualizarOrcamento({
        centroCustoId: ccId,
        contaDescricao: formOrc.contaDescricao,
        ano,
        mes: formOrc.mes,
        valorOrcado: Number(formOrc.valorOrcado),
      });
      setFormOrc({ contaDescricao: '', mes: 1, valorOrcado: '' });
      setOrcamento(await centrosCustoApi.getOrcamento(ccId, ano));
    } catch {
      setErro('Erro ao salvar orçamento');
    } finally {
      setSalvandoOrc(false);
    }
  };

  if (loading) return (
    <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Carregando centro de custo...</div></div>
  );

  if (!cc) return (
    <div className="amr-empty">
      <i className="bi bi-exclamation-triangle" />
      <div>{erro ?? 'Centro de custo não encontrado'}</div>
      <Link to="/financeiro/centros-custo" style={{ marginTop: 8, fontSize: 13 }}>← Voltar para a lista</Link>
    </div>
  );

  const pctAno = orcamento && orcamento.totalOrcado > 0
    ? orcamento.totalRealizado / orcamento.totalOrcado * 100 : 0;

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

      {/* Cabeçalho */}
      <div className="amr-card" style={{ marginBottom: 24, display: 'flex', alignItems: 'center', gap: 24, flexWrap: 'wrap' }}>
        <div style={{ flex: 1, minWidth: 260 }}>
          <div style={{ fontSize: 12, color: '#78909c' }}>
            <Link to="/financeiro/centros-custo" style={{ color: '#78909c', textDecoration: 'none' }}>
              Centros de Custo
            </Link>
            {pai && <> · {pai.codigo} {pai.descricao}</>}
          </div>
          <div style={{ fontSize: 24, fontWeight: 700, color: '#263238' }}>
            <span style={{ fontFamily: 'monospace', color: '#78909c', marginRight: 10, fontSize: 18 }}>{cc.codigo}</span>
            {cc.descricao}
            {!cc.ativo && (
              <span style={{ background: '#eceff1', color: '#78909c', fontSize: 11, fontWeight: 600, borderRadius: 10, padding: '3px 10px', marginLeft: 10 }}>
                INATIVO
              </span>
            )}
          </div>
          <div style={{ fontSize: 12, color: '#546e7a', marginTop: 4 }}>
            {cc.tipo} · Nível {cc.nivel} · Responsável: {cc.responsavelNome}
          </div>
        </div>
        <div>
          <div style={{ fontSize: 12, color: '#78909c' }}>Consumo do orçamento {ano}</div>
          <div style={{ fontSize: 28, fontWeight: 700, color: corDoPercentual(pctAno) }}>
            {orcamento && orcamento.totalOrcado > 0 ? `${pctAno.toFixed(1)}%` : '—'}
          </div>
          {pctAno > 80 && (
            <span style={{
              background: pctAno > 100 ? '#ffebee' : '#fff3e0',
              color: pctAno > 100 ? '#b71c1c' : '#e65100',
              fontSize: 10, fontWeight: 700, borderRadius: 10, padding: '2px 8px',
            }}>
              <i className="bi bi-exclamation-triangle-fill" style={{ marginRight: 4 }} />
              {pctAno > 100 ? 'ORÇAMENTO ESTOURADO' : 'ACIMA DE 80% DO ORÇADO'}
            </span>
          )}
        </div>
        <div>
          <label style={{ fontSize: 12, color: '#78909c', display: 'block' }}>Ano</label>
          <select className="form-select form-select-sm" value={ano}
                  onChange={e => setAno(Number(e.target.value))} style={{ width: 100 }}>
            {[anoAtual - 2, anoAtual - 1, anoAtual, anoAtual + 1].map(a => (
              <option key={a} value={a}>{a}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Evolução mensal + tabela de orçamento */}
      <div style={{ display: 'grid', gridTemplateColumns: 'minmax(320px, 1fr) minmax(380px, 1fr)', gap: 24, marginBottom: 24 }}>
        <div className="amr-card">
          <div style={{ fontSize: 14, fontWeight: 600, color: '#37474f', marginBottom: 8 }}>
            Evolução Mensal {ano}
          </div>
          <div style={{ fontSize: 11, color: '#78909c', marginBottom: 4 }}>
            <span style={{ display: 'inline-block', width: 10, height: 10, background: '#cfd8dc', borderRadius: 2, marginRight: 4 }} />Orçado
            <span style={{ display: 'inline-block', width: 10, height: 10, background: '#2e7d32', borderRadius: 2, margin: '0 4px 0 12px' }} />Realizado
          </div>
          {orcamento && <GraficoEvolucao orcamento={orcamento} />}
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, color: '#546e7a', borderTop: '1px solid #eceff1', paddingTop: 8 }}>
            <span>Orçado: <strong>{fmt(orcamento?.totalOrcado ?? 0)}</strong></span>
            <span>Realizado: <strong style={{ color: corDoPercentual(pctAno) }}>{fmt(orcamento?.totalRealizado ?? 0)}</strong></span>
          </div>
        </div>

        <div className="amr-card">
          <div style={{ fontSize: 14, fontWeight: 600, color: '#37474f', marginBottom: 12 }}>
            Orçado vs Realizado por Mês
          </div>
          <div style={{ maxHeight: 220, overflowY: 'auto' }}>
            <table className="amr-table">
              <thead>
                <tr>
                  <th>Mês</th>
                  <th style={{ textAlign: 'right' }}>Orçado</th>
                  <th style={{ textAlign: 'right' }}>Realizado</th>
                  <th style={{ textAlign: 'right' }}>%</th>
                </tr>
              </thead>
              <tbody>
                {orcamento?.meses.map(m => (
                  <tr key={m.mes}>
                    <td style={{ fontSize: 12 }}>{m.nomeMes}</td>
                    <td style={{ textAlign: 'right', fontSize: 12 }}>{m.orcado > 0 ? fmt(m.orcado) : '—'}</td>
                    <td style={{ textAlign: 'right', fontSize: 12 }}>{m.realizado > 0 ? fmt(m.realizado) : '—'}</td>
                    <td style={{ textAlign: 'right', fontSize: 12, fontWeight: 600, color: corDoPercentual(m.percentualConsumido) }}>
                      {m.orcado > 0 ? `${m.percentualConsumido.toFixed(0)}%` : '—'}
                      {m.estourado && <i className="bi bi-exclamation-triangle-fill" style={{ marginLeft: 4, color: '#b71c1c' }} />}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <form onSubmit={salvarOrcamento} style={{ display: 'flex', gap: 8, marginTop: 12, borderTop: '1px solid #eceff1', paddingTop: 12, flexWrap: 'wrap' }}>
            <input className="form-control form-control-sm" required placeholder="Conta (ex.: Energia Elétrica)"
                   style={{ flex: 2, minWidth: 140 }} maxLength={200} value={formOrc.contaDescricao}
                   onChange={e => setFormOrc(f => ({ ...f, contaDescricao: e.target.value }))} />
            <select className="form-select form-select-sm" style={{ width: 80 }} value={formOrc.mes}
                    onChange={e => setFormOrc(f => ({ ...f, mes: Number(e.target.value) }))}>
              {Array.from({ length: 12 }, (_, i) => i + 1).map(m => <option key={m} value={m}>{m}</option>)}
            </select>
            <input className="form-control form-control-sm" required type="number" min="0" step="0.01"
                   placeholder="Valor orçado" style={{ width: 130 }} value={formOrc.valorOrcado}
                   onChange={e => setFormOrc(f => ({ ...f, valorOrcado: e.target.value }))} />
            <button className="btn btn-primary btn-sm" type="submit" disabled={salvandoOrc}>
              {salvandoOrc ? '...' : 'Definir'}
            </button>
          </form>
        </div>
      </div>

      {/* DRE do centro de custo */}
      <div className="amr-card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, flexWrap: 'wrap', gap: 12 }}>
          <div style={{ fontSize: 14, fontWeight: 600, color: '#37474f' }}>
            DRE do Centro de Custo {dre && <span style={{ fontWeight: 400, color: '#78909c', fontSize: 12 }}>· {dre.periodo}</span>}
          </div>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <input type="date" className="form-control form-control-sm" value={dataInicio}
                   onChange={e => setDataInicio(e.target.value)} />
            <span style={{ color: '#78909c', fontSize: 12 }}>a</span>
            <input type="date" className="form-control form-control-sm" value={dataFim}
                   onChange={e => setDataFim(e.target.value)} />
            <button className="btn btn-outline-primary btn-sm" onClick={carregarDre}>
              <i className="bi bi-search" />
            </button>
          </div>
        </div>

        {!dre ? (
          <div style={{ textAlign: 'center', color: '#78909c', padding: 24, fontSize: 13 }}>
            Sem dados no período selecionado
          </div>
        ) : (
          <>
            <table className="amr-table">
              <tbody>
                <tr style={{ fontWeight: 600, background: '#f5f7f8' }}>
                  <td>(+) Receitas</td>
                  <td style={{ textAlign: 'right', color: '#2e7d32' }}>{fmt(dre.totalReceitas)}</td>
                </tr>
                {dre.receitas.map(r => (
                  <tr key={`rec-${r.contaCodigo}`}>
                    <td style={{ paddingLeft: 28, fontSize: 12, color: '#546e7a' }}>
                      <span style={{ fontFamily: 'monospace', marginRight: 8 }}>{r.contaCodigo}</span>{r.contaDescricao}
                    </td>
                    <td style={{ textAlign: 'right', fontSize: 12 }}>{fmt(r.valor)}</td>
                  </tr>
                ))}
                <tr style={{ fontWeight: 600, background: '#f5f7f8' }}>
                  <td>(-) Despesas Diretas</td>
                  <td style={{ textAlign: 'right', color: '#b71c1c' }}>{fmt(dre.totalDespesas)}</td>
                </tr>
                {dre.despesas.map(d => (
                  <tr key={`des-${d.contaCodigo}`}>
                    <td style={{ paddingLeft: 28, fontSize: 12, color: '#546e7a' }}>
                      <span style={{ fontFamily: 'monospace', marginRight: 8 }}>{d.contaCodigo}</span>{d.contaDescricao}
                    </td>
                    <td style={{ textAlign: 'right', fontSize: 12 }}>{fmt(d.valor)}</td>
                  </tr>
                ))}
                <tr style={{ fontWeight: 600, background: '#f5f7f8' }}>
                  <td>(-) Custos Rateados</td>
                  <td style={{ textAlign: 'right', color: '#e65100' }}>{fmt(dre.totalRateiosRecebidos)}</td>
                </tr>
                <tr style={{ fontWeight: 700, borderTop: '2px solid #37474f' }}>
                  <td>(=) Resultado do Período</td>
                  <td style={{ textAlign: 'right', color: dre.resultado >= 0 ? '#2e7d32' : '#b71c1c', fontSize: 15 }}>
                    {fmt(dre.resultado)}
                  </td>
                </tr>
              </tbody>
            </table>

            <div style={{ fontSize: 13, fontWeight: 600, color: '#37474f', margin: '20px 0 8px' }}>
              Histórico de Rateios Recebidos
            </div>
            {dre.rateiosRecebidos.length === 0 ? (
              <div style={{ fontSize: 12, color: '#78909c' }}>Nenhum rateio recebido no período</div>
            ) : (
              <table className="amr-table">
                <thead>
                  <tr>
                    <th>Competência</th>
                    <th>Regra de Rateio</th>
                    <th style={{ textAlign: 'right' }}>% Aplicado</th>
                    <th style={{ textAlign: 'right' }}>Valor</th>
                  </tr>
                </thead>
                <tbody>
                  {dre.rateiosRecebidos.map((r, i) => (
                    <tr key={i}>
                      <td style={{ fontSize: 12 }}>{fmtData(r.competencia)}</td>
                      <td style={{ fontSize: 12 }}>{r.regraNome}</td>
                      <td style={{ textAlign: 'right', fontSize: 12 }}>{r.percentualAplicado.toFixed(2)}%</td>
                      <td style={{ textAlign: 'right', fontSize: 12, fontWeight: 600 }}>{fmt(r.valor)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </>
        )}
      </div>
    </div>
  );
}
