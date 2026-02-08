<template>
  <div class="dashboard">
    <el-container>
      <el-main>
        <h2>Dashboard</h2>
        <p>Welcome, {{ displayName }}</p>
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
import { computed } from 'vue';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();

const displayName = computed(() => {
  if (!authStore.user) return '';
  return authStore.user.displayName || authStore.user.email;
});
</script>

<style scoped>
.dashboard {
  flex: 1;
}
.el-main {
  padding: 40px;
}
</style>
