<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const credentials = reactive({
  usuario: '',
  contrasena: ''
});

const loading = ref(false);
const errorMessage = ref('');
const showErrorModal = ref(false);

const apiBase = import.meta.env.VITE_API_BASE ?? 'https://localhost:59831';
const invalidCredentialsMessage = 'usuario o contraseña no válidos';

const handleSubmit = async () => {
  errorMessage.value = '';
  showErrorModal.value = false;
  loading.value = true;

  try {
    const response = await fetch(`${apiBase}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(credentials)
    });

    if (!response.ok) {
      let apiErrorMessage = invalidCredentialsMessage;

      try {
        const errorBody = (await response.json()) as Partial<{ message: string; detail: string }>;
        const possibleMessages = [errorBody?.message, errorBody?.detail].filter(
          (value): value is string => typeof value === 'string' && value.trim().length > 0
        );
        if (possibleMessages.length > 0) {
          apiErrorMessage = possibleMessages.join(' ');
        }
      } catch (parseError) {
        // Ignoramos errores de parseo para mantener el mensaje por defecto
      }

      throw new Error(apiErrorMessage);
    }

    const data = await response.json();
    authStore.setUser(data);
    router.push({ name: 'dashboard' });
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      errorMessage.value = error.message;
    } else {
      errorMessage.value = 'Ocurrió un error inesperado. Intenta de nuevo.';
    }
    showErrorModal.value = true;
  } finally {
    loading.value = false;
  }
};

const closeErrorModal = () => {
  showErrorModal.value = false;
};
</script>

<template>
  <main class="flex min-h-screen items-center justify-center px-4 py-8">
    <div class="w-full max-w-4xl overflow-hidden rounded-3xl border border-slate-800 bg-slate-900/70 shadow-2xl shadow-emerald-500/10">
      <div class="grid gap-8 md:grid-cols-2">
        <section class="flex flex-col justify-between gap-6 bg-gradient-to-br from-emerald-500/10 via-emerald-500/5 to-slate-900 p-8">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Consultorio Integral</p>
            <h1 class="mt-3 text-3xl font-bold text-white md:text-4xl">Acceso al panel clínico</h1>
            <p class="mt-4 text-sm leading-relaxed text-slate-300">
              Administra citas, expedientes y seguimientos desde un único lugar. Ingresa con tu usuario asignado por la administración del consultorio.
            </p>
          </div>
          <ul class="space-y-3 text-sm text-slate-300">
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">🗓️</span>
              <span>Agenda citas con recordatorios automáticos para pacientes y profesionales de la salud.</span>
            </li>
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">📁</span>
              <span>Consulta los expedientes médicos digitales con historial actualizado en tiempo real.</span>
            </li>
            <li class="flex items-start gap-3">
              <span class="mt-1 text-emerald-300">🔒</span>
              <span>Información protegida con controles de acceso personalizados para cada rol.</span>
            </li>
          </ul>
        </section>

        <section class="flex flex-col justify-center gap-6 p-8">
          <header class="space-y-2 text-center md:text-left">
            <h2 class="text-2xl font-semibold text-white">Inicia sesión</h2>
            <p class="text-sm text-slate-400">Introduce tus credenciales para continuar</p>
          </header>

          <form class="space-y-5" @submit.prevent="handleSubmit">
            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="usuario">Usuario</label>
              <input
                id="usuario"
                v-model.trim="credentials.usuario"
                autocomplete="username"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Ej. recepcion"
                required
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="contrasena">Contraseña</label>
              <input
                id="contrasena"
                v-model.trim="credentials.contrasena"
                autocomplete="current-password"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Introduce tu contraseña"
                required
                type="password"
              />
            </div>

            <button
              class="flex w-full items-center justify-center rounded-xl bg-emerald-500 px-4 py-3 text-sm font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/60 disabled:cursor-not-allowed disabled:bg-emerald-500/60"
              :disabled="loading"
              type="submit"
            >
              <span v-if="!loading">Ingresar</span>
              <span v-else>Validando...</span>
            </button>
          </form>

          <p class="text-xs text-slate-500">
            ¿Necesitas ayuda? Contacta al área de soporte del consultorio para recuperar tu acceso o solicitar un nuevo usuario.
          </p>
        </section>
      </div>
    </div>

    <transition name="fade">
      <div v-if="showErrorModal" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 px-4">
        <div class="w-full max-w-sm rounded-2xl border border-rose-500/40 bg-slate-900 p-6 text-center shadow-2xl shadow-rose-500/20">
          <h3 class="text-lg font-semibold text-white">Inicio de sesión</h3>
          <p class="mt-3 text-sm text-rose-200">{{ errorMessage || invalidCredentialsMessage }}</p>
          <button
            class="mt-6 inline-flex items-center justify-center rounded-xl bg-emerald-500 px-5 py-2 text-sm font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/60"
            type="button"
            @click="closeErrorModal"
          >
            Aceptar
          </button>
        </div>
      </div>
    </transition>
  </main>
</template>
