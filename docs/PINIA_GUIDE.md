# Pinia State Management Guide

Pinia is the official state management library for Vue 3. It provides a simple, type-safe way to manage shared state across your application.

## Why Pinia?

- **Type-safe** - Full TypeScript support out of the box
- **Devtools support** - Integrates with Vue DevTools
- **Modular** - Create multiple stores, no nested modules
- **Lightweight** - ~1KB bundle size
- **No mutations** - Simpler API than Vuex

## Core Concepts

### 1. Store

A store is a reactive object that holds state, getters, and actions.

```typescript
// stores/counter.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useCounterStore = defineStore('counter', () => {
  // State
  const count = ref(0);

  // Getters (computed)
  const doubleCount = computed(() => count.value * 2);

  // Actions
  function increment() {
    count.value++;
  }

  function decrement() {
    count.value--;
  }

  return { count, doubleCount, increment, decrement };
});
```

### 2. Using a Store in Components

```vue
<template>
  <div>
    <p>Count: {{ counter.count }}</p>
    <p>Double: {{ counter.doubleCount }}</p>
    <button @click="counter.increment()">+</button>
    <button @click="counter.decrement()">-</button>
  </div>
</template>

<script setup lang="ts">
import { useCounterStore } from '../stores/counter';

const counter = useCounterStore();
</script>
```

## Real Example: Auth Store

This project uses Pinia for authentication state. Here's how it works:

```typescript
// stores/auth.ts
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export const useAuthStore = defineStore('auth', () => {
  // State
  const accessToken = ref<string | null>(localStorage.getItem('accessToken'));
  const user = ref<{ userId: string; email: string } | null>(null);

  // Getter-like function
  const isAuthenticated = () => !!accessToken.value;

  // Actions
  const login = async (email: string, password: string) => {
    const response = await axios.post('/auth/login', { email, password });
    accessToken.value = response.data.data.accessToken;
    localStorage.setItem('accessToken', accessToken.value!);
  };

  const logout = () => {
    accessToken.value = null;
    user.value = null;
    localStorage.removeItem('accessToken');
  };

  return { accessToken, user, isAuthenticated, login, logout };
});
```

### Using Auth Store in a Component

```vue
<template>
  <div v-if="authStore.isAuthenticated()">
    <p>Welcome, {{ authStore.user?.email }}</p>
    <button @click="handleLogout">Logout</button>
  </div>
  <div v-else>
    <router-link to="/login">Login</router-link>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '../stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const handleLogout = async () => {
  await authStore.logout();
  router.push('/login');
};
</script>
```

## Common Patterns

### 1. Async Actions with Loading State

```typescript
export const useUserStore = defineStore('user', () => {
  const users = ref<User[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const fetchUsers = async () => {
    loading.value = true;
    error.value = null;
    try {
      const response = await axios.get('/api/users');
      users.value = response.data;
    } catch (e: any) {
      error.value = e.message;
    } finally {
      loading.value = false;
    }
  };

  return { users, loading, error, fetchUsers };
});
```

### 2. Persisting State to LocalStorage

```typescript
export const useSettingsStore = defineStore('settings', () => {
  const theme = ref(localStorage.getItem('theme') || 'light');

  const setTheme = (newTheme: string) => {
    theme.value = newTheme;
    localStorage.setItem('theme', newTheme);
  };

  return { theme, setTheme };
});
```

### 3. Store Composition (Using One Store in Another)

```typescript
import { useAuthStore } from './auth';

export const useProfileStore = defineStore('profile', () => {
  const authStore = useAuthStore();

  const fetchProfile = async () => {
    if (!authStore.isAuthenticated()) {
      throw new Error('Not authenticated');
    }
    // Fetch profile using authStore.accessToken
  };

  return { fetchProfile };
});
```

## Setup in main.ts

```typescript
import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';

const app = createApp(App);
const pinia = createPinia();

app.use(pinia);
app.mount('#app');
```

## Best Practices

1. **One store per concern** - Create separate stores for auth, users, settings, etc.
2. **Use composition API style** - The `setup` function style is more flexible
3. **Keep actions async-aware** - Return promises for async operations
4. **Don't destructure state** - Use `storeToRefs()` if you need to destructure reactively

```typescript
import { storeToRefs } from 'pinia';

const store = useCounterStore();
const { count } = storeToRefs(store); // Reactive!
const { increment } = store; // Actions don't need storeToRefs
```

## Resources

- [Pinia Documentation](https://pinia.vuejs.org/)
- [Vue 3 Composition API](https://vuejs.org/guide/extras/composition-api-faq.html)
