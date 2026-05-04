import { useEffect } from 'react';

interface Props {
  titulo: string;
  aberto: boolean;
  onFechar: () => void;
  children: React.ReactNode;
}

export function Modal({ titulo, aberto, onFechar, children }: Props) {
  // Fecha com Esc
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onFechar();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [onFechar]);

  if (!aberto) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* overlay */}
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm"
        onClick={onFechar}
      />

      {/* painel */}
      <div className="relative z-10 w-full max-w-lg bg-white rounded-2xl shadow-xl mx-4">
        {/* cabeçalho */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-800">{titulo}</h2>
          <button
            onClick={onFechar}
            className="w-7 h-7 flex items-center justify-center rounded-full text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
            aria-label="Fechar"
          >
            ✕
          </button>
        </div>

        {/* conteúdo */}
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  );
}
