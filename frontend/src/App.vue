<script setup lang="ts">
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-900 via-slate-950 to-black">
    <section class="mx-auto flex max-w-3xl flex-col items-center gap-6 px-4 py-16 text-center">
      <img alt="Vue logo" class="h-24 w-24" src="/vite.svg" />
      <h1 class="text-4xl font-bold tracking-tight text-emerald-400 sm:text-5xl">
        Bienvenido a tu nueva aplicación Vue 3 + TypeScript
      </h1>
      <p class="text-lg text-slate-300">
        Este proyecto está listo para usarse con Tailwind CSS, Vue Router y Pinia. Empieza editando
        <span class="font-mono text-emerald-300">src/components/HelloWorld.vue</span>.
      </p>
      <HelloWorld msg="¡Hola desde Vue!" />
    </section>
  </main>
</template>

<style scoped>
main {
  font-family: 'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
}
</style>
