import { useState, useEffect, useCallback } from 'react';
import { financeiroApi } from '../api/financeiroApi';
import type { FluxoCaixaDto, FluxoCaixaDiaDto } from '../api/financeiroApi';

type Horizonte = 30 | 60 | 90;

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const fmtDate = (d: string) =>
  new Date(d).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });

// Simple SVG line chart
function LineChart({ dias }: { dias: FluxoCaixaDiaDto[] }) {
  if (dias.length === 0) return (
    <div className="amr-empty" style={{ minHeight: 180 }}>
      <i className="bi bi-bar-chart" />
      <div>Nenhum movimento projetado</div>
    </div>
  );

  const W = 700, H = 200, PAD = { top: 20, right: 20, bottom: 40, left: 60 };
  const inner = { w: W - PAD.left - PAD.right, h: H - PAD.top - PAD.bottom };

  // Saldo acumulado
  let acc = 0;
  const pontos = dias.map(d => { acc += d.saldo; return acc; });
  const mn = Math.min(0, ...pontos);
  const mx = Math.max(0, ...pontos);
  const range = mx - mn || 1;

  const x = (i: number) => PAD.left + (i / Math.max(pontos.length - 1, 1)) * inner.w;
  const y = (v: number) => PAD.top + (1 - (v - mn) / range) * inner.h;

  const pathD = pontos.map((v, i) => `${i === 0 ? 'M' : 'L'} ${x(i).toFixed(1)} ${y(v).toFixed(1)}`).join(' ');
  const areaD = `${pathD} L ${x(pontos.length - 1).toFixed(1)} ${(PAD.top + inner.h).toFixed(1)} L ${PAD.left} ${(PAD.top + inner.h).toFixed(1)} Z`;

  const zeroY = y(0);
  const ticks = 4;
  const yStep = range / ticks;

  // Select a few date labels
  const labelStep = Math.max(1, Math.floor(dias.length / 6));

  return (
    <svg viewBox={`0 0 ${W} ${H}`} style={{ width: '100%', height: 'auto' }}>
      {/* Zero line */}
      <line x1={PAD.left} y1={zeroY} x2={W - PAD.right} y2={zeroY}
        stroke="#e0e0e0" strokeDasharray="4 2" />

      {/* Y axis ticks */}
      {Array.from({ length: ticks + 1 }).map((_, i) => {
        const val = mn + yStep * i;
        const cy = y(val);
        return (
          <g key={i}>
            <line x1={PAD.left - 4} y1={cy} x2={PAD.left} y2={cy} stroke="#bdbdbd" />
            <text x={PAD.left - 8} y={cy + 4} textAnchor="end" fontSize="10" fill="#90a4ae">
              {(val / 1000).toFixed(0)}k
            </text>
          </g>
        );
      })}

      {/* Area */}
      <path d={areaD} fill="url(#grad)" opacity="0.2" />

      {/* Line */}
      <defs>
        <linearGradient id="grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#1976d2" />
          <stop offset="100%" stopColor="#1976d2" stopOpacity="0" />
        </linearGradient>
      </defs>
      <path d={pathD} fill="none" stroke="#1976d2" strokeWidth="2" strokeLinejoin="round" />

      {/* Points */}
      {pontos.map((v, i) => (
        <circle key={i} cx={x(i)} cy={y(v)} r="3"
          fill={v >= 0 ? '#1976d2' : '#c62828'} />
      ))}

      {/* X axis labels */}
      {dias.filter((_, i) => i % labelStep === 0 || i === dias.length - 1).map((d, _, arr) => {
        const idx = dias.indexOf(d);
        return (
          <text key={idx} x={x(idx)} y={H - 6} textAnchor="middle" fontSize="10" fill="#90a4ae">
            {fmtDate(d.data)}
          </text>
        );
      })}
    </svg>
  );
}

export function FluxoCaixaPage() {
  const [horizonte, setHorizonte] = useState<Horizonte>(30);
  const [data, setData] = useState<FluxoCaixaDto | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(() => {
    setLoading(true);
    financeiroApi.getFluxoCaixa(horizonte)
      .then(setData)
      .finally(() => setLoading(false));
  }, [horizonte]);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <div style={{ display: 'flex', gap: 8, marginBottom: 24 }}>
        {([30, 60, 90] as Horizonte[]).map(h => (
          <button
            key={h}
            className={`amr-btn${horizonte === h ? ' amr-btn-primary' : ''}`}
            onClick={() => setHorizonte(h)}
          >
            {h} dias
          </button>
        ))}
      </div>

      {loading ? (
        <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Calculando projeção...</div></div>
      ) : !data ? (
        <div className="amr-empty"><i className="bi bi-exclamation-triangle" /><div>Erro ao carregar</div></div>
      ) : (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 24 }}>
            <div className="amr-card" style={{ borderLeft: '4px solid #2e7d32' }}>
              <div style={{ fontSize: 12, color: '#78909c', marginBottom: 4 }}>Entradas Previstas</div>
              <div style={{ fontSize: 22, fontWeight: 700, color: '#2e7d32' }}>{fmt(data.totalEntradas)}</div>
            </div>
            <div className="amr-card" style={{ borderLeft: '4px solid #c62828' }}>
              <div style={{ fontSize: 12, color: '#78909c', marginBottom: 4 }}>Saídas Previstas</div>
              <div style={{ fontSize: 22, fontWeight: 700, color: '#c62828' }}>{fmt(data.totalSaidas)}</div>
            </div>
            <div className="amr-card" style={{ borderLeft: `4px solid ${data.saldoFinal >= 0 ? '#1976d2' : '#e65100'}` }}>
              <div style={{ fontSize: 12, color: '#78909c', marginBottom: 4 }}>Saldo Líquido ({horizonte}d)</div>
              <div style={{ fontSize: 22, fontWeight: 700, color: data.saldoFinal >= 0 ? '#1976d2' : '#e65100' }}>
                {fmt(data.saldoFinal)}
              </div>
            </div>
          </div>

          <div className="amr-card" style={{ marginBottom: 24 }}>
            <div style={{ fontWeight: 600, fontSize: 14, marginBottom: 16, color: '#37474f' }}>
              Saldo acumulado — próximos {horizonte} dias
            </div>
            <LineChart dias={data.dias} />
          </div>

          {data.dias.length > 0 && (
            <div className="amr-card">
              <div style={{ fontWeight: 600, fontSize: 14, marginBottom: 12, color: '#37474f' }}>
                Detalhe por dia
              </div>
              <div style={{ maxHeight: 320, overflowY: 'auto' }}>
                <table className="amr-table">
                  <thead>
                    <tr>
                      <th>Data</th>
                      <th style={{ textAlign: 'right' }}>Entradas</th>
                      <th style={{ textAlign: 'right' }}>Saídas</th>
                      <th style={{ textAlign: 'right' }}>Saldo do Dia</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.dias.map((d, i) => (
                      <tr key={i}>
                        <td>{new Date(d.data).toLocaleDateString('pt-BR')}</td>
                        <td style={{ textAlign: 'right', color: '#2e7d32' }}>
                          {d.entradas > 0 ? fmt(d.entradas) : '—'}
                        </td>
                        <td style={{ textAlign: 'right', color: '#c62828' }}>
                          {d.saidas > 0 ? fmt(d.saidas) : '—'}
                        </td>
                        <td style={{ textAlign: 'right', fontWeight: 600, color: d.saldo >= 0 ? '#1976d2' : '#e65100' }}>
                          {fmt(d.saldo)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
