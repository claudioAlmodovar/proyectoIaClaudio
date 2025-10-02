import { test, expect } from '@playwright/test';

test('la aplicación carga la página principal', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('h1')).toContainText('Bienvenido a tu nueva aplicación Vue 3 + TypeScript');
});
