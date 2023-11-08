import "./App.css";
import { Routes, Route } from "@solidjs/router";
import Layout from "./Layout";
import Home from "./Home";
import ChannelById from "./channels/[id]";

function App() {
  return (
    <Routes>
      <Route path="/" component={Layout}>
        <Route path="/" component={Home} />
        <Route path="/channels/:id" component={ChannelById} />
      </Route>
    </Routes>
  );
}

export default App;


