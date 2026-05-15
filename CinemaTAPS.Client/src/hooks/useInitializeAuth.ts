import { useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { apiClient } from '../api/apiClient';

export const useInitializeAuth = () => {
  const { hydrate, setAuth, setInitialized } = useAuthStore();

  useEffect(() => {
    hydrate();

    const token = useAuthStore.getState().token;
    if (token) {
      apiClient
        .get('/auth/me')
        .then((user: any) => {
          setAuth(user, token);
        })
        .catch(() => {
          useAuthStore.getState().clearAuth();
        });
    } else {
      // No token — user is anonymous, mark as initialized
      setInitialized();
    }
  }, [hydrate, setAuth, setInitialized]);
};
