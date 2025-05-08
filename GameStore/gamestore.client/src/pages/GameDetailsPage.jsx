import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import axios from '../utils/axios-config'; 
import LoadingSpinner from '../components/ui/LoadingSpinner'; 
import { FaHeart, FaRegHeart, FaShoppingCart, FaStar, FaTrashAlt } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux'; 
import { addToCartStart, addToCartSuccess, addToCartFailure } from '../store/slices/cartSlice'; 
import { addNotification } from '../store/slices/uiSlice'; 
import './GameDetailsPage.css'; 
import './Store/GameDetails.css'; 

const GameDetailsPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [game, setGame] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isInWishlist, setIsInWishlist] = useState(false);
    const [isPurchased, setIsPurchased] = useState(false);
    const dispatch = useDispatch(); 
    const { user: currentUser, isAuthenticated } = useSelector(state => state.auth);
    const currentUserId = currentUser?.id; 

    const [reviews, setReviews] = useState([]);
    const [myReview, setMyReview] = useState(null);
    const [reviewsLoading, setReviewsLoading] = useState(true);
    const [reviewsError, setReviewsError] = useState(null);
    const [rating, setRating] = useState(0); 
    const [comment, setComment] = useState(''); 
    const [isSubmittingReview, setIsSubmittingReview] = useState(false);

    const fetchGameDetails = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const response = await axios.get(`/api/games/${id}`);
            const data = response.data; 
            setGame(data);
            
            if (isAuthenticated) {
                setIsInWishlist(data.isInWishlist);
                setIsPurchased(data.isOwned); 
            }
        } catch (err) {
            console.error("Error fetching game details:", err);
            setError(err.response?.data?.message || err.message || 'Failed to load game details.');
        } finally {
            setIsLoading(false);
        }
    }, [id, isAuthenticated]);

    const fetchReviews = useCallback(async () => {
      setReviewsLoading(true);
      try {
        const reviewsRes = await axios.get(`/api/games/${id}/reviews`);
        setReviews(reviewsRes.data || []);

        if (isAuthenticated) {
          try {
            const myReviewRes = await axios.get(`/api/games/${id}/reviews/my`);
            setMyReview(myReviewRes.data);
            setRating(myReviewRes.data.rating); // Використовуємо поле з DTO
            setComment(myReviewRes.data.comment || '');
          } catch (myReviewErr) {
            if (myReviewErr.response?.status === 404) {
              setMyReview(null); 
              setRating(0);
              setComment('');
            } else {
              console.error('Error fetching my review:', myReviewErr);
            }
          }
        } else {
          setMyReview(null);
          setRating(0);
          setComment('');
        }
        setReviewsError(null);
      } catch (err) {
        console.error('Error fetching reviews:', err);
        setReviewsError('Error fetching reviews.');
      } finally {
        setReviewsLoading(false);
      }
    }, [id, isAuthenticated]);

    useEffect(() => {
        fetchGameDetails();
        fetchReviews();
    }, [fetchGameDetails, fetchReviews]); 

    const handleWishlistToggle = async () => {
        if (!isAuthenticated) {
            navigate('/login'); 
            return;
        }

        const url = `/api/wishlist/${id}`;
        const method = isInWishlist ? 'delete' : 'post'; 

        try {
            await axios[method](url);
            setIsInWishlist(!isInWishlist); 
        } catch (err) {
            console.error("Error updating wishlist:", err);
            dispatch(addNotification({ type: 'error', message: err.response?.data?.message || 'Error updating wishlist' }));
        }
    };

    const handlePurchase = async () => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }

        if (isPurchased) return; 

        const url = `/api/purchase/${id}`;
        try {
            await axios.post(url);
            setIsPurchased(true); 
            alert('Game purchased successfully!'); 
            fetchReviews(); 
        } catch (err) {
            console.error("Error purchasing game:", err);
            alert(err.response?.data?.message || 'Failed to purchase game.'); 
        }
    };

    const handleSubmitReview = async (e) => {
        e.preventDefault();
        if (rating < 1 || rating > 5) {
            dispatch(addNotification({ type: 'error', message: 'Rating must be between 1 and 5.' }));
            return;
        }
        setIsSubmittingReview(true);
        try {
            await axios.post(`/api/games/${id}/reviews`, { rating, comment });
            dispatch(addNotification({ type: 'success', message: 'Відгук збережено!' }));
            fetchReviews(); 
        } catch (err) {
            console.error('Error submitting review:', err);
            dispatch(addNotification({ type: 'error', message: err.response?.data?.message || 'Error saving review.' }));
        } finally {
            setIsSubmittingReview(false);
        }
    };

    const handleDeleteReview = async () => {
        if (!myReview) return;
        setIsSubmittingReview(true); 
        try {
            await axios.delete(`/api/games/${id}/reviews`);
            dispatch(addNotification({ type: 'success', message: 'Your review has been deleted.' }));
            setMyReview(null);
            setRating(0);
            setComment('');
            fetchReviews(); 
        } catch (err) {
            console.error('Error deleting review:', err);
            dispatch(addNotification({ type: 'error', message: 'Error deleting review.' }));
        } finally {
            setIsSubmittingReview(false);
        }
    };

    const StarRating = ({ value, onChange, readOnly = false }) => {
        return (
            <div className={`star-rating ${readOnly ? 'read-only' : ''}`}>
                {[...Array(5)].map((_, index) => {
                    const starValue = index + 1;
                    return (
                        <FaStar 
                            key={starValue}
                            className={`star ${starValue <= value ? 'filled' : ''}`}
                            onClick={() => !readOnly && onChange(starValue)}
                        />
                    );
                })}
            </div>
        );
    };

    if (isLoading) {
        return <LoadingSpinner />;
    }

    if (error) {
        return <div className="error-message">Error: {error}</div>;
    }

    if (!game) {
        return <div className="error-message">Game not found.</div>;
    }

    const renderPrice = () => {
        if (game.discountPercentage > 0 && game.originalPrice) {
            return (
                <div className="price-section discounted">
                    <span className="original-price">${game.originalPrice.toFixed(2)}</span>
                    <span className="current-price">${game.price.toFixed(2)}</span>
                    <span className="discount-badge">-{game.discountPercentage}%</span>
                </div>
            );
        }
        return <div className="price-section">${game.price.toFixed(2)}</div>;
    };

    return (
        <div className="game-details-container">
            <div className="game-header" style={{ backgroundImage: `url(${game.backgroundImageUrl || game.coverImageUrl})` }}>
                 <div className="header-overlay"></div>
                 <img src={game.coverImageUrl} alt={`${game.title} Cover`} className="game-cover-image" />
                 <div className="header-info">
                     <h1>{game.title}</h1>
                     {/* Об'єднуємо інформацію */} 
                     <p>
                        {game.developer || 'Unknown Developer'} / {game.publisher || 'Unknown Publisher'}
                     </p>
                     <p>Release Date: {new Date(game.releaseDate).toLocaleDateString()}</p>
                     <div className="tags mt-2">
                          {game.genres?.map(genre => <span key={genre} className="tag genre-tag">{genre}</span>)}
                          {game.platforms?.map(platform => <span key={platform} className="tag platform-tag">{platform}</span>)}
                     </div>
                 </div>
            </div>

            <div className="game-actions-price">
                {renderPrice()}
                <div className="action-buttons">
                    <button 
                        onClick={handleWishlistToggle} 
                        disabled={!isAuthenticated} 
                        className={`wishlist-button ${isInWishlist ? 'active' : ''}`}
                    >
                        {isInWishlist ? 'In Wishlist' : 'Add to Wishlist'} 
                        <i className={`fa ${isInWishlist ? 'fa-heart' : 'fa-heart-o'}`}></i>
                    </button>
                    <button 
                        onClick={handlePurchase} 
                        disabled={isPurchased || !isAuthenticated} 
                        className={`purchase-button ${isPurchased ? 'owned' : ''}`}
                    >
                        {isPurchased ? 'Owned' : 'Buy'}
                         <i className={`fa ${isPurchased ? 'fa-check' : 'fa-shopping-cart'}`}></i>
                    </button>
                </div>
            </div>

            <div className="game-content">
                <section className="game-description">
                    <h2>About the game</h2>
                    <p>{game.description}</p>
                </section>

                {game.trailerUrl && (
                    <section className="game-trailer">
                        <h2>Trailer</h2>
                        <iframe 
                            width="560" 
                            height="315" 
                            src={game.trailerUrl.replace("watch?v=", "embed/")} 
                            title={`${game.title} Trailer`}
                            frameBorder="0" 
                            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                            allowFullScreen>
                        </iframe>
                    </section>
                )}

                {game.screenshots && game.screenshots.length > 0 && (
                    <section className="game-screenshots">
                        <h2>Screenshots</h2>
                        <div className="screenshots-grid">
                            {game.screenshots.map((url, index) => (
                                <img key={index} src={url} alt={`Screenshot ${index + 1}`} className="screenshot-image" />
                            ))}
                        </div>
                    </section>
                )}

                {(game.minimumRequirements || game.recommendedRequirements) && (
                     <section className="system-requirements">
                         <h2>System Requirements</h2>
                         <div className="requirements-columns">
                             <div>
                                 <h3>Minimum</h3>
                                 {/* Використовуємо dangerouslySetInnerHTML для відображення <br/> */}
                                 <p dangerouslySetInnerHTML={{ __html: game.minimumRequirements?.replace(/\n/g, '<br/>') || 'N/A' }} /> 
                             </div>
                             <div>
                                 <h3>Recommended</h3>
                                 <p dangerouslySetInnerHTML={{ __html: game.recommendedRequirements?.replace(/\n/g, '<br/>') || 'N/A' }} />
                             </div>
                         </div>
                     </section>
                )}

                {/* --- Секція Відгуків --- */}
                <section className="reviews-section">
                    <h2>User Reviews ({reviews.length})</h2>

                    {/* Форма для відгуку */} 
                    {isAuthenticated ? (
                        isPurchased ? ( // Використовуємо isPurchased
                            <form onSubmit={handleSubmitReview} className="review-form mb-6">
                                <h3 className="text-lg font-semibold text-white mb-3">{myReview ? 'Your review:' : 'Leave a review:'}</h3>
                                <div className="mb-3">
                                    <StarRating value={rating} onChange={setRating} />
                                </div>
                                <textarea
                                    className="review-textarea"
                                    rows="4"
                                    placeholder="Write your comment here..."
                                    value={comment}
                                    onChange={(e) => setComment(e.target.value)}
                                />
                                <div className="review-form-actions mt-3">
                                    <button type="submit" className="button-primary" disabled={isSubmittingReview || rating < 1 || rating > 5}>
                                        {isSubmittingReview ? <LoadingSpinner size="small" /> : (myReview ? 'Update review' : 'Submit review')}
                                    </button>
                                    {myReview && (
                                        <button 
                                            type="button" 
                                            onClick={handleDeleteReview} 
                                            className="button-danger ml-2" 
                                            disabled={isSubmittingReview}
                                        >
                                            <FaTrashAlt /> Delete
                                        </button>
                                    )}
                                </div>
                            </form>
                        ) : (
                            <div className="bg-gray-800 p-4 rounded-lg mb-6 text-center">
                                <p className="text-gray-400">You must own the game to leave a review.</p>
                            </div>
                        )
                    ) : (
                        <div className="bg-gray-800 p-4 rounded-lg mb-6 text-center">
                            <p className="text-gray-400">Please <Link to="/login" className="text-blue-500 hover:underline">login</Link>, to leave a review.</p>
                        </div>
                    )}

                    {/* Список відгуків */} 
                    {reviewsLoading ? (
                        <LoadingSpinner />
                    ) : reviewsError ? (
                        <p className="text-red-500">{reviewsError}</p>
                    ) : reviews.length === 0 ? (
                        <p className="text-gray-400">There are no reviews for this game yet.</p>
                    ) : (
                        <div className="reviews-list">
                            {reviews.map(review => (
                                (isAuthenticated && myReview && currentUserId && review.userId === currentUserId) ? null : (
                                    <div key={review.userId} className="review-item">
                                        <div className="review-author">
                                            <img 
                                                src={review.userAvatarUrl ? `https://localhost:7297${review.userAvatarUrl}` : '/img/default-avatar.png'} 
                                                alt={review.userName}
                                                className="review-avatar"
                                            />
                                            <div>
                                                <Link to={`/profile/id/${review.userId}`} className="review-username">{review.userDisplayName || review.userName}</Link>
                                                <span className="review-date">{new Date(review.updatedAt).toLocaleDateString()}</span>
                                            </div>
                                        </div>
                                        <div className="review-content">
                                            <StarRating value={review.rating} readOnly={true} />
                                            {review.comment && <p className="review-comment">{review.comment}</p>}
                                        </div>
                                    </div>
                                )
                            ))}
                        </div>
                    )}
                </section>
            </div>
        </div>
    );
};

export default GameDetailsPage; 