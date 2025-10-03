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

type UsuarioResponse = {
  id: number;
  correo: string;
  nombreCompleto: string;
  medicoId: number | null;
  activo: boolean;
  fechaCreacion: string;
};

type UpdateUserPayload = {
  correo: string;
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

const showUsersModal = ref(false);
const usersLoading = ref(false);
const usersError = ref('');
const users = ref<UsuarioResponse[]>([]);
const deletingUserIds = ref<number[]>([]);
const confirmDeleteUserId = ref<number | null>(null);
const confirmDeleteUserLoading = ref(false);
const editingUserId = ref<number | null>(null);
const editLoading = ref(false);
const editErrorMessage = ref('');

const editForm = reactive({
  nombreCompleto: '',
  correo: '',
  medicoId: '',
  activo: true
});

const userPendingDeletion = computed(() =>
  confirmDeleteUserId.value === null
    ? null
    : users.value.find((usuario) => usuario.id === confirmDeleteUserId.value) ?? null
);

const editingUser = computed(() =>
  editingUserId.value === null
    ? null
    : users.value.find((usuario) => usuario.id === editingUserId.value) ?? null
);

const isEditModalOpen = computed(() => editingUserId.value !== null);

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

const parseMedicoIdValue = (value: string): number | null => {
  const trimmed = value.trim();
  if (!trimmed) {
    return null;
  }

  const parsed = Number(trimmed);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error('El identificador del médico debe ser un número entero positivo.');
  }

  return parsed;
};

const parseMedicoId = (): number | null => {
  return parseMedicoIdValue(form.medicoId);
};

const buildPayload = (): CreateUserPayload => ({
  correo: form.correo.trim(),
  password: form.password,
  nombreCompleto: form.nombreCompleto.trim(),
  medicoId: parseMedicoId(),
  activo: form.activo
});

const buildUpdatePayload = (): UpdateUserPayload => {
  const correo = editForm.correo.trim();
  if (correo.length === 0) {
    throw new Error('El correo electrónico es obligatorio.');
  }

  const nombreCompleto = editForm.nombreCompleto.trim();
  if (nombreCompleto.length === 0) {
    throw new Error('El nombre completo es obligatorio.');
  }

  return {
    correo,
    nombreCompleto,
    medicoId: parseMedicoIdValue(editForm.medicoId),
    activo: editForm.activo
  };
};

const resetEditForm = () => {
  editForm.nombreCompleto = '';
  editForm.correo = '';
  editForm.medicoId = '';
  editForm.activo = true;
};

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

const buildAuthHeaders = (): Record<string, string> => {
  const headers: Record<string, string> = {};

  if (authStore.token) {
    headers.Authorization = `Bearer ${authStore.token}`;
  }

  return headers;
};

const fetchUsers = async () => {
  usersError.value = '';
  usersLoading.value = true;

  try {
    if (!authStore.token) {
      throw new Error('Debes iniciar sesión para ver los usuarios.');
    }

    const response = await fetch(`${apiBase}/api/usuarios`, {
      method: 'GET',
      headers: buildAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudieron obtener los usuarios.');
    }

    const data = (await response.json()) as UsuarioResponse[];
    users.value = data;
  } catch (error) {
    users.value = [];
    usersError.value =
      error instanceof Error && error.message.trim().length > 0
        ? error.message
        : 'Ocurrió un error al consultar los usuarios.';
  } finally {
    usersLoading.value = false;
  }
};

const openUsersModal = () => {
  showUsersModal.value = true;
  void fetchUsers();
};

const removeDeletingUserId = (id: number) => {
  deletingUserIds.value = deletingUserIds.value.filter((currentId) => currentId !== id);
};

const performDeleteUser = async (id: number) => {
  deletingUserIds.value = [...deletingUserIds.value, id];
  usersError.value = '';

  try {
    if (!authStore.token) {
      throw new Error('Debes iniciar sesión para eliminar usuarios.');
    }

    const response = await fetch(`${apiBase}/api/usuarios/${id}`, {
      method: 'DELETE',
      headers: buildAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudo eliminar el usuario.');
    }

    await fetchUsers();
  } catch (error) {
    usersError.value =
      error instanceof Error && error.message.trim().length > 0
        ? error.message
        : 'Ocurrió un error al eliminar el usuario.';
  } finally {
    removeDeletingUserId(id);
  }
};

const isDeletingUser = (id: number) => deletingUserIds.value.includes(id);

const promptDeleteUser = (id: number) => {
  if (isDeletingUser(id)) {
    return;
  }

  usersError.value = '';
  confirmDeleteUserId.value = id;
};

const closeDeleteModal = () => {
  if (confirmDeleteUserLoading.value) {
    return;
  }

  confirmDeleteUserId.value = null;
};

const confirmDeleteUser = async () => {
  const id = confirmDeleteUserId.value;
  if (id === null) {
    return;
  }

  confirmDeleteUserLoading.value = true;

  try {
    await performDeleteUser(id);
    confirmDeleteUserId.value = null;
  } finally {
    confirmDeleteUserLoading.value = false;
  }
};

const openEditModal = (usuario: UsuarioResponse) => {
  editingUserId.value = usuario.id;
  editForm.nombreCompleto = usuario.nombreCompleto;
  editForm.correo = usuario.correo;
  editForm.medicoId = usuario.medicoId !== null ? String(usuario.medicoId) : '';
  editForm.activo = usuario.activo;
  editErrorMessage.value = '';
};

const closeEditModal = () => {
  if (editLoading.value) {
    return;
  }

  editingUserId.value = null;
  editErrorMessage.value = '';
  resetEditForm();
};

const closeUsersModal = () => {
  showUsersModal.value = false;
  usersError.value = '';
  closeEditModal();
};

const handleUpdateUser = async () => {
  const id = editingUserId.value;
  if (id === null) {
    return;
  }

  editErrorMessage.value = '';
  successMessage.value = '';

  if (!authStore.token) {
    editErrorMessage.value = 'Debes iniciar sesión para actualizar usuarios.';
    return;
  }

  let payload: UpdateUserPayload;

  try {
    payload = buildUpdatePayload();
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      editErrorMessage.value = error.message;
    } else {
      editErrorMessage.value = 'Revisa los datos ingresados.';
    }
    return;
  }

  editLoading.value = true;

  try {
    const headers = {
      'Content-Type': 'application/json',
      ...buildAuthHeaders()
    };

    const response = await fetch(`${apiBase}/api/usuarios/${id}`, {
      method: 'PUT',
      headers,
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo actualizar el usuario. Intenta nuevamente.';
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

    successMessage.value = 'Usuario actualizado correctamente.';
    await fetchUsers();
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
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-10">
    <div class="mx-auto flex max-w-4xl flex-col gap-8">
      <header class="flex flex-col gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-4">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Administración</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Usuarios</h1>
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
              @click="openUsersModal"
            >
              Ver usuarios
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
    <transition name="fade">
      <div
        v-if="showUsersModal"
        class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 px-4 py-10 backdrop-blur"
      >
        <div class="relative w-full max-w-3xl overflow-hidden rounded-3xl border border-emerald-500/30 bg-slate-950 shadow-2xl">
          <header class="flex items-center justify-between border-b border-emerald-500/20 bg-slate-900/60 px-6 py-4">
            <div>
              <p class="text-xs uppercase tracking-[0.3em] text-emerald-300">Usuarios</p>
              <h2 class="text-xl font-semibold text-white">Listado de usuarios</h2>
            </div>
            <button
              class="rounded-lg border border-emerald-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="closeUsersModal"
            >
              Cerrar
            </button>
          </header>
          <section class="max-h-[70vh] overflow-y-auto px-6 py-5 text-sm text-slate-200">
            <div v-if="usersLoading" class="py-10 text-center text-emerald-200">Cargando usuarios...</div>
            <div v-else>
              <div v-if="usersError" class="mb-4 rounded-xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-rose-100">
                {{ usersError }}
              </div>
              <div v-else-if="users.length === 0" class="py-10 text-center text-slate-400">No hay usuarios registrados.</div>
              <table v-else class="min-w-full divide-y divide-slate-800">
                <thead class="bg-slate-900/70 text-left text-xs uppercase tracking-wider text-slate-400">
                  <tr>
                    <th class="px-4 py-3">Nombre</th>
                    <th class="px-4 py-3">Correo</th>
                    <th class="px-4 py-3">Activo</th>
                    <th class="px-4 py-3 text-right">Acciones</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
                  <tr v-for="usuario in users" :key="usuario.id">
                    <td class="px-4 py-3 font-medium text-white">{{ usuario.nombreCompleto }}</td>
                    <td class="px-4 py-3">{{ usuario.correo }}</td>
                    <td class="px-4 py-3">
                      <span
                        :class="[
                          'rounded-full px-3 py-1 text-xs font-semibold',
                          usuario.activo ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-200'
                        ]"
                      >
                        {{ usuario.activo ? 'Activo' : 'Inactivo' }}
                      </span>
                    </td>
                    <td class="px-4 py-3">
                      <div class="flex items-center justify-end gap-2">
                        <button
                          class="inline-flex items-center justify-center rounded-lg border border-emerald-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                          :disabled="isDeletingUser(usuario.id) || confirmDeleteUserLoading"
                          type="button"
                          @click="openEditModal(usuario)"
                        >
                          Editar
                        </button>
                        <button
                          class="inline-flex items-center justify-center rounded-lg border border-rose-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                          :disabled="isDeletingUser(usuario.id) || confirmDeleteUserLoading"
                          type="button"
                          @click="promptDeleteUser(usuario.id)"
                        >
                          <span v-if="!isDeletingUser(usuario.id)">Eliminar</span>
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
        v-if="isEditModalOpen"
        class="fixed inset-0 z-[55] flex items-center justify-center bg-slate-950/80 px-4 backdrop-blur"
      >
        <div class="w-full max-w-2xl rounded-3xl border border-emerald-500/30 bg-slate-950 p-8 shadow-2xl shadow-emerald-500/20">
          <header class="mb-6">
            <p class="text-xs uppercase tracking-[0.3em] text-emerald-300">Usuarios</p>
            <h3 class="text-2xl font-semibold text-white">Editar usuario</h3>
            <div v-if="editingUser" class="mt-2 text-sm text-slate-300">
              <p class="font-semibold text-white">{{ editingUser.nombreCompleto }}</p>
              <p class="text-xs text-slate-400">{{ editingUser.correo }}</p>
            </div>
          </header>

          <div v-if="editErrorMessage" class="mb-4 rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
            {{ editErrorMessage }}
          </div>

          <form class="grid gap-6 md:grid-cols-2" @submit.prevent="handleUpdateUser">
            <div class="space-y-2 md:col-span-2">
              <label class="block text-sm font-medium text-slate-300" for="editNombreCompleto">Nombre completo</label>
              <input
                id="editNombreCompleto"
                v-model.trim="editForm.nombreCompleto"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="Nombre y apellidos"
                required
                type="text"
              />
            </div>

            <div class="space-y-2 md:col-span-2">
              <label class="block text-sm font-medium text-slate-300" for="editCorreo">Correo electrónico</label>
              <input
                id="editCorreo"
                v-model.trim="editForm.correo"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="correo@ejemplo.com"
                required
                type="email"
              />
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium text-slate-300" for="editMedicoId">Médico asociado (opcional)</label>
              <input
                id="editMedicoId"
                v-model.trim="editForm.medicoId"
                class="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-slate-100 shadow-inner focus:border-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/40"
                placeholder="ID numérico del médico"
                type="text"
              />
              <p class="text-xs text-slate-400">Introduce el identificador numérico del médico si corresponde.</p>
            </div>

            <div class="flex items-center gap-3 md:col-span-2">
              <input
                id="editActivo"
                v-model="editForm.activo"
                class="h-5 w-5 rounded border border-slate-700 bg-slate-900 text-emerald-500 focus:ring-emerald-500/60"
                type="checkbox"
              />
              <label class="text-sm text-slate-300" for="editActivo">Usuario activo</label>
            </div>

            <div class="md:col-span-2 flex items-center gap-3">
              <button
                class="inline-flex flex-1 items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                :disabled="editLoading"
                type="button"
                @click="closeEditModal"
              >
                Cancelar
              </button>
              <button
                class="inline-flex flex-1 items-center justify-center rounded-xl bg-emerald-500 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-950 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:bg-emerald-500/60"
                :disabled="editLoading"
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
    <transition name="fade">
      <div
        v-if="confirmDeleteUserId !== null"
        class="fixed inset-0 z-[60] flex items-center justify-center bg-slate-950/80 px-4 backdrop-blur"
      >
        <div class="w-full max-w-md rounded-3xl border border-emerald-500/30 bg-slate-950 p-8 text-center shadow-2xl shadow-emerald-500/20">
          <h3 class="text-2xl font-semibold text-white">Eliminar usuario</h3>
          <p class="mt-4 text-sm text-slate-300">¿Deseas eliminar este usuario? Esta acción no se puede deshacer.</p>
          <div
            v-if="userPendingDeletion"
            class="mt-5 rounded-2xl border border-slate-800 bg-slate-900/70 px-5 py-4 text-left text-sm text-slate-200"
          >
            <p class="font-semibold text-white">{{ userPendingDeletion.nombreCompleto }}</p>
            <p class="text-xs text-slate-400">{{ userPendingDeletion.correo }}</p>
          </div>
          <div class="mt-6 flex items-center gap-3">
            <button
              class="inline-flex flex-1 items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="confirmDeleteUserLoading"
              type="button"
              @click="closeDeleteModal"
            >
              Cancelar
            </button>
            <button
              class="inline-flex flex-1 items-center justify-center rounded-xl border border-rose-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10 disabled:cursor-not-allowed disabled:opacity-60"
              :disabled="confirmDeleteUserLoading"
              type="button"
              @click="confirmDeleteUser"
            >
              <span v-if="!confirmDeleteUserLoading">Eliminar</span>
              <span v-else>Eliminando...</span>
            </button>
          </div>
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
