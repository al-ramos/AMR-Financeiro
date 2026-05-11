import { useState } from 'react';
import {
  usePlanoContasArvore, usePlanoContasLista, useCriarPlanoContas,
  useAtualizarPlanoContas, useInativarPlanoContas, useAtivarPlanoContas,
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

  const criar    = useCriarPlanoContas();
  const atualizar = useAtualizarPlanoContas();
  const inativar = useInativarPlanoContas();
  const ativar   = useAtivarPlanoContas();

  const [modalAberto, setModalAberto]       = useState(false);
  const [contaEditando, setContaEditando]   = useState<PlanoContasDto | null>(null);
  const [erroForm, setErroForm]             = useState<string | null>(null);

  const abrirNovaConta = () => { setContaEditando(null); setErroForm(null); setModalAberto(true); };
  const abrirEditar = (id: number) => {
    setContaEditando(lista.find(c => c.id === id) ?? null);
    setErroForm(null);
    setModalAberto(true);
  };
  const fecharModal = () => { setModalAberto(false); setContaEditando(null); setErroForm(null); };

  const handleSalvar = async (dados: {
    codigo: string; descricao: string; tipo: 'Sintetica' | 'Analitica'; paiId: number | null;
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
      const msg = (e as { response?: { data?: { erro?: string } } })?.response?.data?.erro
        ?? 'Erro ao salvar a conta.';
      setErroForm(msg);
    }
  };

  const handleToggleAtivo = async (id: number, ativo: boolean) => {
    try { if (ativo) await inativar.mutateAsync(id); else await ativar.mutateAsync(id); }
    catch {/* toast */}
  };

  const contasAtivas = lista.filter(c => c.ativo).length;

  return (
    <>
      <div className="row g-3 mb-4">
        {[
          { label: 'Total de contas', value: lista.length,              color: '#212529' },
          { label: 'Ativas',          value: contasAtivas,              color: '#2e7d32' },
          { label: 'Inativas',        value: lista.length - contasAtivas, color: '#757575' },
        ].map(m => (
          <div key={m.label} className="col-md-4">
            <div className="amr-metric-card">
              <p className="amr-metric-label">{m.label}</p>
              <p className="amr-metric-value" style={{ color: m.color }}>{m.value}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="amr-table-card">
        <div className="d-flex align-items-center justify-content-between px-3 py-3 border-bottom">
          <span style={{ fontSize: 13, fontWeight: 600, color: '#495057' }}>
            <i className="bi bi-diagram-3 me-2"></i>Estrutura · Filial {CD_FILIAL}
          </span>
          <button className="btn btn-sm btn-primary" onClick={abrirNovaConta}>
            <i className="bi bi-plus-lg me-1"></i>Nova Conta
          </button>
        </div>

        {isLoading && (
          <div className="amr-empty">
            <div className="spinner-border spinner-border-sm text-primary mb-2" role="status"></div>
            <span style={{ fontSize: 13 }}>Carregando...</span>
          </div>
        )}

        {isError && (
          <div className="p-3">
            <AlertaErro mensagem={(error as Error)?.message ?? 'Erro ao carregar o plano de contas.'} />
          </div>
        )}

        {!isLoading && !isError && (!arvore || arvore.length === 0) && (
          <div className="amr-empty">
            <i className="bi bi-diagram-3"></i>
            <div style={{ fontSize: 14, fontWeight: 500 }}>Nenhuma conta cadastrada</div>
          </div>
        )}

        {!isLoading && arvore && arvore.length > 0 && (
          <div className="table-responsive">
            <table className="table table-hover table-sm mb-0" style={{ fontSize: 13 }}>
              <thead className="table-light">
                <tr>
                  <th>Código</th>
                  <th>Descrição</th>
                  <th>Tipo</th>
                  <th>Status</th>
                  <th className="text-end">Ações</th>
                </tr>
              </thead>
              <tbody>
                {arvore.map(node => (
                  <PlanoContasLinha key={node.id} node={node} onEditar={abrirEditar} onToggleAtivo={handleToggleAtivo} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal
        titulo={contaEditando ? `Editar: ${contaEditando.codigo} — ${contaEditando.descricao}` : 'Nova Conta'}
        aberto={modalAberto}
        onFechar={fecharModal}
      >
        {erroForm && <div className="mb-3"><AlertaErro mensagem={erroForm} /></div>}
        <PlanoContasForm
          contas={lista}
          editando={contaEditando}
          onSalvar={handleSalvar}
          onCancelar={fecharModal}
          carregando={criar.isPending || atualizar.isPending}
        />
      </Modal>
    </>
  );
}
