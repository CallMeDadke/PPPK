import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    name: "Dashboard",
    component: () => import("@/views/DashboardView.vue"),
  },
  {
    path: "/patients",
    name: "Patients",
    component: () => import("@/views/PatientsView.vue"),
  },
  {
    path: "/patients/:id",
    name: "PatientProfile",
    component: () => import("@/views/PatientProfileView.vue"),
  },
  {
    path: "/examinations",
    name: "Examinations",
    component: () => import("@/views/ExaminationsView.vue"),
  },
  {
    path: "/prescriptions",
    name: "Prescriptions",
    component: () => import("@/views/PrescriptionsView.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
});

export default router;
