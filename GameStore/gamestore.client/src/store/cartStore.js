import { create } from 'zustand';
import axios from '../utils/axios-config'; // Використовуємо налаштований axios

const useCartStore = create((set, get) => ({
  items: [],
  totalItems: 0,
  isLoading: false,
  error: null,

  // Завантаження кошика з сервера
  fetchCart: async () => {
    if (!localStorage.getItem('token')) return; // Не завантажувати, якщо не авторизований
    set({ isLoading: true, error: null });
    try {
      const response = await axios.get('/api/cart');
      const cartData = response.data;
      set({
        items: cartData.items || [],
        totalItems: cartData.items?.reduce((sum, item) => sum + item.quantity, 0) || 0,
        isLoading: false,
      });
    } catch (error) {
      console.error("Failed to fetch cart:", error);
      set({ error: 'Failed to load cart', isLoading: false });
    }
  },

  // Додавання товару до кошика
  addToCart: async (gameId) => {
    if (!localStorage.getItem('token')) {
       // Можна перенаправити на логін або показати повідомлення
       alert('Please login to add item to cart.');
       return false; 
    }
    set({ isLoading: true });
    try {
      const response = await axios.post(`/api/cart/${gameId}`);
      const cartData = response.data;
      set({
        items: cartData.items || [],
        totalItems: cartData.items?.reduce((sum, item) => sum + item.quantity, 0) || 0,
        isLoading: false,
      });
      return true; // Успішно додано
    } catch (error) {
      console.error("Failed to add to cart:", error);
      set({ error: error.response?.data?.message || 'Failed to add item', isLoading: false });
      alert(error.response?.data?.message || 'Failed to add item');
      return false; // Помилка
    }
  },

  // Видалення товару з кошика
  removeFromCart: async (gameId) => {
     if (!localStorage.getItem('token')) return;
    set({ isLoading: true });
    try {
      const response = await axios.delete(`/api/cart/${gameId}`);
      const cartData = response.data;
      set({
        items: cartData.items || [],
        totalItems: cartData.items?.reduce((sum, item) => sum + item.quantity, 0) || 0,
        isLoading: false,
      });
    } catch (error) {
      console.error("Failed to remove from cart:", error);
      set({ error: 'Failed to remove item', isLoading: false });
       alert('Failed to remove item');
    }
  },
  
  // Очищення кошика (після виходу з системи або checkout)
  clearCartLocally: () => {
      set({ items: [], totalItems: 0, isLoading: false, error: null });
  },

  // Додамо дію для checkout, хоча основна логіка в OrderController
  checkout: async () => {
      if (!localStorage.getItem('token')) return false;
      set({ isLoading: true });
      try {
          const response = await axios.post('/api/order/checkout');
          set({ items: [], totalItems: 0, isLoading: false }); // Очищаємо кошик локально
          alert(response.data.message || 'Order successfully placed!');
          return true; // Успіх
      } catch(error) {
          console.error("Checkout failed:", error);
          set({ error: error.response?.data || 'Checkout failed', isLoading: false });
           alert(error.response?.data || 'Order placement error');
          return false; // Помилка
      }
  }
}));

export default useCartStore; 