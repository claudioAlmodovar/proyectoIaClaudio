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
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-10">
    <div class="mx-auto flex max-w-6xl flex-col gap-10">
      <header class="flex flex-col justify-between gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-2 text-center md:flex-row md:items-center md:justify-between md:text-left">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Consultorio Integral</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Panel de gestión clínica</h1>
          </div>
          <div class="rounded-2xl border border-emerald-400/30 bg-emerald-400/10 px-5 py-3 text-sm text-emerald-100">
            Sesión iniciada como <span class="font-semibold text-white">{{ nombre }}</span>
            <span class="ml-1 text-emerald-300">({{ usuario }})</span>
          </div>
        </div>
        <div class="flex flex-wrap items-center justify-between gap-4 text-slate-300">
          <p class="max-w-2xl text-sm">
            Desde este panel puedes gestionar la operación diaria del consultorio: revisar agenda médica, monitorear pacientes
            en seguimiento, coordinar personal y mantener informada a la administración.
          </p>
          <button
            class="rounded-xl border border-emerald-400/60 px-4 py-2 text-sm font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
            type="button"
            @click="logout"
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      <section class="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
        <article class="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 shadow-xl shadow-slate-950/40">
          <h2 class="text-lg font-semibold text-white">Agenda del día</h2>
          <p class="mt-2 text-sm text-slate-300">
            Visualiza las citas programadas por especialidad, confirma asistencias y agrega nuevos pacientes en minutos.
          </p>
          <ul class="mt-4 space-y-2 text-sm text-slate-400">
            <li>08:30 - Consulta general · Sala 2</li>
            <li>10:15 - Control prenatal · Sala 1</li>
            <li>12:00 - Terapia física · Sala 3</li>
          </ul>
        </article>

        <article class="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 shadow-xl shadow-slate-950/40">
          <h2 class="text-lg font-semibold text-white">Pacientes en seguimiento</h2>
          <p class="mt-2 text-sm text-slate-300">
            Consulta los indicadores clave y actualiza el estado clínico de pacientes crónicos o en tratamientos prolongados.
          </p>
          <ul class="mt-4 space-y-2 text-sm text-slate-400">
            <li>María Gómez · Control de diabetes</li>
            <li>José Ramírez · Rehabilitación postoperatoria</li>
            <li>Daniela Ruiz · Seguimiento pediátrico</li>
          </ul>
        </article>

        <article class="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 shadow-xl shadow-slate-950/40">
          <h2 class="text-lg font-semibold text-white">Indicadores del consultorio</h2>
          <p class="mt-2 text-sm text-slate-300">
            Monitorea la ocupación de consultorios, tiempos promedio de atención y desempeño del equipo médico.
          </p>
          <div class="mt-4 grid grid-cols-2 gap-3 text-sm text-slate-300">
            <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/5 p-3 text-center">
              <p class="text-2xl font-bold text-emerald-300">85%</p>
              <p>Ocupación</p>
            </div>
            <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/5 p-3 text-center">
              <p class="text-2xl font-bold text-emerald-300">18m</p>
              <p>Promedio atención</p>
            </div>
            <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/5 p-3 text-center">
              <p class="text-2xl font-bold text-emerald-300">92%</p>
              <p>Satisfacción</p>
            </div>
            <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/5 p-3 text-center">
              <p class="text-2xl font-bold text-emerald-300">+12</p>
              <p>Nuevos pacientes</p>
            </div>
          </div>
        </article>
      </section>

      <section class="grid gap-6 rounded-3xl border border-emerald-500/10 bg-slate-950/60 p-8 md:grid-cols-2">
        <div class="space-y-4">
          <h2 class="text-xl font-semibold text-white">Protocolos destacados</h2>
          <ul class="space-y-3 text-sm text-slate-300">
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">✅</span>
              <span>Verifica signos vitales y actualiza el expediente antes de iniciar cualquier consulta.</span>
            </li>
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">🩺</span>
              <span>Registra indicaciones médicas y prescripciones electrónicas en el mismo momento de la consulta.</span>
            </li>
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">💊</span>
              <span>Coordina con farmacia interna la disponibilidad de tratamientos recetados.</span>
            </li>
          </ul>
        </div>
        <div class="space-y-4">
          <h2 class="text-xl font-semibold text-white">Contacto rápido</h2>
          <div class="rounded-2xl border border-slate-800 bg-slate-900/70 p-6 text-sm text-slate-300">
            <p><span class="font-semibold text-white">Soporte técnico:</span> soporte@consultorio.mx</p>
            <p class="mt-2"><span class="font-semibold text-white">Teléfono interno:</span> Ext. 203</p>
            <p class="mt-2"><span class="font-semibold text-white">Horario de atención:</span> Lunes a viernes · 8:00 a 18:00 h</p>
          </div>
        </div>
      </section>
    </div>
  </main>
</template>
