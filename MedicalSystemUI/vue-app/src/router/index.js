import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: () => import('@/views/Dashboard.vue')
  },
  {
    path: '/patients',
    name: 'Patients',
    component: () => import('@/views/Patients.vue')
  },
  {
    path: '/patients/:id',
    name: 'PatientProfile',
    component: () => import('@/views/PatientProfile.vue')
  },
  {
    path: '/examinations',
    name: 'Examinations',
    component: () => import('@/views/Examinations.vue')
  },
  {
    path: '/prescriptions',
    name: 'Prescriptions',
    component: () => import('@/views/Prescriptions.vue')
  },
  {
    path: '/reports',
    name: 'Reports',
    component: () => import('@/views/Reports.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router