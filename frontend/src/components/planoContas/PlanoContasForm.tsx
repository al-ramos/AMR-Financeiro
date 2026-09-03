import { useEffect, useState } from 'react';
import type {
  PlanoContasDto, TipoContaContabil, NaturezaConta, GrupoDRE,
} from '../../api/planoContasApi';

// A conta patrimonial existe no razão e não entra em linha nenhuma da DRE —
// é onde as baixas de Contas a Pagar e a Receber caem.
const TIPOS: { valor: TipoContaContabil; rotulo: string; patrimonial: boolean }[] = [
  { valor: 'Ativo',          rotulo: 'Ativo',            patrimonial: true  },
  { valor: 'Passivo',        rotulo: 'Passivo',          patrimonial: true  },
  { valor: 'Receita',        rotulo: 'Receita',          patrimonial: false },
  { valor: 'Custo',          rotulo: 'Custo',            patrimonial: false },
  { valor: 'Despesa',        rotulo: 'Despesa',          patrimonial: false },
  { valor: 'Imposto',        rotulo: 'Imposto',          patrimonial: false },
  { valor: 'OutrasReceitas', rotulo: 'Outras receitas',  patrimonial: false },
  { valor: 'OutrasDespesas', rotulo: 'Outras despesas',  patrimonial: false },
];

const GRUPOS_DRE: { valor: GrupoDRE; rotulo: string }[] = [
  { valor: 'ReceitaBruta',         rotulo: 'Receita bruta' },
  { valor: 'DeducoesReceita',      rotulo: 'Deduções da receita' },
  { valor: 'CustoMercadorias',     rotulo: 'Custo das mercadorias' },
  { valor: 'DespesasOperacionais', rotulo: 'Despesas operacionais' },
  { valor: 'ReceitasFinanceiras',  rotulo: 'Receitas financeiras' },
  { valor: 'DespesasFinanceiras',  rotulo: 'Despesas financeiras' },
  { valor: 'ImpostosRenda',        rotulo: 'IRPJ / CSLL' },
];

/** O nível vem do código: 3.1.1 é nível 3. */
const nivelDoCodigo = (codigo: string) => codigo.split('.').filter(Boolean).length;

interface Props {
  contas: PlanoContasDto[];
  editando?: PlanoContasDto | null;
  onSalvar: (dados: {
    codigo: string; descricao: string; tipo: TipoContaContabil; natureza: NaturezaConta;
    nivel: number; paiId: number | null; grupoDre: GrupoDRE; ordemExibicao: number;
    aceitaLancamentos: boolean;
  }) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function PlanoContasForm({ contas, editando, onSalvar, onCancelar, carregando }: Props) {
  const [codigo, setCodigo]   = useState('');
  const [descricao, setDescricao] = useState('');
  const [tipo, setTipo]       = useState<TipoContaContabil>('Despesa');
  const [natureza, setNatureza] = useState<NaturezaConta>('Devedora');
  const [grupoDre, setGrupoDre] = useState<GrupoDRE>('DespesasOperacionais');
  const [aceitaLancamentos, setAceitaLancamentos] = useState(true);
  const [paiId, setPaiId]     = useState<number | null>(null);

  const patrimonial = TIPOS.find(t => t.valor === tipo)?.patrimonial ?? false;

  useEffect(() => {
    if (editando) {
      setCodigo(editando.codigo); setDescricao(editando.descricao);
      setTipo(editando.tipo); setNatureza(editando.natureza);
      setGrupoDre(editando.grupoDRE); setAceitaLancamentos(editando.aceitaLancamentos);
      setPaiId(editando.paiId);
    } else {
      setCodigo(''); setDescricao(''); setTipo('Despesa'); setNatureza('Devedora');
      setGrupoDre('DespesasOperacionais'); setAceitaLancamentos(true); setPaiId(null);
    }
  }, [editando]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!codigo.trim() || !descricao.trim()) return;
    onSalvar({
      codigo: codigo.trim(),
      descricao: descricao.trim(),
      tipo,
      natureza,
      nivel: nivelDoCodigo(codigo.trim()),
      paiId,
      // Conta patrimonial não compõe DRE — o grupo é fixado, não escolhido.
      grupoDre: patrimonial ? 'NaoAplicavel' : grupoDre,
      ordemExibicao: 0,
      aceitaLancamentos,
    });
  };

  // Só uma conta agrupadora pode ser pai.
  const sinteticas = contas.filter(c => !c.aceitaLancamentos && c.ativo);

  return (
    <form onSubmit={handleSubmit}>
      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Código *</label>
        <input
          type="text"
          value={codigo}
          onChange={e => setCodigo(e.target.value)}
          disabled={!!editando}
          placeholder="Ex: 1.1.01"
          className="form-control form-control-sm"
          required
        />
        {editando && <div className="form-text">O código não pode ser alterado.</div>}
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Descrição *</label>
        <input
          type="text"
          value={descricao}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Caixa e Equivalentes"
          className="form-control form-control-sm"
          required
        />
      </div>

      <div className="row g-3 mb-3">
        <div className="col-sm-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Tipo</label>
          <select
            value={tipo}
            onChange={e => setTipo(e.target.value as TipoContaContabil)}
            disabled={!!editando}
            className="form-select form-select-sm"
          >
            {TIPOS.map(t => <option key={t.valor} value={t.valor}>{t.rotulo}</option>)}
          </select>
        </div>
        <div className="col-sm-6">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Natureza</label>
          <select
            value={natureza}
            onChange={e => setNatureza(e.target.value as NaturezaConta)}
            disabled={!!editando}
            className="form-select form-select-sm"
          >
            <option value="Devedora">Devedora</option>
            <option value="Credora">Credora</option>
          </select>
        </div>
      </div>

      <div className="mb-3">
        <label className="form-label fw-medium" style={{ fontSize: 13 }}>Grupo na DRE</label>
        <select
          value={patrimonial ? 'NaoAplicavel' : grupoDre}
          onChange={e => setGrupoDre(e.target.value as GrupoDRE)}
          disabled={patrimonial}
          className="form-select form-select-sm"
        >
          {patrimonial
            ? <option value="NaoAplicavel">Não compõe a DRE</option>
            : GRUPOS_DRE.map(g => <option key={g.valor} value={g.valor}>{g.rotulo}</option>)}
        </select>
        {patrimonial && (
          <div className="form-text" style={{ fontSize: 12 }}>
            Conta patrimonial: aparece no razão, não entra em linha da DRE.
          </div>
        )}
      </div>

      <div className="form-check mb-3">
        <input
          id="aceitaLancamentos"
          type="checkbox"
          className="form-check-input"
          checked={aceitaLancamentos}
          onChange={e => setAceitaLancamentos(e.target.checked)}
        />
        <label className="form-check-label" htmlFor="aceitaLancamentos" style={{ fontSize: 13 }}>
          Analítica — aceita lançamento direto
        </label>
        <div className="form-text" style={{ fontSize: 12 }}>
          Desmarque para uma conta que só agrupa outras.
        </div>
      </div>

      {!editando && (
        <div className="mb-3">
          <label className="form-label fw-medium" style={{ fontSize: 13 }}>Conta Pai</label>
          <select
            value={paiId ?? ''}
            onChange={e => setPaiId(e.target.value ? Number(e.target.value) : null)}
            className="form-select form-select-sm"
          >
            <option value="">— Raiz (sem pai) —</option>
            {sinteticas.map(c => (
              <option key={c.id} value={c.id}>{c.codigo} — {c.descricao}</option>
            ))}
          </select>
        </div>
      )}

      <div className="d-flex justify-content-end gap-2 mt-4">
        <button type="button" onClick={onCancelar} className="btn btn-sm btn-outline-secondary">
          Cancelar
        </button>
        <button type="submit" disabled={carregando} className="btn btn-sm btn-primary">
          {carregando ? <><span className="spinner-border spinner-border-sm me-1"></span>Salvando...</> : editando ? 'Atualizar' : 'Criar conta'}
        </button>
      </div>
    </form>
  );
}
