import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./app/app.tsx";
import ConfigurationLoader from "./configuration/configuration.loader.ts";

ConfigurationLoader.Load().then(() => {
  createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
});
