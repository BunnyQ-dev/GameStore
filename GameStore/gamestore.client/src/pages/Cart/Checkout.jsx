import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import axios from 'axios';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { clearCartSuccess } from '../../store/slices/cartSlice';
import { addNotification } from '../../store/slices/uiSlice';
import { FaCheckCircle, FaCreditCard, FaPaypal, FaLock, FaRegCreditCard, FaCalendarAlt, FaUser } from 'react-icons/fa';
import './Checkout.css';

const Checkout = () => {
  const { items: cartItems, bundles: cartBundles, total } = useSelector(state => state.cart);
  const totalCount = (cartItems?.length || 0) + (cartBundles?.length || 0);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [orderSuccess, setOrderSuccess] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formErrors, setFormErrors] = useState({});
  const [formData, setFormData] = useState({
    paymentMethod: 'card',
    cardNumber: '',
    cardExpiry: '',
    cardCvv: '',
    nameOnCard: '',
    agreeToTerms: false,
  });

  useEffect(() => {
    if (totalCount === 0 && !orderSuccess) {
      navigate('/cart');
    }
  }, [totalCount, navigate, orderSuccess]);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
    if (formErrors[name]) {
        setFormErrors(prev => ({...prev, [name]: null}));
    }
  };

  const calculateTotal = () => {
    return cartItems
      .filter(item => item && item.gameId && typeof item.price === 'number')
      .reduce((total, item) => total + item.price, 0);
  };
  const totalAmount = total;

  const validateForm = () => {
      const errors = {};
      if (!formData.agreeToTerms) {
          errors.agreeToTerms = 'You must agree to the terms.';
      }
      if (formData.paymentMethod === 'card') {
          if (!formData.nameOnCard.trim()) errors.nameOnCard = 'Enter the name on the card.';
          if (!formData.cardNumber.replace(/\s+/g, '').match(/^\d{16}$/)) errors.cardNumber = 'Invalid card number format (16 digits).';
          if (!formData.cardExpiry.match(/^(0[1-9]|1[0-2])\/(\d{2})$/)) errors.cardExpiry = 'Invalid card expiry format (MM/YY).';
          if (!formData.cardCvv.match(/^\d{3}$/)) errors.cardCvv = 'Invalid CVV format (3 digits).';
      }
      setFormErrors(errors);
      return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    if (!validateForm()) {
        return;
    }

    setSubmitting(true);
    
    try {
      const validCartItems = cartItems.filter(item => item && item.gameId && typeof item.gameId === 'number' && item.gameId > 0);
      const bundleGameItems = cartBundles.flatMap(bundle => bundle.gameIds || []);
      if (validCartItems.length + bundleGameItems.length === 0) {
         setError('There are no valid items or bundles to place an order.');
         setSubmitting(false);
         return;
      }
      const orderItems = [
        ...validCartItems.map(item => ({ gameId: item.gameId, quantity: item.quantity })),
        ...bundleGameItems.map(id => ({ gameId: id, quantity: 1 }))
      ];
      
      console.log('Submitting order items:', orderItems);
      console.log('Payment details (simulation):', formData);
      await axios.post('/api/order', orderItems);
      
      dispatch(clearCartSuccess());
      await axios.delete('/api/cart/clear');
      for (const b of cartBundles) {
        await axios.delete(`/api/cart/bundles/${b.bundleId}`);
      }
      
      dispatch(addNotification({
        type: 'success',
        message: 'Order successfully placed! Games added to your library.'
      }));
      
      setOrderSuccess(true);
    } catch (err) {
      console.error('Error creating order:', err);
      const serverError = err.response?.data?.title || err.response?.data?.message || err.response?.data || 'Помилка оформлення замовлення. Спробуйте ще раз.';
      setError(serverError);
      dispatch(addNotification({
        type: 'error',
        message: serverError
      }));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  if (orderSuccess) {
    return (
      <div className="checkout-success-container">
        <div className="checkout-success-box">
          <FaCheckCircle className="success-icon" />
          <h2 className="success-title">Thank you for your order!</h2>
          <p className="success-message">
            Your order has been successfully placed. The games will appear in your library shortly.
          </p>
          <div className="success-actions">
            <button
              onClick={() => navigate('/library')}
              className="action-button primary-button"
            >
              Go to library
            </button>
            <button
              onClick={() => navigate('/store')}
              className="action-button secondary-button"
            >
              Continue shopping
            </button>
          </div>
        </div>
      </div>
    );
  }

  const renderOrderSummary = () => {
    if (totalCount === 0) return null;
    
    return (
        <div className="order-summary-box">
            <h2 className="summary-title">Order summary</h2>
            <div className="summary-items-list">
                {cartItems.filter(item => item && item.gameId).map(item => (
                        <div key={item.gameId} className="summary-item">
                            <img 
                                src={item.imageUrl || '/img/game-placeholder.jpg'} 
                                alt={item.title || 'Game'}
                                className="summary-item-image" 
                            />
                            <div className="summary-item-details">
                                <p className="summary-item-title">{item.title || 'Unknown title'}</p>
                                <p className="summary-item-price">${(item.price ?? 0).toFixed(2)}</p>
                            </div>
                        </div>
                ))}
                {cartBundles.map(bundle => (
                  <div key={bundle.bundleId} className="summary-item">
                    <p className="summary-item-title">{bundle.name}</p>
                    <p className="summary-item-price">${bundle.price.toFixed(2)}</p>
                  </div>
                ))}
            </div>
            <div className="summary-total-section">
                <div className="summary-total-row">
                    <span>Total:</span>
                    <span className="summary-total-amount">${totalAmount.toFixed(2)}</span>
                </div>
            </div>
             <button 
                type="submit"
                form="checkout-form"
                className="place-order-button" 
                disabled={submitting || totalCount === 0}
            >
                {submitting ? <LoadingSpinner size="small" color="white" /> : 'Pay'}
            </button>
        </div>
    );
  };

  return (
    <div className="checkout-container container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-white mb-8">Order placement</h1>
      
      {error && (
        <div className="server-error-message" role="alert">
            {error}
        </div>
      )}
      
      <div className="checkout-layout">
        <div className="payment-details-section">
          <form id="checkout-form" onSubmit={handleSubmit} noValidate>
            
            <div className="payment-method-selection">
              <h3 className="section-title">Payment method</h3>
              <div className="radio-group">
                <label className={`radio-label ${formData.paymentMethod === 'card' ? 'selected' : ''}`}>
                  <input
                    type="radio"
                    name="paymentMethod"
                    value="card"
                    checked={formData.paymentMethod === 'card'}
                    onChange={handleInputChange}
                    className="form-radio"
                  />
                  <FaCreditCard className="radio-icon" />
                  <span>Credit/debit card</span>
                </label>
                <label className={`radio-label ${formData.paymentMethod === 'paypal' ? 'selected' : ''}`}>
                  <input
                    type="radio"
                    name="paymentMethod"
                    value="paypal"
                    checked={formData.paymentMethod === 'paypal'}
                    onChange={handleInputChange}
                    className="form-radio"
                  />
                  <FaPaypal className="radio-icon" />
                  <span>PayPal</span>
                </label>
              </div>
            </div>
            
            {formData.paymentMethod === 'card' && (
              <div className="card-details-form">
                <h3 className="section-title">Card details</h3>
                
                <div className="form-group">
                  <label htmlFor="nameOnCard" className="form-label">Name on card</label>
                  <div className="input-icon-wrapper">
                    <FaUser className="input-icon" />
                    <input
                      type="text"
                      id="nameOnCard"
                      name="nameOnCard"
                      value={formData.nameOnCard}
                      onChange={handleInputChange}
                      className={`form-input ${formErrors.nameOnCard ? 'is-invalid' : ''}`}
                      placeholder="IVAN IVANOV"
                      required
                    />
                  </div>
                  {formErrors.nameOnCard && <p className="error-text">{formErrors.nameOnCard}</p>}
                </div>

                <div className="form-group">
                  <label htmlFor="cardNumber" className="form-label">Card number</label>
                  <div className="input-icon-wrapper">
                     <FaRegCreditCard className="input-icon" />
                    <input
                      type="text" 
                      id="cardNumber"
                      name="cardNumber"
                      value={formData.cardNumber}
                      onChange={handleInputChange}
                      className={`form-input ${formErrors.cardNumber ? 'is-invalid' : ''}`}
                      placeholder="0000 0000 0000 0000"
                      maxLength="19"
                      required
                    />
                  </div>
                  {formErrors.cardNumber && <p className="error-text">{formErrors.cardNumber}</p>}
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="form-group">
                    <label htmlFor="cardExpiry" className="form-label">Card expiration</label>
                    <div className="input-icon-wrapper">
                      <FaCalendarAlt className="input-icon" />
                      <input
                        type="text"
                        id="cardExpiry"
                        name="cardExpiry"
                        value={formData.cardExpiry}
                        onChange={handleInputChange}
                        className={`form-input ${formErrors.cardExpiry ? 'is-invalid' : ''}`}
                        placeholder="MM/YY"
                        maxLength="5"
                        required
                      />
                    </div>
                     {formErrors.cardExpiry && <p className="error-text">{formErrors.cardExpiry}</p>}
                  </div>
                  <div className="form-group">
                      <label htmlFor="cardCvv" className="form-label">CVV</label>
                     <div className="input-icon-wrapper">
                       <FaLock className="input-icon" />
                        <input
                          type="text" 
                          id="cardCvv"
                          name="cardCvv"
                          value={formData.cardCvv}
                          onChange={handleInputChange}
                          className={`form-input ${formErrors.cardCvv ? 'is-invalid' : ''}`}
                          placeholder="123"
                          maxLength="3"
                          required
                        />
                     </div>
                     {formErrors.cardCvv && <p className="error-text">{formErrors.cardCvv}</p>}
                  </div>
                </div>
              </div>
            )}
            
            <div className="terms-agreement">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  name="agreeToTerms"
                  checked={formData.agreeToTerms}
                  onChange={handleInputChange}
                  className={`form-checkbox ${formErrors.agreeToTerms ? 'is-invalid' : ''}`}
                  required
                />
                <span className="text-gray-300 text-sm">
                  I agree with <Link to="/terms" className="text-blue-400 hover:underline">Terms of use</Link> and <Link to="/privacy" className="text-blue-400 hover:underline">Privacy policy</Link>.
                </span>
              </label>
               {formErrors.agreeToTerms && <p className="error-text mt-1">{formErrors.agreeToTerms}</p>}
            </div>
          </form>
        </div>

        <div className="order-summary-section">
          {renderOrderSummary()}
        </div>
      </div>
    </div>
  );
};

export default Checkout; 