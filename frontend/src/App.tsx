import { BrowserRouter, Route, Routes, NavLink } from 'react-router-dom';
import { PlanoContasPage } from './pages/PlanoContasPage';
import { LancamentosPage } from './pages/LancamentosPage';
import { ContasPagarPage } from './pages/ContasPagarPage';

function Sidebar() {
  const navClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
      isActive
        ? 'bg-blue-50 text-blue-700'
        : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
    }`;

  return (
    <aside className="hidden md:flex flex-col w-56 shrink-0 bg-white border-r border-gray-200 min-h-screen px-3 py-5">
      <div className="mb-6 px-3">
        <span className="text-lg font-bold text-gray-900">AMR</span>
        <span className="text-lg font-light text-blue-600"> Financeiro</span>
      </div>
      <nav className="flex flex-col gap-0.5">
        <p className="px-3 pt-2 pb-1 text-xs font-semibold text-gray-400 uppercase tracking-wider">Cadastros</p>
        <NavLink to="/plano-contas" className={navClass}>Plano de Contas</NavLink>

        <p className="px-3 pt-4 pb-1 text-xs font-semibold text-gray-400 uppercase tracking-wider">Movimento</p>
        <NavLink to="/lancamentos" className={navClass}>Lancamentos</NavLink>
        <NavLink to="/contas-pagar" className={navClass}>Contas a Pagar</NavLink>
        <NavLink to="/contas-receber" className={navClass}>Contas a Receber</NavLink>

        <p className="px-3 pt-4 pb-1 text-xs font-semibold text-gray-400 uppercase tracking-wider">Relatorios</p>
        <NavLink to="/dashboard" className={navClass}>Dashboard</NavLink>
      </nav>
    </aside>
  );
}

function EmConstrucao({ titulo }: { titulo: string }) {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="text-center">
        <p className="text-5xl mb-4">🚧</p>
        <h2 className="text-xl font-semibold text-gray-700">{titulo}</h2>
        <p className="text-sm text-gray-400 mt-2">Em desenvolvimento - Sprint 3</p>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <div className="flex">
        <Sidebar />
        <div className="flex-1">
          <Routes>
            <Route path="/" element={<LancamentosPage />} />
            <Route path="/plano-contas" element={<PlanoContasPage />} />
            <Route path="/lancamentos" element={<LancamentosPage />} />
            <Route path="/contas-pagar" element={<ContasPagarPage />} />
            <Route path="/contas-receber" element={<EmConstrucao titulo="Contas a Receber" />} />
            <Route path="/dashboard" element={<EmConstrucao titulo="Dashboard" />} />
          </Routes>
        </div>
      </div>
    </BrowserRouter>
  );
}
