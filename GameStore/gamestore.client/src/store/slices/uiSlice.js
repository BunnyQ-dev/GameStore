import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  notifications: [],
  nextNotificationId: 1,
  searchQuery: '',
  theme: localStorage.getItem('theme') || 'dark',
  isSidebarOpen: false,
  isCartOpen: false
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    // Керування повідомленнями
    addNotification: (state, action) => {
      const { type = 'info', message, duration = 5000 } = action.payload;
      const id = state.nextNotificationId;
      
      state.notifications.push({
        id,
        type,
        message,
        duration
      });
      
      state.nextNotificationId += 1;
    },
    removeNotification: (state, action) => {
      state.notifications = state.notifications.filter(
        notification => notification.id !== action.payload
      );
    },
    clearNotifications: (state) => {
      state.notifications = [];
    },
    
    // Керування пошуком
    setSearchQuery: (state, action) => {
      state.searchQuery = action.payload;
    },
    clearSearchQuery: (state) => {
      state.searchQuery = '';
    },
    
    // Керування темою
    setTheme: (state, action) => {
      state.theme = action.payload;
      localStorage.setItem('theme', action.payload);
    },
    toggleTheme: (state) => {
      state.theme = state.theme === 'dark' ? 'light' : 'dark';
      localStorage.setItem('theme', state.theme);
    },
    
    // Керування сайдбаром та кошиком
    toggleSidebar: (state) => {
      state.isSidebarOpen = !state.isSidebarOpen;
    },
    closeSidebar: (state) => {
      state.isSidebarOpen = false;
    },
    toggleCart: (state) => {
      state.isCartOpen = !state.isCartOpen;
    },
    closeCart: (state) => {
      state.isCartOpen = false;
    }
  }
});

export const {
  addNotification,
  removeNotification,
  clearNotifications,
  setSearchQuery,
  clearSearchQuery,
  setTheme,
  toggleTheme,
  toggleSidebar,
  closeSidebar,
  toggleCart,
  closeCart
} = uiSlice.actions;

export default uiSlice.reducer; 