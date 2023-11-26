import { render } from "preact";
import { Provider as UrqlProvider } from "@urql/preact";
import { client } from "./data/index.js";
import { createBrowserRouter, RouterProvider } from "react-router-dom";

import Home from "./pages/home/index.js";
import "./style.css";
import ChannelById from "./pages/channels/[id]";
import Layout from "./Layout";
import { SessionProvider } from "./model/index.js";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      {
        path: "/",
        element: <Home />,
      },
      {
        path: "/channels/:id",
        element: <ChannelById />,
      },
    ],
  },
]);

export function App() {
  return (
    <SessionProvider>
      <UrqlProvider value={client}>
        <RouterProvider router={router} />
      </UrqlProvider>
    </SessionProvider>
  );
}

render(<App />, document.getElementById("root"));
