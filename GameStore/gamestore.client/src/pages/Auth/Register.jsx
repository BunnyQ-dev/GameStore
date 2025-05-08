import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { registerStart, registerSuccess, registerFailure } from '../../store/slices/authSlice';
import { addNotification } from '../../store/slices/uiSlice';
import axios from 'axios';
import './Register.css';

const Register = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    displayName: '',
    agreeToTerms: false
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
    
    if (formData.password !== formData.confirmPassword) {
      dispatch(registerFailure('Passwords do not match'));
      dispatch(addNotification({
        message: 'Passwords do not match',
        type: 'error'
      }));
      return;
    }

    if (!formData.agreeToTerms) {
      dispatch(registerFailure('You must agree to the Terms of Service'));
      dispatch(addNotification({
        message: 'You must agree to the Terms of Service',
        type: 'error'
      }));
      return;
    }

    dispatch(registerStart());

    try {
      const registerData = {
        username: formData.username,
        email: formData.email,
        password: formData.password,
        displayName: formData.displayName || formData.username
      };
      
      const response = await axios.post('/api/auth/register', registerData);
      
      dispatch(registerSuccess(response.data));
      
      dispatch(addNotification({
        message: 'Registration successful! You can now log in.',
        type: 'success'
      }));
      
      navigate('/login');
    } catch (err) {
      console.error('Registration error:', err);
      
      const errorMessage = err.response?.data?.message || 'Registration error. Please try again later.';
      
      dispatch(registerFailure(errorMessage));
      
      dispatch(addNotification({
        message: errorMessage,
        type: 'error'
      }));
    }
  };

  return (
    <div className="register-page">
      <div className="register-container">
        <h2 className="register-title">Create an Account</h2>
        
        {error && <div className="register-error">{error}</div>}
        
        <form onSubmit={handleSubmit} className="register-form">
          <input
            type="text"
            name="username"
            className="register-input"
            placeholder="Username"
            value={formData.username}
            onChange={handleChange}
            required
            minLength={3}
          />
          
          <input
            type="email"
            name="email"
            className="register-input"
            placeholder="Email"
            value={formData.email}
            onChange={handleChange}
            required
          />

          <input
            type="text"
            name="displayName"
            className="register-input"
            placeholder="Display Name (optional)"
            value={formData.displayName}
            onChange={handleChange}
          />
          
          <input
            type="password"
            name="password"
            className="register-input"
            placeholder="Password"
            value={formData.password}
            onChange={handleChange}
            required
            minLength={6}
          />
          
          <input
            type="password"
            name="confirmPassword"
            className="register-input"
            placeholder="Confirm Password"
            value={formData.confirmPassword}
            onChange={handleChange}
            required
            minLength={6}
          />
          
          <div className="register-remember">
            <input
              type="checkbox"
              id="agreeToTerms"
              name="agreeToTerms"
              className="register-checkbox"
              checked={formData.agreeToTerms}
              onChange={handleChange}
              required
            />
            <label className="text-gray-400 text-sm" htmlFor="agreeToTerms">
              I agree to the <Link to="/terms" className="text-[#66c0f4] hover:text-white">Terms of Service</Link>
            </label>
          </div>
          
          <button type="submit" className="register-button" disabled={loading}>
            {loading ? 'Registering...' : 'Register'}
          </button>
        </form>
        <p className="register-link">
          Already have an account? <Link to="/login">Sign In</Link>
        </p>
      </div>
    </div>
  );
};

export default Register; 