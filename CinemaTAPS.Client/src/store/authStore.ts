import { create } from 'zustand';

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  role: string;
  concurrencyToken: string;
}

export interface AuthStore {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isInitialized: boolean; //true: /auth/me check completed

  setAuth: (user: User, token: string) => void;
  setUser: (user: User) => void;
  clearAuth: () => void;
  hydrate: () => void;
  setInitialized: () => void;
}

export const useAuthStore = create<AuthStore>((set) => ({
  user: null,
  token: localStorage.getItem('token'),
  isAuthenticated: !!localStorage.getItem('token'),
  isAdmin: false,
  isInitialized: false,

  setAuth: (user, token) => {
    localStorage.setItem('token', token);
    set({
      user,
      token,
      isAuthenticated: true,
      isAdmin: user.role === 'Admin',
      isInitialized: true,
    });
  },

  setUser: (user) => {
    set({
      user,
      isAdmin: user.role === 'Admin',
    });
  },

  clearAuth: () => {
    localStorage.removeItem('token');
    set({
      user: null,
      token: null,
      isAuthenticated: false,
      isAdmin: false,
      isInitialized: true,
    });
  },

  hydrate: () => {
    const token = localStorage.getItem('token');
    set({
      token,
      isAuthenticated: !!token,
    });
  },

  setInitialized: () => set({ isInitialized: true }),
}));
