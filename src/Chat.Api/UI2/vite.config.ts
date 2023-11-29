import { defineConfig } from 'vite';
import preact from '@preact/preset-vite';
import mkcert from'vite-plugin-mkcert';
import codegen from 'vite-plugin-graphql-codegen';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [mkcert(), codegen(), preact()],
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
