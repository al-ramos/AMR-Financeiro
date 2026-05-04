import { useState } from 'react';
import type { CriarContaPagarPayload } from '../../api/contasPagarApi';

interface Props {
  onSalvar: (dados: Omit<CriarContaPagarPayload, 'cdFilial'>) => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export function ContaPagarForm({ onSalvar, onCancelar, carregando }: Props) {
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [vencimento, setVencimento] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!descricao.trim() || !valor || !vencimento) return;
    const valorNum = parseFloat(valor.replace(',', '.'));
    if (isNaN(valorNum) || valorNum <= 0) return;
    onSalvar({ descricao: descricao.trim(), valor: valorNum, vencimento });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Descricao *</label>
        <input
          type="text"
          value={descricao}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Ex: Aluguel maio/2026"
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
          maxLength={200}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
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
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Vencimento *</label>
          <input
            type="date"
            value={vencimento}
            onChange={e => setVencimento(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
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
          {carregando ? 'Salvando...' : 'Registrar conta'}
        </button>
      </div>
    </form>
  );
}
