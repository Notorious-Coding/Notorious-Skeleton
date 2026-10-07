import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
// https://vite.dev/config/
export default defineConfig(() => {
  const target = "http://localhost:8080";

  return {
    plugins: [react()],
    resolve: {
      alias: {
        "@UI": fileURLToPath(new URL("./src/1. UI", import.meta.url)),
        "@business": fileURLToPath(new URL("./src/2. business", import.meta.url)),
        "@data": fileURLToPath(new URL("./src/3. data", import.meta.url)),
        "@common": fileURLToPath(new URL("./src/4. common", import.meta.url)),
      },
    },
    server: {
      proxy: {
        "/api": {
          target,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
