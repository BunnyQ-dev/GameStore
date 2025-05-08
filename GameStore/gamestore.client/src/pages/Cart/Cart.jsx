import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import axios from '../../utils/axios-config';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { 
  fetchCartStart,
  fetchCartSuccess,
  fetchCartFailure,
  removeFromCartSuccess,
  removeBundleFromCartSuccess
} from '../../store/slices/cartSlice';
import { FaTrashAlt, FaReceipt, FaShoppingCart } from 'react-icons/fa';
import './Cart.css';

const Cart = () => {
  const { items: cartItems, bundles: cartBundles, loading, error, total } = useSelector(state => state.cart);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  
  // Total count of items and bundles, used to show UI even if price is zero
  const totalCount = (cartItems?.length || 0) + (cartBundles?.length || 0);

  useEffect(() => {
    const fetchCartDirectly = async () => {
      dispatch(fetchCartStart());
      try {
        const response = await axios.get('/api/cart');
        console.log('Fetched cart:', response.data);
        dispatch(fetchCartSuccess(response.data));
      } catch (err) {
        console.error('Error fetching cart:', err);
        dispatch(fetchCartFailure('Failed to load cart. Please try again later.'));
      }
    };
    // Always fetch cart on component mount
    fetchCartDirectly();
  }, [dispatch]);

  const handleRemoveItem = async (gameId) => {
    const originalItems = [...cartItems];
    dispatch(removeFromCartSuccess(gameId));
    
    try {
      await axios.delete(`/api/cart/${gameId}`);
    } catch (err) {
      console.error('Error removing item:', err);
      alert('Failed to remove item.');
    }
  };

  const handleRemoveBundle = async (bundleId) => {
    dispatch(removeBundleFromCartSuccess(bundleId));
    try {
      await axios.delete(`/api/cart/bundles/${bundleId}`);
    } catch (err) {
      console.error('Error removing bundle:', err);
      alert('Failed to remove bundle from cart.');
    }
  };

  const handleCheckout = () => {
    navigate('/checkout');
  };

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    return (
      <div className="cart-container container mx-auto px-4 py-10">
         <h1 className="text-3xl font-bold text-white mb-8">Cart</h1>
         <div className="bg-red-900 border border-red-700 text-red-100 px-4 py-3 rounded relative mb-6" role="alert">
            <strong className="font-bold">Error!</strong>
            <span className="block sm:inline"> {error}</span>
        </div>
        <div className="text-center mt-8">
          <Link 
            to="/store" 
            className="inline-block bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded transition duration-300 shadow hover:shadow-lg"
          >
            Go to store
          </Link>
        </div>
      </div>
    );
  }

  if (totalCount === 0) {
    return (
      <div className="cart-container container mx-auto px-4 py-16 text-center">
        <div className="bg-gray-800 p-10 rounded-lg shadow-lg inline-block">
          <FaShoppingCart size={60} className="mx-auto mb-6 text-gray-500" />
          <h2 className="text-2xl font-semibold text-gray-300 mb-3"> 
            Your cart is empty
          </h2> 
          <p className="text-gray-400 mb-8"> 
            It seems you haven't added anything yet. Time to shop!
          </p>
          <Link 
            to="/store" 
            className="inline-block bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded transition duration-300 shadow hover:shadow-lg"
          >
            Go to store
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-container container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-white mb-8">Cart</h1>
      
      <div className="flex flex-col lg:flex-row gap-8">
        {/* List of items */}
        <div className="cart-items-list lg:w-2/3">
          {cartItems.map(item => (
            item && item.gameId ? (
              <div 
                key={item.gameId}
                className="cart-item-card"
              >
                <Link to={`/store/games/${item.gameId}`} className="cart-item-image-link">
                    <img 
                      src={item.imageUrl || '/img/game-placeholder.jpg'}
                      alt={item.title || 'Game Image'}
                      className="cart-item-image"
                    />
                </Link>
                
                <div className="cart-item-details">
                  <Link to={`/game/${item.gameId}`} className="hover:text-blue-400 transition duration-300">
                    <h3 className="cart-item-title">{item.title || 'Unknown title'}</h3>
                  </Link>
                  <p className="cart-item-price">${(item.price ?? 0).toFixed(2)}</p>
                </div>
                
                <div className="cart-item-actions">
                  <button 
                    className="cart-item-remove-button"
                    onClick={() => handleRemoveItem(item.gameId)}
                    title="Remove from cart"
                    disabled={loading}
                  >
                    <FaTrashAlt />
                  </button>
                </div>
              </div>
            ) : (
              <div key={item?.id || item?.gameId || Math.random()} className="bg-red-900 text-white p-4 rounded mb-4">
                Failed to process one of the items in the cart.
              </div>
            )
          ))}
          {cartBundles.map(bundle => (
            <div key={bundle.bundleId} className="cart-item-card">
              <div className="cart-item-details">
                <h3 className="cart-item-title">{bundle.name}</h3>
                <p className="cart-item-price">${bundle.price.toFixed(2)}</p>
              </div>
              <div className="cart-item-actions">
                <button
                  className="cart-item-remove-button"
                  onClick={() => handleRemoveBundle(bundle.bundleId)}
                  disabled={loading}
                >
                  <FaTrashAlt />
                </button>
              </div>
            </div>
          ))}
        </div>
        
        {}
        <div className="cart-summary lg:w-1/3">
          <div className="cart-summary-box">
            <h2 className="cart-summary-title">
                <FaReceipt className="title-icon" /> 
                Order summary
            </h2>
            
            <div className="summary-items-list">
                {cartItems
                    .filter(item => item && item.gameId && typeof item.price === 'number')
                    .map(item => (
                        <div key={item.gameId} className="summary-item">
                            <img 
                                src={item.imageUrl || '/img/game-placeholder.jpg'} 
                                alt={item.title || 'Game'}
                                className="summary-item-image"
                            />
                            <div className="summary-item-details">
                                <Link to={`/store/games/${item.gameId}`} className="summary-item-link">
                                    <p className="summary-item-title">{item.title || 'Unknown title'}</p> 
                                </Link>
                                <p className="summary-item-price">${item.price.toFixed(2)}</p>
                            </div>
                        </div>
                ))}
                {cartBundles.map(bundle => (
                  <div key={bundle.bundleId} className="summary-item">
                    <div className="summary-item-details">
                      <p className="summary-item-title">{bundle.name}</p>
                      <p className="summary-item-price">${bundle.price.toFixed(2)}</p>
                    </div>
                  </div>
                ))}
                 {cartItems.filter(item => item).length === 0 && cartBundles.filter(b => b).length === 0 && (
                    <p className="summary-empty-text">Your cart for summary is empty.</p>
                 )}
            </div>

            <div className="summary-total-section">
              <div className="summary-total-row">
                <span>Total to pay:</span>
                <span className="summary-total-amount">${total.toFixed(2)}</span>
              </div>
            </div>
            
            <button 
              className="checkout-button"
              onClick={handleCheckout}
              disabled={loading || totalCount === 0}
            >
              Place order
            </button>
            
            <Link 
              to="/store" 
              className="continue-shopping-link"
            >
              Continue shopping
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart; 