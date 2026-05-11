import { useEffect } from 'react';

interface Props {
  titulo: string;
  aberto: boolean;
  onFechar: () => void;
  children: React.ReactNode;
}

export function Modal({ titulo, aberto, onFechar, children }: Props) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onFechar(); };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [onFechar]);

  if (!aberto) return null;

  return (
    <div
      style={{
        position: 'fixed', inset: 0, zIndex: 1050,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        background: 'rgba(0,0,0,.45)',
      }}
      onClick={e => { if (e.target === e.currentTarget) onFechar(); }}
    >
      <div className="bg-white rounded-3 shadow" style={{ width: '100%', maxWidth: 520, margin: '0 1rem' }}>
        <div className="d-flex align-items-center justify-content-between px-4 py-3 border-bottom">
          <h6 className="mb-0 fw-semibold" style={{ fontSize: 15 }}>{titulo}</h6>
          <button
            onClick={onFechar}
            className="btn-close btn-sm"
            aria-label="Fechar"
          />
        </div>
        <div className="px-4 py-4">{children}</div>
      </div>
    </div>
  );
}
