import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contasPagarApi, type CriarContaPagarPayload } from '../api/contasPagarApi';

const KEY = 'contasPagar';

export function useContasPagar(cdFilial: number) {
  return useQuery({
    queryKey: [KEY, cdFilial],
    queryFn: () => contasPagarApi.getAll(cdFilial),
  });
}

export function useCriarContaPagar() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarContaPagarPayload) => contasPagarApi.criar(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function usePagarConta() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dataPagamento }: { id: number; dataPagamento: string }) =>
      contasPagarApi.pagar(id, dataPagamento),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useCancelarConta() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => contasPagarApi.cancelar(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}
