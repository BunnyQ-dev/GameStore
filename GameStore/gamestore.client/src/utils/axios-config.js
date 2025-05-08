import axios from 'axios';

// Налаштування базового URL до бекенду
axios.defaults.baseURL = 'https://localhost:7297';

// Додаємо JWT токен до всіх запитів, якщо він є в localStorage
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers = config.headers || {};
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Обробка відповідей
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    // Обробка помилки автентифікації (401)
    if (error.response && error.response.status === 401) {
      // Можна додати логіку перенаправлення на сторінку входу
      console.log('Authorization required');
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default axios; 