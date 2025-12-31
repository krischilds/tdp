<template>
  <div class="login-container">
    <el-card class="login-card">
      <h2>Login</h2>
      <el-form @submit.prevent="handleLogin">
        <el-form-item label="Email">
          <el-input v-model="email" type="email" required />
        </el-form-item>
        <el-form-item label="Password">
          <el-input v-model="password" type="password" required />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" native-type="submit" :loading="loading">Login</el-button>
          <el-button @click="$router.push('/register')">Register</el-button>
        </el-form-item>
      </el-form>
      <el-alert v-if="error" type="error" :title="error" :closable="false" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { ElMessage } from 'element-plus';

const router = useRouter();
const authStore = useAuthStore();
const email = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');

const handleLogin = async () => {
  loading.value = true;
  error.value = '';
  try {
    await authStore.login(email.value, password.value);
    ElMessage.success('Logged in successfully');
    router.push('/dashboard');
  } catch (e: any) {
    error.value = e.response?.data?.title || 'Login failed';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: #f5f5f5;
}
.login-card {
  width: 400px;
  padding: 20px;
}
</style>
