import { createContext } from 'react';
import type { LoginRequest } from '../api/authApi';

// O contexto e seus tipos vivem num modulo sem componentes. A regra
// react-refresh/only-export-components exige que um arquivo de componente
// exporte apenas componentes: exportar o objeto de contexto ao lado do
// AuthProvider derrubava o fast refresh do provider a cada edicao.
export interface AuthState {
  token: string | null;
  username: string | null;
  role: string | null;
}

export interface AuthContextType extends AuthState {
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | null>(null);
