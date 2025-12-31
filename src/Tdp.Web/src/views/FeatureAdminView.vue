<template>
  <div class="features-admin">
    <el-container>
      <el-header>
        <h2>Feature Admin</h2>
        <div class="header-actions">
          <el-button @click="$router.push('/dashboard')">Back</el-button>
          <el-button type="primary" @click="openCreate">New Feature</el-button>
        </div>
      </el-header>
      <el-main>
        <el-table :data="features" v-loading="loading" style="width: 100%">
          <el-table-column prop="name" label="Name" width="200" />
          <el-table-column prop="description" label="Description" />
          <el-table-column prop="createdAt" label="Created" width="200" />
          <el-table-column label="Actions" width="220">
            <template #default="scope">
              <el-button size="small" @click="openEdit(scope.row)">Edit</el-button>
              <el-button size="small" type="danger" @click="confirmDelete(scope.row)">Delete</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-main>
    </el-container>

    <el-dialog v-model="dialogVisible" :title="dialogMode === 'create' ? 'Create Feature' : 'Edit Feature'">
      <el-form label-position="top">
        <el-form-item label="Name">
          <el-input v-model="form.name" placeholder="Feature name" />
        </el-form-item>
        <el-form-item label="Description">
          <el-input v-model="form.description" type="textarea" placeholder="Feature description" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Cancel</el-button>
        <el-button type="primary" :loading="saving" @click="saveFeature">Save</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import apiClient from '../api/client';

interface Feature {
  id?: string;
  name?: string;
  description?: string | null;
  createdAt?: string;
}

const features = ref<Feature[]>([]);
const loading = ref(false);
const saving = ref(false);
const dialogVisible = ref(false);
const dialogMode = ref<'create' | 'edit'>('create');
const form = ref<Feature>({ name: '', description: '' });
const editingId = ref<string | null>(null);

const loadFeatures = async () => {
  loading.value = true;
  try {
    const response = await apiClient.get('/features');
    features.value = response.data.data || [];
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Failed to load features');
  } finally {
    loading.value = false;
  }
};

const openCreate = () => {
  dialogMode.value = 'create';
  editingId.value = null;
  form.value = { name: '', description: '' };
  dialogVisible.value = true;
};

const openEdit = (feature: Feature) => {
  dialogMode.value = 'edit';
  editingId.value = feature.id || null;
  form.value = { name: feature.name, description: feature.description };
  dialogVisible.value = true;
};

const saveFeature = async () => {
  if (!form.value.name?.trim()) {
    ElMessage.warning('Name is required');
    return;
  }
  saving.value = true;
  try {
    if (dialogMode.value === 'create') {
      await apiClient.post('/features', {
        name: form.value.name,
        description: form.value.description,
      });
      ElMessage.success('Feature created');
    } else if (dialogMode.value === 'edit' && editingId.value) {
      await apiClient.put(`/features/${editingId.value}`, {
        name: form.value.name,
        description: form.value.description,
      });
      ElMessage.success('Feature updated');
    }
    dialogVisible.value = false;
    await loadFeatures();
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.title || 'Save failed');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = (feature: Feature) => {
  if (!feature.id) return;
  ElMessageBox.confirm(`Delete feature "${feature.name}"?`, 'Confirm', {
    type: 'warning',
  })
    .then(async () => {
      try {
        await apiClient.delete(`/features/${feature.id}`);
        ElMessage.success('Feature deleted');
        await loadFeatures();
      } catch (e: any) {
        ElMessage.error(e?.response?.data?.title || 'Delete failed');
      }
    })
    .catch(() => {});
};

onMounted(() => {
  loadFeatures();
});
</script>

<style scoped>
.features-admin {
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
  padding: 24px;
}
.header-actions {
  display: flex;
  gap: 12px;
}
</style>
