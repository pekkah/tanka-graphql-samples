"use client";

import { experimental_useFormStatus as useFormStatus } from "react-dom";
import { postMessage } from "./postMessage";
import { useRef, useState } from "react";

export function AddMessage({ channelId }: { channelId: number }) {
  const { pending } = useFormStatus();
  const formRef = useRef<HTMLFormElement>(null);

  function addMessage(data: FormData) {
    console.log("addMessage", data);
    postMessage(data).then(() => {
      formRef.current?.reset();
    });
  }

  return (
    <div>
      <form
        ref={formRef}
        className="flex justify-center items-center h-16"
        action={addMessage}
      >
        <input type="hidden" name="channelId" value={channelId} />
        <label htmlFor="message" className="m-2 flex-none">
          Message
        </label>
        <input
          className="bg-black rounded-sm m-2 mr-0 grow h-11"
          type="text"
          name="message"
          id="message"
        />
        <button
          className="rounded-sm ml-0 border-gray-600 border-2 bg-black w-16 h-11 hover:bg-gray-800"
          type="submit"
          disabled={pending}
        >
          Send
        </button>
      </form>
    </div>
  );
}
