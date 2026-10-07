export default function fetch(input: RequestInfo | URL, init?: RequestInit): Promise<Response> {
  return window
    .fetch(input, init)
    .then((response) => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response;
    })
    .catch((error) => {
      console.error("Fetch error:", error);
      throw error;
    });
}
