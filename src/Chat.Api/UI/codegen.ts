import type { CodegenConfig } from "@graphql-codegen/cli";

const config: CodegenConfig = {
  overwrite: true,
  //schema: "https://localhost:8000/graphql",
  schema: "graphql/Default.graphql",
  documents: ["src/**/*.tsx", "src/**/*.ts"],
  generates: {
    "src/generated/": {
      preset: "client",
      config: {
        useTypeImports: true,
        
      },
    },
  },
};

export default config;
