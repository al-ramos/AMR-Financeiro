import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contasReceberApi, type CriarContaReceberPayload } from '../api/contasReceberApi';

const KEY = 'contasReceber';

export function useContasReceber(cdFilial: number) {
  return useQuery({
    queryKey: [KEY, cdFilial],
    queryFn: () => contasReceberApi.getAll(cdFilial),
  });
}

export function useCriarContaReceber() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarContaReceberPayload) => contasReceberApi.criar(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useReceberConta() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dataRecebimento, valorRecebido }: { id: number; dataRecebimento: string; valorRecebido?: number }) =>
      contasReceberApi.receber(id, dataRecebimento, valorRecebido),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useCancelarContaReceber() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => contasReceberApi.cancelar(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}
