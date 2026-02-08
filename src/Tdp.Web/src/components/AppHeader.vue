<template>
  <el-header class="app-header">
    <div class="header-left">
      <router-link to="/dashboard" class="app-title">{{ appName }}</router-link>
    </div>
    <div class="header-right">
      <template v-if="authStore.isAuthenticated()">
        <el-dropdown @command="handleCommand">
          <span class="user-dropdown">
            {{ displayName }}
            <el-icon class="el-icon--right"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">Logout</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </template>
      <template v-else>
        <router-link to="/login" class="login-link">Login</router-link>
      </template>
    </div>
  </el-header>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { APP_NAME } from '../config';
import { ArrowDown } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';

const router = useRouter();
const authStore = useAuthStore();
const appName = APP_NAME;

const displayName = computed(() => {
  if (!authStore.user) return '';
  return authStore.user.displayName || authStore.user.email;
});

const handleCommand = async (command: string) => {
  if (command === 'logout') {
    await authStore.logout();
    ElMessage.info('Logged out');
    router.push('/login');
  }
};
</script>

<style scoped>
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #409eff;
  color: white;
  padding: 0 20px;
  height: 60px;
}

.header-left {
  display: flex;
  align-items: center;
}

.app-title {
  font-size: 1.5rem;
  font-weight: bold;
  color: white;
  text-decoration: none;
}

.app-title:hover {
  opacity: 0.9;
}

.header-right {
  display: flex;
  align-items: center;
}

.user-dropdown {
  display: flex;
  align-items: center;
  color: white;
  cursor: pointer;
  font-size: 1rem;
}

.user-dropdown:hover {
  opacity: 0.9;
}

.login-link {
  color: white;
  text-decoration: none;
  font-size: 1rem;
  padding: 8px 16px;
  border: 1px solid white;
  border-radius: 4px;
}

.login-link:hover {
  background: rgba(255, 255, 255, 0.1);
}
</style>
