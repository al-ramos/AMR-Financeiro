import api from './axiosInstance';

export interface LancamentoFinanceiroDto {
  id: number;
  cdFilial: number;
  planoContasId: number;
  planoContasCodigo: string;
  planoContasDescricao: string;
  tipo: 'Debito' | 'Credito';
  origem: 'Manual' | 'ContaPagar' | 'ContaReceber';
  valor: number;
  dataLancamento: string;
  historico: string;
  documentoOrigemId: number | null;
  criadoEm: string;
}

export interface CriarLancamentoPayload {
  cdFilial: number;
  planoContasId: number;
  tipo: 'Debito' | 'Credito';
  valor: number;
  dataLancamento: string;
  historico: string;
  documentoOrigemId?: number | null;
}

export const lancamentosApi = {
  getAll: (cdFilial: number) =>
    api
      .get<LancamentoFinanceiroDto[]>('/lancamentos', { params: { cdFilial } })
      .then((r: { data: LancamentoFinanceiroDto[] }) => r.data),

  getByPeriodo: (cdFilial: number, inicio: string, fim: string) =>
    api
      .get<LancamentoFinanceiroDto[]>('/lancamentos/periodo', {
        params: { cdFilial, inicio, fim },
      })
      .then((r: { data: LancamentoFinanceiroDto[] }) => r.data),

  getById: (id: number) =>
    api
      .get<LancamentoFinanceiroDto>(`/lancamentos/${id}`)
      .then((r: { data: LancamentoFinanceiroDto }) => r.data),

  criar: (payload: CriarLancamentoPayload) =>
    api
      .post<{ id: number }>('/lancamentos', payload)
      .then((r: { data: { id: number } }) => r.data),
};
