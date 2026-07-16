import api from './axiosInstance';

export interface AgingFaixaDto {
  faixa: string;
  quantidade: number;
  valorTotal: number;
}

export interface AgingDto {
  aVencer: AgingFaixaDto;
  de1a30: AgingFaixaDto;
  de31a60: AgingFaixaDto;
  de61a90: AgingFaixaDto;
  acima90: AgingFaixaDto;
  totalEmAberto: number;
}

export interface FluxoCaixaDiaDto {
  data: string;
  entradas: number;
  saidas: number;
  saldo: number;
}

export interface FluxoCaixaDto {
  horizonteDias: number;
  dias: FluxoCaixaDiaDto[];
  totalEntradas: number;
  totalSaidas: number;
  saldoFinal: number;
}

export const financeiroApi = {
  getAging: () =>
    api.get<AgingDto>('/financeiro/aging').then(r => r.data),

  getFluxoCaixa: (horizonteDias: 30 | 60 | 90 = 30) =>
    api.get<FluxoCaixaDto>('/financeiro/fluxo-caixa', { params: { horizonteDias } }).then(r => r.data),
};
