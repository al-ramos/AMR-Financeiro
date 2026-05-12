import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError]   = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login({ username, password });
      navigate('/', { replace: true });
    } catch {
      setError('Usuário ou senha inválidos.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{
      minHeight: '100vh',
      background: 'var(--amr-bg, #f0f2f5)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
    }}>
      <div style={{
        background: '#fff',
        borderRadius: 12,
        boxShadow: '0 4px 24px rgba(0,0,0,.08)',
        padding: '2.5rem 2rem',
        width: '100%',
        maxWidth: 380,
      }}>
        {/* Cabeçalho */}
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <div style={{
            display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
            width: 56, height: 56, borderRadius: '50%',
            background: 'linear-gradient(135deg, #1565c0, #0d47a1)',
            marginBottom: '1rem',
          }}>
            <i className="bi bi-shield-lock-fill" style={{ fontSize: 24, color: '#fff' }}></i>
          </div>
          <h4 style={{ fontWeight: 700, color: '#1a237e', marginBottom: 4 }}>AMR Financeiro</h4>
          <p style={{ fontSize: 13, color: '#78909c', margin: 0 }}>Acesso ao sistema</p>
        </div>

        {/* Formulário */}
        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: '#546e7a', display: 'block', marginBottom: 4 }}>
              USUÁRIO
            </label>
            <div style={{ position: 'relative' }}>
              <i className="bi bi-person" style={{
                position: 'absolute', left: 10, top: '50%', transform: 'translateY(-50%)',
                color: '#90a4ae', fontSize: 15,
              }}></i>
              <input
                type="text"
                value={username}
                onChange={e => setUsername(e.target.value)}
                placeholder="seu usuário"
                required
                autoFocus
                style={{
                  width: '100%', padding: '0.6rem 0.75rem 0.6rem 2rem',
                  border: '1px solid #cfd8dc', borderRadius: 8,
                  fontSize: 14, outline: 'none', boxSizing: 'border-box',
                }}
              />
            </div>
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: '#546e7a', display: 'block', marginBottom: 4 }}>
              SENHA
            </label>
            <div style={{ position: 'relative' }}>
              <i className="bi bi-lock" style={{
                position: 'absolute', left: 10, top: '50%', transform: 'translateY(-50%)',
                color: '#90a4ae', fontSize: 15,
              }}></i>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                placeholder="••••••••"
                required
                style={{
                  width: '100%', padding: '0.6rem 0.75rem 0.6rem 2rem',
                  border: '1px solid #cfd8dc', borderRadius: 8,
                  fontSize: 14, outline: 'none', boxSizing: 'border-box',
                }}
              />
            </div>
          </div>

          {error && (
            <div style={{
              background: '#ffebee', border: '1px solid #ef9a9a', borderRadius: 8,
              padding: '0.5rem 0.75rem', fontSize: 13, color: '#c62828',
              marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: 6,
            }}>
              <i className="bi bi-exclamation-circle"></i> {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            style={{
              width: '100%', padding: '0.7rem',
              background: loading ? '#90a4ae' : 'linear-gradient(135deg, #1565c0, #0d47a1)',
              color: '#fff', border: 'none', borderRadius: 8,
              fontSize: 14, fontWeight: 600, cursor: loading ? 'not-allowed' : 'pointer',
              transition: 'opacity .2s',
            }}
          >
            {loading
              ? <><i className="bi bi-hourglass-split" style={{ marginRight: 6 }}></i>Entrando...</>
              : <><i className="bi bi-box-arrow-in-right" style={{ marginRight: 6 }}></i>Entrar</>
            }
          </button>
        </form>

        <p style={{ fontSize: 11, color: '#b0bec5', textAlign: 'center', marginTop: '1.5rem', marginBottom: 0 }}>
          AMR Ecosystem · Sprint 3
        </p>
      </div>
    </div>
  );
}
