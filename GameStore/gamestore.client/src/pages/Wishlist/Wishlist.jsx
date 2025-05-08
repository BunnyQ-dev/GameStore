import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { FaTrash, FaShoppingCart, FaFrown, FaTrashAlt, FaHeartBroken } from 'react-icons/fa';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { useDispatch } from 'react-redux';
import { addToCartStart, addToCartSuccess, addToCartFailure } from '../../store/slices/cartSlice';
import { addNotification } from '../../store/slices/uiSlice';

const Wishlist = () => {
  const [wishlistItems, setWishlistItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionInProgress, setActionInProgress] = useState(null);
  
  const dispatch = useDispatch();

  useEffect(() => {
    fetchWishlist();
  }, []);

  const fetchWishlist = async () => {
    setLoading(true);
    try {
      const response = await axios.get('/api/wishlist');
      setWishlistItems(response.data);
      setError(null);
    } catch (err) {
      console.error('Error fetching wishlist:', err);
      setError('Не вдалось завантажити список бажань. Спробуйте пізніше.');
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveFromWishlist = async (gameId) => {
    setActionInProgress(gameId);
    try {
      await axios.delete(`/api/wishlist/${gameId}`);
      setWishlistItems(prev => prev.filter(item => item.id !== gameId));
      dispatch(addNotification({
        type: 'success',
        message: 'Гру видалено зі списку бажань'
      }));
    } catch (err) {
      console.error('Error removing from wishlist:', err);
      dispatch(addNotification({
        type: 'error',
        message: 'Не вдалось видалити гру зі списку бажань'
      }));
    } finally {
      setActionInProgress(null);
    }
  };

  const handleAddToCart = async (gameId) => {
    if (!gameId || typeof gameId !== 'number') {
        console.error('Invalid gameId for AddToCart:', gameId);
        dispatch(addNotification({ type: 'error', message: 'Неправильний ID гри.' }));
        return;
    }

    setActionInProgress(gameId);
    dispatch(addToCartStart());
    try {
      const response = await axios.post(`/api/cart/${gameId}`);
      dispatch(addToCartSuccess(response.data));
      dispatch(addNotification({
        type: 'success',
        message: 'Гру додано до кошика'
      }));
    } catch (err) {
      console.error('Error adding to cart:', err);
      const errorMessage = err.response?.data || 'Не вдалось додати гру до кошика';
      dispatch(addToCartFailure(errorMessage));
      dispatch(addNotification({
        type: 'error',
        message: errorMessage
      }));
    } finally {
      setActionInProgress(null);
    }
  };

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-10">
        <div className="bg-red-500 text-white p-4 rounded mb-6">
          {error}
        </div>
        <div className="text-center">
          <Link 
            to="/" 
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded transition duration-300"
          >
            На головну
          </Link>
        </div>
      </div>
    );
  }

  if (!wishlistItems.length) {
    return (
      <div className="text-center py-16 px-4">
        <FaHeartBroken size={60} className="mx-auto mb-6 text-gray-500" />
        <h2 className="text-2xl font-semibold text-gray-300 mb-3">
          Список бажань порожній
        </h2>
        <p className="text-gray-400 mb-8">
          Додайте ігри, які вам сподобались, щоб не втратити їх!
        </p>
        <Link
          to="/store"
          className="inline-block bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded transition duration-300 shadow hover:shadow-lg"
        >
          Перейти до магазину
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-white mb-6">Список бажань</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {wishlistItems.map(game => (
          <div 
            key={game.id} 
            className="bg-gray-800 rounded-lg overflow-hidden shadow-lg transition-all duration-300 hover:shadow-2xl"
          >
            <Link to={`/store/game/${game.id}`}>
              <img 
                src={game.imageUrl || '/img/game-placeholder.jpg'} 
                alt={game.title}
                className="w-full h-48 object-cover transition-transform duration-300 hover:scale-105"
              />
            </Link>
            
            <div className="p-4">
              <Link to={`/store/game/${game.id}`}>
                <h3 className="text-xl font-semibold text-white mb-2 hover:text-blue-400 transition-colors duration-300">
                  {game.title}
                </h3>
              </Link>
              
              <div className="flex flex-wrap gap-2 mb-3">
                {game.genres?.map(genre => (
                  <span 
                    key={genre.id} 
                    className="bg-gray-700 text-gray-300 px-2 py-1 rounded-full text-xs"
                  >
                    {genre.name}
                  </span>
                ))}
              </div>
              
              <div className="flex justify-between items-center">
                <span className="text-green-500 font-bold text-lg">
                  {game.price === 0 
                    ? 'Безкоштовно' 
                    : `₴${game.price.toFixed(2)}`}
                </span>
                
                <div className="flex gap-2">
                  <button 
                    className="p-2 rounded bg-red-600 hover:bg-red-700 text-white transition duration-300 disabled:opacity-50"
                    onClick={() => handleRemoveFromWishlist(game.id)}
                    disabled={actionInProgress === game.id}
                  >
                    <FaTrash />
                  </button>
                  
                  <button 
                    className="p-2 rounded bg-blue-600 hover:bg-blue-700 text-white transition duration-300 disabled:opacity-50"
                    onClick={() => handleAddToCart(game.id)}
                    disabled={actionInProgress === game.id}
                  >
                    <FaShoppingCart />
                  </button>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Wishlist; 