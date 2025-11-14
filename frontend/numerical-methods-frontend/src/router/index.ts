import { createRouter, createWebHistory } from 'vue-router';
import LinearSystemsView from '../views/LinearSystemsView.vue';
import RootFindingView from '../views/RootFindingView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/linear-systems',
    },
    {
      path: '/linear-systems',
      name: 'linear-systems',
      component: LinearSystemsView,
    },
    {
      path: '/root-finding',
      name: 'root-finding',
      component: RootFindingView,
    },
  ],
});

export default router;
