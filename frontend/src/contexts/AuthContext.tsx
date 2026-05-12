import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import { login as apiLogin, type LoginRequest, type TokenResponse } from '../api/authApi';

interface AuthState {
  token: string | null;
  username: string | null;
  role: string | null;
}

interface AuthContextType extends AuthState {
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

const TOKEN_KEY = 'amr_fin_token';
const USER_KEY  = 'amr_fin_user';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>(() => ({
    token:    sessionStorage.getItem(TOKEN_KEY),
    username: sessionStorage.getItem(USER_KEY),
    role:     null,
  }));

  const login = useCallback(async (data: LoginRequest) => {
    const res: TokenResponse = await apiLogin(data);
    sessionStorage.setItem(TOKEN_KEY, res.token);
    sessionStorage.setItem(USER_KEY, res.username);
    setAuth({ token: res.token, username: res.username, role: res.role });
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    setAuth({ token: null, username: null, role: null });
  }, []);

  return (
    <AuthContext.Provider value={{
      ...auth,
      isAuthenticated: !!auth.token,
      login,
      logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
