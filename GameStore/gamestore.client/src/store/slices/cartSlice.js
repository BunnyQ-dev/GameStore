import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  items: [],
  bundles: [],
  loading: false,
  error: null,
  total: 0
};

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    fetchCartStart(state) {
      state.loading = true;
      state.error = null;
    },
    fetchCartSuccess(state, action) {
      state.loading = false;
      state.items = action.payload.items || [];
      state.bundles = action.payload.bundles || [];
      state.total = calculateTotal(state.items, state.bundles);
    },
    fetchCartFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    addToCartStart(state) {
      state.loading = true;
      state.error = null;
    },
    addToCartSuccess(state, action) {
      state.loading = false;
      const newItem = action.payload;
      if (!state.items.some(item => item.gameId === newItem.gameId)) {
        state.items.push({ ...newItem, quantity: 1 });
      }
      state.total = calculateTotal(state.items, state.bundles);
    },
    addToCartFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    removeFromCartStart(state) {
      state.loading = true;
      state.error = null;
    },
    removeFromCartSuccess(state, action) {
      state.loading = false;
      const gameIdToRemove = action.payload;
      state.items = state.items.filter(item => item.gameId !== gameIdToRemove);
      state.total = calculateTotal(state.items, state.bundles);
    },
    removeFromCartFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    addBundleToCartSuccess(state, action) {
      state.loading = false;
      const newBundle = action.payload;
      if (!state.bundles.some(b => b.bundleId === newBundle.bundleId)) {
        state.bundles.push({ ...newBundle, quantity: 1 });
      }
      state.total = calculateTotal(state.items, state.bundles);
    },
    removeBundleFromCartSuccess(state, action) {
      state.loading = false;
      const bundleIdToRemove = action.payload;
      state.bundles = state.bundles.filter(b => b.bundleId !== bundleIdToRemove);
      state.total = calculateTotal(state.items, state.bundles);
    },
    clearCartStart(state) {
      state.loading = true;
      state.error = null;
    },
    clearCartSuccess(state) {
      state.loading = false;
      state.items = [];
      state.bundles = [];
      state.total = 0;
    },
    clearCartFailure(state, action) {
      state.loading = false;
      state.error = action.payload;
    },
    clearError(state) {
      state.error = null;
    }
  }
});

const calculateTotal = (items, bundles) => {
  const itemsTotal = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const bundlesTotal = bundles.reduce((sum, b) => sum + b.price * b.quantity, 0);
  return itemsTotal + bundlesTotal;
};

export const {
  fetchCartStart,
  fetchCartSuccess,
  fetchCartFailure,
  addToCartStart,
  addToCartSuccess,
  addToCartFailure,
  removeFromCartStart,
  removeFromCartSuccess,
  removeFromCartFailure,
  addBundleToCartSuccess,
  removeBundleFromCartSuccess,
  clearCartStart,
  clearCartSuccess,
  clearCartFailure,
  clearError
} = cartSlice.actions;

export default cartSlice.reducer; 