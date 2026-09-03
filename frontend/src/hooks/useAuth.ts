import { useContext } from 'react';
import { AuthContext, type AuthContextType } from '../contexts/auth-context';

// Vive fora de AuthContext.tsx porque o react-refresh só preserva estado em
// arquivos que exportam apenas componentes — um hook exportado ao lado do
// AuthProvider derrubava o fast refresh do provider inteiro a cada edição.
export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
