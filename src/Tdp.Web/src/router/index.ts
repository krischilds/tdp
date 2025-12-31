import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import LoginView from '../views/LoginView.vue';
import RegisterView from '../views/RegisterView.vue';
import DashboardView from '../views/DashboardView.vue';
import UserFeaturesView from '../views/UserFeaturesView.vue';
import FeatureAdminView from '../views/FeatureAdminView.vue';
import UserFeatureAdminView from '../views/UserFeatureAdminView.vue';

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', component: LoginView, meta: { public: true } },
  { path: '/register', component: RegisterView, meta: { public: true } },
  { path: '/dashboard', component: DashboardView, meta: { requiresAuth: true } },
  { path: '/features', component: UserFeaturesView, meta: { requiresAuth: true } },
  { path: '/admin/features', component: FeatureAdminView, meta: { requiresAuth: true } },
  { path: '/admin/user-features', component: UserFeatureAdminView, meta: { requiresAuth: true } },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore();
  if (to.meta.requiresAuth) {
    const authed = await authStore.checkAuth();
    if (!authed) {
      next('/login');
    } else {
      next();
    }
  } else {
    next();
  }
});

export default router;
