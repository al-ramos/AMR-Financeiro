import { useState } from 'react';
import { useContasPagar, useCriarContaPagar, usePagarConta, useCancelarConta } from '../hooks/useContasPagar';
import type { ContaPagarDto } from '../api/contasPagarApi';
import { ContaPagarForm } from '../components/contasPagar/ContaPagarForm';
import { Modal } from '../components/ui/Modal';
import { AlertaErro } from '../components/ui/AlertaErro';

const CD_FILIAL = 1;

function formatDate(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

function formatValor(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function StatusBadge({ status }: { status: ContaPagarDto['status'] }) {
  const map: Record<string, string> = {
    Aberta:    'bg-blue-50 text-blue-700 border-blue-200',
    Paga:      'bg-emerald-50 text-emerald-700 border-emerald-200',
    Vencida:   'bg-red-50 text-red-700 border-red-200',
    Cancelada: 'bg-gray-100 text-gray-500 border-gray-200',
  };
  const icon: Record<string, string> = {
    Aberta: '⏳', Paga: '✅', Vencida: '⚠️', Cancelada: '🚫',
  };
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold border ${map[status] ?? ''}`}>
      {icon[status]} {status}
    </span>
  );
}

export function ContasPagarPage() {
  const [modalAberto, setModalAberto] = useState(false);
  const [erroForm, setErroForm] = useState<string | null>(null);
  const [pagandoId, setPagandoId] = useState<number | null>(null);
  const [dataPagamento, setDataPagamento] = useState(new Date().toISOString().slice(0, 10));

  const { data: contas = [], isLoading, isError, error } = useContasPagar(CD_FILIAL);
  const criar = useCriarContaPagar();
  const pagar = usePagarConta();
  const cancelar = useCancelarConta();

  const totalAberto  = contas.filter(c => c.status === 'Aberta').reduce((s, c) => s + c.valor, 0);
  const totalVencido = contas.filter(c => c.status === 'Vencida').reduce((s, c) => s + c.valor, 0);
  const totalPago    = contas.filter(c => c.status === 'Paga').reduce((s, c) => s + c.valor, 0);

  const contasOrdenadas = [...contas].sort((a, b) => {
    const ordem: Record<string, number> = { Vencida: 0, Aberta: 1, Paga: 2, Cancelada: 3 };
    if (ordem[a.status] !== ordem[b.status]) return ordem[a.status] - ordem[b.status];
    return a.vencimento.localeCompare(b.vencimento);
  });

  const handleSalvar = async (dados: { descricao: string; valor: number; vencimento: string }) => {
    setErroForm(null);
    try {
      await criar.mutateAsync({ ...dados, cdFilial: CD_FILIAL });
      setModalAberto(false);
    } catch {
      setErroForm('Erro ao registrar a conta. Verifique os dados e tente novamente.');
    }
  };

  const handlePagar = async (id: number) => {
    try {
      await pagar.mutateAsync({ id, dataPagamento });
      setPagandoId(null);
    } catch {/* toast */}
  };

  const handleCancelar = async (id: number) => {
    if (!confirm('Cancelar esta conta a pagar?')) return;
    try { await cancelar.mutateAsync(id); } catch {/* toast */}
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Contas a Pagar</h1>
            <p className="text-sm text-gray-500 mt-0.5">Controle de obrigacoes -- Filial {CD_FILIAL}</p>
          </div>
          <button
            onClick={() => { setErroForm(null); setModalAberto(true); }}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
          >
            + Nova Conta
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-6">
        {/* Cards */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Em aberto</p>
            <p className="text-2xl font-bold text-blue-600 mt-1">{formatValor(totalAberto)}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Vencidas</p>
            <p className="text-2xl font-bold text-red-500 mt-1">{formatValor(totalVencido)}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Pagas</p>
            <p className="text-2xl font-bold text-emerald-600 mt-1">{formatValor(totalPago)}</p>
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
              Carregando contas...
            </div>
          )}

          {isError && (
            <div className="p-6">
              <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar contas a pagar.'} />
            </div>
          )}

          {!isLoading && !isError && contasOrdenadas.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16 text-gray-400">
              <p className="text-sm font-medium">Nenhuma conta registrada</p>
              <p className="text-xs mt-1">Clique em "Nova Conta" para comecar</p>
            </div>
          )}

          {!isLoading && contasOrdenadas.length > 0 && (
            <table className="w-full text-left">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70">
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Descricao</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-28">Vencimento</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-28">Pagamento</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-28">Status</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-32 text-right">Valor</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-40 text-right">Acoes</th>
                </tr>
              </thead>
              <tbody>
                {contasOrdenadas.map(c => (
                  <tr key={c.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3 text-sm text-gray-800">{c.descricao}</td>
                    <td className="px-4 py-3 text-sm text-gray-600 tabular-nums">{formatDate(c.vencimento)}</td>
                    <td className="px-4 py-3 text-sm text-gray-500 tabular-nums">
                      {c.dataPagamento ? formatDate(c.dataPagamento) : '--'}
                    </td>
                    <td className="px-4 py-3"><StatusBadge status={c.status} /></td>
                    <td className="px-4 py-3 text-sm font-semibold text-right tabular-nums text-gray-800">
                      {formatValor(c.valor)}
                    </td>
                    <td className="px-4 py-3 text-right">
                      {(c.status === 'Aberta' || c.status === 'Vencida') && (
                        <div className="flex items-center justify-end gap-2">
                          {pagandoId === c.id ? (
                            <>
                              <input
                                type="date"
                                value={dataPagamento}
                                onChange={e => setDataPagamento(e.target.value)}
                                className="border border-gray-300 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                              />
                              <button
                                onClick={() => handlePagar(c.id)}
                                className="px-2 py-1 text-xs rounded bg-emerald-500 text-white hover:bg-emerald-600 transition-colors"
                              >
                                Confirmar
                              </button>
                              <button
                                onClick={() => setPagandoId(null)}
                                className="px-2 py-1 text-xs rounded border border-gray-300 hover:bg-gray-50 transition-colors"
                              >
                                X
                              </button>
                            </>
                          ) : (
                            <>
                              <button
                                onClick={() => setPagandoId(c.id)}
                                className="px-2 py-1 text-xs rounded bg-emerald-50 text-emerald-700 border border-emerald-200 hover:bg-emerald-100 transition-colors"
                              >
                                Pagar
                              </button>
                              <button
                                onClick={() => handleCancelar(c.id)}
                                className="px-2 py-1 text-xs rounded bg-red-50 text-red-600 border border-red-200 hover:bg-red-100 transition-colors"
                              >
                                Cancelar
                              </button>
                            </>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </main>

      <Modal titulo="Nova Conta a Pagar" aberto={modalAberto} onFechar={() => setModalAberto(false)}>
        {erroForm && <div className="mb-4"><AlertaErro mensagem={erroForm} /></div>}
        <ContaPagarForm
          onSalvar={handleSalvar}
          onCancelar={() => setModalAberto(false)}
          carregando={criar.isPending}
        />
      </Modal>
    </div>
  );
}
