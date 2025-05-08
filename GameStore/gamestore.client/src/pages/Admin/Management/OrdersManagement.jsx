import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import styled from 'styled-components';

const ManagementContainer = styled.div`
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
`;
const ManagementHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  h2 { margin: 0; }
`;
const DataTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); }
  th { font-weight: bold; color: var(--text-color-secondary); }
`;
const Select = styled.select`
  padding: 6px;
  border: 1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius: 4px;
`;
const Button = styled.button`
  padding: 6px 12px;
  margin-left: 8px;
  cursor: pointer;
`;

const OrdersManagement = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const fetchOrders = useCallback(async (page = 1) => {
    setLoading(true);
    try {
      const response = await axios.get('/api/admin/orders', { params: { page, pageSize: 10 } });
      setOrders(response.data.orders || []);
    } catch (err) {
      console.error('Error fetching orders:', err);
      dispatch(addNotification({ message: 'Failed to load orders.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => { fetchOrders(); }, [fetchOrders]);

  const handleStatusChange = async (orderId, status) => {
    try {
      await axios.put(`/api/admin/orders/${orderId}/status`, { status });
      dispatch(addNotification({ message: 'Order status updated.', type: 'success' }));
      fetchOrders();
    } catch (err) {
      console.error('Error updating status:', err);
      const msg = err.response?.data?.message || 'Failed to update order status.';
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;

  const statusOptions = ['Pending', 'Processing', 'Completed', 'Cancelled'];

  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Orders Management</h2>
      </ManagementHeader>
      <DataTable>
        <thead>
          <tr>
            <th>Order ID</th>
            <th>User</th>
            <th>Date</th>
            <th>Total</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {orders.length > 0 ? orders.map(o => (
            <tr key={o.id}>
              <td>{o.id}</td>
              <td>{o.userEmail || o.userId}</td>
              <td>{new Date(o.orderDate).toLocaleDateString()}</td>
              <td>${o.totalAmount?.toFixed(2)}</td>
              <td>
                <Select
                  value={o.status}
                  onChange={e => handleStatusChange(o.id, e.target.value)}
                >
                  {statusOptions.map(s => <option key={s} value={s}>{s}</option>)}
                </Select>
              </td>
              <td>
                <Button onClick={() => navigate(`/admin/orders/${o.id}`)}>View Details</Button>
              </td>
            </tr>
          )) : (
            <tr><td colSpan={6}>No orders found</td></tr>
          )}
        </tbody>
      </DataTable>
    </ManagementContainer>
  );
};

export default OrdersManagement; 