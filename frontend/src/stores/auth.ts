import { defineStore } from 'pinia';

type AuthUser = {
  id: number;
  correo: string;
  nombreCompleto: string;
  medicoId: number | null;
};

type AuthSession = {
  token: string;
  expiracion: string;
  usuario: AuthUser;
};

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  expiracion: string | null;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: null,
    expiracion: null
  }),
  actions: {
    setSession(session: AuthSession) {
      this.user = session.usuario;
      this.token = session.token;
      this.expiracion = session.expiracion;
    },
    logout() {
      this.user = null;
      this.token = null;
      this.expiracion = null;
    }
  }
});
