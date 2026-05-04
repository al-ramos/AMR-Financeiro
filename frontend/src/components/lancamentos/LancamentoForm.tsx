import { useState } from 'react';
import type { PlanoContasDto } from '../../api/planoContasApi';
import type { CriarLancamentoPayload } from '../../api/lancamentosApi';

interface Props {
  contas: PlanoContasDto[];
  onSalvar: (dados: Omit<CriarLancamentoPayload, 'cdFilial'>) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function LancamentoForm({ contas, onSalvar, onCancelar, carregando }: Props) {
  const today = new Date().toISOString().slice(0, 10);

  const [planoContasId, setPlanoContasId] = useState<number | ''>('');
  const [tipo, setTipo] = useState<'Debito' | 'Credito'>('Debito');
  const [valor, setValor] = useState('');
  const [dataLancamento, setDataLancamento] = useState(today);
  const [historico, setHistorico] = useState('');

  const analiticas = contas.filter(c => c.tipo === 'Analitica' && c.ativo);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!planoContasId || !valor || !dataLancamento || !historico.trim()) return;
    const valorNum = parseFloat(valor.replace(',', '.'));
    if (isNaN(valorNum) || valorNum <= 0) return;
    onSalvar({
      planoContasId: Number(planoContasId),
      tipo,
      valor: valorNum,
      dataLancamento,
      historico: historico.trim(),
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Conta *
        </label>
        <select
          value={planoContasId}
          onChange={e => setPlanoContasId(e.target.value ? Number(e.target.value) : '')}
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        >
          <option value="">-- Selecione uma conta --</option>
          {analiticas.map(c => (
            <option key={c.id} value={c.id}>
              {c.codigo} -- {c.descricao}
            </option>
          ))}
        </select>
        <p className="mt-1 text-xs text-gray-400">Apenas contas analiticas aceitam lancamentos.</p>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Tipo *</label>
          <div className="flex rounded-lg overflow-hidden border border-gray-300">
            <button
              type="button"
              onClick={() => setTipo('Debito')}
              className={`flex-1 py-2 text-sm font-medium transition-colors ${
                tipo === 'Debito'
                  ? 'bg-red-500 text-white'
                  : 'bg-white text-gray-600 hover:bg-gray-50'
              }`}
            >
              Debito (-)
            </button>
            <button
              type="button"
              onClick={() => setTipo('Credito')}
              className={`flex-1 py-2 text-sm font-medium transition-colors ${
                tipo === 'Credito'
                  ? 'bg-emerald-500 text-white'
                  : 'bg-white text-gray-600 hover:bg-gray-50'
              }`}
            >
              Credito (+)
            </button>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Valor *</label>
          <input
            type="number"
            step="0.01"
            min="0.01"
            value={valor}
            onChange={e => setValor(e.target.value)}
            placeholder="0,00"
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Data *</label>
        <input
          type="date"
          value={dataLancamento}
          onChange={e => setDataLancamento(e.target.value)}
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Historico *</label>
        <input
          type="text"
          value={historico}
          onChange={e => setHistorico(e.target.value)}
          placeholder="Ex: Pagamento fornecedor"
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
          maxLength={200}
        />
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <button
          type="button"
          onClick={onCancelar}
          className="px-4 py-2 text-sm rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={carregando}
          className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {carregando ? 'Salvando...' : 'Registrar lancamento'}
        </button>
      </div>
    </form>
  );
}
