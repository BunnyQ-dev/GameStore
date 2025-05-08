import React from 'react';
import { Link } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux'; 
import { addToCartStart, addToCartSuccess, addToCartFailure } from '../../store/slices/cartSlice';
import axios from '../../utils/axios-config'; 
import { FaStar, FaRegStar, FaStarHalfAlt, FaPlay, FaShoppingCart, FaCheckCircle, FaCheck } from 'react-icons/fa'; 
import './GameCard.css';

// Price display function using base price to calculate discount
const renderPrice = (price, discountPercentage) => {
    if (discountPercentage > 0) {
        const originalPrice = price;
        const discountedPrice = (price * (100 - discountPercentage)) / 100;
        return (
            <div className="game-card-price discounted">
                <span className="game-card-discount">-{discountPercentage}%</span>
                <div className="price-values">
                    <span className="game-card-original-price">${originalPrice.toFixed(2)}</span>
                    <span className="game-card-current-price">${discountedPrice.toFixed(2)}</span>
                </div>
            </div>
        );
    }
    return <div className="game-card-price">${price.toFixed(2)}</div>;
};

const RatingStars = ({ rating, count }) => {
  if (rating == null || count === 0) {
    return <div className="rating-stars no-rating">No reviews</div>;
  }

  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;
  const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

  return (
    <div className="rating-stars">
      {[...Array(fullStars)].map((_, i) => <FaStar key={`full-${i}`} />)}
      {hasHalfStar && <FaStarHalfAlt key="half" />}
      {[...Array(emptyStars)].map((_, i) => <FaRegStar key={`empty-${i}`} />)}
      <span className="review-count">({count})</span>
    </div>
  );
};

const GameCard = ({ game }) => {
    const dispatch = useDispatch();
    const { items: cartItems, loading: cartLoading } = useSelector(state => state.cart);
    const { user: currentUser } = useSelector((state) => state.auth);
    
    const isInCart = !game.isOwned && cartItems.some(item => item.gameId === game.id);

    const handleAddToCart = async (e) => {
        e.preventDefault(); 
        e.stopPropagation();
        if (!isInCart && !cartLoading) { 

            dispatch(addToCartStart());
            try {
                const response = await axios.post(`/api/cart/${game.id}`); 
                dispatch(addToCartSuccess(response.data));
                 alert('Game added to cart!');
            } catch (error) {
                console.error("Failed to add to cart:", error);
                dispatch(addToCartFailure(error.response?.data?.message || 'Failed to add game to cart'));
                 alert(error.response?.data || 'Failed to add to cart');
            }
        }
    };

    if (!game) return null;

    return (
        <Link to={`/game/${game.id}`} className={`game-card-link ${game.isOwned ? 'owned' : ''}`}>
            <div className="game-card">
                <div className="game-card-image-container">
                    <img src={game.imageUrl || '/img/game-placeholder.png'} alt={`${game.title} cover`} className="game-card-image" />
                    {}
                    {game.isOwned && <div className="game-card-owned-badge">Owned</div>}
                </div>
                <div className="game-card-info">
                    <h3 className="game-card-title">{game.title}</h3>
                    <div className="game-card-tags">
                        {game.genres?.slice(0, 3).map(genre => (
                            <span key={genre} className="game-card-tag">{genre}</span>
                        ))}
                        {} 
                    </div>
                    {renderPrice(game.price, game.discountPercentage)}
                    {}
                    <div className="game-card-rating">
                        <RatingStars rating={game.averageRating} count={game.reviewCount} />
                    </div>
                    {} 
                    {game.isOwned ? (
                        <button 
                            className="game-card-button play-button" 
                            onClick={(e) => { 
                                e.preventDefault(); 
                                e.stopPropagation(); 
                                alert('Launching game...'); 
                            }}
                        >
                           <FaPlay /> Play
                        </button>
                    ) : currentUser ? (
                        <button 
                            className={`game-card-button cart-button ${isInCart ? 'in-cart' : ''}`} 
                            onClick={handleAddToCart} 
                            disabled={isInCart || cartLoading}
                        >
                            {cartLoading ? '...' : (isInCart ? <><FaCheck /> In Cart</> : <><FaShoppingCart /> Add to Cart</>)}
                        </button>
                    ) : null }
                </div>
            </div>
        </Link>
    );
};

export default GameCard; 