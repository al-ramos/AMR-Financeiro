import { BrowserRouter, Route, Routes, NavLink, useLocation } from 'react-router-dom';
import { PlanoContasPage } from './pages/PlanoContasPage';
import { LancamentosPage } from './pages/LancamentosPage';
import { ContasPagarPage } from './pages/ContasPagarPage';
import { ContasReceberPage } from './pages/ContasReceberPage';

const NAV = [
  {
    section: 'Cadastros',
    items: [
      { to: '/plano-contas', icon: 'bi-diagram-3', label: 'Plano de Contas' },
    ],
  },
  {
    section: 'Movimento',
    items: [
      { to: '/lancamentos',    icon: 'bi-journal-text',  label: 'Lançamentos' },
      { to: '/contas-pagar',   icon: 'bi-arrow-up-circle', label: 'Contas a Pagar' },
      { to: '/contas-receber', icon: 'bi-arrow-down-circle', label: 'Contas a Receber' },
    ],
  },
  {
    section: 'Relatórios',
    items: [
      { to: '/dashboard', icon: 'bi-bar-chart-line', label: 'Dashboard' },
    ],
  },
];

const PAGE_LABELS: Record<string, { title: string; subtitle: string }> = {
  '/':               { title: 'Lançamentos',       subtitle: 'Histórico de movimentos' },
  '/lancamentos':    { title: 'Lançamentos',       subtitle: 'Histórico de movimentos' },
  '/plano-contas':   { title: 'Plano de Contas',   subtitle: 'Estrutura contábil' },
  '/contas-pagar':   { title: 'Contas a Pagar',    subtitle: 'Controle de obrigações' },
  '/contas-receber': { title: 'Contas a Receber',  subtitle: 'Controle de recebimentos' },
  '/dashboard':      { title: 'Dashboard',         subtitle: 'Visão gerencial' },
};

function Sidebar() {
  return (
    <nav className="amr-sidebar">
      <a href="/" className="amr-sidebar-brand">
        AMR <span>Financeiro</span>
      </a>
      {NAV.map(group => (
        <div key={group.section}>
          <p className="amr-sidebar-section">{group.section}</p>
          {group.items.map(item => (
            <NavLink key={item.to} to={item.to} className="nav-link">
              <i className={`bi ${item.icon}`}></i>
              {item.label}
            </NavLink>
          ))}
        </div>
      ))}
      <div style={{ marginTop: 'auto', padding: '1rem 1.25rem', borderTop: '1px solid rgba(255,255,255,.06)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <div style={{
            width: 30, height: 30, borderRadius: '50%',
            background: 'var(--amr-sidebar-active)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 12, fontWeight: 700, color: '#fff'
          }}>A</div>
          <div>
            <div style={{ fontSize: 12, color: '#cfd8dc', fontWeight: 500 }}>AMR Sistema</div>
            <div style={{ fontSize: 10, color: '#546e7a' }}>Financeiro</div>
          </div>
        </div>
      </div>
    </nav>
  );
}

function Topbar() {
  const loc = useLocation();
  const info = PAGE_LABELS[loc.pathname] ?? { title: 'AMR Financeiro', subtitle: '' };
  return (
    <header className="amr-topbar">
      <div style={{ flex: 1 }}>
        <p className="amr-topbar-title">{info.title}</p>
        {info.subtitle && <p className="amr-topbar-subtitle">{info.subtitle}</p>}
      </div>
      <span style={{ fontSize: 11, color: '#adb5bd' }}>
        <i className="bi bi-circle-fill" style={{ color: '#4caf50', fontSize: 8, marginRight: 4 }}></i>
        Online
      </span>
    </header>
  );
}

function EmConstrucao({ titulo }: { titulo: string }) {
  return (
    <div className="amr-empty">
      <i className="bi bi-tools"></i>
      <div style={{ fontSize: 15, fontWeight: 600, color: '#495057' }}>{titulo}</div>
      <div style={{ fontSize: 13, marginTop: 4 }}>Em desenvolvimento · Sprint 3</div>
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <div className="amr-wrapper">
        <Sidebar />
        <div className="amr-content-wrapper">
          <Topbar />
          <main className="amr-content">
            <Routes>
              <Route path="/"                element={<LancamentosPage />} />
              <Route path="/plano-contas"    element={<PlanoContasPage />} />
              <Route path="/lancamentos"     element={<LancamentosPage />} />
              <Route path="/contas-pagar"    element={<ContasPagarPage />} />
              <Route path="/contas-receber"  element={<ContasReceberPage />} />
              <Route path="/dashboard"       element={<EmConstrucao titulo="Dashboard" />} />
            </Routes>
          </main>
        </div>
      </div>
    </BrowserRouter>
  );
}
