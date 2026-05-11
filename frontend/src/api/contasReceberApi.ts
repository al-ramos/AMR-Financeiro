import api from './axiosInstance';

export interface ContaReceberDto {
  id: number;
  cdFilial: number;
  descricao: string;
  valor: number;
  vencimento: string;
  dataRecebimento: string | null;
  valorRecebido: number | null;
  documentoOrigem: string | null;
  status: 'Aberta' | 'Recebida' | 'Cancelada' | 'Vencida';
}

export interface CriarContaReceberPayload {
  cdFilial: number;
  descricao: string;
  valor: number;
  vencimento: string;
  documentoOrigem?: string;
}

export const contasReceberApi = {
  getAll: (cdFilial: number) =>
    api.get<ContaReceberDto[]>('/contasreceber', { params: { cdFilial } }).then(r => r.data),

  getById: (id: number) =>
    api.get<ContaReceberDto>(`/contasreceber/${id}`).then(r => r.data),

  criar: (payload: CriarContaReceberPayload) =>
    api.post<{ id: number }>('/contasreceber', payload).then(r => r.data),

  receber: (id: number, dataRecebimento: string, valorRecebido?: number) =>
    api.patch(`/contasreceber/${id}/receber`, { dataRecebimento, valorRecebido }).then(r => r.data),

  cancelar: (id: number) =>
    api.patch(`/contasreceber/${id}/cancelar`).then(r => r.data),
};
