import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/', redirect: '/solicitudes',
    },
    { path: '/login', name: 'login', component: () => import('@/views/LoginView.vue'), meta: { publica: true } },
    { path: '/solicitudes', name: 'solicitudes', component: () => import('@/views/SolicitudesView.vue') },
    { path: '/solicitudes/nueva', name: 'solicitud-nueva', component: () => import('@/views/SolicitudFormularioView.vue') },
    { path: '/solicitudes/:id', name: 'solicitud-detalle', component: () => import('@/views/SolicitudDetalleView.vue') },
    { path: '/solicitudes/:id/editar', name: 'solicitud-editar', component: () => import('@/views/SolicitudFormularioView.vue') },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.publica && !auth.autenticado) return { name: 'login', query: { redirect: to.fullPath } }
  if (to.name === 'login' && auth.autenticado) return { name: 'solicitudes' }
})

export default router
