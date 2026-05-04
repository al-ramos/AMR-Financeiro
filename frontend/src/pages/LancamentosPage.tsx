import { useState } from 'react';
import { useLancamentosPorPeriodo, useCriarLancamento } from '../hooks/useLancamentos';
import { usePlanoContasLista } from '../hooks/usePlanoContas';
import type { LancamentoFinanceiroDto } from '../api/lancamentosApi';
import { LancamentoForm } from '../components/lancamentos/LancamentoForm';
import { Modal } from '../components/ui/Modal';
import { AlertaErro } from '../components/ui/AlertaErro';

const CD_FILIAL = 1;

function getMesAtual() {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  return { inicio: `${y}-${m}-01`, fim: `${y}-${m}-${new Date(y, now.getMonth() + 1, 0).getDate()}` };
}

function formatDate(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

function formatValor(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function TipoBadge({ tipo }: { tipo: LancamentoFinanceiroDto['tipo'] }) {
  if (tipo === 'Credito') {
    return (
      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
        + Credito
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold bg-red-50 text-red-700 border border-red-200">
      - Debito
    </span>
  );
}

function OrigemBadge({ origem }: { origem: LancamentoFinanceiroDto['origem'] }) {
  const map: Record<string, string> = {
    Manual: 'bg-gray-100 text-gray-600',
    ContaPagar: 'bg-orange-50 text-orange-700',
    ContaReceber: 'bg-blue-50 text-blue-700',
  };
  const label: Record<string, string> = {
    Manual: 'Manual',
    ContaPagar: 'Ct. Pagar',
    ContaReceber: 'Ct. Receber',
  };
  return (
    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${map[origem] ?? ''}`}>
      {label[origem] ?? origem}
    </span>
  );
}

export function LancamentosPage() {
  const mesAtual = getMesAtual();
  const [inicio, setInicio] = useState(mesAtual.inicio);
  const [fim, setFim] = useState(mesAtual.fim);
  const [modalAberto, setModalAberto] = useState(false);
  const [erroForm, setErroForm] = useState<string | null>(null);

  const { data: lancamentos = [], isLoading, isError, error } = useLancamentosPorPeriodo(CD_FILIAL, inicio, fim);
  const { data: contas = [] } = usePlanoContasLista();
  const criar = useCriarLancamento();

  const totalCredito = lancamentos.filter(l => l.tipo === 'Credito').reduce((s: number, l: LancamentoFinanceiroDto) => s + l.valor, 0);
  const totalDebito = lancamentos.filter(l => l.tipo === 'Debito').reduce((s: number, l: LancamentoFinanceiroDto) => s + l.valor, 0);
  const saldo = totalCredito - totalDebito;

  const lancamentosOrdenados = [...lancamentos].sort(
    (a, b) => new Date(b.dataLancamento).getTime() - new Date(a.dataLancamento).getTime()
  );

  const abrirModal = () => {
    setErroForm(null);
    setModalAberto(true);
  };

  const fecharModal = () => {
    setModalAberto(false);
    setErroForm(null);
  };

  const handleSalvar = async (dados: {
    planoContasId: number;
    tipo: 'Debito' | 'Credito';
    valor: number;
    dataLancamento: string;
    historico: string;
  }) => {
    setErroForm(null);
    try {
      await criar.mutateAsync({ ...dados, cdFilial: CD_FILIAL });
      fecharModal();
    } catch (e: unknown) {
      const msg =
        (e as { response?: { data?: { erro?: string } } })?.response?.data?.erro ??
        'Erro ao registrar o lancamento. Verifique os dados e tente novamente.';
      setErroForm(msg);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Lancamentos</h1>
            <p className="text-sm text-gray-500 mt-0.5">Movimentacao financeira -- Filial {CD_FILIAL}</p>
          </div>
          <button
            onClick={abrirModal}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 active:bg-blue-800 transition-colors shadow-sm"
          >
            + Novo Lancamento
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-6">
        {/* Filtro de periodo */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 mb-6 flex flex-wrap items-end gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">De</label>
            <input
              type="date"
              value={inicio}
              onChange={e => setInicio(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Ate</label>
            <input
              type="date"
              value={fim}
              onChange={e => setFim(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <p className="text-xs text-gray-400 self-center">
            {lancamentos.length} lancamento{lancamentos.length !== 1 ? 's' : ''} no periodo
          </p>
        </div>

        {/* Cards resumo */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Total Creditos</p>
            <p className="text-2xl font-bold text-emerald-600 mt-1">{formatValor(totalCredito)}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Total Debitos</p>
            <p className="text-2xl font-bold text-red-500 mt-1">{formatValor(totalDebito)}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Saldo do Periodo</p>
            <p className={`text-2xl font-bold mt-1 ${saldo >= 0 ? 'text-emerald-600' : 'text-red-500'}`}>
              {formatValor(saldo)}
            </p>
          </div>
        </div>

        {/* Tabela */}
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          {isLoading && (
            <div className="flex items-center justify-center py-16 text-gray-400 text-sm">
              <svg className="animate-spin h-5 w-5 mr-2 text-blue-500" viewBox="0 0 24 24" fill="none">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
              </svg>
              Carregando lancamentos...
            </div>
          )}

          {isError && (
            <div className="p-6">
              <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar os lancamentos.'} />
            </div>
          )}

          {!isLoading && !isError && lancamentosOrdenados.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16 text-gray-400">
              <p className="text-sm font-medium">Nenhum lancamento no periodo</p>
              <p className="text-xs mt-1">Clique em "Novo Lancamento" para registrar</p>
            </div>
          )}

          {!isLoading && lancamentosOrdenados.length > 0 && (
            <table className="w-full text-left">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70">
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-28">Data</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-40">Conta</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Historico</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-28">Tipo</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-24">Origem</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-32 text-right">Valor</th>
                </tr>
              </thead>
              <tbody>
                {lancamentosOrdenados.map(l => (
                  <tr key={l.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3 text-sm text-gray-600 tabular-nums">{formatDate(l.dataLancamento)}</td>
                    <td className="px-4 py-3">
                      <span className="text-xs font-mono text-gray-500">{l.planoContasCodigo}</span>
                      <span className="text-xs text-gray-700 ml-1">{l.planoContasDescricao}</span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700">{l.historico}</td>
                    <td className="px-4 py-3">
                      <TipoBadge tipo={l.tipo} />
                    </td>
                    <td className="px-4 py-3">
                      <OrigemBadge origem={l.origem} />
                    </td>
                    <td className={`px-4 py-3 text-sm font-semibold text-right tabular-nums ${l.tipo === 'Credito' ? 'text-emerald-600' : 'text-red-500'}`}>
                      {l.tipo === 'Credito' ? '+' : '-'} {formatValor(l.valor)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </main>

      <Modal titulo="Novo Lancamento" aberto={modalAberto} onFechar={fecharModal}>
        {erroForm && (
          <div className="mb-4">
            <AlertaErro mensagem={erroForm} />
          </div>
        )}
        <LancamentoForm
          contas={contas}
          onSalvar={handleSalvar}
          onCancelar={fecharModal}
          carregando={criar.isPending}
        />
      </Modal>
    </div>
  );
}
