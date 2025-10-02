import { defineStore } from 'pinia';

type AuthUser = {
  id: number;
  usuario: string;
  nombre: string;
};

interface AuthState {
  user: AuthUser | null;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null
  }),
  actions: {
    setUser(user: AuthUser) {
      this.user = user;
    },
    logout() {
      this.user = null;
    }
  }
});
