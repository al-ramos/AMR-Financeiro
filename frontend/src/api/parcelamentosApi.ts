import api from './axiosInstance';

export type StatusParcela = 'Pendente' | 'Pago' | 'Vencido' | 'Cancelado';
export type TipoVinculo = 'Lancamento' | 'Boleto' | 'Avulso';

export interface ParcelaDto {
  id: number;
  numeroParcela: number;
  valorParcela: number;
  dataVencimento: string;
  dataPagamento: string | null;
  status: StatusParcela;
  contaBancariaId: number | null;
}

export interface ParcelamentoDto {
  id: number;
  descricao: string;
  valorTotal: number;
  numeroParcelas: number;
  tipoVinculo: TipoVinculo;
  vinculoId: number | null;
  createdAt: string;
  parcelas: ParcelaDto[];
  totalPagas: number;
  totalPendentes: number;
}

export interface CriarParcelamentoPayload {
  descricao: string;
  valorTotal: number;
  numeroParcelas: number;
  tipoVinculo: TipoVinculo;
  vinculoId: number | null;
  primeiroVencimento: string;
}

export const parcelamentosApi = {
  getAll: () =>
    api.get<ParcelamentoDto[]>('/parcelamentos').then(r => r.data),

  getById: (id: number) =>
    api.get<ParcelamentoDto>(`/parcelamentos/${id}`).then(r => r.data),

  criar: (payload: CriarParcelamentoPayload) =>
    api.post<{ id: number }>('/parcelamentos', payload).then(r => r.data),

  pagarParcela: (parcelamentoId: number, parcelaId: number, dataPagamento: string, contaBancariaId?: number) =>
    api.patch(`/parcelamentos/${parcelamentoId}/parcelas/${parcelaId}/pagar`, {
      dataPagamento,
      contaBancariaId: contaBancariaId ?? null,
    }),
};
