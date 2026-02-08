import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:6767',
        changeOrigin: true,
      },
      '/signalr': {
        target: 'http://localhost:6767',
        ws: true,
      },
    },
  },
  build: {
    outDir: '../src/Kagarr.Host/UI',
    emptyOutDir: true,
  },
});
