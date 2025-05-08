import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { loginStart, loginSuccess, loginFailure } from '../../store/slices/authSlice';
import { addNotification } from '../../store/slices/uiSlice';
import axios from '../../utils/axios-config';
import { FaLock } from 'react-icons/fa';
import './Login.css';

const Login = () => {
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    rememberMe: false
  });
  
  const { loading, error } = useSelector(state => state.auth);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    dispatch(loginStart());

    try {
      const response = await axios.post('/api/auth/login', formData);
      
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
      }
      
      if (response.data.token) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
      }
      
      dispatch(loginSuccess(response.data));
      
      dispatch(addNotification({
        message: 'Login successful',
        type: 'success'
      }));
      
      navigate('/');
    } catch (err) {
      console.error('Login error:', err);
      
      dispatch(loginFailure(
        err.response?.data?.message || 'Login failed. Please check your credentials.'
      ));
      
      dispatch(addNotification({
        message: err.response?.data?.message || 'Login failed. Please check your credentials.',
        type: 'error'
      }));
    }
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <h2 className="text-sm font-medium text-[#66c0f4] uppercase mb-4">Sign In</h2>
        
        {error && (
          <div className="login-error">
            {error}
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="login-form">
          <div className="mb-4">
            <input
              type="text"
              name="username"
              placeholder="Username"
              className="login-input"
              value={formData.username}
              onChange={handleChange}
              required
            />
          </div>
          
          <div className="mb-4">
            <input
              type="password"
              name="password"
              placeholder="Password"
              className="login-input"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>
          
          <div className="login-remember">
            <input
              type="checkbox"
              id="rememberMe"
              name="rememberMe"
              className="login-checkbox"
              checked={formData.rememberMe}
              onChange={handleChange}
            />
            <label htmlFor="rememberMe" className="text-gray-400 text-sm">Remember me</label>
          </div>
          
          <button type="submit" className="login-button" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
          
          <p className="login-link">
            <Link to="/forgot-password">Forgot password?</Link>
          </p>
        </form>
        
        <p className="login-link">
          Don't have an account? <Link to="/register">Register</Link>
        </p>
      </div>
    </div>
  );
};

export default Login; 