import { useState, useMemo } from 'react';
import {
  AreaChart, Area, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Legend,
} from 'recharts';
import { useLancamentosPorPeriodo } from '../hooks/useLancamentos';
import { useContasPagar } from '../hooks/useContasPagar';
import { useContasReceber } from '../hooks/useContasReceber';

const CD_FILIAL = 1;

// ── Helpers de período ────────────────────────────────────────────────────────
type Periodo = 'mes' | 'trimestre' | 'semestre' | 'ano';

const PERIODOS: { value: Periodo; label: string }[] = [
  { value: 'mes',       label: 'Mês atual'       },
  { value: 'trimestre', label: 'Últimos 3 meses'  },
  { value: 'semestre',  label: 'Últimos 6 meses'  },
  { value: 'ano',       label: 'Ano atual'        },
];

function getPeriodoDatas(periodo: Periodo): { inicio: string; fim: string; label: string } {
  const hoje = new Date();
  const y = hoje.getFullYear();
  const m = hoje.getMonth();

  const pad = (n: number) => String(n).padStart(2, '0');
  const isoDate = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

  if (periodo === 'mes') {
    const ultimo = new Date(y, m + 1, 0).getDate();
    return {
      inicio: `${y}-${pad(m + 1)}-01`,
      fim:    `${y}-${pad(m + 1)}-${ultimo}`,
      label:  hoje.toLocaleString('pt-BR', { month: 'long', year: 'numeric' }),
    };
  }
  if (periodo === 'trimestre') {
    const inicio = new Date(y, m - 2, 1);
    return { inicio: isoDate(inicio), fim: isoDate(new Date(y, m + 1, 0)), label: 'Últimos 3 meses' };
  }
  if (periodo === 'semestre') {
    const inicio = new Date(y, m - 5, 1);
    return { inicio: isoDate(inicio), fim: isoDate(new Date(y, m + 1, 0)), label: 'Últimos 6 meses' };
  }
  // ano
  return { inicio: `${y}-01-01`, fim: `${y}-12-31`, label: `Ano ${y}` };
}

function brl(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

// ── Card de métrica ───────────────────────────────────────────────────────────
function MetricCard({ label, value, icon, color, sub }: {
  label: string; value: string; icon: string; color: string; sub?: string
}) {
  return (
    <div className="amr-metric-card h-100">
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <div>
          <p className="amr-metric-label">{label}</p>
          <p className="amr-metric-value" style={{ color }}>{value}</p>
          {sub && <p style={{ fontSize: 11, color: '#9e9e9e', marginTop: 2 }}>{sub}</p>}
        </div>
        <div style={{
          width: 40, height: 40, borderRadius: 10,
          background: `${color}18`,
          display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
        }}>
          <i className={`bi ${icon}`} style={{ color, fontSize: 18 }}></i>
        </div>
      </div>
    </div>
  );
}

// ── Tooltip customizado ───────────────────────────────────────────────────────
function CustomTooltip({ active, payload, label }: {
  active?: boolean;
  payload?: { name: string; value: number; color: string }[];
  label?: string;
}) {
  if (!active || !payload?.length) return null;
  return (
    <div style={{
      background: '#1e1e2e', border: '1px solid #2a2a3d',
      borderRadius: 8, padding: '10px 14px', fontSize: 12,
    }}>
      <p style={{ color: '#adb5bd', marginBottom: 6, fontWeight: 600 }}>{label}</p>
      {payload.map(p => (
        <p key={p.name} style={{ color: p.color, margin: '2px 0' }}>
          {p.name}: {brl(p.value)}
        </p>
      ))}
    </div>
  );
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
export function DashboardPage() {
  const [periodo, setPeriodo] = useState<Periodo>('mes');
  const { inicio, fim, label: periodoLabel } = getPeriodoDatas(periodo);

  const { data: lancamentos = [], isLoading: loadLanc } = useLancamentosPorPeriodo(CD_FILIAL, inicio, fim);
  const { data: contasPagar = [], isLoading: loadCP }   = useContasPagar(CD_FILIAL);
  const { data: contasReceber = [], isLoading: loadCR } = useContasReceber(CD_FILIAL);

  const isLoading = loadLanc || loadCP || loadCR;

  // ── KPIs ─────────────────────────────────────────────────────────────────────
  const receitas = useMemo(
    () => lancamentos.filter(l => l.tipo === 'Credito').reduce((s, l) => s + l.valor, 0),
    [lancamentos],
  );
  const despesas = useMemo(
    () => lancamentos.filter(l => l.tipo === 'Debito').reduce((s, l) => s + l.valor, 0),
    [lancamentos],
  );
  const resultado = receitas - despesas;

  const cpAberto   = useMemo(() => contasPagar.filter(c => c.status === 'Aberta' || c.status === 'Vencida'), [contasPagar]);
  const cpValor    = cpAberto.reduce((s, c) => s + c.valor, 0);
  const cpVencidas = cpAberto.filter(c => c.status === 'Vencida').length;

  const crAberto = useMemo(() => contasReceber.filter(c => c.status === 'Aberta' || c.status === 'Vencida'), [contasReceber]);
  const crValor  = crAberto.reduce((s, c) => s + c.valor, 0);

  // ── Dados do gráfico ──────────────────────────────────────────────────────────
  const chartData = useMemo(() => {
    if (periodo === 'mes') {
      // Agrupar por dia
      const map: Record<string, { label: string; Receitas: number; Despesas: number }> = {};
      lancamentos.forEach(l => {
        const dia = l.dataLancamento.slice(8, 10); // DD
        if (!map[dia]) map[dia] = { label: `Dia ${dia}`, Receitas: 0, Despesas: 0 };
        if (l.tipo === 'Credito') map[dia].Receitas += l.valor;
        else map[dia].Despesas += l.valor;
      });
      return Object.entries(map).sort(([a], [b]) => Number(a) - Number(b)).map(([, v]) => v);
    } else {
      // Agrupar por mês
      const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
      const map: Record<string, { label: string; Receitas: number; Despesas: number; ordem: string }> = {};
      lancamentos.forEach(l => {
        const ym = l.dataLancamento.slice(0, 7); // YYYY-MM
        const mes = parseInt(l.dataLancamento.slice(5, 7)) - 1;
        const ano = l.dataLancamento.slice(0, 4);
        if (!map[ym]) map[ym] = { label: `${MESES[mes]}/${ano.slice(2)}`, Receitas: 0, Despesas: 0, ordem: ym };
        if (l.tipo === 'Credito') map[ym].Receitas += l.valor;
        else map[ym].Despesas += l.valor;
      });
      return Object.values(map).sort((a, b) => a.ordem.localeCompare(b.ordem));
    }
  }, [lancamentos, periodo]);

  if (isLoading) {
    return (
      <div className="amr-empty">
        <div className="spinner-border spinner-border-sm text-primary mb-2" role="status"></div>
        <span style={{ fontSize: 13 }}>Carregando dashboard...</span>
      </div>
    );
  }

  const margemPct = receitas > 0 ? ((resultado / receitas) * 100).toFixed(1) : '0.0';

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>

      {/* ── Cabeçalho + filtro de período ─────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12 }}>
        <div>
          <h1 style={{ fontSize: 20, fontWeight: 700, color: '#212529', margin: 0 }}>Dashboard Financeiro</h1>
          <p style={{ fontSize: 13, color: '#6c757d', margin: '2px 0 0', textTransform: 'capitalize' }}>
            {periodoLabel} · {lancamentos.length} lançamento{lancamentos.length !== 1 ? 's' : ''}
          </p>
        </div>
        <div className="btn-group btn-group-sm" role="group">
          {PERIODOS.map(p => (
            <button
              key={p.value}
              type="button"
              onClick={() => setPeriodo(p.value)}
              className={`btn ${periodo === p.value ? 'btn-primary' : 'btn-outline-secondary'}`}
              style={{ fontSize: 12 }}
            >
              {p.label}
            </button>
          ))}
        </div>
      </div>

      {/* ── DRE simplificada ──────────────────────────────────────────────────── */}
      <div className="amr-table-card" style={{ padding: '16px 20px' }}>
        <p style={{ fontSize: 11, fontWeight: 700, color: '#6c757d', textTransform: 'uppercase', letterSpacing: '0.08em', marginBottom: 12 }}>
          <i className="bi bi-file-earmark-text me-2"></i>DRE — Demonstrativo de Resultado
        </p>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16 }}>
          <div style={{ textAlign: 'center', padding: '12px 0', borderRight: '1px solid #f0f0f0' }}>
            <p style={{ fontSize: 11, color: '#6c757d', margin: '0 0 6px', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              <i className="bi bi-arrow-down-circle me-1" style={{ color: '#2e7d32' }}></i>Receitas
            </p>
            <p style={{ fontSize: 22, fontWeight: 700, color: '#2e7d32', margin: 0 }}>{brl(receitas)}</p>
            <p style={{ fontSize: 11, color: '#9e9e9e', margin: '4px 0 0' }}>
              {lancamentos.filter(l => l.tipo === 'Credito').length} créditos
            </p>
          </div>
          <div style={{ textAlign: 'center', padding: '12px 0', borderRight: '1px solid #f0f0f0' }}>
            <p style={{ fontSize: 11, color: '#6c757d', margin: '0 0 6px', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              <i className="bi bi-arrow-up-circle me-1" style={{ color: '#c62828' }}></i>Despesas
            </p>
            <p style={{ fontSize: 22, fontWeight: 700, color: '#c62828', margin: 0 }}>{brl(despesas)}</p>
            <p style={{ fontSize: 11, color: '#9e9e9e', margin: '4px 0 0' }}>
              {lancamentos.filter(l => l.tipo === 'Debito').length} débitos
            </p>
          </div>
          <div style={{ textAlign: 'center', padding: '12px 0' }}>
            <p style={{ fontSize: 11, color: '#6c757d', margin: '0 0 6px', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              <i className="bi bi-bullseye me-1" style={{ color: resultado >= 0 ? '#1565c0' : '#c62828' }}></i>Resultado
            </p>
            <p style={{ fontSize: 22, fontWeight: 700, color: resultado >= 0 ? '#1565c0' : '#c62828', margin: 0 }}>
              {brl(resultado)}
            </p>
            <p style={{ fontSize: 11, color: '#9e9e9e', margin: '4px 0 0' }}>
              Margem: <span style={{ color: resultado >= 0 ? '#2e7d32' : '#c62828', fontWeight: 600 }}>{margemPct}%</span>
            </p>
          </div>
        </div>
      </div>

      {/* ── KPI cards ─────────────────────────────────────────────────────────── */}
      <div className="row g-3">
        <div className="col-6 col-md-4">
          <MetricCard label="A pagar em aberto" value={brl(cpValor)} icon="bi-receipt-cutoff" color="#e65100"
            sub={`${cpAberto.length} conta${cpAberto.length !== 1 ? 's' : ''}${cpVencidas > 0 ? ` · ${cpVencidas} vencida${cpVencidas !== 1 ? 's' : ''}` : ''}`} />
        </div>
        <div className="col-6 col-md-4">
          <MetricCard label="A receber em aberto" value={brl(crValor)} icon="bi-cash-coin" color="#00695c"
            sub={`${crAberto.length} conta${crAberto.length !== 1 ? 's' : ''}`} />
        </div>
        <div className="col-12 col-md-4">
          <MetricCard
            label="Resultado projetado"
            value={brl(resultado + crValor - cpValor)}
            icon="bi-graph-up-arrow"
            color="#6a1b9a"
            sub="Resultado + receber − pagar"
          />
        </div>
      </div>

      {/* ── Gráfico ──────────────────────────────────────────────────────────── */}
      <div className="amr-table-card" style={{ padding: '20px 16px' }}>
        <p style={{ fontSize: 13, fontWeight: 600, color: '#495057', marginBottom: 16 }}>
          <i className="bi bi-bar-chart-line me-2"></i>
          Receitas × Despesas — {periodoLabel}
        </p>

        {chartData.length === 0 ? (
          <div className="amr-empty" style={{ minHeight: 180 }}>
            <i className="bi bi-inbox"></i>
            <div style={{ fontSize: 14 }}>Sem lançamentos no período</div>
          </div>
        ) : periodo === 'mes' ? (
          // Gráfico de área por dia (mês atual)
          <ResponsiveContainer width="100%" height={260}>
            <AreaChart data={chartData} margin={{ top: 4, right: 16, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id="gradReceitas" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#2e7d32" stopOpacity={0.25} />
                  <stop offset="95%" stopColor="#2e7d32" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="gradDespesas" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#c62828" stopOpacity={0.25} />
                  <stop offset="95%" stopColor="#c62828" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="label" tick={{ fontSize: 11, fill: '#9e9e9e' }} tickLine={false} axisLine={false} />
              <YAxis tick={{ fontSize: 11, fill: '#9e9e9e' }} tickLine={false} axisLine={false}
                tickFormatter={v => v >= 1000 ? `${(v / 1000).toFixed(0)}k` : String(v)} width={40} />
              <Tooltip content={<CustomTooltip />} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Area type="monotone" dataKey="Receitas" stroke="#2e7d32" strokeWidth={2}
                fill="url(#gradReceitas)" dot={false} activeDot={{ r: 4 }} />
              <Area type="monotone" dataKey="Despesas" stroke="#c62828" strokeWidth={2}
                fill="url(#gradDespesas)" dot={false} activeDot={{ r: 4 }} />
            </AreaChart>
          </ResponsiveContainer>
        ) : (
          // Gráfico de barras por mês (trimestre / semestre / ano)
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={chartData} margin={{ top: 4, right: 16, left: 0, bottom: 0 }} barCategoryGap="30%">
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
              <XAxis dataKey="label" tick={{ fontSize: 11, fill: '#9e9e9e' }} tickLine={false} axisLine={false} />
              <YAxis tick={{ fontSize: 11, fill: '#9e9e9e' }} tickLine={false} axisLine={false}
                tickFormatter={v => v >= 1000 ? `${(v / 1000).toFixed(0)}k` : String(v)} width={40} />
              <Tooltip content={<CustomTooltip />} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Bar dataKey="Receitas" fill="#2e7d32" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Despesas" fill="#c62828" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

    </div>
  );
}
