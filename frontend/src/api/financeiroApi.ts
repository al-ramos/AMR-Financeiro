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

export interface ContaDreDto {
  codigo: string;
  descricao: string;
  valor: number;
}

export interface LinhaDreDto {
  grupo: string;
  descricao: string;
  valorAtual: number;
  valorPeriodoAnterior: number;
  valorMesmoMesAnoAnterior: number;
  variacaoMes: number;
  variacaoAno: number;
  negrito: boolean;
  ehSubtotal: boolean;
  contas: ContaDreDto[];
}

export interface DreDto {
  periodo: string;
  linhas: LinhaDreDto[];
  margemBruta: number;
  margemOperacional: number;
  margemLiquida: number;
}

export const financeiroApi = {
  getAging: () =>
    api.get<AgingDto>('/financeiro/aging').then(r => r.data),

  getFluxoCaixa: (horizonteDias: 30 | 60 | 90 = 30) =>
    api.get<FluxoCaixaDto>('/financeiro/fluxo-caixa', { params: { horizonteDias } }).then(r => r.data),

  getDre: (cdFilial: number, ano: number, mes: number) =>
    api.get<DreDto>('/dre', { params: { cdFilial, ano, mes } }).then(r => r.data),
};
