import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  notifications: [],
  nextId: 1
};

const notificationsSlice = createSlice({
  name: 'notifications',
  initialState,
  reducers: {
    addNotification: (state, action) => {
      const { type = 'info', message, duration = 5000 } = action.payload;
      const id = state.nextId;
      
      state.notifications.push({
        id,
        type,
        message,
        duration
      });
      
      state.nextId += 1;
    },
    removeNotification: (state, action) => {
      state.notifications = state.notifications.filter(
        notification => notification.id !== action.payload
      );
    },
    clearNotifications: (state) => {
      state.notifications = [];
    }
  }
});

export const { 
  addNotification, 
  removeNotification, 
  clearNotifications 
} = notificationsSlice.actions;

export default notificationsSlice.reducer; 