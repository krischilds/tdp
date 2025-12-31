import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5201';

// Initialize user from token on store creation
const initUser = () => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  try {
    const parts = token.split('.');
    if (parts.length < 2) return null;
    const payload = JSON.parse(atob(parts[1]!));
    return { userId: payload.sub, email: payload.email };
  } catch {
    return null;
  }
};

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(localStorage.getItem('accessToken'));
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'));
  const user = ref<{ userId: string; email: string } | null>(initUser());

  const isAuthenticated = () => !!accessToken.value;

  const register = async (email: string, password: string, displayName?: string) => {
    const response = await axios.post(`${API_BASE}/auth/register`, {
      email,
      password,
      displayName,
    });
    return response.data;
  };

  const login = async (email: string, password: string) => {
    const response = await axios.post(`${API_BASE}/auth/login`, {
      email,
      password,
      deviceInfo: navigator.userAgent,
    });
    const { data } = response.data;
    accessToken.value = data.accessToken;
    refreshToken.value = data.refreshToken;
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    // Decode token to get user info (simple base64 decode)
    const parts = data.accessToken.split('.');
    if (parts.length >= 2) {
      const payload = JSON.parse(atob(parts[1]));
      user.value = { userId: payload.sub, email: payload.email };
    }
  };

  const refreshAccessToken = async (): Promise<boolean> => {
    if (!refreshToken.value) return false;
    try {
      const response = await axios.post(`${API_BASE}/auth/refresh`, {
        refreshToken: refreshToken.value,
        deviceInfo: navigator.userAgent,
      });
      const { data } = response.data;
      accessToken.value = data.accessToken;
      refreshToken.value = data.refreshToken;
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      return true;
    } catch {
      return false;
    }
  };

  const logout = async () => {
    if (refreshToken.value) {
      try {
        await axios.post(`${API_BASE}/auth/logout`, {
          refreshToken: refreshToken.value,
        }, {
          headers: { Authorization: `Bearer ${accessToken.value}` }
        });
      } catch {
        // Ignore errors on logout
      }
    }
    accessToken.value = null;
    refreshToken.value = null;
    user.value = null;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  };

  const checkAuth = async () => {
    if (accessToken.value) {
      // Decode and check expiry
      try {
        const parts = accessToken.value.split('.');
        if (parts.length < 2) { logout(); return false; }
        const payload = JSON.parse(atob(parts[1]!));
        const exp = payload.exp * 1000; // Convert to milliseconds
        if (Date.now() >= exp) {
          // Token expired, try refresh
          const refreshed = await refreshAccessToken();
          if (!refreshed) {
            logout();
            return false;
          }
        }
        user.value = { userId: payload.sub, email: payload.email };
        return true;
      } catch {
        logout();
        return false;
      }
    }
    return false;
  };

  return {
    accessToken,
    refreshToken,
    user,
    isAuthenticated,
    register,
    login,
    logout,
    refreshAccessToken,
    checkAuth,
  };
});
