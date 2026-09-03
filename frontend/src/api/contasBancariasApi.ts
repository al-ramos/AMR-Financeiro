import api from './axiosInstance';

export type TipoContaBancaria = 'ContaCorrente' | 'Poupanca' | 'Investimento';

export interface ContaBancariaDto {
  id: number;
  nome: string;
  banco: string;
  agencia: string;
  conta: string;
  tipoConta: TipoContaBancaria;
  ativa: boolean;
  saldoInicial: number;
  dataSaldoInicial: string;
  saldoAtual: number;
}

export interface ExtratoItemDto {
  id: number;
  dataLancamento: string;
  historico: string;
  tipo: 'Debito' | 'Credito';
  valor: number;
  planoContas: string;
}

export interface CriarContaBancariaPayload {
  nome: string;
  banco: string;
  agencia: string;
  conta: string;
  tipoConta: TipoContaBancaria;
  saldoInicial: number;
  dataSaldoInicial: string;
}

export const contasBancariasApi = {
  getAll: (incluirInativas = false) =>
    api.get<ContaBancariaDto[]>('/contas-bancarias', { params: { incluirInativas } }).then(r => r.data),

  getById: (id: number) =>
    api.get<ContaBancariaDto>(`/contas-bancarias/${id}`).then(r => r.data),

  getExtrato: (id: number) =>
    api.get<ExtratoItemDto[]>(`/contas-bancarias/${id}/extrato`).then(r => r.data),

  criar: (payload: CriarContaBancariaPayload) =>
    api.post<{ id: number }>('/contas-bancarias', payload).then(r => r.data),

  atualizar: (id: number, payload: CriarContaBancariaPayload) =>
    api.put(`/contas-bancarias/${id}`, { id, ...payload }),

  desativar: (id: number) =>
    api.delete(`/contas-bancarias/${id}`),
};
