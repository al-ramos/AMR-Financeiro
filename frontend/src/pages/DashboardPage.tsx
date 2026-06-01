import { useMemo } from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Legend,
} from 'recharts';
import { useLancamentosPorPeriodo } from '../hooks/useLancamentos';
import { useContasPagar } from '../hooks/useContasPagar';
import { useContasReceber } from '../hooks/useContasReceber';

const CD_FILIAL = 1;

function getMesAtual() {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const ultimo = new Date(y, now.getMonth() + 1, 0).getDate();
  return { inicio: `${y}-${m}-01`, fim: `${y}-${m}-${ultimo}` };
}

function brl(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function fmtDia(iso: string) {
  const [, , d] = iso.split('-');
  return `${d}`;
}

// ── Card de métrica ───────────────────────────────────────────────────────────
interface MetricCardProps {
  label: string;
  value: string;
  icon: string;
  color: string;
  sub?: string;
}
function MetricCard({ label, value, icon, color, sub }: MetricCardProps) {
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
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          flexShrink: 0,
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
      <p style={{ color: '#adb5bd', marginBottom: 6, fontWeight: 600 }}>Dia {label}</p>
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
  const { inicio, fim } = getMesAtual();

  const { data: lancamentos = [], isLoading: loadLanc } = useLancamentosPorPeriodo(CD_FILIAL, inicio, fim);
  const { data: contasPagar = [], isLoading: loadCP } = useContasPagar(CD_FILIAL);
  const { data: contasReceber = [], isLoading: loadCR } = useContasReceber(CD_FILIAL);

  const isLoading = loadLanc || loadCP || loadCR;

  // ── Métricas mensais ────────────────────────────────────────────────────────
  const receitas = useMemo(
    () => lancamentos.filter(l => l.tipo === 'Credito').reduce((s, l) => s + l.valor, 0),
    [lancamentos],
  );
  const despesas = useMemo(
    () => lancamentos.filter(l => l.tipo === 'Debito').reduce((s, l) => s + l.valor, 0),
    [lancamentos],
  );
  const saldo = receitas - despesas;

  // ── Contas a pagar em aberto ────────────────────────────────────────────────
  const cpAberto = useMemo(
    () => contasPagar.filter(c => c.status === 'Aberta' || c.status === 'Vencida'),
    [contasPagar],
  );
  const cpValor = cpAberto.reduce((s, c) => s + c.valor, 0);
  const cpVencidas = cpAberto.filter(c => c.status === 'Vencida').length;

  // ── Contas a receber em aberto ──────────────────────────────────────────────
  const crAberto = useMemo(
    () => contasReceber.filter(c => c.status === 'Aberta' || c.status === 'Vencida'),
    [contasReceber],
  );
  const crValor = crAberto.reduce((s, c) => s + c.valor, 0);

  // ── Dados do gráfico (agrupado por dia) ─────────────────────────────────────
  const chartData = useMemo(() => {
    const map: Record<string, { dia: string; Receitas: number; Despesas: number }> = {};
    lancamentos.forEach(l => {
      const dia = fmtDia(l.dataLancamento);
      if (!map[dia]) map[dia] = { dia, Receitas: 0, Despesas: 0 };
      if (l.tipo === 'Credito') map[dia].Receitas += l.valor;
      else map[dia].Despesas += l.valor;
    });
    return Object.values(map).sort((a, b) => Number(a.dia) - Number(b.dia));
  }, [lancamentos]);

  if (isLoading) {
    return (
      <div className="amr-empty">
        <div className="spinner-border spinner-border-sm text-primary mb-2" role="status"></div>
        <span style={{ fontSize: 13 }}>Carregando dashboard...</span>
      </div>
    );
  }

  const mesLabel = new Date().toLocaleString('pt-BR', { month: 'long', year: 'numeric' });

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>

      {/* ── Cabeçalho ─────────────────────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div>
          <p style={{ fontSize: 13, color: '#6c757d', margin: 0 }}>Período</p>
          <p style={{ fontSize: 15, fontWeight: 600, color: '#343a40', margin: 0, textTransform: 'capitalize' }}>
            {mesLabel}
          </p>
        </div>
        <span style={{ fontSize: 11, color: '#adb5bd' }}>
          <i className="bi bi-circle-fill" style={{ color: '#4caf50', fontSize: 8, marginRight: 4 }}></i>
          {lancamentos.length} lançamento{lancamentos.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* ── Cards de métricas ──────────────────────────────────────────────── */}
      <div className="row g-3">
        <div className="col-6 col-md-4">
          <MetricCard
            label="Receitas do mês"
            value={brl(receitas)}
            icon="bi-arrow-down-circle-fill"
            color="#2e7d32"
            sub={`${lancamentos.filter(l => l.tipo === 'Credito').length} créditos`}
          />
        </div>
        <div className="col-6 col-md-4">
          <MetricCard
            label="Despesas do mês"
            value={brl(despesas)}
            icon="bi-arrow-up-circle-fill"
            color="#c62828"
            sub={`${lancamentos.filter(l => l.tipo === 'Debito').length} débitos`}
          />
        </div>
        <div className="col-12 col-md-4">
          <MetricCard
            label="Saldo do mês"
            value={brl(saldo)}
            icon={saldo >= 0 ? 'bi-graph-up-arrow' : 'bi-graph-down-arrow'}
            color={saldo >= 0 ? '#1565c0' : '#c62828'}
            sub={saldo >= 0 ? 'Resultado positivo' : 'Resultado negativo'}
          />
        </div>
        <div className="col-6 col-md-4">
          <MetricCard
            label="A pagar em aberto"
            value={brl(cpValor)}
            icon="bi-receipt-cutoff"
            color="#e65100"
            sub={`${cpAberto.length} conta${cpAberto.length !== 1 ? 's' : ''}${cpVencidas > 0 ? ` · ${cpVencidas} vencida${cpVencidas !== 1 ? 's' : ''}` : ''}`}
          />
        </div>
        <div className="col-6 col-md-4">
          <MetricCard
            label="A receber em aberto"
            value={brl(crValor)}
            icon="bi-cash-coin"
            color="#00695c"
            sub={`${crAberto.length} conta${crAberto.length !== 1 ? 's' : ''}`}
          />
        </div>
        <div className="col-12 col-md-4">
          <MetricCard
            label="Resultado projetado"
            value={brl(saldo + crValor - cpValor)}
            icon="bi-bullseye"
            color="#6a1b9a"
            sub="Saldo + receber − pagar"
          />
        </div>
      </div>

      {/* ── Gráfico de lançamentos por dia ─────────────────────────────────── */}
      <div className="amr-table-card" style={{ padding: '20px 16px' }}>
        <p style={{ fontSize: 13, fontWeight: 600, color: '#495057', marginBottom: 16 }}>
          <i className="bi bi-bar-chart-line me-2"></i>
          Receitas × Despesas — {mesLabel}
        </p>

        {chartData.length === 0 ? (
          <div className="amr-empty" style={{ minHeight: 180 }}>
            <i className="bi bi-inbox"></i>
            <div style={{ fontSize: 14 }}>Sem lançamentos no mês</div>
          </div>
        ) : (
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
              <XAxis dataKey="dia" tick={{ fontSize: 11, fill: '#9e9e9e' }} tickLine={false} axisLine={false} />
              <YAxis
                tick={{ fontSize: 11, fill: '#9e9e9e' }}
                tickLine={false}
                axisLine={false}
                tickFormatter={v => v >= 1000 ? `${(v / 1000).toFixed(0)}k` : String(v)}
                width={40}
              />
              <Tooltip content={<CustomTooltip />} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
              <Area
                type="monotone"
                dataKey="Receitas"
                stroke="#2e7d32"
                strokeWidth={2}
                fill="url(#gradReceitas)"
                dot={false}
                activeDot={{ r: 4 }}
              />
              <Area
                type="monotone"
                dataKey="Despesas"
                stroke="#c62828"
                strokeWidth={2}
                fill="url(#gradDespesas)"
                dot={false}
                activeDot={{ r: 4 }}
              />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </div>

    </div>
  );
}
