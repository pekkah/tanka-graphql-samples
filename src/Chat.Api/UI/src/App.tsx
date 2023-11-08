import "./App.css";
import { Routes, Route } from "@solidjs/router";
import Layout from "./Layout";
import Home from "./Home";

function App() {
  return (
    <Routes>
      <Route path="/" component={Layout}>
        <Route path="/" component={Home} />
      </Route>
    </Routes>
  );
}

export default App;


