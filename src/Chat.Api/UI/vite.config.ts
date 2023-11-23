import { defineConfig } from "vite";
import solid from "vite-plugin-solid";
import mkcert from'vite-plugin-mkcert';
import codegen from 'vite-plugin-graphql-codegen';

export default defineConfig({
  plugins: [mkcert(), codegen(), solid()],
  build: {
    manifest: true,
    emptyOutDir: true,
    outDir:"../wwwroot",
    rollupOptions: {
      input: "src/index.tsx"
    }
  },
  server: {
    https: true,
    hmr: {
      clientPort: 5173,
      host: "localhost",
    },
  },
});
