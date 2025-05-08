import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import styled from 'styled-components';
import { FaSearch, FaShoppingCart, FaBars, FaSignOutAlt, FaShieldAlt } from 'react-icons/fa';
import { logout } from '../../store/slices/authSlice';
import { setSearchQuery } from '../../store/slices/uiSlice';

const serverUrl = 'https://localhost:7297';

const NavbarContainer = styled.nav`
  background-color: var(--bg-color);
  color: var(--text-color);
  height: var(--header-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2rem;
  position: sticky;
  top: 0;
  z-index: 1000;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.3);
`;

const Logo = styled(Link)`
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-color);
  display: flex;
  align-items: center;
  gap: 0.5rem;
  
  &:hover {
    color: var(--accent-color);
  }
`;

const NavLinks = styled.div`
  display: flex;
  align-items: center;
  gap: 1.5rem;
  
  @media (max-width: 768px) {
    display: none;
  }
`;

const NavLink = styled(Link)`
  color: var(--text-color);
  font-weight: 500;
  padding: 0.5rem;
  border-radius: 4px;
  
  &:hover, &.active {
    color: var(--accent-color);
    background-color: rgba(102, 192, 244, 0.1);
  }
`;

const AdminLink = styled(NavLink)`
  display: flex;
  align-items: center;
  gap: 0.3rem;
  color: var(--accent-color);
  
  &:hover {
    background-color: rgba(102, 192, 244, 0.2);
  }
`;

const SearchContainer = styled.div`
  display: flex;
  align-items: center;
  background-color: var(--bg-color-secondary);
  border-radius: 4px;
  padding: 0.5rem;
  width: 300px;
  
  @media (max-width: 968px) {
    width: 200px;
  }
  
  @media (max-width: 768px) {
    display: none;
  }
`;

const SearchInput = styled.input`
  background: transparent;
  border: none;
  color: var(--text-color);
  width: 100%;
  font-size: 0.9rem;
  
  &:focus {
    outline: none;
  }
`;

const SearchIcon = styled(FaSearch)`
  color: var(--text-color-secondary);
  margin-right: 0.5rem;
`;

const RightSection = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
`;

const CartButton = styled(Link)`
  position: relative;
  color: var(--text-color);
  font-size: 1.2rem;
  
  &:hover {
    color: var(--accent-color);
  }
`;

const CartBadge = styled.span`
  position: absolute;
  top: -5px;
  right: -8px;
  background-color: var(--accent-color);
  color: white;
  border-radius: 50%;
  padding: 0.1rem 0.3rem;
  font-size: 0.7rem;
  font-weight: bold;
`;

const UserContainer = styled.div`
  position: relative;
`;

const UserButton = styled.button`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: transparent;
  border: none;
  color: var(--text-color);
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 4px;
  
  &:hover {
    background-color: var(--bg-color-tertiary);
  }
`;

const UserMenu = styled.div`
  position: absolute;
  top: 100%;
  right: 0;
  background-color: var(--bg-color-secondary);
  border-radius: 4px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
  min-width: 200px;
  z-index: 1000;
  display: ${props => (props.$isOpen ? 'block' : 'none')};
`;

const UserMenuItem = styled(Link)`
  display: block;
  padding: 0.8rem 1rem;
  color: var(--text-color);
  transition: background-color 0.2s;
  
  &:hover {
    background-color: var(--bg-color-tertiary);
    color: var(--text-color);
  }
`;

const LogoutButton = styled.button`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  text-align: left;
  padding: 0.8rem 1rem;
  background: transparent;
  border: none;
  color: var(--text-color);
  cursor: pointer;
  
  &:hover {
    background-color: var(--bg-color-tertiary);
  }
`;

const MobileMenuButton = styled.button`
  display: none;
  background: transparent;
  border: none;
  color: var(--text-color);
  font-size: 1.5rem;
  cursor: pointer;
  
  @media (max-width: 768px) {
    display: block;
  }
`;

const Navbar = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useSelector((state) => state.auth);
  const { items } = useSelector((state) => state.cart);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [searchValue, setSearchValue] = useState('');

  const currentUserId = user?.id || user?.userId;

  const handleLogout = () => {
    dispatch(logout());
    setUserMenuOpen(false);
    navigate('/');
  };

  const handleSearch = (e) => {
    if (e.key === 'Enter') {
      dispatch(setSearchQuery(searchValue));
      navigate(`/store/search?q=${encodeURIComponent(searchValue)}`);
    }
  };

  const isAdmin = user?.roles?.includes('Admin');

  return (
    <NavbarContainer>
      <Logo to="/">
        GameStore
      </Logo>

      <NavLinks>
        <NavLink to="/store">Store</NavLink>
        <NavLink to="/library">Library</NavLink>
        <NavLink to="/bundles">Bundles</NavLink>
        {isAdmin && (
          <AdminLink to="/admin">
            <FaShieldAlt /> Admin
          </AdminLink>
        )}
      </NavLinks>

      <SearchContainer>
        <SearchIcon />
        <SearchInput
          type="text"
          placeholder="Search..."
          value={searchValue}
          onChange={(e) => setSearchValue(e.target.value)}
          onKeyDown={handleSearch}
        />
      </SearchContainer>

      <RightSection>
        <CartButton to="/cart">
          <FaShoppingCart />
          {items.length > 0 && <CartBadge>{items.length}</CartBadge>}
        </CartButton>

        {isAuthenticated ? (
          <UserContainer>
            <UserButton onClick={() => setUserMenuOpen(!userMenuOpen)}>
              <img 
                src={user?.profilePictureUrl ? `${serverUrl}${user.profilePictureUrl}` : '/img/default-avatar.jpg'} 
                alt="avatar" 
                style={{ width: '30px', height: '30px', borderRadius: '50%' }}
              />
              <span>{user?.displayName || user?.username}</span>
            </UserButton>
            
            <UserMenu $isOpen={userMenuOpen}>
              {isAuthenticated ? (
                currentUserId ? (
                  <UserMenuItem to={`/profile/id/${currentUserId}`} onClick={() => setUserMenuOpen(false)}>
                    My Profile
                  </UserMenuItem>
                ) : (
                  <UserMenuItem as="div" style={{ opacity: 0.6, cursor: 'not-allowed', color: 'var(--text-color-secondary)' }}>
                    My Profile (Loading...)
                  </UserMenuItem>
                )
              ) : null}
              
              <UserMenuItem to="/library" onClick={() => setUserMenuOpen(false)}>
                My Games
              </UserMenuItem>
              
              <UserMenuItem to="/friends" onClick={() => setUserMenuOpen(false)}>
                Friends
              </UserMenuItem>
              
              <UserMenuItem to="/wishlist" onClick={() => setUserMenuOpen(false)}>
                Wishlist
              </UserMenuItem>
              
              {isAdmin && (
                <UserMenuItem to="/admin" onClick={() => setUserMenuOpen(false)}>
                  Admin Panel
                </UserMenuItem>
              )}
              
              <LogoutButton onClick={handleLogout}>
                <FaSignOutAlt /> Log Out
              </LogoutButton>
            </UserMenu>
          </UserContainer>
        ) : (
          <NavLink to="/login">Sign In</NavLink>
        )}
        
        <MobileMenuButton>
          <FaBars />
        </MobileMenuButton>
      </RightSection>
    </NavbarContainer>
  );
};

export default Navbar; 