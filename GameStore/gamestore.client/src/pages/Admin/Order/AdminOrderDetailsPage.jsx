import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import axios from '../../../utils/axios-config';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import styled from 'styled-components';

const Container = styled.div`
  padding: 20px;
  max-width: 800px;
  margin: 20px auto;
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
`;
const BackLink = styled(Link)`
  display: inline-block;
  margin-bottom: 15px;
  color: var(--accent-color);
  text-decoration: none;
`;
const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); }
  th { font-weight: bold; color: var(--text-color-secondary); }
`;

const AdminOrderDetailsPage = () => {
  const { orderId } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        const res = await axios.get(`/api/admin/orders/${orderId}`);
        setOrder(res.data);
      } catch (err) {
        console.error('Error fetching order:', err);
        setError('Не вдалося завантажити замовлення.');
      } finally {
        setLoading(false);
      }
    };
    fetchOrder();
  }, [orderId]);

  if (loading) return <LoadingSpinner />;
  if (error) return <Container><p style={{ color: 'red' }}>{error}</p></Container>;
  if (!order) return null;

  return (
    <Container>
      <BackLink to="/admin?tab=orders">← Back to Orders</BackLink>
      <h1>Order #{order.id}</h1>
      <p>User: {order.userName} ({order.userEmail})</p>
      <p>Date: {new Date(order.orderDate).toLocaleString()}</p>
      <p>Total: ${order.totalAmount.toFixed(2)}</p>
      <p>Status: {order.status}</p>

      <h2>Items</h2>
      {order.items && order.items.length > 0 ? (
        <Table>
          <thead>
            <tr>
              <th>Item ID</th>
              <th>Game Title</th>
              <th>Price</th>
              <th>Quantity</th>
            </tr>
          </thead>
          <tbody>
            {order.items.map(item => (
              <tr key={item.gameId || item.id}>
                <td>{item.gameId || item.id}</td>
                <td>{item.gameTitle}</td>
                <td>${item.price.toFixed(2)}</td>
                <td>{item.quantity}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      ) : (
        <p>No individual items in this order.</p>
      )}

      {order.bundles && order.bundles.length > 0 && (
        <>
          <h2>Bundles</h2>
          <Table>
            <thead>
              <tr>
                <th>Bundle ID</th>
                <th>Bundle Name</th>
                <th>Price Paid</th>
                <th>Quantity</th>
              </tr>
            </thead>
            <tbody>
              {order.bundles.map(bundle => (
                <tr key={bundle.bundleId}>
                  <td>{bundle.bundleId}</td>
                  <td>{bundle.bundleName}</td>
                  <td>${bundle.priceAtPurchase.toFixed(2)}</td>
                  <td>{bundle.quantity}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        </>
      )}
    </Container>
  );
};

export default AdminOrderDetailsPage; 