import { useState, useEffect } from 'react';
import { financeiroApi } from '../api/financeiroApi';
import type { AgingDto, AgingFaixaDto } from '../api/financeiroApi';

const fmt = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

interface FaixaConfig {
  key: keyof Omit<AgingDto, 'totalEmAberto'>;
  label: string;
  color: string;
  bg: string;
}

const FAIXAS: FaixaConfig[] = [
  { key: 'aVencer',  label: 'A Vencer',      color: '#2e7d32', bg: '#e8f5e9' },
  { key: 'de1a30',   label: '1 a 30 dias',   color: '#f57f17', bg: '#fff9c4' },
  { key: 'de31a60',  label: '31 a 60 dias',  color: '#e65100', bg: '#fbe9e7' },
  { key: 'de61a90',  label: '61 a 90 dias',  color: '#b71c1c', bg: '#ffebee' },
  { key: 'acima90',  label: 'Acima 90 dias', color: '#880e4f', bg: '#fce4ec' },
];

function AgingCard({ faixa, data }: { faixa: FaixaConfig; data: AgingFaixaDto }) {
  return (
    <div style={{
      background: faixa.bg,
      borderLeft: `4px solid ${faixa.color}`,
      borderRadius: 8,
      padding: '16px 20px',
    }}>
      <div style={{ fontSize: 12, fontWeight: 600, color: faixa.color, marginBottom: 8 }}>
        {data.faixa}
      </div>
      <div style={{ fontSize: 26, fontWeight: 700, color: faixa.color, marginBottom: 4 }}>
        {fmt(data.valorTotal)}
      </div>
      <div style={{ fontSize: 12, color: faixa.color + 'cc' }}>
        {data.quantidade} {data.quantidade === 1 ? 'parcela' : 'parcelas'} em aberto
      </div>
    </div>
  );
}

export function AgingPage() {
  const [aging, setAging] = useState<AgingDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    financeiroApi.getAging()
      .then(setAging)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return (
    <div className="amr-empty"><i className="bi bi-arrow-repeat amr-spin" /><div>Carregando aging...</div></div>
  );

  if (!aging) return (
    <div className="amr-empty"><i className="bi bi-exclamation-triangle" /><div>Erro ao carregar dados</div></div>
  );

  const total = aging.totalEmAberto;

  return (
    <div>
      <div className="amr-card" style={{ marginBottom: 24, display: 'flex', alignItems: 'center', gap: 24 }}>
        <div>
          <div style={{ fontSize: 12, color: '#78909c', fontWeight: 500 }}>Total em Aberto</div>
          <div style={{ fontSize: 32, fontWeight: 700, color: '#b71c1c' }}>{fmt(total)}</div>
        </div>
        <div style={{ flex: 1, height: 8, background: '#f5f5f5', borderRadius: 4, overflow: 'hidden', display: 'flex' }}>
          {FAIXAS.map(f => {
            const val = aging[f.key].valorTotal;
            const pct = total > 0 ? (val / total) * 100 : 0;
            return pct > 0 ? (
              <div key={f.key} style={{ width: `${pct}%`, background: f.color, transition: 'width 0.5s' }} title={`${f.label}: ${fmt(val)}`} />
            ) : null;
          })}
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 16, marginBottom: 32 }}>
        {FAIXAS.map(f => (
          <AgingCard key={f.key} faixa={f} data={aging[f.key]} />
        ))}
      </div>

      <div className="amr-card">
        <table className="amr-table">
          <thead>
            <tr>
              <th>Faixa</th>
              <th style={{ textAlign: 'right' }}>Parcelas</th>
              <th style={{ textAlign: 'right' }}>Valor Total</th>
              <th style={{ textAlign: 'right' }}>% do Total</th>
            </tr>
          </thead>
          <tbody>
            {FAIXAS.map(f => {
              const d = aging[f.key];
              const pct = total > 0 ? (d.valorTotal / total * 100).toFixed(1) : '0.0';
              return (
                <tr key={f.key}>
                  <td>
                    <span style={{
                      display: 'inline-block', width: 10, height: 10,
                      borderRadius: 2, background: f.color, marginRight: 8,
                    }} />
                    {d.faixa}
                  </td>
                  <td style={{ textAlign: 'right' }}>{d.quantidade}</td>
                  <td style={{ textAlign: 'right', color: f.color, fontWeight: 600 }}>{fmt(d.valorTotal)}</td>
                  <td style={{ textAlign: 'right', color: '#78909c' }}>{pct}%</td>
                </tr>
              );
            })}
            <tr style={{ fontWeight: 700 }}>
              <td>Total</td>
              <td style={{ textAlign: 'right' }}>
                {FAIXAS.reduce((s, f) => s + aging[f.key].quantidade, 0)}
              </td>
              <td style={{ textAlign: 'right', color: '#b71c1c' }}>{fmt(total)}</td>
              <td style={{ textAlign: 'right' }}>100%</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
