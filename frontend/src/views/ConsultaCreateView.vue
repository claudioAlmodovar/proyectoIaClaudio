<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

type PacienteResponse = {
  id: number;
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  telefono: string | null;
  activo: boolean;
  fechaCreacion: string;
};

type CreatePacientePayload = {
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  telefono: string | null;
  activo: boolean;
};

type ConsultaResponse = {
  id: number;
  medicoId: number;
  pacienteId: number;
  fechaConsulta: string;
  sintomas: string | null;
  recomendaciones: string | null;
  diagnostico: string | null;
};

type CreateConsultaPayload = {
  medicoId: number;
  pacienteId: number;
  fechaConsulta: string;
  sintomas: string | null;
  recomendaciones: string | null;
  diagnostico: string | null;
};

const router = useRouter();
const authStore = useAuthStore();

const apiBase = import.meta.env.VITE_API_BASE ?? 'https://localhost:59831';

const pacientes = ref<PacienteResponse[]>([]);
const pacientesLoading = ref(false);
const pacientesError = ref('');

const searchPaciente = ref('');
const selectedPacienteId = ref<number | null>(null);

const showPacientesModal = ref(false);
const pacienteModalError = ref('');
const pacienteModalSuccess = ref('');
const pacienteModalLoading = ref(false);
const editingPacienteId = ref<number | null>(null);
const confirmDeletePacienteId = ref<number | null>(null);
const confirmDeleteLoading = ref(false);

const consultaForm = reactive({
  observaciones: '',
  receta: '',
  diagnostico: ''
});

const consultaError = ref('');
const consultaSuccess = ref('');
const savingConsulta = ref(false);

const historialConsultas = ref<ConsultaResponse[]>([]);
const historialLoading = ref(false);
const historialError = ref('');
const showHistorialModal = ref(false);

const pacienteForm = reactive({
  primerNombre: '',
  segundoNombre: '',
  apellidoPaterno: '',
  apellidoMaterno: '',
  telefono: '',
  activo: true
});

const buildHeaders = () => {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };

  if (authStore.token) {
    headers.Authorization = `Bearer ${authStore.token}`;
  }

  return headers;
};

const fetchPacientes = async (withSpinner = true) => {
  if (withSpinner) {
    pacientesLoading.value = true;
  }
  pacientesError.value = '';

  try {
    const response = await fetch(`${apiBase}/api/pacientes`, {
      headers: buildHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudieron obtener los pacientes.');
    }

    const data = (await response.json()) as PacienteResponse[];
    pacientes.value = data;
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      pacientesError.value = error.message;
    } else {
      pacientesError.value = 'Ocurrió un error inesperado al cargar los pacientes.';
    }
  } finally {
    if (withSpinner) {
      pacientesLoading.value = false;
    }
  }
};

void fetchPacientes();

watch(pacientes, (lista) => {
  if (selectedPacienteId.value === null) {
    return;
  }

  const stillExists = lista.some((paciente) => paciente.id === selectedPacienteId.value);
  if (!stillExists) {
    selectedPacienteId.value = null;
  }
});

const formatPacienteNombre = (paciente: PacienteResponse): string => {
  const nombres = [paciente.primerNombre, paciente.segundoNombre].filter(
    (valor): valor is string => typeof valor === 'string' && valor.trim().length > 0
  );
  const apellidos = [paciente.apellidoPaterno, paciente.apellidoMaterno].filter(
    (valor): valor is string => typeof valor === 'string' && valor.trim().length > 0
  );

  return [...nombres, ...apellidos].join(' ');
};

const filteredPacientes = computed(() => {
  const term = searchPaciente.value.trim().toLowerCase();
  if (term.length === 0) {
    return pacientes.value.slice(0, 10);
  }

  return pacientes.value
    .filter((paciente) => {
      const nombre = formatPacienteNombre(paciente).toLowerCase();
      const telefono = paciente.telefono?.toLowerCase() ?? '';
      return nombre.includes(term) || telefono.includes(term);
    })
    .slice(0, 10);
});

const selectedPaciente = computed(() => {
  if (selectedPacienteId.value === null) {
    return null;
  }

  return pacientes.value.find((paciente) => paciente.id === selectedPacienteId.value) ?? null;
});

const selectPaciente = (paciente: PacienteResponse) => {
  selectedPacienteId.value = paciente.id;
  searchPaciente.value = formatPacienteNombre(paciente);
  consultaSuccess.value = '';
  consultaError.value = '';
};

const clearPacienteSelection = () => {
  selectedPacienteId.value = null;
  consultaSuccess.value = '';
  consultaError.value = '';
};

const resetPacienteForm = () => {
  pacienteForm.primerNombre = '';
  pacienteForm.segundoNombre = '';
  pacienteForm.apellidoPaterno = '';
  pacienteForm.apellidoMaterno = '';
  pacienteForm.telefono = '';
  pacienteForm.activo = true;
};

const openPacienteModalForCreate = () => {
  resetPacienteForm();
  editingPacienteId.value = null;
  pacienteModalError.value = '';
  pacienteModalSuccess.value = '';
  showPacientesModal.value = true;
};

const openPacienteModalForEdit = (paciente: PacienteResponse) => {
  editingPacienteId.value = paciente.id;
  pacienteForm.primerNombre = paciente.primerNombre;
  pacienteForm.segundoNombre = paciente.segundoNombre ?? '';
  pacienteForm.apellidoPaterno = paciente.apellidoPaterno;
  pacienteForm.apellidoMaterno = paciente.apellidoMaterno ?? '';
  pacienteForm.telefono = paciente.telefono ?? '';
  pacienteForm.activo = paciente.activo;
  pacienteModalError.value = '';
  pacienteModalSuccess.value = '';
  showPacientesModal.value = true;
};

const closePacientesModal = () => {
  showPacientesModal.value = false;
  editingPacienteId.value = null;
  confirmDeletePacienteId.value = null;
};

const buildPacientePayload = (): CreatePacientePayload => {
  const primerNombre = pacienteForm.primerNombre.trim();
  const apellidoPaterno = pacienteForm.apellidoPaterno.trim();

  if (primerNombre.length === 0) {
    throw new Error('El primer nombre es obligatorio.');
  }

  if (apellidoPaterno.length === 0) {
    throw new Error('El apellido paterno es obligatorio.');
  }

  const segundoNombre = pacienteForm.segundoNombre.trim();
  const apellidoMaterno = pacienteForm.apellidoMaterno.trim();
  const telefono = pacienteForm.telefono.trim();

  return {
    primerNombre,
    segundoNombre: segundoNombre.length === 0 ? null : segundoNombre,
    apellidoPaterno,
    apellidoMaterno: apellidoMaterno.length === 0 ? null : apellidoMaterno,
    telefono: telefono.length === 0 ? null : telefono,
    activo: pacienteForm.activo
  };
};

const handlePacienteSubmit = async () => {
  pacienteModalError.value = '';
  pacienteModalSuccess.value = '';

  let payload: CreatePacientePayload;
  try {
    payload = buildPacientePayload();
  } catch (error) {
    if (error instanceof Error) {
      pacienteModalError.value = error.message;
    } else {
      pacienteModalError.value = 'Revisa la información del paciente.';
    }
    return;
  }

  pacienteModalLoading.value = true;

  try {
    const isEdit = editingPacienteId.value !== null;
    const url = isEdit
      ? `${apiBase}/api/pacientes/${editingPacienteId.value}`
      : `${apiBase}/api/pacientes`;
    const method = isEdit ? 'PUT' : 'POST';

    const response = await fetch(url, {
      method,
      headers: buildHeaders(),
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo guardar el paciente.';
      try {
        const errorBody = (await response.json()) as Partial<{ message: string; detail: string }>;
        const possibleMessages = [errorBody?.message, errorBody?.detail].filter(
          (value): value is string => typeof value === 'string' && value.trim().length > 0
        );
        if (possibleMessages.length > 0) {
          apiMessage = possibleMessages.join(' ');
        }
      } catch (parseError) {
        // Se mantiene el mensaje por defecto.
      }

      throw new Error(apiMessage);
    }

    await fetchPacientes(false);
    pacienteModalSuccess.value = isEdit ? 'Paciente actualizado correctamente.' : 'Paciente creado correctamente.';
    if (!isEdit) {
      resetPacienteForm();
    }
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      pacienteModalError.value = error.message;
    } else {
      pacienteModalError.value = 'Ocurrió un error al guardar la información del paciente.';
    }
  } finally {
    pacienteModalLoading.value = false;
  }
};

const pacientePendingDeletion = computed(() => {
  if (confirmDeletePacienteId.value === null) {
    return null;
  }

  return pacientes.value.find((paciente) => paciente.id === confirmDeletePacienteId.value) ?? null;
});

const requestDeletePaciente = (paciente: PacienteResponse) => {
  confirmDeletePacienteId.value = paciente.id;
  pacienteModalError.value = '';
  pacienteModalSuccess.value = '';
};

const cancelDeletePaciente = () => {
  confirmDeletePacienteId.value = null;
};

const performDeletePaciente = async () => {
  if (pacientePendingDeletion.value === null) {
    return;
  }

  confirmDeleteLoading.value = true;
  pacienteModalError.value = '';
  pacienteModalSuccess.value = '';

  try {
    const response = await fetch(`${apiBase}/api/pacientes/${pacientePendingDeletion.value.id}`, {
      method: 'DELETE',
      headers: buildHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudo eliminar el paciente.');
    }

    pacientes.value = pacientes.value.filter((paciente) => paciente.id !== pacientePendingDeletion.value?.id);
    if (selectedPacienteId.value === pacientePendingDeletion.value.id) {
      selectedPacienteId.value = null;
    }

    pacienteModalSuccess.value = 'Paciente eliminado correctamente.';
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      pacienteModalError.value = error.message;
    } else {
      pacienteModalError.value = 'Ocurrió un error al eliminar el paciente.';
    }
  } finally {
    confirmDeleteLoading.value = false;
    confirmDeletePacienteId.value = null;
  }
};

const canSaveConsulta = computed(() => {
  return (
    selectedPacienteId.value !== null &&
    consultaForm.diagnostico.trim().length > 0 &&
    !savingConsulta.value
  );
});

const resetConsultaForm = () => {
  consultaForm.observaciones = '';
  consultaForm.receta = '';
  consultaForm.diagnostico = '';
};

const handleGuardarConsulta = async () => {
  consultaError.value = '';
  consultaSuccess.value = '';

  if (selectedPacienteId.value === null) {
    consultaError.value = 'Selecciona un paciente para continuar.';
    return;
  }

  const medicoId = authStore.user?.medicoId ?? null;
  if (medicoId === null) {
    consultaError.value = 'Tu usuario no tiene un médico asignado. Comunícate con el administrador.';
    return;
  }

  const payload: CreateConsultaPayload = {
    medicoId,
    pacienteId: selectedPacienteId.value,
    fechaConsulta: new Date().toISOString(),
    sintomas: consultaForm.observaciones.trim().length === 0 ? null : consultaForm.observaciones.trim(),
    recomendaciones: consultaForm.receta.trim().length === 0 ? null : consultaForm.receta.trim(),
    diagnostico: consultaForm.diagnostico.trim()
  };

  savingConsulta.value = true;

  try {
    const response = await fetch(`${apiBase}/api/consultas`, {
      method: 'POST',
      headers: buildHeaders(),
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let apiMessage = 'No se pudo guardar la consulta médica.';
      try {
        const errorBody = (await response.json()) as Partial<{ message: string; detail: string }>;
        const possibleMessages = [errorBody?.message, errorBody?.detail].filter(
          (value): value is string => typeof value === 'string' && value.trim().length > 0
        );
        if (possibleMessages.length > 0) {
          apiMessage = possibleMessages.join(' ');
        }
      } catch (parseError) {
        // Mantener mensaje por defecto.
      }

      throw new Error(apiMessage);
    }

    consultaSuccess.value = 'Consulta médica guardada correctamente.';
    resetConsultaForm();
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      consultaError.value = error.message;
    } else {
      consultaError.value = 'Ocurrió un error al guardar la consulta médica.';
    }
  } finally {
    savingConsulta.value = false;
  }
};

const formatFecha = (isoDate: string): string => {
  const date = new Date(isoDate);
  return date.toLocaleString('es-MX', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const fetchHistorialConsultas = async () => {
  if (selectedPacienteId.value === null) {
    return;
  }

  historialLoading.value = true;
  historialError.value = '';

  try {
    const response = await fetch(`${apiBase}/api/consultas`, {
      headers: buildHeaders()
    });

    if (!response.ok) {
      throw new Error('No se pudo obtener el historial de consultas.');
    }

    const data = (await response.json()) as ConsultaResponse[];
    historialConsultas.value = data
      .filter((consulta) => consulta.pacienteId === selectedPacienteId.value)
      .sort((a, b) => new Date(b.fechaConsulta).getTime() - new Date(a.fechaConsulta).getTime());
  } catch (error) {
    if (error instanceof Error && error.message.trim().length > 0) {
      historialError.value = error.message;
    } else {
      historialError.value = 'Ocurrió un error al cargar el historial de consultas.';
    }
  } finally {
    historialLoading.value = false;
  }
};

const openHistorialModal = () => {
  if (selectedPacienteId.value === null) {
    return;
  }

  showHistorialModal.value = true;
  void fetchHistorialConsultas();
};

const closeHistorialModal = () => {
  showHistorialModal.value = false;
};

const goBack = () => {
  router.push({ name: 'dashboard' });
};
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-950 px-4 py-8">
    <div class="mx-auto flex max-w-5xl flex-col gap-8">
      <header class="flex flex-col gap-6 rounded-3xl border border-emerald-500/20 bg-slate-950/80 p-8 shadow-2xl shadow-emerald-500/10 backdrop-blur">
        <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-emerald-300">Consultas médicas</p>
            <h1 class="text-3xl font-bold text-white md:text-4xl">Registrar nueva consulta</h1>
            <p class="mt-3 max-w-2xl text-sm text-slate-300">
              Gestiona pacientes, captura el diagnóstico y registra las indicaciones médicas realizadas durante la cita.
            </p>
          </div>
          <div class="flex flex-col items-start gap-3 md:items-end">
            <button
              class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
              type="button"
              @click="goBack"
            >
              Volver al panel
            </button>
            <button
              class="inline-flex items-center justify-center rounded-xl bg-emerald-500 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-white shadow-md shadow-emerald-500/30 transition hover:bg-emerald-400"
              type="button"
              @click="openPacienteModalForCreate"
            >
              Gestionar pacientes
            </button>
          </div>
        </div>
      </header>

      <section class="space-y-6 rounded-3xl border border-slate-800/80 bg-slate-950/70 p-8 shadow-xl shadow-slate-950/40">
        <div class="space-y-2">
          <label class="text-sm font-semibold uppercase tracking-wide text-emerald-200" for="buscar-paciente">
            Buscar paciente
          </label>
          <input
            id="buscar-paciente"
            v-model="searchPaciente"
            type="text"
            placeholder="Escribe el nombre o teléfono"
            class="w-full rounded-2xl border border-slate-800 bg-slate-900/70 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
          />
          <p v-if="pacientesError" class="text-sm text-rose-400">{{ pacientesError }}</p>
        </div>

        <div v-if="pacientesLoading" class="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-center text-sm text-slate-300">
          Cargando pacientes...
        </div>

        <ul v-else class="grid gap-3 md:grid-cols-2">
          <li
            v-for="paciente in filteredPacientes"
            :key="paciente.id"
            class="flex cursor-pointer flex-col gap-2 rounded-2xl border border-slate-800 bg-slate-900/60 p-4 transition hover:border-emerald-500/60 hover:bg-emerald-500/5"
            @click="selectPaciente(paciente)"
          >
            <p class="text-sm font-semibold text-white">{{ formatPacienteNombre(paciente) }}</p>
            <p class="text-xs text-slate-400">Teléfono: {{ paciente.telefono ?? 'No registrado' }}</p>
            <span
              class="inline-flex w-fit items-center gap-2 rounded-full px-3 py-1 text-[11px] font-semibold"
              :class="[
                paciente.activo
                  ? 'bg-emerald-500/10 text-emerald-300'
                  : 'bg-rose-500/10 text-rose-300'
              ]"
            >
              <span class="h-2 w-2 rounded-full" :class="paciente.activo ? 'bg-emerald-400' : 'bg-rose-400'"></span>
              {{ paciente.activo ? 'Activo' : 'Inactivo' }}
            </span>
          </li>
        </ul>

        <div
          v-if="selectedPaciente"
          class="space-y-6 rounded-3xl border border-emerald-500/20 bg-emerald-500/5 p-6 shadow-inner shadow-emerald-500/10"
        >
          <div class="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
            <div>
              <p class="text-sm uppercase tracking-[0.3em] text-emerald-200">Paciente seleccionado</p>
              <h2 class="mt-2 text-xl font-semibold text-white">{{ formatPacienteNombre(selectedPaciente) }}</h2>
              <p class="text-sm text-slate-300">Teléfono: {{ selectedPaciente.telefono ?? 'No registrado' }}</p>
            </div>
            <div class="flex flex-wrap gap-3">
              <button
                class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
                type="button"
                @click="openHistorialModal"
              >
                Historial consultas
              </button>
              <button
                class="inline-flex items-center justify-center rounded-xl border border-rose-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10"
                type="button"
                @click="clearPacienteSelection"
              >
                Quitar selección
              </button>
              <button
                class="inline-flex items-center justify-center rounded-xl border border-emerald-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
                type="button"
                @click="openPacienteModalForEdit(selectedPaciente)"
              >
                Editar paciente
              </button>
            </div>
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <label class="flex flex-col gap-2 text-sm text-slate-200">
              <span class="font-semibold text-emerald-200">Observaciones médicas</span>
              <textarea
                v-model="consultaForm.observaciones"
                class="min-h-[120px] rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
                placeholder="Notas generales y síntomas reportados"
              ></textarea>
            </label>
            <label class="flex flex-col gap-2 text-sm text-slate-200">
              <span class="font-semibold text-emerald-200">Receta / Medicamentos</span>
              <textarea
                v-model="consultaForm.receta"
                class="min-h-[120px] rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
                placeholder="Indicaciones de medicamentos y dosificación"
              ></textarea>
            </label>
          </div>

          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Diagnóstico</span>
            <textarea
              v-model="consultaForm.diagnostico"
              class="min-h-[140px] rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
              placeholder="Conclusiones médicas de la consulta"
            ></textarea>
          </label>

          <div class="space-y-3">
            <p v-if="consultaError" class="text-sm text-rose-400">{{ consultaError }}</p>
            <p v-if="consultaSuccess" class="text-sm text-emerald-300">{{ consultaSuccess }}</p>
            <button
              class="inline-flex w-full items-center justify-center rounded-2xl bg-emerald-500 px-6 py-3 text-sm font-semibold uppercase tracking-wide text-white shadow-lg shadow-emerald-500/30 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:bg-emerald-500/50"
              type="button"
              :disabled="!canSaveConsulta"
              @click="handleGuardarConsulta"
            >
              {{ savingConsulta ? 'Guardando...' : 'Guardar consulta' }}
            </button>
          </div>
        </div>

        <div v-else class="rounded-3xl border border-slate-800 bg-slate-900/60 p-6 text-center text-sm text-slate-300">
          Selecciona un paciente para registrar la consulta.
        </div>
      </section>
    </div>

    <section
      v-if="showPacientesModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 p-4 backdrop-blur"
      role="dialog"
      aria-modal="true"
    >
      <div class="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-3xl border border-emerald-500/30 bg-slate-950 p-6 shadow-2xl shadow-emerald-500/20">
        <header class="mb-6 flex items-start justify-between">
          <div>
            <h2 class="text-xl font-semibold text-white">
              {{ editingPacienteId === null ? 'Registrar paciente' : 'Editar paciente' }}
            </h2>
            <p class="mt-1 text-sm text-slate-300">
              Completa la información para crear o actualizar pacientes del consultorio.
            </p>
          </div>
          <button
            class="rounded-full border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-300 transition hover:bg-slate-800"
            type="button"
            @click="closePacientesModal"
          >
            Cerrar
          </button>
        </header>

        <form class="grid gap-4 md:grid-cols-2" @submit.prevent="handlePacienteSubmit">
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Primer nombre *</span>
            <input
              v-model="pacienteForm.primerNombre"
              type="text"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            />
          </label>
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Segundo nombre</span>
            <input
              v-model="pacienteForm.segundoNombre"
              type="text"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            />
          </label>
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Apellido paterno *</span>
            <input
              v-model="pacienteForm.apellidoPaterno"
              type="text"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            />
          </label>
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Apellido materno</span>
            <input
              v-model="pacienteForm.apellidoMaterno"
              type="text"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            />
          </label>
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Teléfono</span>
            <input
              v-model="pacienteForm.telefono"
              type="text"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            />
          </label>
          <label class="flex flex-col gap-2 text-sm text-slate-200">
            <span class="font-semibold text-emerald-200">Estado</span>
            <select
              v-model="pacienteForm.activo"
              class="rounded-2xl border border-slate-800 bg-slate-900/80 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-emerald-400 focus:ring-1 focus:ring-emerald-400"
            >
              <option :value="true">Activo</option>
              <option :value="false">Inactivo</option>
            </select>
          </label>

          <div class="md:col-span-2 space-y-3">
            <p v-if="pacienteModalError" class="text-sm text-rose-400">{{ pacienteModalError }}</p>
            <p v-if="pacienteModalSuccess" class="text-sm text-emerald-300">{{ pacienteModalSuccess }}</p>
            <button
              class="inline-flex w-full items-center justify-center rounded-2xl bg-emerald-500 px-6 py-3 text-sm font-semibold uppercase tracking-wide text-white shadow-lg shadow-emerald-500/30 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:bg-emerald-500/50"
              type="submit"
              :disabled="pacienteModalLoading"
            >
              {{ pacienteModalLoading ? 'Guardando...' : editingPacienteId === null ? 'Crear paciente' : 'Actualizar paciente' }}
            </button>
          </div>
        </form>

        <section class="mt-8 space-y-4">
          <header class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-white">Pacientes registrados</h3>
            <button
              class="text-xs font-semibold uppercase tracking-wide text-emerald-300 underline-offset-4 hover:underline"
              type="button"
              @click="fetchPacientes(false)"
            >
              Actualizar lista
            </button>
          </header>

          <div v-if="pacientes.length === 0" class="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-center text-sm text-slate-300">
            No hay pacientes registrados por el momento.
          </div>

          <ul v-else class="space-y-3">
            <li
              v-for="paciente in pacientes"
              :key="paciente.id"
              class="flex flex-col gap-3 rounded-2xl border border-slate-800 bg-slate-900/60 p-4 md:flex-row md:items-center md:justify-between"
            >
              <div>
                <p class="text-sm font-semibold text-white">{{ formatPacienteNombre(paciente) }}</p>
                <p class="text-xs text-slate-400">Teléfono: {{ paciente.telefono ?? 'No registrado' }}</p>
              </div>
              <div class="flex flex-wrap gap-2">
                <button
                  class="rounded-xl border border-emerald-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-emerald-200 transition hover:bg-emerald-500/10"
                  type="button"
                  @click="openPacienteModalForEdit(paciente)"
                >
                  Editar
                </button>
                <button
                  class="rounded-xl border border-rose-400/60 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/10"
                  type="button"
                  @click="requestDeletePaciente(paciente)"
                >
                  Eliminar
                </button>
              </div>
            </li>
          </ul>
        </section>

        <section v-if="pacientePendingDeletion" class="mt-6 rounded-2xl border border-rose-500/40 bg-rose-500/10 p-5">
          <p class="text-sm text-rose-100">
            ¿Deseas eliminar al paciente
            <strong>{{ formatPacienteNombre(pacientePendingDeletion) }}</strong>? Esta acción no se puede deshacer.
          </p>
          <div class="mt-4 flex flex-wrap gap-3">
            <button
              class="rounded-xl border border-rose-400/60 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-rose-200 transition hover:bg-rose-500/20"
              type="button"
              :disabled="confirmDeleteLoading"
              @click="performDeletePaciente"
            >
              {{ confirmDeleteLoading ? 'Eliminando...' : 'Eliminar paciente' }}
            </button>
            <button
              class="rounded-xl border border-slate-600 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-200 transition hover:bg-slate-800"
              type="button"
              :disabled="confirmDeleteLoading"
              @click="cancelDeletePaciente"
            >
              Cancelar
            </button>
          </div>
        </section>
      </div>
    </section>

    <section
      v-if="showHistorialModal"
      class="fixed inset-0 z-40 flex items-center justify-center bg-slate-950/80 p-4 backdrop-blur"
      role="dialog"
      aria-modal="true"
    >
      <div class="max-h-[85vh] w-full max-w-2xl overflow-y-auto rounded-3xl border border-emerald-500/30 bg-slate-950 p-6 shadow-2xl shadow-emerald-500/20">
        <header class="mb-6 flex items-start justify-between">
          <div>
            <h2 class="text-xl font-semibold text-white">Historial de consultas</h2>
            <p class="mt-1 text-sm text-slate-300">
              Registros asociados a {{ selectedPaciente ? formatPacienteNombre(selectedPaciente) : 'el paciente' }}.
            </p>
          </div>
          <button
            class="rounded-full border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-300 transition hover:bg-slate-800"
            type="button"
            @click="closeHistorialModal"
          >
            Cerrar
          </button>
        </header>

        <div v-if="historialLoading" class="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-center text-sm text-slate-300">
          Cargando historial...
        </div>

        <p v-else-if="historialError" class="rounded-2xl border border-rose-500/40 bg-rose-500/10 p-4 text-sm text-rose-100">
          {{ historialError }}
        </p>

        <div v-else>
          <div v-if="historialConsultas.length === 0" class="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 text-center text-sm text-slate-300">
            No se han registrado consultas para este paciente.
          </div>
          <ul v-else class="space-y-3">
            <li
              v-for="consulta in historialConsultas"
              :key="consulta.id"
              class="rounded-2xl border border-slate-800 bg-slate-900/60 p-4"
            >
              <p class="text-xs uppercase tracking-wide text-emerald-200">{{ formatFecha(consulta.fechaConsulta) }}</p>
              <p class="mt-2 text-sm text-slate-200"><strong>Diagnóstico:</strong> {{ consulta.diagnostico ?? 'Sin registro' }}</p>
              <p class="mt-1 text-xs text-slate-300"><strong>Observaciones:</strong> {{ consulta.sintomas ?? 'Sin registro' }}</p>
              <p class="mt-1 text-xs text-slate-300"><strong>Receta:</strong> {{ consulta.recomendaciones ?? 'Sin registro' }}</p>
            </li>
          </ul>
        </div>
      </div>
    </section>
  </main>
</template>
