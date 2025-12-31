<template>
  <div class="register-container">
    <el-card class="register-card">
      <h2>Register</h2>
      <el-form @submit.prevent="handleRegister">
        <el-form-item label="Email">
          <el-input v-model="email" type="email" required />
        </el-form-item>
        <el-form-item label="Display Name">
          <el-input v-model="displayName" />
        </el-form-item>
        <el-form-item label="Password">
          <el-input v-model="password" type="password" required />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" native-type="submit" :loading="loading">Register</el-button>
          <el-button @click="$router.push('/login')">Back to Login</el-button>
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
const displayName = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');

const handleRegister = async () => {
  loading.value = true;
  error.value = '';
  try {
    await authStore.register(email.value, password.value, displayName.value);
    ElMessage.success('Registered successfully! Please log in.');
    router.push('/login');
  } catch (e: any) {
    error.value = e.response?.data?.title || 'Registration failed';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: #f5f5f5;
}
.register-card {
  width: 400px;
  padding: 20px;
}
</style>
