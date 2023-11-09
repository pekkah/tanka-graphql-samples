import { useParams } from "@solidjs/router";
import { createMemo } from "solid-js";


export default function Channel() { 
  const params = useParams<{id: string}>();
  const id = createMemo(()=> params.id);

  return (
    <>
     <h2>Channel {id()}</h2>
    </>
  );
}
