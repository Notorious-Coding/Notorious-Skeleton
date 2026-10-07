import type Configuration from "./configuration.model";

export default class ConfigurationLoader {
  static configuration: Configuration;
  static isLoaded: boolean = false;

  constructor() {}

  static async Load(): Promise<any> {
    const config = await fetch("/configuration/configuration.json").then((res) => res.json());
    const envConfig = await fetch(`/configuration/configuration.${import.meta.env.MODE?.trim()}.json`)
      .then((res) => res.json())
      .catch(() => {
        console.log("No environment specific configuration found, skipping...");
      });
    this.isLoaded = true;
    ConfigurationLoader.configuration = { ...config, ...envConfig };
  }
}
