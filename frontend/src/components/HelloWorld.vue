<script setup lang="ts">
import { computed, ref } from 'vue';
import { useCounterStore } from '@/stores/counter';

defineProps<{
  msg: string;
}>();

const counter = useCounterStore();
const doubledCount = computed(() => counter.count * 2);
const name = ref('Vue');
</script>

<template>
  <div class="flex flex-col items-center gap-4 rounded-xl border border-slate-800 bg-slate-900/60 p-8 shadow-lg">
    <h2 class="text-2xl font-semibold text-emerald-300">{{ msg }}</h2>
    <p class="text-slate-300">
      Contador global: <span class="font-bold text-emerald-400">{{ counter.count }}</span>
      (doble: <span class="font-bold text-emerald-400">{{ doubledCount }}</span>)
    </p>
    <div class="flex flex-wrap items-center justify-center gap-3">
      <button
        class="rounded-lg bg-emerald-500 px-4 py-2 font-semibold text-slate-950 transition hover:bg-emerald-400"
        type="button"
        @click="counter.increment()"
      >
        Incrementar
      </button>
      <button
        class="rounded-lg border border-emerald-500 px-4 py-2 font-semibold text-emerald-300 transition hover:bg-emerald-500/10"
        type="button"
        @click="counter.reset()"
      >
        Reiniciar
      </button>
    </div>
    <label class="text-slate-300">
      <span class="mr-2">¿Cómo te llamas?</span>
      <input
        v-model="name"
        class="rounded-md border border-slate-700 bg-slate-800 px-3 py-2 text-slate-100 focus:border-emerald-400 focus:outline-none"
        placeholder="Introduce tu nombre"
        type="text"
      />
    </label>
    <p class="text-slate-400">Encantado de conocerte, {{ name || 'amigo' }} 👋</p>
    <RouterLink
      class="text-sm font-semibold uppercase tracking-wider text-emerald-400 hover:text-emerald-300"
      to="/about"
    >
      Ir a la página de ejemplo
    </RouterLink>
  </div>
</template>
