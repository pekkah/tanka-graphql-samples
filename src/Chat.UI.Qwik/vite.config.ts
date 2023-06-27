import { defineConfig } from "vite";
import { qwikVite } from '@builder.io/qwik/optimizer';
import { qwikCity } from '@builder.io/qwik-city/vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import mkcert from 'vite-plugin-mkcert';

export default defineConfig(() => {
  return {
    plugins: [mkcert(), qwikCity(), qwikVite(), tsconfigPaths()],
    server: {
      https: true,
    },
    preview: {
      headers: {
        'Cache-Control': 'public, max-age=600',
      },
    },
    optimizeDeps: {
      include: ["@auth/core"]
    }
  };
});