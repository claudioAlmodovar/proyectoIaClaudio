<template>
  <div class="flex min-h-screen flex-col items-center justify-center gap-6 bg-slate-950 px-6 py-16 text-center text-slate-200">
    <h2 class="text-3xl font-bold text-emerald-400">Página de ejemplo</h2>
    <p class="max-w-xl text-base text-slate-300">
      Esta es una ruta adicional configurada con Vue Router. Utiliza Tailwind CSS para definir estilos
      utilitarios y demuestra cómo navegar entre páginas en una aplicación Vue 3.
    </p>
    <RouterLink
      class="rounded-md bg-emerald-500 px-4 py-2 font-semibold text-slate-950 transition hover:bg-emerald-400"
      to="/"
    >
      Volver al inicio
    </RouterLink>
  </div>
</template>
