import api from './axiosInstance';

export interface PlanoContasDto {
  id: number;
  cdFilial: number;
  codigo: string;
  descricao: string;
  tipo: TipoContaContabil;
  natureza: NaturezaConta;
  /** Na leitura a API expõe `grupoDRE`; no POST o campo é `grupoDre`. */
  grupoDRE: GrupoDRE;
  ordemExibicao: number;
  paiId: number | null;
  paiDescricao: string | null;
  nivel: number;
  ativo: boolean;
  aceitaLancamentos: boolean;
}

export interface PlanoContasArvoreDto {
  id: number;
  cdFilial: number;
  codigo: string;
  descricao: string;
  tipo: TipoContaContabil;
  natureza: NaturezaConta;
  grupoDRE: GrupoDRE;
  paiId: number | null;
  nivel: number;
  ativo: boolean;
  aceitaLancamentos: boolean;
  filhos: PlanoContasArvoreDto[];
}

export type TipoContaContabil =
  | 'Ativo' | 'Passivo'
  | 'Receita' | 'Custo' | 'Despesa' | 'Imposto' | 'OutrasReceitas' | 'OutrasDespesas';

export type NaturezaConta = 'Devedora' | 'Credora';

export type GrupoDRE =
  | 'NaoAplicavel' | 'ReceitaBruta' | 'DeducoesReceita' | 'ReceitaLiquida'
  | 'CustoMercadorias' | 'LucroBruto' | 'DespesasOperacionais' | 'ResultadoOperacional'
  | 'ReceitasFinanceiras' | 'DespesasFinanceiras' | 'ResultadoFinanceiro'
  | 'ResultadoAntesIR' | 'ImpostosRenda' | 'LucroLiquido';

export interface CriarPlanoContasPayload {
  cdFilial: number;
  codigo: string;
  descricao: string;
  tipo: TipoContaContabil;
  natureza: NaturezaConta;
  nivel: number;
  paiId: number | null;
  grupoDre: GrupoDRE;
  ordemExibicao: number;
  /** Conta analítica: aceita lançamento direto. */
  aceitaLancamentos: boolean;
}

// GET /api/planocontas/arvore?cdFilial=1
export const getArvore = (cdFilial: number) =>
  api.get<PlanoContasArvoreDto[]>('/planocontas/arvore', { params: { cdFilial } }).then(r => r.data);

// GET /api/planocontas?cdFilial=1
export const getAll = (cdFilial: number) =>
  api.get<PlanoContasDto[]>('/planocontas', { params: { cdFilial } }).then(r => r.data);

// GET /api/planocontas/:id
export const getById = (id: number) =>
  api.get<PlanoContasDto>(`/planocontas/${id}`).then(r => r.data);

// POST /api/planocontas
export const criar = (payload: CriarPlanoContasPayload) =>
  api.post<{ id: number }>('/planocontas', payload).then(r => r.data);

// PUT /api/planocontas/:id
export const atualizarDescricao = (id: number, descricao: string) =>
  api.put(`/planocontas/${id}`, { descricao });

// PATCH /api/planocontas/:id/inativar
export const inativar = (id: number) =>
  api.patch(`/planocontas/${id}/inativar`);

// PATCH /api/planocontas/:id/ativar
export const ativar = (id: number) =>
  api.patch(`/planocontas/${id}/ativar`);
