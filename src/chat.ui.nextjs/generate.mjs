import { generate } from "@genql/cli";
import path from "path";

process.env.NODE_TLS_REJECT_UNAUTHORIZED=0

const output = path.resolve(process.cwd(), "lib/client");
console.log('output: ', output);
await generate({
  endpoint: "https://localhost:8000/graphql",
  //schema: fs.readFileSync(path.join(__dirname, "schema.graphql")).toString(),
  output: output,
  scalarTypes: {
  },
  verbose: true,
}).catch(console.error);
