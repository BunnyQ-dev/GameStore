import React, { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useLocation, useParams, useNavigate } from 'react-router-dom';
import axios from '../../utils/axios-config';
import GameCard from '../../components/Game/GameCard';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import Pagination from '../../components/ui/Pagination';
import Slider from 'rc-slider';
import 'rc-slider/assets/index.css';
import './StorePage.css';

const FiltersSidebar = ({ 
    genres, 
    platforms, 
    filters, 
    onFilterChange, 
    priceRange, 
    onPriceChange, 
    onMinPriceInputChange, 
    onMaxPriceInputChange,
    MAX_PRICE
}) => {
    
    const handleGenreChange = (e) => {
        onFilterChange('genres', [e.target.value]); // Allow only single selection
    };
    
    const handlePlatformChange = (e) => {
        onFilterChange('platforms', [e.target.value]); // Allow only single selection
    };

    return (
        <aside className="store-sidebar">
            <h3>Filters</h3>
            
            <div className="filter-section">
                <h4>Genres</h4>
                <select 
                    className="filter-select"
                    value={filters.genres?.[0] || ''} // Select first genre or empty
                    onChange={handleGenreChange}
                >
                    <option value="">All Genres</option>
                    {genres.map(genre => (
                        <option key={genre.id} value={genre.id}>{genre.name}</option>
                    ))}
                </select>
            </div>

            <div className="filter-section">
                 <h4>Platforms</h4>
                 <select
                    className="filter-select"
                    value={filters.platforms?.[0] || ''} // Select first platform or empty
                    onChange={handlePlatformChange}
                 >
                     <option value="">All Platforms</option>
                     {platforms.map(platform => (
                         <option key={platform.id} value={platform.id}>{platform.name}</option>
                     ))}
                 </select>
            </div>
            
             <div className="filter-section">
                 <h4>Price</h4>
                 <div className="price-inputs-container">
                     <div className="price-input-group">
                         <label htmlFor="minPrice">Min $</label>
                         <input 
                            type="number" 
                            id="minPrice"
                            name="minPrice"
                            value={priceRange[0]} 
                            onChange={onMinPriceInputChange}
                            min="0"
                            max={MAX_PRICE}
                            className="price-input"
                         />
                     </div>
                     <div className="price-input-group">
                         <label htmlFor="maxPrice">Max $</label>
                         <input 
                            type="number" 
                            id="maxPrice"
                            name="maxPrice"
                            value={priceRange[1]} 
                            onChange={onMaxPriceInputChange}
                            min="0"
                            max={MAX_PRICE}
                            className="price-input"
                         />
                     </div>
                 </div>
             </div>
        </aside>
    );
};

const StorePage = () => {
    const [games, setGames] = useState([]);
    const [genres, setGenres] = useState([]); 
    const [platforms, setPlatforms] = useState([]); 
    const [totalPages, setTotalPages] = useState(1);
    const [currentPage, setCurrentPage] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchParams, setSearchParams] = useSearchParams();
    const location = useLocation();
    const navigate = useNavigate();
    const { categoryId } = useParams(); 

    const [filters, setFilters] = useState({ genres: [], platforms: [] });
    const [selectedFilters, setSelectedFilters] = useState({ genres: [], platforms: [] });
    const [priceRange, setPriceRange] = useState([0, 2000]);
    const [debouncedPriceRange, setDebouncedPriceRange] = useState(priceRange);
    const [sort, setSort] = useState('releaseDate_desc'); 
    const [pagination, setPagination] = useState({ currentPage: 1, totalPages: 1, totalCount: 0 });
    const [searchQuery, setSearchQuery] = useState('');

    const MAX_PRICE = 2000; 

    useEffect(() => {
        const fetchFilterData = async () => {
            try {
                const [genresRes, platformsRes] = await Promise.all([
                    axios.get('/api/genres'), 
                    axios.get('/api/platforms') 
                ]);
                setGenres(genresRes.data || []);
                setPlatforms(platformsRes.data || []);
            } catch (err) {
                console.error("Error fetching filter data:", err);
            }
        };
        fetchFilterData();
    }, []);
    
    useEffect(() => {
        const urlFilters = {};
        if (searchParams.get('genres')) {
            urlFilters.genres = searchParams.get('genres').split(',');
        }
        if (searchParams.get('platforms')) {
             urlFilters.platforms = searchParams.get('platforms').split(',');
        }
        if (searchParams.get('sort')) {
            setSort(searchParams.get('sort'));
        }
        if (categoryId) { 
             urlFilters.genres = [categoryId]; 
        }
        
        setFilters(urlFilters);
        setCurrentPage(parseInt(searchParams.get('page') || '1', 10));

    }, [searchParams, categoryId]);

    useEffect(() => {
        const handler = setTimeout(() => {
            setDebouncedPriceRange(priceRange);
        }, 500); 
        return () => clearTimeout(handler);
    }, [priceRange]);

    const fetchStoreGames = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams();
        params.set('page', currentPage.toString());
        params.set('pageSize', '16'); 
        params.set('sort', sort);

        if (filters.genres && filters.genres.length > 0) {
            params.set('genres', filters.genres.join(','));
        }
        if (filters.platforms && filters.platforms.length > 0) {
             params.set('platforms', filters.platforms.join(','));
        }
        if (debouncedPriceRange[0] > 0) {
            params.set('minPrice', debouncedPriceRange[0].toString());
        }
        if (debouncedPriceRange[1] < MAX_PRICE) {
            params.set('maxPrice', debouncedPriceRange[1].toString());
        }
        
        let apiUrl = '/api/games';
        const searchQuery = searchParams.get('q');
        if (searchQuery) {
             apiUrl = '/api/games/search';
             params.set('q', searchQuery);
        } else if (categoryId) {

        }

        try {
            const response = await axios.get(apiUrl, { params });
            setGames(response.data.items || []); 
            setTotalPages(response.data.totalPages || 1); 
        } catch (err) {
            console.error("Error fetching store games:", err);
            setError(err.response?.data?.message || 'Failed to load games.');
             setGames([]); 
        } finally {
            setIsLoading(false);
        }
    }, [currentPage, sort, filters, categoryId, searchParams, debouncedPriceRange]);

    useEffect(() => {
        fetchStoreGames();
    }, [fetchStoreGames]);

    const updateUrlParams = (newFilters, newSort, newPage) => {
        const params = new URLSearchParams();
        params.set('page', newPage.toString());
        params.set('sort', newSort);
        if (newFilters.genres && newFilters.genres.length > 0) {
            params.set('genres', newFilters.genres.join(','));
        }
         if (newFilters.platforms && newFilters.platforms.length > 0) {
            params.set('platforms', newFilters.platforms.join(','));
        }
         
         const searchQuery = searchParams.get('q');
         if (searchQuery) {
             params.set('q', searchQuery);
         }
         
        setSearchParams(params);
    };

    const handleFilterChange = (filterKey, value) => {
        const newFilters = { 
            ...filters, 
            [filterKey]: Array.isArray(value) ? value : (value ? [value] : []) // Ensure value is array
        };
        setFilters(newFilters);
        updateUrlParams(newFilters, sort, 1);
        setCurrentPage(1);
    };

    const handlePriceChange = (newRange) => {
        setPriceRange(newRange);
    };

    const handleMinPriceInputChange = (e) => {
        const newMin = Math.max(0, Math.min(parseInt(e.target.value, 10) || 0, priceRange[1]));
        handlePriceChange([newMin, priceRange[1]]);
    };

    const handleMaxPriceInputChange = (e) => {
        const newMax = Math.min(MAX_PRICE, Math.max(parseInt(e.target.value, 10) || MAX_PRICE, priceRange[0]));
        handlePriceChange([priceRange[0], newMax]);
    };

    const handleSortChange = (e) => {
        const newSort = e.target.value;
        setSort(newSort);
        setCurrentPage(1); 
        updateUrlParams(filters, newSort, 1);
    };

    const handlePageChange = (page) => {
        setCurrentPage(page);
        updateUrlParams(filters, sort, page);
    };

    return (
        <div className="store-page-container">
            <FiltersSidebar 
                genres={genres} 
                platforms={platforms} 
                filters={filters} 
                onFilterChange={handleFilterChange}
                priceRange={priceRange}
                onPriceChange={handlePriceChange}
                onMinPriceInputChange={handleMinPriceInputChange}
                onMaxPriceInputChange={handleMaxPriceInputChange}
                MAX_PRICE={MAX_PRICE}
            />
            <main className="store-content">
                <div className="store-header">
                    <h1>Game Store</h1>
                    <div className="sort-options">
                        <label htmlFor="sort">Sort by:</label>
                        <select id="sort" value={sort} onChange={handleSortChange}>
                            <option value="releaseDate_desc">Release Date (newest)</option>
                            <option value="releaseDate_asc">Release Date (oldest)</option>
                            <option value="price_asc">Price (low to high)</option>
                            <option value="price_desc">Price (high to low)</option>
                            <option value="name_asc">Name (A-Z)</option>
                            <option value="name_desc">Name (Z-A)</option>
                        </select>
                    </div>
                </div>

                {isLoading ? (
                    <LoadingSpinner />
                ) : error ? (
                    <div className="error-message">{error}</div>
                ) : (
                    <>
                        <div className="store-game-grid">
                            {games.length > 0 ? (
                                games.map(game => (
                                    <GameCard key={game.id} game={game} />
                                ))
                            ) : (
                                <p>No games found matching your criteria.</p> 
                            )}
                        </div>
                        {totalPages > 1 && (
                           <Pagination 
                                currentPage={currentPage} 
                                totalPages={totalPages} 
                                onPageChange={handlePageChange} 
                            /> 
                        )}
                        
                    </>
                )}
            </main>
        </div>
    );
};

export default StorePage; 