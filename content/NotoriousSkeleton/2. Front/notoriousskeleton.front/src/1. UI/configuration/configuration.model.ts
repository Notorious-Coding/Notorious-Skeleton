export interface BuildConfiguration {
  version: string;
  branch: string;
  commitSha: string;
}

export default interface Configuration {
  build: BuildConfiguration;
}
