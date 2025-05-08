import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import GameCard from '../../components/Game/GameCard';
import { FaHeart, FaRegHeart, FaBoxOpen } from 'react-icons/fa';
import './Library.css';

const Library = () => {
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState('all'); // 'all', 'installed', 'favorites'
  const [searchQuery, setSearchQuery] = useState('');
  
  useEffect(() => {
    const fetchLibrary = async () => {
      setLoading(true);
      try {
        const response = await axios.get('/api/purchase/library');
        setGames(response.data?.map(g => ({...g, isOwned: true})) || []);
        setError(null);
      } catch (err) {
        console.error('Error fetching library:', err);
        setError('Не вдалось завантажити бібліотеку. Спробуйте пізніше.');
        setGames([]);
      } finally {
        setLoading(false);
      }
    };

    fetchLibrary();
  }, []);

  const handleToggleFavorite = async (gameId, isFavorite) => {
    const originalGames = [...games];
    
    setGames(prev => 
      prev.map(game => 
        game.id === gameId ? { ...game, isInWishlist: !isFavorite } : game
      )
    );
    setError(null);

    try {
      const url = `/api/wishlist/${gameId}`;
      if (isFavorite) {
        await axios.delete(url);
      } else {
        await axios.post(url);
      }
    } catch (err) {
      console.error('Error updating favorite status:', err);
      setError('Не вдалось оновити статус улюбленого. Спробуйте пізніше.');
      setGames(originalGames);
    }
  };

  const handleInstall = async (gameId) => {
    try {
      await axios.post(`/api/library/${gameId}/install`);
      
      setGames(prev => 
        prev.map(game => 
          game.id === gameId ? { ...game, isInstalled: true } : game
        )
      );
    } catch (err) {
      console.error('Error installing game:', err);
      setError('Не вдалось встановити гру. Спробуйте пізніше.');
    }
  };

  const handleUninstall = async (gameId) => {
    try {
      await axios.post(`/api/library/${gameId}/uninstall`);
      
      setGames(prev => 
        prev.map(game => 
          game.id === gameId ? { ...game, isInstalled: false } : game
        )
      );
    } catch (err) {
      console.error('Error uninstalling game:', err);
      setError('Не вдалось видалити гру. Спробуйте пізніше.');
    }
  };

  const filteredGames = games.filter(game => {
    const matchesSearch = game.title.toLowerCase().includes(searchQuery.toLowerCase());
    
    if (filter === 'favorites') {
      return game.isInWishlist && matchesSearch;
    }
    
    return matchesSearch;
  });

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  return (
    <div className="library-container container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-white mb-6">Моя бібліотека</h1>
      
      {error && (
        <div className="bg-red-900 border border-red-700 text-red-100 px-4 py-3 rounded relative mb-6" role="alert">
            <strong className="font-bold">Помилка!</strong>
            <span className="block sm:inline"> {error}</span>
        </div>
      )}
      
      {/* Панель фільтрації та пошуку */}
      <div className="bg-gray-800 p-4 rounded-lg mb-6 shadow-lg">
        <div className="flex flex-col md:flex-row gap-4 items-center">
          <div className="flex-grow">
            <input
              type="text"
              placeholder="Пошук у бібліотеці..."
              className="w-full p-3 rounded bg-gray-700 text-white border border-gray-600 focus:border-blue-500 focus:outline-none transition duration-300"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
          
          <div className="flex gap-2 flex-shrink-0">
            <button
              className={`px-4 py-2 rounded transition duration-300 font-medium ${
                filter === 'all' 
                  ? 'bg-blue-600 text-white shadow-md' 
                  : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
              }`}
              onClick={() => setFilter('all')}
            >
              Всі
            </button>
            <button
               className={`px-4 py-2 rounded transition duration-300 font-medium ${
                filter === 'favorites' 
                  ? 'bg-blue-600 text-white shadow-md' 
                  : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
              }`}
              onClick={() => setFilter('favorites')}
            >
              Улюблені
            </button>
          </div>
        </div>
      </div>
      
      {/* Список ігор у вигляді сітки карток */}
      {filteredGames.length === 0 ? (
        <div className="text-center py-16 px-4">
          {/* Показуємо іконку тільки якщо немає результатів пошуку і бібліотека дійсно порожня */} 
          {!searchQuery && games.length === 0 && (
            <FaBoxOpen size={60} className="mx-auto mb-6 text-gray-500" />
          )}
          <p className="text-gray-400 text-xl mb-4">
            {searchQuery
              ? `Не знайдено ігор за запитом "${searchQuery}"`
              : filter === 'favorites'
              ? 'У вас немає улюблених ігор у бібліотеці'
              : 'Ваша бібліотека порожня'}
          </p>
          {/* Посилання "Перейти до магазину" показуємо тільки якщо бібліотека дійсно порожня (не через пошук/фільтр) */}
          {games.length === 0 && !searchQuery && (
            <Link
              to="/store"
              className="inline-block bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded transition duration-300 shadow hover:shadow-lg"
            >
              Перейти до магазину
            </Link>
          )}
        </div>
      ) : (
        <div className="library-grid-container">
          {filteredGames.map((game) => (
            <GameCard key={game.id} game={game} />
          ))}
        </div>
      )}
    </div>
  );
};

export default Library;
