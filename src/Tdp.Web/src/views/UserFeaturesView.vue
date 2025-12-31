<template>
  <div class="features-view">
    <el-container>
      <el-header>
        <h2>My Features</h2>
        <el-button @click="$router.push('/dashboard')">Back</el-button>
      </el-header>
      <el-main>
        <el-button type="primary" @click="loadData" :loading="loading">Refresh</el-button>
        <el-table :data="myFeatures" style="width: 100%; margin-top: 20px;" v-loading="loading">
          <el-table-column prop="name" label="Feature Name" />
          <el-table-column prop="description" label="Description" />
        </el-table>

        <h3 style="margin-top: 32px;">All Features</h3>
        <el-table :data="allFeatures" style="width: 100%; margin-top: 12px;" v-loading="loading">
          <el-table-column prop="name" label="Feature Name" width="240" />
          <el-table-column prop="description" label="Description" />
          <el-table-column label="Assigned" width="140">
            <template #default="scope">
              <el-tag :type="isAssigned(scope.row.id) ? 'success' : 'info'">
                {{ isAssigned(scope.row.id) ? 'Yes' : 'No' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="220">
            <template #default="scope">
              <el-button size="small" type="primary" v-if="!isAssigned(scope.row.id)" @click="assign(scope.row.id)">Assign</el-button>
              <el-button size="small" type="danger" v-else @click="unassign(scope.row.id)">Remove</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { ElMessage } from 'element-plus';

const myFeatures = ref<any[]>([]);
const allFeatures = ref<any[]>([]);
const loading = ref(false);

const loadMyFeatures = async () => {
  loading.value = true;
  try {
    const response = await apiClient.get('/users/me/features');
    myFeatures.value = response.data.data || [];
  } catch (e: any) {
    ElMessage.error('Failed to load features');
  } finally {
    loading.value = false;
  }
};

const loadAllFeatures = async () => {
  try {
    const response = await apiClient.get('/features');
    allFeatures.value = response.data.data || [];
  } catch {
    ElMessage.error('Failed to load all features');
  }
};

const loadData = async () => {
  loading.value = true;
  await Promise.all([loadMyFeatures(), loadAllFeatures()]);
  loading.value = false;
};

const isAssigned = (featureId: string) => {
  return myFeatures.value.some((f: any) => f.id === featureId);
};

const assign = async (featureId: string) => {
  try {
    await apiClient.post(`/users/me/features/${featureId}`);
    ElMessage.success('Feature assigned');
    await loadMyFeatures();
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Assign failed');
  }
};

const unassign = async (featureId: string) => {
  try {
    await apiClient.delete(`/users/me/features/${featureId}`);
    ElMessage.success('Feature removed');
    await loadMyFeatures();
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Remove failed');
  }
};

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.features-view {
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
