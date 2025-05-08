import React, { useState, useEffect } from 'react';
import axios from '../utils/axios-config';
import GameCard from '../components/Game/GameCard';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import './HomePage.css'; 

const GameSection = ({ title, games, isLoading, error }) => {
    if (isLoading) return <LoadingSpinner />;
    if (error) return <div className="error-message">{error}</div>;
    if (!games || games.length === 0) return null; 

    return (
        <section className="home-section">
            <h2>{title}</h2>
            <div className="game-grid">
                {games.map(game => (
                    <GameCard key={game.id} game={game} />
                ))}
            </div>
        </section>
    );
};

const HomePage = () => {
    const [featuredGames, setFeaturedGames] = useState([]);
    const [specialOffers, setSpecialOffers] = useState([]);
    const [newReleases, setNewReleases] = useState([]);
    const [isLoading, setIsLoading] = useState({ featured: true, special: true, new: true });
    const [error, setError] = useState({ featured: null, special: null, new: null });

    useEffect(() => {
        const fetchGames = async (endpoint, setter, loadingKey, errorKey) => {
            try {
                const response = await axios.get(`/api/games/${endpoint}`);
                setter(response.data);
            } catch (err) {
                console.error(`Error fetching ${endpoint}:`, err);
                setError(prev => ({ ...prev, [errorKey]: `Failed to load ${endpoint}` }));
            } finally {
                setIsLoading(prev => ({ ...prev, [loadingKey]: false }));
            }
        };

        fetchGames('featured', setFeaturedGames, 'featured', 'featured');
        fetchGames('special-offers', setSpecialOffers, 'special', 'special');
        fetchGames('new-releases', setNewReleases, 'new', 'new');
    }, []);

    return (
        <div className="home-page-container">
            {/* <HeroBanner /> */}
            
            <GameSection 
                title="Recommended"
                games={featuredGames} 
                isLoading={isLoading.featured} 
                error={error.featured}
            />
            <GameSection 
                title="Special Offers"
                games={specialOffers} 
                isLoading={isLoading.special} 
                error={error.special}
            />
            <GameSection 
                title="New Releases"
                games={newReleases} 
                isLoading={isLoading.new} 
                error={error.new}
            />
        </div>
    );
};

export default HomePage; 