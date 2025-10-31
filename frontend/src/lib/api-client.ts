import axios from 'axios';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Interceptor для добавления токенов авторизации (если необходимо)
 */
api.interceptors.request.use(
  config => {
    // Здесь можно добавить токены авторизации
    return config;
  },
  error => Promise.reject(error)
);

/**
 * Обработка ошибок API запросов
 */
api.interceptors.response.use(
  response => response,
  error => {
    // Логируем ошибки только в development режиме
    if (process.env.NODE_ENV === 'development') {
      if (error.response) {
        console.error('API Error:', error.response.status, error.response.data);
      } else if (error.request) {
        console.error('Network Error:', error.message);
      } else {
        console.error('Request Error:', error.message);
      }
    }
    return Promise.reject(error);
  }
);

export default api;

