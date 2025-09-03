import axios from "axios";

const API_BASE_URL =
  process.env.VUE_APP_API_URL || "https://localhost:7214/api";

// Simple cache for GET requests
const cache = new Map();

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Combined request interceptor with caching and logging
apiClient.interceptors.request.use(
  (config) => {
    if (process.env.NODE_ENV === "development") {
      console.log(
        `Making ${config.method?.toUpperCase()} request to ${config.url}`
      );
    }
    return config;
  },
  (error) => {
    console.error("Request error:", error);
    return Promise.reject(error);
  }
);

function shouldCache(url) {
  // Cache static data like examination types, doctors, medicines
  return (
    url?.includes("/VrstePregleda") ||
    url?.includes("/Doktori") ||
    url?.includes("/Lijekovi")
  );
}

apiClient.interceptors.response.use(
  (response) => {
    // Cache GET responses for specific endpoints
    if (response.config.method === "get" && shouldCache(response.config.url)) {
      const cacheKey = `${response.config.method}:${response.config.url}`;
      cache.set(cacheKey, {
        response: response,
        timestamp: Date.now(),
      });
      if (process.env.NODE_ENV === "development") {
        console.log(`Cached response for ${response.config.url}`);
      }
    }
    return response;
  },
  (error) => {
    if (process.env.NODE_ENV === "development") {
      console.error("Response error:", error);
      if (error.response) {
        console.error("Status:", error.response.status);
        console.error("Data:", error.response.data);
      }
    }
    return Promise.reject(error);
  }
);

// Export cache management functions
export const clearCache = () => {
  cache.clear();
  if (process.env.NODE_ENV === "development") {
    console.log("API cache cleared");
  }
};

export const clearCacheByPattern = (pattern) => {
  for (const [key] of cache) {
    if (key.includes(pattern)) {
      cache.delete(key);
    }
  }
};

export default apiClient;
