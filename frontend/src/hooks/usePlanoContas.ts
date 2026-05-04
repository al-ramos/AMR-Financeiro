import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from '../api/planoContasApi';
import type { CriarPlanoContasPayload } from '../api/planoContasApi';

const CD_FILIAL = 1; // TODO: tornar configurável quando houver autenticação

export const KEYS = {
  arvore: (cdFilial: number) => ['planoContas', 'arvore', cdFilial] as const,
  lista: (cdFilial: number) => ['planoContas', 'lista', cdFilial] as const,
};

export function usePlanoContasArvore() {
  return useQuery({
    queryKey: KEYS.arvore(CD_FILIAL),
    queryFn: () => api.getArvore(CD_FILIAL),
    staleTime: 1000 * 60, // 1 min
  });
}

export function usePlanoContasLista() {
  return useQuery({
    queryKey: KEYS.lista(CD_FILIAL),
    queryFn: () => api.getAll(CD_FILIAL),
    staleTime: 1000 * 60,
  });
}

export function useCriarPlanoContas() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarPlanoContasPayload) => api.criar(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['planoContas'] });
    },
  });
}

export function useAtualizarPlanoContas() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, descricao }: { id: number; descricao: string }) =>
      api.atualizarDescricao(id, descricao),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['planoContas'] });
    },
  });
}

export function useInativarPlanoContas() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.inativar(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['planoContas'] });
    },
  });
}

export function useAtivarPlanoContas() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => api.ativar(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['planoContas'] });
    },
  });
}
