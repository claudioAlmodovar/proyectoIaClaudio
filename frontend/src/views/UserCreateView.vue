<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

type CreateUserPayload = {
  correo: string;
  password: string;
  nombreCompleto: string;
  medicoId: number | null;
  activo: boolean;
};

const router = useRouter();
const authStore = useAuthStore();

const form = reactive({
  nombreCompleto: '',
  correo: '',
  password: '',
  confirmPassword: '',
  medicoId: '',
  activo: true
});

const loading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
const apiBase = import.meta.env.VITE_API_BASE ?? 'https://localhost:59831';

const canSubmit = computed(() => {
  return (
    form.nombreCompleto.trim().length > 0 &&
    form.correo.trim().length > 0 &&
    form.password.trim().length >= 6 &&
    form.confirmPassword.trim().length >= 6
  );
});

const resetForm = () => {
  form.nombreCompleto = '';
  form.correo = '';
  form.password = '';
  form.confirmPassword = '';
  form.medicoId = '';
  form.activo = true;
};

const parseMedicoId = (): number | null => {
  const trimmed = form.medicoId.trim();
  if (!trimmed) {
    return null;
  }

  const parsed = Number(trimmed);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error('El identificador del médico debe ser un número entero positivo.');
  }

  return parsed;
};

const buildPayload = (): CreateUserPayload => ({
  correo: form.correo.trim(),
  password: form.password,
  nombreCompleto: form.nombreCompleto.trim(),
  medicoId: parseMedicoId(),
  activo: form.activo
});

const handleSubmit = async () => {
  errorMessage.value = '';
  successMessage.value = '';

  if (form.password !== form.confirmPassword) {
    errorMessage.value = 'Las contraseñas no coinciden.';
    return;
  }

  let payload: CreateUserPayload;
  try {
    payload = buildPayload();
  } catch (error) {
    if (error instanceof Error) {
      errorMessage.value = error.message;
    } else {
      errorMessage.value = 'Revisa los datos ingresados.';
    }
    return;
  }

  loading.value = true;

  try {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json'
    };

    if (authStore.token) {
      headers.Authorization = `Bearer ${authStore.token}`;
    }

    const response = await fetch(`${apiBase}/api/usuarios`, {
      method: 'POST',
      headers,
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo registrar al usuario. Intenta nuevamente.';
      try {
        const errorBody = (await response.json()) as Partial<{ message: string; detail: string }>;
        const possibleMessages = [errorBody?.message, errorBody?.detail].filter(
          (value): value is string => typeof value === 'string' && value.trim().length > 0
        );
        if (possibleMessages.length > 0) {
          apiMessage = possibleMessages.join(' ');
        }
      } catch (parseError) {
        // Mantenemos el mensaje por defecto si no es posible parsear la respuesta.
      }

      throw new Error(apiMessage);
    }

    successMessage.value = 'Usuario creado correctamente.';
    resetForm();
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      errorMessage.value = error.message;
    } else {
      errorMessage.value = 'Ocurrió un error inesperado. Intenta nuevamente.';
    }
  } finally {
    loading.value = false;
  }
};

const goBack = () => {
  router.push({ name: 'dashboard' });
};
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-10">
    <div class="mx-auto flex max-w-4xl flex-col gap-8">
      <header class="flex flex-col gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Administración</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Alta de usuarios</h1>
            <p class="mt-3 max-w-2xl text-sm text-slate-300">
              Completa el formulario para registrar nuevos usuarios en el sistema. Los datos se guardarán automáticamente en la base de datos.
            </p>
          </div>
          <div class="flex flex-col items-start gap-3 text-sm text-emerald-100 md:items-end">
            <button
              class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="goBack"
            >
              Volver al panel
            </button>
          </div>
        </div>
      </header>

      <section class="space-y-6 rounded-3xl border border-slate-800/80 bg-slate-950/70 p-8 shadow-xl shadow-slate-950/40">
        <div v-if="successMessage" class="rounded-2xl border border-emerald-500/40 bg-emerald-500/10 px-5 py-4 text-sm text-emerald-100">
          {{ successMessage }}
        </div>

        <div v-if="errorMessage" class="rounded-2xl border border-rose-500/40 bg-rose-500/10 px-5 py-4 text-sm text-rose-100">
          {{ errorMessage }}
        </div>

        <form class="grid gap-6 md:grid-cols-2" @submit.prevent="handleSubmit">
          <div class="space-y-2 md:col-span-2">
            <label class="block text-sm font-medium text-slate-300" for="nombreCompleto">Nombre completo</label>
            <input
              id="nombreCompleto"
              v-model.trim="form.nombreCompleto"
              autocomplete="name"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Nombre y apellidos"
              required
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="correo">Correo electrónico</label>
            <input
              id="correo"
              v-model.trim="form.correo"
              autocomplete="email"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="correo@ejemplo.com"
              required
              type="email"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="medicoId">Médico asociado (opcional)</label>
            <input
              id="medicoId"
              v-model.trim="form.medicoId"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="ID numérico del médico"
              type="text"
            />
            <p class="text-xs text-slate-400">Introduce el identificador numérico del médico si corresponde.</p>
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="password">Contraseña</label>
            <input
              id="password"
              v-model.trim="form.password"
              autocomplete="new-password"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Mínimo 6 caracteres"
              required
              type="password"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="confirmPassword">Confirmar contraseña</label>
            <input
              id="confirmPassword"
              v-model.trim="form.confirmPassword"
              autocomplete="new-password"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Repite la contraseña"
              required
              type="password"
            />
          </div>

          <div class="flex items-center gap-3 md:col-span-2">
            <input
              id="activo"
              v-model="form.activo"
              class="h-5 w-5 rounded border border-slate-700 bg-slate-900 text-emerald-500 focus:ring-emerald-500/60"
              type="checkbox"
            />
            <label class="text-sm text-slate-300" for="activo">Usuario activo desde su creación</label>
          </div>

          <div class="md:col-span-2">
            <button
              class="flex w-full items-center justify-center rounded-xl bg-emerald-500 px-4 py-3 text-sm font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/60 disabled:cursor-not-allowed disabled:bg-emerald-500/60"
              :disabled="!canSubmit || loading"
              type="submit"
            >
              <span v-if="!loading">Registrar usuario</span>
              <span v-else>Guardando...</span>
            </button>
          </div>
        </form>
      </section>
    </div>
  </main>
</template>
