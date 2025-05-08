import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from '../utils/axios-config';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import './BundlesPage.css';

const BundlesPage = () => {
  const [bundles, setBundles] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBundles = async () => {
      try {
        const res = await axios.get('/api/bundles');
        setBundles(res.data || []);
      } catch (err) {
        console.error('Failed to fetch bundles:', err);
      } finally {
        setLoading(false);
      }
    };
    fetchBundles();
  }, []);

  if (loading) return <LoadingSpinner />;

  return (
    <div className="bundles-container">
      <h1>Bundles</h1>
      {bundles.length === 0 ? (
        <p>No bundles available.</p>
      ) : (
        <div className="bundle-list">
          {bundles.map(bundle => (
            <div className="bundle-card" key={bundle.id}>
              <img 
                src={bundle.imageUrl || '/img/game-placeholder.jpg'}
                alt={`${bundle.name} cover`}
                className="bundle-image"
                onError={(e) => { e.target.onerror = null; e.target.src='/img/game-placeholder.jpg'; }}
              />
              <h3>{bundle.name}</h3>
              <p>Price: ${bundle.price.toFixed(2)}</p>
              {bundle.discountPercentage > 0 && <p>Discount: {bundle.discountPercentage}%</p>}
              <Link to={`/bundles/${bundle.id}`}>View Details</Link>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default BundlesPage; 