import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import axios from '../utils/axios-config';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import './BundleDetailsPage.css';
import { useDispatch } from 'react-redux';
import { addNotification } from '../store/slices/uiSlice';

const BundleDetailsPage = () => {
  const { id } = useParams();
  const [bundle, setBundle] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchBundle = async () => {
      try {
        const response = await axios.get(`/api/bundles/${id}`);
        setBundle(response.data);
      } catch (err) {
        console.error('Failed to fetch bundle:', err);
        setError(err.response?.data?.message || err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchBundle();
  }, [id]);

  const handleAddBundleToCart = async () => {
    if (!bundle) return;
    try {
      await axios.post(`/api/cart/bundles/${bundle.id}`);
      dispatch(addNotification({ message: 'Bundle successfully added to cart', type: 'success' }));
      navigate('/cart');
    } catch (err) {
      console.error('Error adding bundle to cart:', err);
      const msg = err.response?.data?.message || 'Failed to add bundle to cart';
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <p>Error: {error}</p>;
  if (!bundle) return <p>Bundle not found.</p>;

  return (
    <div className="bundle-details-container">
      <Link to="/bundles" className="back-link">← Back to Bundles</Link>
      <img src={bundle.imageUrl || '/img/game-placeholder.jpg'} alt={`${bundle.name} banner`} className="bundle-banner-image" />
      <h1>{bundle.name}</h1>
      {bundle.description && <p className="bundle-description">{bundle.description}</p>}
      <p className="bundle-price">Price: ${bundle.price.toFixed(2)}</p>
      {bundle.ownedDiscount > 0 && <p className="bundle-price">Owned Discount: ${bundle.ownedDiscount.toFixed(2)}</p>}
      <p className="bundle-price">Final Price: ${bundle.finalPrice.toFixed(2)}</p>
      <button onClick={handleAddBundleToCart} className="add-bundle-button">
        Add Bundle to Cart
      </button>
      {bundle.discountPercentage > 0 && <p className="bundle-price">Discount: {bundle.discountPercentage}%</p>}
      {bundle.discountPrice != null && <p className="bundle-price">Discounted Price: ${bundle.discountPrice.toFixed(2)}</p>}
      {bundle.startDate && <p className="bundle-price">Start: {new Date(bundle.startDate).toLocaleDateString()}</p>}
      {bundle.endDate && <p className="bundle-price">End: {new Date(bundle.endDate).toLocaleDateString()}</p>}
      <h2 className="bundle-title">Games in Bundle:</h2>
      {bundle.gameIds && bundle.gameIds.length > 0 ? (
        <div className="bundle-game-grid">
          {bundle.games.map(game => (
            <Link key={game.gameId} to={`/game/${game.gameId}`} className="bundle-game-item">
              <img src={game.imageUrl || '/img/game-placeholder.png'} alt={game.title} className="bundle-game-image" />
              <span className="bundle-game-title">{game.title}</span>
            </Link>
          ))}
        </div>
      ) : (
        <p>No games in this bundle.</p>
      )}
    </div>
  );
};

export default BundleDetailsPage; 