import React, { useEffect } from 'react';
import { Link, NavLink } from 'react-router-dom';
import useAuthStore from '../../store/authStore'; 
import useCartStore from '../../store/cartStore'; 
import './Header.css';

const serverUrl = 'https://localhost:7297';

const Header = () => {
    const { user, logout, token } = useAuthStore();
    const { totalItems, fetchCart, clearCartLocally } = useCartStore();
    const isAuthenticated = !!token;

    useEffect(() => {
        if (isAuthenticated) {
            fetchCart();
        } else {
            clearCartLocally(); 
        }
    }, [isAuthenticated, fetchCart, clearCartLocally]);

    const handleLogout = () => {
        logout();
    };

    return (
        <header className="header">
            <div className="header-container">
                <Link to="/" className="logo">
                    GameStore
                </Link>
                <nav className="navigation">
                    <NavLink to="/store" className={({ isActive }) => isActive ? 'active' : ''}>Store</NavLink>
                    {isAuthenticated && (
                        <>
                            <NavLink to="/library" className={({ isActive }) => isActive ? 'active' : ''}>Library</NavLink>
                            <NavLink to="/wishlist" className={({ isActive }) => isActive ? 'active' : ''}>Wishlist</NavLink>
                            <NavLink to="/friends" className={({ isActive }) => isActive ? 'active' : ''}>Friends</NavLink>
                        </>
                    )}
                    {}
                </nav>
                <div className="user-actions">
                    {isAuthenticated ? (
                        <>
                            <Link to="/cart" className="cart-link">
                                <i className="fa fa-shopping-cart"></i>
                                {totalItems > 0 && <span className="cart-count">{totalItems}</span>}
                            </Link>
                            <div className="profile-menu">
                                <Link to={`/profile/id/${user.id}`} className="profile-link">
                                    <img 
                                        src={user?.profilePictureUrl ? `${serverUrl}${user.profilePictureUrl}` : '/img/default-avatar.jpg'} 
                                        alt="Avatar" 
                                        className="navbar-avatar" 
                                    />
                                    <span>{user?.displayName || user?.userName || 'Profile'}</span> 
                                </Link>
                                <button onClick={handleLogout} className="logout-button">Logout</button>
                                {user?.roles?.includes('Admin') && (
                                     <Link to="/admin" className="admin-link">Admin</Link>
                                )}
                            </div>
                        </>
                    ) : (
                        <>
                            <Link to="/login" className="login-button">Login</Link>
                            <Link to="/register" className="register-button">Register</Link>
                        </>
                    )}
                </div>
            </div>
        </header>
    );
};

export default Header; 