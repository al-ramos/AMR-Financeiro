import { useState } from 'react';
import {
  usePlanoContasArvore,
  usePlanoContasLista,
  useCriarPlanoContas,
  useAtualizarPlanoContas,
  useInativarPlanoContas,
  useAtivarPlanoContas,
} from '../hooks/usePlanoContas';
import type { PlanoContasDto } from '../api/planoContasApi';
import { PlanoContasLinha } from '../components/planoContas/PlanoContasLinha';
import { PlanoContasForm } from '../components/planoContas/PlanoContasForm';
import { Modal } from '../components/ui/Modal';
import { AlertaErro } from '../components/ui/AlertaErro';

const CD_FILIAL = 1;

export function PlanoContasPage() {
  const { data: arvore, isLoading, isError, error } = usePlanoContasArvore();
  const { data: lista = [] } = usePlanoContasLista();

  const criar = useCriarPlanoContas();
  const atualizar = useAtualizarPlanoContas();
  const inativar = useInativarPlanoContas();
  const ativar = useAtivarPlanoContas();

  const [modalAberto, setModalAberto] = useState(false);
  const [contaEditando, setContaEditando] = useState<PlanoContasDto | null>(null);
  const [erroForm, setErroForm] = useState<string | null>(null);

  const abrirNovaConta = () => {
    setContaEditando(null);
    setErroForm(null);
    setModalAberto(true);
  };

  const abrirEditar = (id: number) => {
    const conta = lista.find(c => c.id === id) ?? null;
    setContaEditando(conta);
    setErroForm(null);
    setModalAberto(true);
  };

  const fecharModal = () => {
    setModalAberto(false);
    setContaEditando(null);
    setErroForm(null);
  };

  const handleSalvar = async (dados: {
    codigo: string;
    descricao: string;
    tipo: 'Sintetica' | 'Analitica';
    paiId: number | null;
  }) => {
    setErroForm(null);
    try {
      if (contaEditando) {
        await atualizar.mutateAsync({ id: contaEditando.id, descricao: dados.descricao });
      } else {
        await criar.mutateAsync({ ...dados, cdFilial: CD_FILIAL });
      }
      fecharModal();
    } catch (e: unknown) {
      const msg =
        (e as { response?: { data?: { erro?: string } } })?.response?.data?.erro ??
        'Erro ao salvar a conta. Verifique os dados e tente novamente.';
      setErroForm(msg);
    }
  };

  const handleToggleAtivo = async (id: number, ativo: boolean) => {
    try {
      if (ativo) {
        await inativar.mutateAsync(id);
      } else {
        await ativar.mutateAsync(id);
      }
    } catch {
      // TODO: toast de erro
    }
  };

  const contasAtivas = lista.filter(c => c.ativo).length;
  const contasTotal = lista.length;

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Plano de Contas</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Estrutura hierarquica de contas -- Filial {CD_FILIAL}
            </p>
          </div>
          <button
            onClick={abrirNovaConta}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 active:bg-blue-800 transition-colors shadow-sm"
          >
            + Nova Conta
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-6">
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Total de contas</p>
            <p className="text-2xl font-bold text-gray-900 mt-1">{contasTotal}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Contas ativas</p>
            <p className="text-2xl font-bold text-emerald-600 mt-1">{contasAtivas}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">Inativas</p>
            <p className="text-2xl font-bold text-gray-400 mt-1">{contasTotal - contasAtivas}</p>
          </div>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          {isLoading && (
            <div className="flex items-center justify-center py-16 text-gray-400 text-sm">
              <svg className="animate-spin h-5 w-5 mr-2 text-blue-500" viewBox="0 0 24 24" fill="none">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
              </svg>
              Carregando plano de contas...
            </div>
          )}

          {isError && (
            <div className="p-6">
              <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar o plano de contas.'} />
            </div>
          )}

          {!isLoading && !isError && (!arvore || arvore.length === 0) && (
            <div className="flex flex-col items-center justify-center py-16 text-gray-400">
              <p className="text-sm font-medium">Nenhuma conta cadastrada</p>
              <p className="text-xs mt-1">Clique em "Nova Conta" para comecar</p>
            </div>
          )}

          {!isLoading && arvore && arvore.length > 0 && (
            <table className="w-full text-left">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70">
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-48">Codigo</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Descricao</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-32">Tipo</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-24">Status</th>
                  <th className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide w-32 text-right">Acoes</th>
                </tr>
              </thead>
              <tbody>
                {arvore.map(node => (
                  <PlanoContasLinha
                    key={node.id}
                    node={node}
                    onEditar={abrirEditar}
                    onToggleAtivo={handleToggleAtivo}
                  />
                ))}
              </tbody>
            </table>
          )}
        </div>
      </main>

      <Modal
        titulo={contaEditando ? `Editar: ${contaEditando.codigo} -- ${contaEditando.descricao}` : 'Nova Conta'}
        aberto={modalAberto}
        onFechar={fecharModal}
      >
        {erroForm && (
          <div className="mb-4">
            <AlertaErro mensagem={erroForm} />
          </div>
        )}
        <PlanoContasForm
          contas={lista}
          editando={contaEditando}
          onSalvar={handleSalvar}
          onCancelar={fecharModal}
          carregando={criar.isPending || atualizar.isPending}
        />
      </Modal>
    </div>
  );
}
