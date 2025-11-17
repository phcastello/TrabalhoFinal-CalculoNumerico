import axios from 'axios';

const FALLBACK_PORT = import.meta.env.VITE_API_PORT || '5000';

const resolveBaseUrl = () => {
  const envUrl = import.meta.env.VITE_API_BASE_URL;
  const runtimeHost =
    typeof window !== 'undefined'
      ? `${window.location.protocol}//${window.location.hostname}`
      : 'http://localhost';

  const defaultUrl = `${runtimeHost}:${FALLBACK_PORT}`;

  if (!envUrl) {
    return defaultUrl;
  }

  try {
    const parsed = new URL(envUrl);
    if (parsed.hostname === 'backend') {
      const normalizedPort = parsed.port || FALLBACK_PORT;
      const host =
        typeof window !== 'undefined' ? window.location.hostname : 'localhost';
      return `${parsed.protocol}//${host}:${normalizedPort}`;
    }

    return envUrl;
  } catch {
    return envUrl;
  }
};

const baseURL = resolveBaseUrl();

export const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});
