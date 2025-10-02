<script setup lang="ts">
const hotdogs = [
  {
    name: 'Clásico',
    description: 'Salchicha 100% res, cebolla crujiente y aderezo de la casa.',
  },
  {
    name: 'Mexicano',
    description: 'Jalapeños, guacamole y queso fundido sobre nuestro pan artesanal.',
  },
  {
    name: 'BBQ Ahumado',
    description: 'Tocino caramelizado, salsa BBQ y col rallada dulce.',
  },
  {
    name: 'Tex-Mex',
    description: 'Frijoles refritos, pico de gallo y crema agria.',
  },
  {
    name: 'Veggie Power',
    description: 'Salchicha vegetal, verduras asadas y mayo de chipotle.',
  },
  {
    name: 'Doble Queso',
    description: 'Queso cheddar y queso suizo derretido con cebollín fresco.',
  },
];
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-amber-950 via-stone-950 to-lime-900 text-stone-100">
    <div class="mx-auto flex min-h-screen max-w-6xl flex-col px-6 py-10">
      <header class="flex flex-col gap-6 rounded-3xl bg-stone-950/60 p-8 shadow-2xl shadow-amber-900/30 backdrop-blur">
        <div class="flex flex-col items-center justify-between gap-4 text-center md:flex-row md:text-left">
          <div>
            <p class="text-sm uppercase tracking-[0.4em] text-lime-200">Bienvenido a</p>
            <h1 class="text-4xl font-extrabold text-amber-300 sm:text-5xl md:text-6xl">KamaronFils</h1>
            <p class="mt-3 text-lg text-stone-300 md:max-w-xl">
              El lugar donde cada mordida es una fiesta. Descubre nuestros hotdogs artesanales, hechos con pan fresco y
              ingredientes vibrantes.
            </p>
          </div>
          <div class="flex flex-col gap-3 sm:flex-row">
            <button
              class="w-full rounded-full bg-lime-600 px-6 py-3 text-sm font-semibold uppercase tracking-wide text-white shadow-lg transition hover:bg-lime-500 hover:shadow-lime-500/40"
              type="button"
            >
              Iniciar sesión
            </button>
            <button
              class="w-full rounded-full border border-amber-300 px-6 py-3 text-sm font-semibold uppercase tracking-wide text-amber-200 transition hover:border-amber-200 hover:bg-amber-500/10"
              type="button"
            >
              Hacer pedido
            </button>
          </div>
        </div>
        <div class="grid gap-6 rounded-2xl bg-gradient-to-r from-lime-900/30 via-stone-900/60 to-amber-900/40 p-6 md:grid-cols-3">
          <div class="flex flex-col gap-2">
            <h2 class="text-lg font-semibold text-lime-200">Horario</h2>
            <p class="text-sm text-stone-300">Lunes a Domingo · 12:00 pm - 12:00 am</p>
          </div>
          <div class="flex flex-col gap-2">
            <h2 class="text-lg font-semibold text-lime-200">Ubicación</h2>
            <p class="text-sm text-stone-300">Av. del Sabor 123, Ciudad del Antojo</p>
          </div>
          <div class="flex flex-col gap-2">
            <h2 class="text-lg font-semibold text-lime-200">Contacto</h2>
            <p class="text-sm text-stone-300">Tel. (555) 123-4567 · pedidos@kamaronfils.mx</p>
          </div>
        </div>
      </header>

      <section class="mt-12 rounded-3xl bg-stone-950/50 p-8 shadow-2xl shadow-amber-900/20 backdrop-blur">
        <div class="flex flex-col gap-6 text-center">
          <div>
            <p class="text-sm uppercase tracking-[0.3em] text-lime-200">Catálogo</p>
            <h2 class="text-3xl font-bold text-stone-100 sm:text-4xl">Hotdogs a solo $10 MXN</h2>
            <p class="mt-2 text-stone-300">
              Elige tu favorito y disfruta de una combinación perfecta de sabores. Todos incluyen papas crujientes.
            </p>
          </div>
          <div class="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
            <article
              v-for="hotdog in hotdogs"
              :key="hotdog.name"
              class="flex h-full flex-col justify-between rounded-2xl border border-lime-900/40 bg-stone-900/60 p-6 text-left shadow-lg shadow-stone-950/40 transition hover:-translate-y-1 hover:border-amber-400/80 hover:shadow-amber-900/40"
            >
              <div>
                <h3 class="text-2xl font-semibold text-amber-300">{{ hotdog.name }}</h3>
                <p class="mt-2 text-sm text-stone-300">{{ hotdog.description }}</p>
              </div>
              <p class="mt-6 text-lg font-bold text-lime-200">$10.00 MXN</p>
            </article>
          </div>
        </div>
      </section>

      <section class="mt-12 grid gap-8 rounded-3xl bg-gradient-to-r from-amber-900/40 via-stone-900/50 to-lime-900/40 p-8 text-center md:grid-cols-3">
        <div class="flex flex-col items-center gap-3">
          <span class="flex h-14 w-14 items-center justify-center rounded-full bg-amber-500/20 text-2xl">🌭</span>
          <h3 class="text-lg font-semibold text-stone-100">Ingredientes Premium</h3>
          <p class="text-sm text-stone-300">Seleccionamos los mejores ingredientes para garantizar sabor y calidad en cada hotdog.</p>
        </div>
        <div class="flex flex-col items-center gap-3">
          <span class="flex h-14 w-14 items-center justify-center rounded-full bg-lime-500/20 text-2xl">⚡</span>
          <h3 class="text-lg font-semibold text-stone-100">Servicio Rápido</h3>
          <p class="text-sm text-stone-300">Haz tu pedido en minutos y recíbelo caliente, recién salido de la parrilla.</p>
        </div>
        <div class="flex flex-col items-center gap-3">
          <span class="flex h-14 w-14 items-center justify-center rounded-full bg-rose-500/20 text-2xl">🎉</span>
          <h3 class="text-lg font-semibold text-stone-100">Ambiente Inolvidable</h3>
          <p class="text-sm text-stone-300">Comparte con amigos en un espacio creado para disfrutar, celebrar y saborear.</p>
        </div>
      </section>

      <footer class="mt-12 rounded-3xl border border-lime-900/30 bg-stone-950/40 p-6 text-center text-sm text-stone-400">
        © {{ new Date().getFullYear() }} KamaronFils. Todos los derechos reservados.
      </footer>
    </div>
  </main>
</template>

<style scoped>
main {
  font-family: 'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
}
</style>
