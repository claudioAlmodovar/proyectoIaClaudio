<template>
  <RouterView />
</template>

<style scoped>
:global(body) {
  background-color: #020617;
  background-image: linear-gradient(rgba(2, 6, 23, 0.8), rgba(2, 6, 23, 0.9)),
    url('https://images.unsplash.com/photo-1580281658629-54d6c9044070?auto=format&fit=crop&w=1920&q=80');
  background-size: cover;
  background-position: center;
  background-attachment: fixed;
  color: #e2e8f0;
  font-family: 'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  min-height: 100vh;
}
</style>
