import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import type { UserConfig } from 'vitest/config'

const root = new URL('.', import.meta.url).pathname

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, root, '')
  const proxyTarget = process.env.VITE_API_BASE_URL ?? env.VITE_API_BASE_URL ?? 'http://localhost:5001'

  console.log('proxyTarget', proxyTarget)

  return {
    plugins: [
      react(),
      tailwindcss(),
    ],
    resolve: {
      alias: {
        '@': new URL('src', import.meta.url).pathname,
      },
    },
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: proxyTarget,
          changeOrigin: true,
        },
      },
    },
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html'],
        reportsDirectory: './coverage',
      },
    } satisfies UserConfig['test'],
  }
})
