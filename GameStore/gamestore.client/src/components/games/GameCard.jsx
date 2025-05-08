import { Link } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import styled from 'styled-components';
import { FaPlus, FaCheck, FaHeart, FaRegHeart } from 'react-icons/fa';
import { addToCartStart, addToCartSuccess, addToCartFailure } from '../../store/slices/cartSlice';
import { addNotification } from '../../store/slices/uiSlice';
import axios from 'axios';
import { useState, useEffect } from 'react';

const Card = styled(Link)`
  display: flex;
  flex-direction: column;
  background-color: var(--bg-color-secondary);
  border-radius: 4px;
  overflow: hidden;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  color: var(--text-color);
  text-decoration: none;
  height: 100%;
  
  &:hover {
    transform: translateY(-5px);
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    color: var(--text-color);
  }
`;

const ImageContainer = styled.div`
  position: relative;
  width: 100%;
  padding-top: 56.25%; /* 16:9 аспект */
  overflow: hidden;
`;

const GameImage = styled.img`
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
  
  ${Card}:hover & {
    transform: scale(1.05);
  }
`;

const DiscountBadge = styled.div`
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background-color: var(--success-color);
  color: white;
  font-weight: bold;
  padding: 0.3rem 0.5rem;
  border-radius: 4px;
  font-size: 0.9rem;
`;

const CardContent = styled.div`
  padding: 1rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
`;

const Title = styled.h3`
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
`;

const GenresList = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
  margin-bottom: 0.5rem;
`;

const Genre = styled.span`
  font-size: 0.7rem;
  color: var(--text-color-secondary);
  background-color: var(--bg-color-tertiary);
  padding: 0.1rem 0.4rem;
  border-radius: 2px;
`;

const PriceContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: auto;
`;

const Price = styled.div`
  font-weight: 600;
  font-size: 1.1rem;
  
  ${props => props.$hasDiscount && `
    display: flex;
    align-items: center;
    gap: 0.5rem;
  `}
`;

const OriginalPrice = styled.span`
  text-decoration: line-through;
  color: var(--text-color-secondary);
  font-size: 0.9rem;
`;

const DiscountedPrice = styled.span`
  color: var(--accent-color);
`;

const ActionsContainer = styled.div`
  display: flex;
  gap: 0.5rem;
`;

const ActionButton = styled.button`
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--accent-color);
  color: white;
  border: none;
  border-radius: 2px;
  padding: 0.4rem;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background-color 0.2s;
  
  &:hover {
    background-color: var(--accent-color-hover);
  }
  
  &:disabled {
    background-color: var(--bg-color-tertiary);
    cursor: not-allowed;
  }
`;

const WishlistButton = styled(ActionButton)`
  background-color: ${props => props.$isInWishlist ? 'var(--danger-color)' : 'var(--bg-color-tertiary)'};
  
  &:hover {
    background-color: ${props => props.$isInWishlist ? 'var(--danger-color-hover)' : 'var(--bg-color-tertiary-hover)'};
  }
`;

const GameCard = ({ game }) => {
  const dispatch = useDispatch();
  const [isInWishlist, setIsInWishlist] = useState(false);
  const [loading, setLoading] = useState(false);
  const [addingToCart, setAddingToCart] = useState(false);
  const isAuthenticated = useSelector(state => state.auth.isAuthenticated);

  useEffect(() => {
    const checkWishlistStatus = async () => {
      if (!isAuthenticated || !game.id) return;
      
      try {
        const response = await axios.get(`/api/wishlist/IsInWishlist/${game.id}`);
        setIsInWishlist(response.data);
      } catch (err) {
        console.error('Error checking wishlist status:', err);
      }
    };
    
    checkWishlistStatus();
  }, [game.id, isAuthenticated]);

  const handleAddToCart = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (addingToCart) return;
    
    setAddingToCart(true);
    dispatch(addToCartStart());
    
    try {
      const response = await axios.post('/api/cart/add', {
        gameId: game.id,
        quantity: 1
      });
      
      dispatch(addToCartSuccess(response.data));
      dispatch(addNotification({
        type: 'success',
        message: 'Game added to cart'
      }));
    } catch (error) {
      console.error('Failed to add game to cart:', error);
      dispatch(addToCartFailure(error.message));
      dispatch(addNotification({
        type: 'error',
        message: 'Failed to add game to cart'
      }));
    } finally {
      setAddingToCart(false);
    }
  };

  const handleWishlistToggle = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!isAuthenticated) {
      dispatch(addNotification({
        type: 'info',
        message: 'Login to add game to wishlist'
      }));
      return;
    }
    
    if (loading) return;
    
    setLoading(true);
    try {
      if (isInWishlist) {
        await axios.delete(`/api/wishlist/remove/${game.id}`);
        setIsInWishlist(false);
        dispatch(addNotification({
          type: 'success',
          message: 'Game removed from wishlist'
        }));
      } else {
        await axios.post(`/api/wishlist/add/${game.id}`);
        setIsInWishlist(true);
        dispatch(addNotification({
          type: 'success',
          message: 'Game added to wishlist'
        }));
      }
    } catch (err) {
      console.error('Error updating wishlist:', err);
      dispatch(addNotification({
        type: 'error',
        message: 'Failed to update wishlist'
      }));
    } finally {
      setLoading(false);
    }
  };

  const hasDiscount = game.discountPercent > 0;
  
  return (
    <Card to={`/store/game/${game.id}`}>
      <ImageContainer>
        <GameImage src={game.imageUrl || '/img/game-placeholder.png'} alt={game.title} />
        {hasDiscount && <DiscountBadge>-{game.discountPercent}%</DiscountBadge>}
      </ImageContainer>
      
      <CardContent>
        <Title>{game.title}</Title>
        
        <GenresList>
          {game.genres?.slice(0, 3).map((genre, index) => (
            <Genre key={index}>{genre}</Genre>
          ))}
        </GenresList>
        
        <PriceContainer>
          {hasDiscount ? (
            <Price $hasDiscount>
              <OriginalPrice>${game.originalPrice.toFixed(2)}</OriginalPrice>
              <DiscountedPrice>${game.price.toFixed(2)}</DiscountedPrice>
            </Price>
          ) : (
            <Price>
              {game.price === 0 ? 'Free' : `$${game.price.toFixed(2)}`}
            </Price>
          )}
          
          <ActionsContainer>
            <WishlistButton 
              $isInWishlist={isInWishlist} 
              onClick={handleWishlistToggle}
              disabled={loading}
            >
              {isInWishlist ? <FaHeart /> : <FaRegHeart />}
            </WishlistButton>
            
            <ActionButton 
              onClick={handleAddToCart} 
              disabled={addingToCart}
            >
              {addingToCart ? <FaCheck /> : <FaPlus />}
            </ActionButton>
          </ActionsContainer>
        </PriceContainer>
      </CardContent>
    </Card>
  );
};

export default GameCard; 