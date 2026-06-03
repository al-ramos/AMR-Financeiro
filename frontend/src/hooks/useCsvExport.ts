import { useState } from 'react';

const TOKEN_KEY = 'amr_fin_token';
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

/**
 * Hook para download de CSV autenticado via Bearer token.
 * Uso: const { exportar, exportando } = useCsvExport()
 *      exportar('/contaspagar/export?cdFilial=1', 'contas_pagar')
 */
export function useCsvExport() {
  const [exportando, setExportando] = useState(false);

  async function exportar(path: string, nomeArquivo?: string) {
    setExportando(true);
    try {
      const token = sessionStorage.getItem(TOKEN_KEY);
      const res = await fetch(`${BASE_URL}${path}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);

      const blob = await res.blob();
      const url  = URL.createObjectURL(blob);
      const a    = document.createElement('a');

      // Tenta pegar o nome do header Content-Disposition, senão usa o fallback
      const cd   = res.headers.get('Content-Disposition') ?? '';
      const match = cd.match(/filename[^;=\n]*=(["']?)([^"'\n]*)\1/);
      a.download = match?.[2] ?? `${nomeArquivo ?? 'export'}.csv`;
      a.href     = url;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error('Erro ao exportar CSV:', e);
    } finally {
      setExportando(false);
    }
  }

  return { exportar, exportando };
}
