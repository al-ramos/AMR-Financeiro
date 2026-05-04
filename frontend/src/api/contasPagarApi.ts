import api from './axiosInstance';

export interface ContaPagarDto {
  id: number;
  cdFilial: number;
  descricao: string;
  valor: number;
  vencimento: string;
  dataPagamento: string | null;
  status: 'Aberta' | 'Paga' | 'Cancelada' | 'Vencida';
}

export interface CriarContaPagarPayload {
  cdFilial: number;
  descricao: string;
  valor: number;
  vencimento: string;
}

export const contasPagarApi = {
  getAll: (cdFilial: number) =>
    api.get<ContaPagarDto[]>('/contaspagar', { params: { cdFilial } }).then(r => r.data),

  getById: (id: number) =>
    api.get<ContaPagarDto>(`/contaspagar/${id}`).then(r => r.data),

  criar: (payload: CriarContaPagarPayload) =>
    api.post<{ id: number }>('/contaspagar', payload).then(r => r.data),

  pagar: (id: number, dataPagamento: string) =>
    api.patch(`/contaspagar/${id}/pagar`, { dataPagamento }).then(r => r.data),

  cancelar: (id: number) =>
    api.patch(`/contaspagar/${id}/cancelar`).then(r => r.data),
};
