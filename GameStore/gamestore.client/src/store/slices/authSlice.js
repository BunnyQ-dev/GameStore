import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axios from 'axios';

// Асинхронний thunk для отримання профілю користувача
export const getUserProfile = createAsyncThunk(
  'auth/getUserProfile',
  async (_, { rejectWithValue, getState }) => {
    try {
      const { token } = getState().auth;
      if (!token) {
        return rejectWithValue('No authorization token');
      }

      const response = await axios.get('/api/users/me');
      return response.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to get user profile'
      );
    }
  }
);

const initialState = {
  user: null,
  token: localStorage.getItem('token'),
  isAuthenticated: !!localStorage.getItem('token'),
  loading: false,
  error: null
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginStart(state) {
      state.loading = true;
      state.error = null;
    },
    loginSuccess(state, action) {
      state.loading = false;
      state.isAuthenticated = true;
      state.user = action.payload.user;
      state.token = action.payload.token;
      localStorage.setItem('token', action.payload.token);
    },
    loginFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    logout(state) {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      localStorage.removeItem('token');
    },
    registerStart(state) {
      state.loading = true;
      state.error = null;
    },
    registerSuccess(state) {
      state.loading = false;
    },
    registerFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    loadUserStart(state) {
      state.loading = true;
      state.error = null;
    },
    loadUserSuccess(state, action) {
      state.loading = false;
      state.user = action.payload;
      state.isAuthenticated = true;
    },
    loadUserFailure(state, action) {
      state.loading = false;
      state.user = null;
      state.isAuthenticated = false;
      state.error = action.payload;
    },
    clearError(state) {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(getUserProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getUserProfile.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload;
        state.isAuthenticated = true;
      })
      .addCase(getUserProfile.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        // Якщо помилка 401, вийти з облікового запису
          if (action.payload === 'No authorization token' || action.error?.message === 'Request failed with status code 401') {
          state.isAuthenticated = false;
          state.user = null;
          state.token = null;
          localStorage.removeItem('token');
        }
      });
  }
});

export const {
  loginStart,
  loginSuccess,
  loginFailure,
  logout,
  registerStart,
  registerSuccess,
  registerFailure,
  loadUserStart,
  loadUserSuccess,
  loadUserFailure,
  clearError
} = authSlice.actions;

export default authSlice.reducer; 