<template>
  <div class="user-feature-admin">
    <el-container>
      <el-header>
        <h2>User Feature Admin</h2>
        <div class="header-actions">
          <el-button @click="$router.push('/dashboard')">Back</el-button>
        </div>
      </el-header>
      <el-main>
        <el-form inline>
          <el-form-item label="Search users">
            <el-input v-model="query" placeholder="Email or name" @input="debouncedSearch" style="width: 320px" />
          </el-form-item>
          <el-form-item>
            <el-button @click="searchUsers" :loading="loading">Search</el-button>
          </el-form-item>
        </el-form>

        <el-row :gutter="16" style="margin-top: 12px;">
          <el-col :span="8">
            <el-card>
              <template #header>
                <div class="card-header">Users</div>
              </template>
              <el-table :data="users" height="420" @row-click="selectUser" highlight-current-row>
                <el-table-column prop="email" label="Email" />
                <el-table-column prop="displayName" label="Name" width="160" />
              </el-table>
            </el-card>
          </el-col>
          <el-col :span="16">
            <el-card>
              <template #header>
                <div class="card-header">User Features <span v-if="selectedUser">— {{ selectedUser.email }}</span></div>
              </template>
              <div v-if="!selectedUser" class="empty">Select a user to manage features.</div>
              <div v-else>
                <el-row :gutter="12">
                  <el-col :span="12">
                    <h4>Assigned</h4>
                    <el-table :data="userFeatures" height="180" style="margin-bottom: 12px;">
                      <el-table-column prop="name" label="Name" />
                      <el-table-column label="Actions" width="140">
                        <template #default="scope">
                          <el-button size="small" type="danger" @click="unassign(scope.row.id)">Remove</el-button>
                        </template>
                      </el-table-column>
                    </el-table>
                  </el-col>
                  <el-col :span="12">
                    <h4>All Features</h4>
                    <el-table :data="allFeatures" height="180">
                      <el-table-column prop="name" label="Name" />
                      <el-table-column label="Actions" width="140">
                        <template #default="scope">
                          <el-button size="small" type="primary" :disabled="isAssigned(scope.row.id)" @click="assign(scope.row.id)">Assign</el-button>
                        </template>
                      </el-table-column>
                    </el-table>
                  </el-col>
                </el-row>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
  
</template>

<script setup lang="ts">
import { ref } from 'vue';
import apiClient from '../api/client';
import { ElMessage } from 'element-plus';

interface UserItem { id: string; email: string; displayName?: string | null }
interface Feature { id: string; name: string; description?: string | null }

const query = ref('');
const loading = ref(false);
const users = ref<UserItem[]>([]);
const selectedUser = ref<UserItem | null>(null);
const userFeatures = ref<Feature[]>([]);
const allFeatures = ref<Feature[]>([]);

const debounce = (fn: Function, ms: number) => {
  let t: any;
  return (...args: any[]) => {
    clearTimeout(t);
    t = setTimeout(() => fn(...args), ms);
  };
};

const doSearch = async () => {
  loading.value = true;
  try {
    const response = await apiClient.get('/users', { params: { q: query.value } });
    users.value = response.data.data || [];
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Search failed');
  } finally {
    loading.value = false;
  }
};

const debouncedSearch = debounce(doSearch, 300);
const searchUsers = async () => doSearch();

const selectUser = async (user: UserItem) => {
  selectedUser.value = user;
  await loadUserFeatures();
};

const loadAllFeatures = async () => {
  const response = await apiClient.get('/features');
  allFeatures.value = response.data.data || [];
};

const loadUserFeatures = async () => {
  if (!selectedUser.value) return;
  const response = await apiClient.get(`/users/${selectedUser.value.id}/features`);
  userFeatures.value = response.data.data || [];
};

const isAssigned = (featureId: string) => userFeatures.value.some(f => f.id === featureId);

const assign = async (featureId: string) => {
  if (!selectedUser.value) return;
  try {
    await apiClient.post(`/users/${selectedUser.value.id}/features/${featureId}`);
    ElMessage.success('Assigned');
    await loadUserFeatures();
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Assign failed');
  }
};

const unassign = async (featureId: string) => {
  if (!selectedUser.value) return;
  try {
    await apiClient.delete(`/users/${selectedUser.value.id}/features/${featureId}`);
    ElMessage.success('Removed');
    await loadUserFeatures();
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Remove failed');
  }
};

// initial
loadAllFeatures();
doSearch();
</script>

<style scoped>
.user-feature-admin { height: 100vh; }
.el-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #409eff;
  color: white;
}
.el-main { padding: 24px; }
.card-header { font-weight: 600; }
.empty { color: #999; padding: 12px; }
.header-actions { display: flex; gap: 12px; }
</style>
