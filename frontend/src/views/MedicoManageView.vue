<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

type CreateMedicoPayload = {
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  cedula: string;
  telefono: string | null;
  especialidad: string | null;
  email: string | null;
  activo: boolean;
};

type UpdateMedicoPayload = CreateMedicoPayload;

type MedicoResponse = {
  id: number;
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  cedula: string;
  telefono: string | null;
  especialidad: string | null;
  email: string | null;
  activo: boolean;
  fechaCreacion: string;
};

const router = useRouter();
const authStore = useAuthStore();

const form = reactive({
  primerNombre: '',
  segundoNombre: '',
  apellidoPaterno: '',
  apellidoMaterno: '',
  cedula: '',
  telefono: '',
  especialidad: '',
  email: '',
  activo: true
});

const loading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
const apiBase = import.meta.env.VITE_API_BASE ?? 'https://localhost:59831';

const showMedicosModal = ref(false);
const medicosLoading = ref(false);
const medicosError = ref('');
const medicos = ref<MedicoResponse[]>([]);
const deletingMedicoIds = ref<number[]>([]);
const confirmDeleteMedicoId = ref<number | null>(null);
const confirmDeleteMedicoLoading = ref(false);

const editingMedicoId = ref<number | null>(null);
const editForm = reactive({
  primerNombre: '',
  segundoNombre: '',
  apellidoPaterno: '',
  apellidoMaterno: '',
  cedula: '',
  telefono: '',
  especialidad: '',
  email: '',
  activo: true
});
const editLoading = ref(false);
const editErrorMessage = ref('');

const medicoPendingDeletion = computed(() =>
  confirmDeleteMedicoId.value === null
    ? null
    : medicos.value.find((medico) => medico.id === confirmDeleteMedicoId.value) ?? null
);

const editingMedico = computed(() =>
  editingMedicoId.value === null
    ? null
    : medicos.value.find((medico) => medico.id === editingMedicoId.value) ?? null
);

const isEditModalOpen = computed(() => editingMedicoId.value !== null);

const canSubmit = computed(() => {
  return (
    form.primerNombre.trim().length > 0 &&
    form.apellidoPaterno.trim().length > 0 &&
    form.cedula.trim().length > 0
  );
});

const canSubmitEdit = computed(() => {
  return (
    editForm.primerNombre.trim().length > 0 &&
    editForm.apellidoPaterno.trim().length > 0 &&
    editForm.cedula.trim().length > 0
  );
});

const resetForm = () => {
  form.primerNombre = '';
  form.segundoNombre = '';
  form.apellidoPaterno = '';
  form.apellidoMaterno = '';
  form.cedula = '';
  form.telefono = '';
  form.especialidad = '';
  form.email = '';
  form.activo = true;
};

const optionalValue = (value: string): string | null => {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
};

const buildCreatePayload = (): CreateMedicoPayload => ({
  primerNombre: form.primerNombre.trim(),
  segundoNombre: optionalValue(form.segundoNombre),
  apellidoPaterno: form.apellidoPaterno.trim(),
  apellidoMaterno: optionalValue(form.apellidoMaterno),
  cedula: form.cedula.trim(),
  telefono: optionalValue(form.telefono),
  especialidad: optionalValue(form.especialidad),
  email: optionalValue(form.email),
  activo: form.activo
});

const buildUpdatePayload = (): UpdateMedicoPayload => ({
  primerNombre: editForm.primerNombre.trim(),
  segundoNombre: optionalValue(editForm.segundoNombre),
  apellidoPaterno: editForm.apellidoPaterno.trim(),
  apellidoMaterno: optionalValue(editForm.apellidoMaterno),
  cedula: editForm.cedula.trim(),
  telefono: optionalValue(editForm.telefono),
  especialidad: optionalValue(editForm.especialidad),
  email: optionalValue(editForm.email),
  activo: editForm.activo
});

const buildAuthHeaders = (): Record<string, string> => {
  const headers: Record<string, string> = {};

  if (authStore.token) {
    headers.Authorization = `Bearer ${authStore.token}`;
  }

  return headers;
};

const handleSubmit = async () => {
  errorMessage.value = '';
  successMessage.value = '';

  if (!authStore.token) {
    errorMessage.value = 'Debes iniciar sesión para registrar médicos.';
    return;
  }

  let payload: CreateMedicoPayload;
  try {
    payload = buildCreatePayload();
  } catch (error) {
    errorMessage.value = 'Revisa los datos ingresados.';
    return;
  }

  loading.value = true;

  try {
    const response = await fetch(`${apiBase}/api/medicos`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...buildAuthHeaders()
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo registrar al médico. Intenta nuevamente.';
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

    successMessage.value = 'Médico creado correctamente.';
    resetForm();
    await fetchMedicos();
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

const fetchMedicos = async () => {
  medicosError.value = '';
  medicosLoading.value = true;

  try {
    if (!authStore.token) {
      throw new Error('Debes iniciar sesión para ver los médicos.');
    }

    const response = await fetch(`${apiBase}/api/medicos`, {
      method: 'GET',
      headers: buildAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudieron obtener los médicos.');
    }

    const data = (await response.json()) as MedicoResponse[];
    medicos.value = data;
  } catch (error) {
    medicos.value = [];
    medicosError.value =
      error instanceof Error && error.message.trim().length > 0
        ? error.message
        : 'Ocurrió un error al consultar los médicos.';
  } finally {
    medicosLoading.value = false;
  }
};

const openMedicosModal = () => {
  showMedicosModal.value = true;
  void fetchMedicos();
};

const closeMedicosModal = () => {
  showMedicosModal.value = false;
  medicosError.value = '';
};

const removeDeletingMedicoId = (id: number) => {
  deletingMedicoIds.value = deletingMedicoIds.value.filter((currentId) => currentId !== id);
};

const performDeleteMedico = async (id: number) => {
  deletingMedicoIds.value = [...deletingMedicoIds.value, id];
  medicosError.value = '';

  try {
    if (!authStore.token) {
      throw new Error('Debes iniciar sesión para eliminar médicos.');
    }

    const response = await fetch(`${apiBase}/api/medicos/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudo eliminar el médico.');
    }

    await fetchMedicos();
  } catch (error) {
    medicosError.value =
      error instanceof Error && error.message.trim().length > 0
        ? error.message
        : 'Ocurrió un error al eliminar el médico.';
  } finally {
    removeDeletingMedicoId(id);
  }
};

const isDeletingMedico = (id: number) => deletingMedicoIds.value.includes(id);

const promptDeleteMedico = (id: number) => {
  if (isDeletingMedico(id)) {
    return;
  }

  medicosError.value = '';
  confirmDeleteMedicoId.value = id;
};

const closeDeleteModal = () => {
  if (confirmDeleteMedicoLoading.value) {
    return;
  }

  confirmDeleteMedicoId.value = null;
};

const confirmDeleteMedico = async () => {
  const id = confirmDeleteMedicoId.value;
  if (id === null) {
    return;
  }

  confirmDeleteMedicoLoading.value = true;

  try {
    await performDeleteMedico(id);
    confirmDeleteMedicoId.value = null;
  } finally {
    confirmDeleteMedicoLoading.value = false;
  }
};

const openEditModal = (medico: MedicoResponse) => {
  editingMedicoId.value = medico.id;
  editForm.primerNombre = medico.primerNombre;
  editForm.segundoNombre = medico.segundoNombre ?? '';
  editForm.apellidoPaterno = medico.apellidoPaterno;
  editForm.apellidoMaterno = medico.apellidoMaterno ?? '';
  editForm.cedula = medico.cedula;
  editForm.telefono = medico.telefono ?? '';
  editForm.especialidad = medico.especialidad ?? '';
  editForm.email = medico.email ?? '';
  editForm.activo = medico.activo;
  editErrorMessage.value = '';
};

const closeEditModal = () => {
  if (editLoading.value) {
    return;
  }

  editingMedicoId.value = null;
};

const handleUpdateMedico = async () => {
  const id = editingMedicoId.value;
  if (id === null) {
    return;
  }

  editErrorMessage.value = '';
  successMessage.value = '';

  if (!authStore.token) {
    editErrorMessage.value = 'Debes iniciar sesión para actualizar médicos.';
    return;
  }

  let payload: UpdateMedicoPayload;
  try {
    payload = buildUpdatePayload();
  } catch (error) {
    editErrorMessage.value = 'Revisa los datos ingresados.';
    return;
  }

  editLoading.value = true;

  try {
    const response = await fetch(`${apiBase}/api/medicos/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...buildAuthHeaders()
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo actualizar el médico. Intenta nuevamente.';
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

    successMessage.value = 'Médico actualizado correctamente.';
    await fetchMedicos();
    closeEditModal();
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      editErrorMessage.value = error.message;
    } else {
      editErrorMessage.value = 'Ocurrió un error inesperado. Intenta nuevamente.';
    }
  } finally {
    editLoading.value = false;
  }
};

const buildMedicoFullName = (medico: MedicoResponse) => {
  return [
    medico.primerNombre,
    medico.segundoNombre ?? '',
    medico.apellidoPaterno,
    medico.apellidoMaterno ?? ''
  ]
    .map((part) => part.trim())
    .filter((part) => part.length > 0)
    .join(' ');
};
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-10">
    <div class="mx-auto flex max-w-4xl flex-col gap-8">
      <header class="flex flex-col gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-4">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Administración</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Médicos</h1>
          </div>
          <div class="flex items-center gap-4 text-sm text-emerald-100">
            <button
              class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="goBack"
            >
              Volver al panel
            </button>
            <button
              class="ml-auto inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="openMedicosModal"
            >
              Ver médicos
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
          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="primerNombre">Primer nombre</label>
            <input
              id="primerNombre"
              v-model.trim="form.primerNombre"
              autocomplete="given-name"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Primer nombre"
              required
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="segundoNombre">Segundo nombre (opcional)</label>
            <input
              id="segundoNombre"
              v-model.trim="form.segundoNombre"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Segundo nombre"
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="apellidoPaterno">Apellido paterno</label>
            <input
              id="apellidoPaterno"
              v-model.trim="form.apellidoPaterno"
              autocomplete="family-name"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Apellido paterno"
              required
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="apellidoMaterno">Apellido materno (opcional)</label>
            <input
              id="apellidoMaterno"
              v-model.trim="form.apellidoMaterno"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Apellido materno"
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="cedula">Cédula profesional</label>
            <input
              id="cedula"
              v-model.trim="form.cedula"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Número de cédula"
              required
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="telefono">Teléfono (opcional)</label>
            <input
              id="telefono"
              v-model.trim="form.telefono"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Teléfono de contacto"
              type="tel"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="especialidad">Especialidad (opcional)</label>
            <input
              id="especialidad"
              v-model.trim="form.especialidad"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="Especialidad médica"
              type="text"
            />
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-medium text-slate-300" for="email">Correo electrónico (opcional)</label>
            <input
              id="email"
              v-model.trim="form.email"
              autocomplete="email"
              class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
              placeholder="correo@ejemplo.com"
              type="email"
            />
          </div>

          <div class="flex items-center gap-3 md:col-span-2">
            <input
              id="activo"
              v-model="form.activo"
              class="h-5 w-5 rounded border border-slate-700 bg-slate-900 text-emerald-500 focus:ring-emerald-500/60"
              type="checkbox"
            />
            <label class="text-sm text-slate-300" for="activo">Médico activo desde su creación</label>
          </div>

          <div class="md:col-span-2">
            <button
              class="flex w-full items-center justify-center rounded-xl bg-emerald-500 px-4 py-3 text-sm font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/60 disabled:cursor-not-allowed disabled:bg-emerald-500/60"
              :disabled="!canSubmit || loading"
              type="submit"
            >
              <span v-if="!loading">Registrar médico</span>
              <span v-else>Guardando...</span>
            </button>
          </div>
        </form>
      </section>
    </div>

    <transition name="fade">
      <div
        v-if="showMedicosModal"
        class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 px-4 py-10 backdrop-blur"
      >
        <div class="relative w-full max-w-4xl overflow-hidden rounded-3xl border border-emerald-500/30 bg-slate-950 shadow-2xl">
          <header class="flex items-center justify-between border-b border-emerald-500/20 bg-slate-900/60 px-6 py-4">
            <div>
              <p class="text-xs uppercase tracking-[0.3em] text-emerald-300">Médicos</p>
              <h2 class="text-xl font-semibold text-white">Listado de médicos</h2>
            </div>
            <button
              class="rounded-lg border border-emerald-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="closeMedicosModal"
            >
              Cerrar
            </button>
          </header>
          <section class="max-h-[70vh] overflow-y-auto px-6 py-5 text-sm text-slate-200">
            <div v-if="medicosLoading" class="py-10 text-center text-emerald-200">Cargando médicos...</div>
            <div v-else>
              <div v-if="medicosError" class="mb-4 rounded-xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-rose-100">
                {{ medicosError }}
              </div>
              <div v-else-if="medicos.length === 0" class="py-10 text-center text-slate-400">No hay médicos registrados.</div>
              <table v-else class="min-w-full divide-y divide-slate-800">
                <thead class="bg-slate-900/70 text-left text-xs uppercase tracking-wider text-slate-400">
                  <tr>
                    <th class="px-4 py-3">Nombre</th>
                    <th class="px-4 py-3">Cédula</th>
                    <th class="px-4 py-3">Especialidad</th>
                    <th class="px-4 py-3">Estado</th>
                    <th class="px-4 py-3 text-right">Acciones</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
                  <tr v-for="medico in medicos" :key="medico.id">
                    <td class="px-4 py-3 font-medium text-white">{{ buildMedicoFullName(medico) }}</td>
                    <td class="px-4 py-3">{{ medico.cedula }}</td>
                    <td class="px-4 py-3">{{ medico.especialidad ?? 'Sin especialidad' }}</td>
                    <td class="px-4 py-3">
                      <span
                        :class="[
                          'rounded-full px-3 py-1 text-xs font-semibold',
                          medico.activo ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-200'
                        ]"
                      >
                        {{ medico.activo ? 'Activo' : 'Inactivo' }}
                      </span>
                    </td>
                    <td class="px-4 py-3 text-right">
                      <div class="flex items-center justify-end gap-2">
                        <button
                          class="inline-flex items-center justify-center rounded-lg border border-emerald-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
                          type="button"
                          @click="openEditModal(medico)"
                        >
                          Editar
                        </button>
                        <button
                          class="inline-flex items-center justify-center rounded-lg border border-rose-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                          :disabled="isDeletingMedico(medico.id) || confirmDeleteMedicoLoading"
                          type="button"
                          @click="promptDeleteMedico(medico.id)"
                        >
                          <span v-if="!isDeletingMedico(medico.id)">Eliminar</span>
                          <span v-else>Eliminando...</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </div>
    </transition>

    <transition name="fade">
      <div
        v-if="confirmDeleteMedicoId !== null"
        class="fixed inset-0 z-[60] flex items-center justify-center bg-slate-950/80 px-4 backdrop-blur"
      >
        <div class="w-full max-w-md rounded-3xl border border-emerald-500/30 bg-slate-950 p-8 text-center shadow-2xl shadow-emerald-500/20">
          <h3 class="text-2xl font-semibold text-white">Eliminar médico</h3>
          <p class="mt-4 text-sm text-slate-300">¿Deseas eliminar este médico? Esta acción no se puede deshacer.</p>
          <div
            v-if="medicoPendingDeletion"
            class="mt-5 rounded-2xl border border-slate-800 bg-slate-900/70 px-5 py-4 text-left text-sm text-slate-200"
          >
            <p class="font-semibold text-white">{{ buildMedicoFullName(medicoPendingDeletion) }}</p>
            <p class="text-xs text-slate-400">Cédula: {{ medicoPendingDeletion.cedula }}</p>
          </div>
          <div class="mt-6 flex items-center gap-3">
            <button
              class="inline-flex flex-1 items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="confirmDeleteMedicoLoading"
              type="button"
              @click="closeDeleteModal"
            >
              Cancelar
            </button>
            <button
              class="inline-flex flex-1 items-center justify-center rounded-xl border border-rose-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="confirmDeleteMedicoLoading"
              type="button"
              @click="confirmDeleteMedico"
            >
              <span v-if="!confirmDeleteMedicoLoading">Eliminar</span>
              <span v-else>Eliminando...</span>
            </button>
          </div>
        </div>
      </div>
    </transition>

    <transition name="fade">
      <div
        v-if="isEditModalOpen"
        class="fixed inset-0 z-[65] flex items-center justify-center bg-slate-950/80 px-4 backdrop-blur"
      >
        <div class="w-full max-w-2xl rounded-3xl border border-emerald-500/30 bg-slate-950 p-8 shadow-2xl shadow-emerald-500/20">
          <header class="mb-6">
            <p class="text-xs uppercase tracking-[0.3em] text-emerald-300">Médicos</p>
            <h3 class="text-2xl font-semibold text-white">Editar médico</h3>
            <p v-if="editingMedico" class="mt-2 text-sm text-slate-300">{{ buildMedicoFullName(editingMedico) }}</p>
          </header>

          <div v-if="editErrorMessage" class="mb-4 rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
            {{ editErrorMessage }}
          </div>

          <form class="grid gap-6 md:grid-cols-2" @submit.prevent="handleUpdateMedico">
            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editPrimerNombre">Primer nombre</label>
              <input
                id="editPrimerNombre"
                v-model.trim="editForm.primerNombre"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Primer nombre"
                required
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editSegundoNombre">Segundo nombre (opcional)</label>
              <input
                id="editSegundoNombre"
                v-model.trim="editForm.segundoNombre"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Segundo nombre"
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editApellidoPaterno">Apellido paterno</label>
              <input
                id="editApellidoPaterno"
                v-model.trim="editForm.apellidoPaterno"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Apellido paterno"
                required
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editApellidoMaterno">Apellido materno (opcional)</label>
              <input
                id="editApellidoMaterno"
                v-model.trim="editForm.apellidoMaterno"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Apellido materno"
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editCedula">Cédula profesional</label>
              <input
                id="editCedula"
                v-model.trim="editForm.cedula"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Número de cédula"
                required
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editTelefono">Teléfono (opcional)</label>
              <input
                id="editTelefono"
                v-model.trim="editForm.telefono"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Teléfono de contacto"
                type="tel"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editEspecialidad">Especialidad (opcional)</label>
              <input
                id="editEspecialidad"
                v-model.trim="editForm.especialidad"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Especialidad médica"
                type="text"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editEmail">Correo electrónico (opcional)</label>
              <input
                id="editEmail"
                v-model.trim="editForm.email"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="correo@ejemplo.com"
                type="email"
              />
            </div>

            <div class="flex items-center gap-3 md:col-span-2">
              <input
                id="editActivo"
                v-model="editForm.activo"
                class="h-5 w-5 rounded border border-slate-700 bg-slate-900 text-emerald-500 focus:ring-emerald-500/60"
                type="checkbox"
              />
              <label class="text-sm text-slate-300" for="editActivo">Médico activo</label>
            </div>

            <div class="md:col-span-2 flex items-center gap-3">
              <button
                class="inline-flex flex-1 items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-3 text-sm font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                :disabled="editLoading"
                type="button"
                @click="closeEditModal"
              >
                Cancelar
              </button>
              <button
                class="inline-flex flex-1 items-center justify-center rounded-xl bg-emerald-500 px-4 py-3 text-sm font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/60 disabled:cursor-not-allowed disabled:bg-emerald-500/60"
                :disabled="!canSubmitEdit || editLoading"
                type="submit"
              >
                <span v-if="!editLoading">Guardar cambios</span>
                <span v-else>Guardando...</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </transition>
  </main>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
