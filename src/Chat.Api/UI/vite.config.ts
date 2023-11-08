import { defineConfig } from "vite";
import solid from "vite-plugin-solid";
import mkcert from'vite-plugin-mkcert';

export default defineConfig({
  plugins: [mkcert(), solid()],
  server: {
    https: true,
    hmr: {
      clientPort: 5173,
      host: "localhost",
    },
  },
});
