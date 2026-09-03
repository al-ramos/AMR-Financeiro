import api from './axiosInstance';

export interface CentroCustoDto {
  id: number;
  codigo: string;
  descricao: string;
  tipo: string;
  paiId: number | null;
  nivel: number;
  responsavelNome: string;
  ativo: boolean;
}

export interface CriarCentroCustoPayload {
  cdFilial: number;
  codigo: string;
  descricao: string;
  tipo: string;
  paiId: number | null;
  nivel: number;
  responsavelNome: string;
}

export interface OrcamentoPorMesDto {
  mes: number;
  nomeMes: string;
  orcado: number;
  realizado: number;
  percentualConsumido: number;
  emAlerta: boolean;
  estourado: boolean;
}

export interface OrcamentoAnualDto {
  centroCustoId: number;
  ano: number;
  meses: OrcamentoPorMesDto[];
  totalOrcado: number;
  totalRealizado: number;
}

export interface AtualizarOrcamentoPayload {
  centroCustoId: number;
  contaDescricao: string;
  ano: number;
  mes: number;
  valorOrcado: number;
}

export interface AlertaDto {
  centroCustoId: number;
  centroCustoNome: string;
  responsavel: string;
  contaDescricao: string;
  mes: number;
  ano: number;
  orcado: number;
  realizado: number;
  percentualConsumido: number;
  estourado: boolean;
}

export interface ItemDreCCDto {
  contaCodigo: string;
  contaDescricao: string;
  valor: number;
}

export interface RateioRecebidoDto {
  regraRateioId: number;
  regraNome: string;
  competencia: string;
  percentualAplicado: number;
  valor: number;
}

export interface DreCentroCustoDto {
  centroCustoId: number;
  centroCustoCodigo: string;
  centroCustoDescricao: string;
  periodo: string;
  receitas: ItemDreCCDto[];
  totalReceitas: number;
  despesas: ItemDreCCDto[];
  totalDespesas: number;
  rateiosRecebidos: RateioRecebidoDto[];
  totalRateiosRecebidos: number;
  resultado: number;
}

const CD_FILIAL = 1;

export const centrosCustoApi = {
  listar: (cdFilial: number = CD_FILIAL) =>
    api.get<CentroCustoDto[]>('/centros-custo', { params: { cdFilial } }).then(r => r.data),

  criar: (payload: CriarCentroCustoPayload) =>
    api.post<{ id: number }>('/centros-custo', payload).then(r => r.data),

  getOrcamento: (centroCustoId: number, ano: number) =>
    api.get<OrcamentoAnualDto>(`/centros-custo/${centroCustoId}/orcamento`, { params: { ano } })
      .then(r => r.data),

  atualizarOrcamento: (payload: AtualizarOrcamentoPayload) =>
    api.put<{ sucesso: boolean }>('/centros-custo/orcamento', payload).then(r => r.data),

  getAlertas: (cdFilial: number = CD_FILIAL) =>
    api.get<AlertaDto[]>('/centros-custo/alertas', { params: { cdFilial } }).then(r => r.data),

  getDre: (centroCustoId: number, dataInicio: string, dataFim: string) =>
    api.get<DreCentroCustoDto>(`/centros-custo/${centroCustoId}/dre`, {
      params: { dataInicio, dataFim },
    }).then(r => r.data),
};
