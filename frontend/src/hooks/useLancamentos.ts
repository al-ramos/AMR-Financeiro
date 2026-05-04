import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { lancamentosApi, type CriarLancamentoPayload } from '../api/lancamentosApi';

const KEY = 'lancamentos';

export function useLancamentosPorPeriodo(cdFilial: number, inicio: string, fim: string) {
  return useQuery({
    queryKey: [KEY, cdFilial, inicio, fim],
    queryFn: () => lancamentosApi.getByPeriodo(cdFilial, inicio, fim),
    enabled: !!inicio && !!fim,
  });
}

export function useCriarLancamento() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CriarLancamentoPayload) => lancamentosApi.criar(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [KEY] });
    },
  });
}
