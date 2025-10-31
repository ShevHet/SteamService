import axios from 'axios';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000',
  timeout: 10000,
  withCredentials: false,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Обработка ошибок API запросов
 */
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response) {
      // Сервер вернул ошибку
      const status = error.response.status;
      const url = `${error.config?.baseURL || ''}${error.config?.url || ''}`;
      
      // Логируем только критические ошибки (5xx) или в development режиме
      if (status >= 500 || process.env.NODE_ENV === 'development') {
        console.error(`API Error [${status}]:`, url, error.response.data);
      }
    } else if (error.request) {
      // Запрос был отправлен, но ответа не было
      if (process.env.NODE_ENV === 'development') {
        console.error('Network Error:', error.message);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
