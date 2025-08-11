import { defineConfig } from 'vite';
import preact from '@preact/preset-vite';
import codegen from 'vite-plugin-graphql-codegen';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [codegen(), preact()],
	build: {
		manifest: true,
		emptyOutDir: true,
		outDir:"../wwwroot",
		rollupOptions: {
		  input: "src/index.tsx"
		}
	  },
	  server: {
		port: 5173,
		host: "localhost",
		hmr: {
		  port: 5173,
		  host: "localhost",
		},
		cors: true,
	  },
});
