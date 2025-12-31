<template>
  <div class="dashboard">
    <el-container>
      <el-header>
        <h2>Dashboard</h2>
        <el-button @click="handleLogout">Logout</el-button>
      </el-header>
      <el-main>
        <p>Welcome, {{ authStore.user?.email }}</p>
        <el-space>
          <el-button type="primary" @click="$router.push('/features')">My Features</el-button>
          <el-button type="success" @click="$router.push('/admin/features')">Admin: Feature CRUD</el-button>
          <el-button type="warning" @click="$router.push('/admin/user-features')">Admin: User Features</el-button>
        </el-space>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { ElMessage } from 'element-plus';

const router = useRouter();
const authStore = useAuthStore();

const handleLogout = async () => {
  await authStore.logout();
  ElMessage.info('Logged out');
  router.push('/login');
};
</script>

<style scoped>
.dashboard {
  height: 100vh;
}
.el-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #409eff;
  color: white;
}
.el-main {
  padding: 40px;
}
</style>
