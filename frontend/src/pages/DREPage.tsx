import { useState, useEffect, useCallback } from 'react';
import { financeiroApi } from '../api/financeiroApi';
import type { DreDto, LinhaDreDto } from '../api/financeiroApi';
import { useCsvExport } from '../hooks/useCsvExport';

const CD_FILIAL = 1;

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const fmtPct = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

const MESES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
];

function Variacao({ valor }: { valor: number }) {
  if (valor === 0) return <span style={{ color: '#90a4ae' }}>—</span>;
  const positiva = valor > 0;
  return (
    <span style={{ color: positiva ? '#2e7d32' : '#c62828', fontWeight: 500 }}>
      <i className={`bi bi-caret-${positiva ? 'up' : 'down'}-fill`} style={{ fontSize: 10 }}></i>
      {' '}{fmtPct(Math.abs(valor))}
    </span>
  );
}

function LinhaDre({ linha }: { linha: LinhaDreDto }) {
  const [expandida, setExpandida] = useState(false);
  const temContas = linha.contas.length > 0;

  return (
    <>
      <tr
        style={{
          background: linha.ehSubtotal ? '#f5f5f5' : undefined,
          cursor: temContas ? 'pointer' : undefined,
        }}
        onClick={() => temContas && setExpandida(e => !e)}
      >
        <td style={{ fontWeight: linha.ehSubtotal || linha.negrito ? 700 : 400 }}>
          {temContas && (
            <i className={`bi bi-chevron-${expandida ? 'down' : 'right'} me-2`} style={{ fontSize: 11, color: '#90a4ae' }}></i>
          )}
          {linha.descricao}
        </td>
        <td style={{ textAlign: 'right', fontWeight: linha.ehSubtotal ? 700 : 400 }}>{fmt(linha.valorAtual)}</td>
        <td style={{ textAlign: 'right', color: '#607d8b' }}>{fmt(linha.valorPeriodoAnterior)}</td>
        <td style={{ textAlign: 'right', color: '#607d8b' }}>{fmt(linha.valorMesmoMesAnoAnterior)}</td>
        <td style={{ textAlign: 'right' }}><Variacao valor={linha.variacaoMes} /></td>
        <td style={{ textAlign: 'right' }}><Variacao valor={linha.variacaoAno} /></td>
      </tr>
      {expandida && linha.contas.map(conta => (
        <tr key={conta.codigo} style={{ fontSize: 12 }}>
          <td style={{ paddingLeft: 40, color: '#607d8b' }}>
            {conta.codigo} — {conta.descricao}
          </td>
          <td style={{ textAlign: 'right', color: '#607d8b' }}>{fmt(conta.valor)}</td>
          <td colSpan={4}></td>
        </tr>
      ))}
    </>
  );
}

export function DREPage() {
  const hoje = new Date();
  const [ano, setAno] = useState(hoje.getFullYear());
  const [mes, setMes] = useState(hoje.getMonth() + 1);
  const [data, setData] = useState<DreDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const { exportar, exportando } = useCsvExport();

  const load = useCallback(() => {
    setLoading(true);
    setErro(null);
    financeiroApi.getDre(CD_FILIAL, ano, mes)
      .then(setData)
      .catch(() => setErro('Erro ao calcular a DRE. Verifique o período informado.'))
      .finally(() => setLoading(false));
  }, [ano, mes]);

  useEffect(() => { load(); }, [load]);

  const anos = Array.from({ length: 6 }, (_, i) => hoje.getFullYear() - 4 + i);
  const qs = `cdFilial=${CD_FILIAL}&ano=${ano}&mes=${mes}`;

  return (
    <div>
      {/* Filtros + export */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 24, alignItems: 'center', flexWrap: 'wrap' }}>
        <select className="form-select form-select-sm" style={{ width: 140 }}
          value={mes} onChange={e => setMes(Number(e.target.value))}>
          {MESES.map((nome, i) => <option key={i + 1} value={i + 1}>{nome}</option>)}
        </select>
        <select className="form-select form-select-sm" style={{ width: 100 }}
          value={ano} onChange={e => setAno(Number(e.target.value))}>
          {anos.map(a => <option key={a} value={a}>{a}</option>)}
        </select>

        <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
          <button className="btn btn-sm btn-outline-success" disabled={exportando}
            onClick={() => exportar(`/dre/export/excel?${qs}`, `dre_${ano}_${mes}`)}>
            <i className="bi bi-file-earmark-excel me-1"></i>Excel
          </button>
          <button className="btn btn-sm btn-outline-danger" disabled={exportando}
            onClick={() => exportar(`/dre/export/pdf?${qs}`, `dre_${ano}_${mes}`)}>
            <i className="bi bi-file-earmark-pdf me-1"></i>PDF
          </button>
          <button className="btn btn-sm btn-outline-secondary" disabled={exportando}
            onClick={() => exportar(`/dre/export?${qs}`, `dre_${ano}_${mes}`)}>
            <i className="bi bi-download me-1"></i>CSV
          </button>
        </div>
      </div>

      {loading ? (
        <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Calculando DRE...</div></div>
      ) : erro || !data ? (
        <div className="amr-empty"><i className="bi bi-exclamation-triangle" /><div>{erro ?? 'Erro ao carregar'}</div></div>
      ) : (
        <>
          {/* Margens */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 24 }}>
            {[
              { label: 'Margem Bruta', valor: data.margemBruta, cor: '#1976d2' },
              { label: 'Margem Operacional', valor: data.margemOperacional, cor: '#6a1b9a' },
              { label: 'Margem Líquida', valor: data.margemLiquida, cor: data.margemLiquida >= 0 ? '#2e7d32' : '#c62828' },
            ].map(m => (
              <div key={m.label} className="amr-card" style={{ borderLeft: `4px solid ${m.cor}` }}>
                <div style={{ fontSize: 12, color: '#78909c', marginBottom: 4 }}>{m.label}</div>
                <div style={{ fontSize: 22, fontWeight: 700, color: m.cor }}>{fmtPct(m.valor)}</div>
              </div>
            ))}
          </div>

          {/* Tabela DRE */}
          <div className="amr-card">
            <div style={{ fontWeight: 600, fontSize: 14, marginBottom: 12, color: '#37474f' }}>
              Demonstração de Resultado — {data.periodo}
            </div>
            <div className="table-responsive">
              <table className="amr-table" style={{ fontSize: 13 }}>
                <thead>
                  <tr>
                    <th>Linha</th>
                    <th style={{ textAlign: 'right' }}>Atual</th>
                    <th style={{ textAlign: 'right' }}>Mês Anterior</th>
                    <th style={{ textAlign: 'right' }}>Mesmo Mês Ano Ant.</th>
                    <th style={{ textAlign: 'right' }}>Var. Mês</th>
                    <th style={{ textAlign: 'right' }}>Var. Ano</th>
                  </tr>
                </thead>
                <tbody>
                  {data.linhas.map(linha => (
                    <LinhaDre key={`${linha.grupo}-${linha.descricao}`} linha={linha} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
