import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axios from 'axios';

// Async thunks
export const fetchGames = createAsyncThunk(
  'games/fetchGames',
  async (params, { rejectWithValue }) => {
    try {
      const response = await axios.get('/api/games', { params });
      return response.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to load games.'
      );
    }
  }
);

export const fetchGameById = createAsyncThunk(
  'games/fetchGameById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await axios.get(`/api/games/${id}`);
      return response.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to load game details.'
      );
    }
  }
);

export const fetchGenres = createAsyncThunk(
  'games/fetchGenres',
  async (_, { rejectWithValue }) => {
    try {
      const response = await axios.get('/api/genres');
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response.data || 'Failed to load genres.');
    }
  }
);

export const rateGame = createAsyncThunk(
  'games/rateGame',
  async ({ gameId, rating, review }, { rejectWithValue }) => {
    try {
      const response = await axios.post('/api/gamerating', {
        gameId,
        rating,
        review
      }, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`
        }
      });
      return response.data;
    } catch (error) {
      return rejectWithValue(error.response.data || 'Failed to rate game.');
    }
  }
);

const initialState = {
  games: [],
  currentGame: null,
  genres: [],
  featured: [],
  newReleases: [],
  topSellers: [],
  specialOffers: [],
  loading: false,
  error: null,
  pagination: {
    currentPage: 1,
    pageSize: 12,
    totalCount: 0,
    totalPages: 1
  },
  categories: []
};

const gamesSlice = createSlice({
  name: 'games',
  initialState,
  reducers: {
    clearCurrentGame: (state) => {
      state.currentGame = null;
    },
    clearError: (state) => {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      // Fetch games cases
      .addCase(fetchGames.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchGames.fulfilled, (state, action) => {
        state.loading = false;
        state.games = action.payload.items || [];
        state.pagination.totalCount = action.payload.totalCount;
        state.pagination.pageSize = action.payload.pageSize;
        state.pagination.currentPage = action.payload.currentPage;
        state.pagination.totalPages = action.payload.totalPages;
        
        if (action.payload.categories) {
          state.categories = action.payload.categories;
        }
      })
      .addCase(fetchGames.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      
      // Fetch game by id cases
      .addCase(fetchGameById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchGameById.fulfilled, (state, action) => {
        state.loading = false;
        state.currentGame = action.payload;
      })
      .addCase(fetchGameById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
        state.currentGame = null;
      })
      
      // Fetch genres cases
      .addCase(fetchGenres.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchGenres.fulfilled, (state, action) => {
        state.loading = false;
        state.genres = action.payload;
      })
      .addCase(fetchGenres.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      
      // Rate game cases
      .addCase(rateGame.fulfilled, (state, action) => {
        if (state.currentGame && state.currentGame.id === action.payload.gameId) {
          // Оновити рейтинг гри
          if (state.currentGame.ratings) {
            state.currentGame.ratings.push(action.payload);
          } else {
            state.currentGame.ratings = [action.payload];
          }
        }
      });
  }
});

export const { clearCurrentGame, clearError } = gamesSlice.actions;
export default gamesSlice.reducer; 