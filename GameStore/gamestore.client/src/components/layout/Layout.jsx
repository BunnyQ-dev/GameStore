import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Outlet } from 'react-router-dom';
import styled from 'styled-components';
import axios from 'axios';
import Navbar from './Navbar';
import Footer from './Footer';
import Notifications from '../ui/Notifications';
import { getUserProfile } from '../../store/slices/authSlice';

const LayoutContainer = styled.div`
  display: flex;
  flex-direction: column;
  min-height: 100vh;
`;

const MainContent = styled.main`
  flex: 1;
  padding: 1rem 0;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
`;

const Layout = () => {
  const dispatch = useDispatch();
  const { isAuthenticated, token } = useSelector((state) => state.auth);

  useEffect(() => {
    axios.defaults.baseURL = import.meta.env.VITE_API_URL || '';
    
    if (token) {
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      delete axios.defaults.headers.common['Authorization'];
    }
    
    const responseInterceptor = axios.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response && error.response.status === 401) {
          console.error('Authentication error:', error);
        }
        return Promise.reject(error);
      }
    );
    
    return () => {
      axios.interceptors.response.eject(responseInterceptor);
    };
  }, [token]);

  useEffect(() => {
    if (isAuthenticated) {
      dispatch(getUserProfile());
    }
  }, [dispatch, isAuthenticated]);

  return (
    <LayoutContainer>
      <Navbar />
      <MainContent>
        <Outlet />
      </MainContent>
      <Footer />
      <Notifications />
    </LayoutContainer>
  );
};

export default Layout; 