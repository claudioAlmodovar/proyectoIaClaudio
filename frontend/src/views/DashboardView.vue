<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const router = useRouter();

const nombre = computed(() => authStore.user?.nombre ?? '');
const usuario = computed(() => authStore.user?.usuario ?? '');

const logout = () => {
  authStore.logout();
  router.push({ name: 'login' });
};

const menuSections = [
  {
    name: 'Consultas',
    icon: '🩺',
    description: 'Gestiona la atención diaria de pacientes y el seguimiento clínico.',
    items: [
      {
        title: 'Crear consulta',
        detail: 'Registra una nueva consulta, asigna profesionales y prepara el consultorio.'
      },
      {
        title: 'Historial de consultas',
        detail: 'Explora consultas anteriores, notas médicas y evolución de pacientes.'
      }
    ]
  },
  {
    name: 'Administración',
    icon: '🛠️',
    description: 'Organiza el equipo y la operación administrativa del consultorio.',
    items: [
      {
        title: 'Usuarios',
        detail: 'Gestiona cuentas, restablece contraseñas y define roles de acceso.'
      },
      {
        title: 'Médicos',
        detail: 'Actualiza información profesional, especialidades y disponibilidad.'
      }
    ]
  }
];
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-10">
    <div class="mx-auto flex max-w-5xl flex-col gap-10">
      <header class="flex flex-col gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Consultorio Integral</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Menú principal</h1>
            <p class="mt-3 max-w-2xl text-sm text-slate-300">
              Selecciona un módulo para comenzar a trabajar. Desde aquí podrás registrar nuevas consultas, revisar historiales y administrar el equipo del consultorio.
            </p>
          </div>
          <div class="flex flex-col items-start gap-3 text-sm text-emerald-100 md:items-end">
            <div class="rounded-2xl border border-emerald-400/30 bg-emerald-400/10 px-5 py-3">
              Sesión iniciada como <span class="font-semibold text-white">{{ nombre }}</span>
              <span class="ml-1 text-emerald-300">({{ usuario }})</span>
            </div>
            <button
              class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="logout"
            >
              Cerrar sesión
            </button>
          </div>
        </div>
      </header>

      <section class="grid gap-6 md:grid-cols-2">
        <article
          v-for="section in menuSections"
          :key="section.name"
          class="flex flex-col gap-5 rounded-3xl border border-slate-800/80 bg-slate-950/70 p-8 shadow-xl shadow-slate-950/40"
        >
          <div class="flex items-start gap-4">
            <span class="flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-500/10 text-2xl">{{ section.icon }}</span>
            <div>
              <h2 class="text-xl font-semibold text-white">{{ section.name }}</h2>
              <p class="mt-2 text-sm text-slate-300">{{ section.description }}</p>
            </div>
          </div>

          <ul class="space-y-4">
            <li
              v-for="item in section.items"
              :key="item.title"
              class="group rounded-2xl border border-slate-800 bg-slate-900/80 p-5 transition hover:border-emerald-500/60 hover:bg-emerald-500/5"
            >
              <div class="flex items-center justify-between">
                <p class="text-base font-semibold text-white">{{ item.title }}</p>
                <span class="text-sm text-emerald-300 transition group-hover:translate-x-1">➔</span>
              </div>
              <p class="mt-2 text-sm text-slate-400">{{ item.detail }}</p>
            </li>
          </ul>
        </article>
      </section>
    </div>
  </main>
</template>
