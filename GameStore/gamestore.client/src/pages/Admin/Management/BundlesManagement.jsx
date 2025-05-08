import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa';
import styled from 'styled-components';

const ManagementContainer = styled.div`
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
`;

const ManagementHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  h2 { margin: 0; }
  button { padding: 8px 16px; }
`;

const DataTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); }
  th { font-weight: bold; color: var(--text-color-secondary); }
  td input[type="checkbox"] { transform: scale(1.2); }
  tbody tr:hover { background-color: rgba(255,255,255,0.1); }
`;

const ButtonGroup = styled.div`
  display: flex;
  gap: 8px;
  button { padding: 6px 12px; font-size: 0.8rem; cursor: pointer; }
`;

const ModalOverlay = styled.div`
  position: fixed; top:0; left:0; right:0; bottom:0;
  background-color: rgba(0,0,0,0.6);
  display: flex; justify-content:center; align-items:center;
  z-index:1000;
`;

const ModalContent = styled.div`
  background-color: var(--bg-color-secondary);
  padding:30px;
  border-radius:8px;
  min-width:450px;
  box-shadow: 0 5px 15px rgba(0,0,0,0.2);
  h3 { margin-top:0; margin-bottom:20px; color: var(--accent-color); }
`;

const Input = styled.input`
  width:100%;
  padding:10px;
  margin-bottom:15px;
  border:1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius:4px;
`;

const TextArea = styled.textarea`
  width:100%;
  padding:10px;
  margin-bottom:15px;
  border:1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius:4px;
  min-height:80px;
`;

const ModalActions = styled.div`
  display:flex; justify-content:flex-end; gap:10px; margin-top:20px;
  button { padding:8px 16px; }
`;

// Styled spans for original and discounted prices
const OriginalPrice = styled.span`
  text-decoration: line-through;
  color: var(--text-color-tertiary);
  margin-right: 8px;
`;
const DiscountedPrice = styled.span`
  color: var(--accent-color);
  font-weight: bold;
`;

const BundlesManagement = () => {
  const [bundles, setBundles] = useState([]);
  const [allGames, setAllGames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentBundle, setCurrentBundle] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    imageUrl: '',
    isActive: true,
    price: 0,
    discountPercentage: 0,
    discountPrice: 0,
    startDate: '',
    endDate: '',
    gameIds: []
  });
  const dispatch = useDispatch();

  const fetchBundles = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get('/api/admin/manage/bundles');
      setBundles(response.data || []);
    } catch (err) {
      console.error('Error fetching bundles:', err);
      setError('Failed to load bundles.');
      dispatch(addNotification({ message: 'Failed to load bundles.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  const fetchAllGames = useCallback(async () => {
    try {
      const res = await axios.get('/api/games?page=1&pageSize=1000');
      setAllGames(res.data.items || []);
    } catch (err) {
      console.error('Error fetching games list:', err);
      dispatch(addNotification({ message: 'Failed to load games list.', type: 'error' }));
    }
  }, [dispatch]);

  useEffect(() => {
    fetchBundles();
    fetchAllGames();
  }, [fetchBundles, fetchAllGames]);

  const handleGameSelection = (e) => {
    const id = parseInt(e.target.value, 10);
    setFormData(prev => {
      const exists = prev.gameIds.includes(id);
      const gameIds = exists ? prev.gameIds.filter(g => g !== id) : [...prev.gameIds, id];
      return { ...prev, gameIds };
    });
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]:
        type === 'checkbox' ? checked
        : ['price','discountPercentage','discountPrice'].includes(name) ? parseFloat(value) || 0
        : value
    }));
  };

  const openModalForCreate = () => {
    setCurrentBundle(null);
    setFormData({
      name: '', description: '', imageUrl: '', isActive: true,
      price: 0, discountPercentage: 0, discountPrice: 0,
      startDate: '', endDate: '', gameIds: []
    });
    setIsModalOpen(true);
  };

  const openModalForEdit = (bundle) => {
    setCurrentBundle(bundle);
    setFormData({
      name: bundle.name || '',
      description: bundle.description || '',
      imageUrl: bundle.imageUrl || '',
      isActive: bundle.isActive,
      price: bundle.price || 0,
      discountPercentage: bundle.discountPercentage || 0,
      discountPrice: bundle.discountPrice || 0,
      startDate: bundle.startDate ? bundle.startDate.split('T')[0] : '',
      endDate: bundle.endDate ? bundle.endDate.split('T')[0] : '',
      gameIds: bundle.gameIds || []
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setCurrentBundle(null);
    setError(null);
  };

  const handleSave = async () => {
    setError(null);
    const dto = {
      name: formData.name,
      description: formData.description,
      imageUrl: formData.imageUrl,
      isActive: formData.isActive,
      price: formData.price,
      discountPercentage: formData.discountPercentage,
      discountPrice: formData.discountPrice,
      startDate: formData.startDate ? new Date(formData.startDate).toISOString() : null,
      endDate: formData.endDate ? new Date(formData.endDate).toISOString() : null,
      gameIds: formData.gameIds
    };
    const url = currentBundle
      ? `/api/admin/manage/bundles/${currentBundle.id}`
      : '/api/admin/manage/bundles';

    try {
      if (currentBundle) {
        await axios.put(url, dto);
        dispatch(addNotification({ message: 'Bundle successfully updated', type: 'success' }));
      } else {
        await axios.post(url, dto);
        dispatch(addNotification({ message: 'Bundle successfully created', type: 'success' }));
      }
      closeModal();
      fetchBundles();
    } catch (err) {
      console.error('Error saving bundle:', err);
      const errorMsg = err.response?.data?.message || (currentBundle ? 'Error updating bundle.' : 'Error creating bundle.');
      setError(errorMsg);
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this bundle?')) return;
    try {
      await axios.delete(`/api/admin/manage/bundles/${id}`);
      dispatch(addNotification({ message: 'Bundle successfully deleted', type: 'success' }));
      fetchBundles();
    } catch (err) {
      console.error('Error deleting bundle:', err);
      const errorMsg = err.response?.data?.message || 'Error deleting bundle.';
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error && bundles.length === 0) return <p>Error: {error}</p>;

  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Bundles Management</h2>
        <button onClick={openModalForCreate}><FaPlus /> Add New Bundle</button>
      </ManagementHeader>

      <DataTable>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Price</th>
            <th>Active</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {bundles.length > 0 ? (
            bundles.map(bundle => (
              <tr key={bundle.id}>
                <td>{bundle.id}</td>
                <td>{bundle.name}</td>
                <td>
                  {bundle.discountPrice > 0 ? (
                    <>
                      <OriginalPrice>${bundle.price.toFixed(2)}</OriginalPrice>
                      <DiscountedPrice>${bundle.discountPrice.toFixed(2)}</DiscountedPrice>
                    </>
                  ) : bundle.discountPercentage > 0 ? (
                    <>
                      <OriginalPrice>${bundle.price.toFixed(2)}</OriginalPrice>
                      <DiscountedPrice>${(bundle.price * (1 - bundle.discountPercentage / 100)).toFixed(2)}</DiscountedPrice>
                    </>
                  ) : (
                    <>${bundle.price.toFixed(2)}</>
                  )}
                </td>
                <td><input type="checkbox" checked={bundle.isActive} readOnly /></td>
                <td>
                  <ButtonGroup>
                    <button onClick={() => openModalForEdit(bundle)}><FaEdit /></button>
                    <button onClick={() => handleDelete(bundle.id)}><FaTrash /></button>
                  </ButtonGroup>
                </td>
              </tr>
            ))
          ) : (
            <tr><td colSpan="5">No bundles found</td></tr>
          )}
        </tbody>
      </DataTable>

      {isModalOpen && (
        <ModalOverlay onClick={closeModal}>
          <ModalContent onClick={e => e.stopPropagation()}>
            <h3>{currentBundle ? 'Edit Bundle' : 'Create New Bundle'}</h3>
            {error && <p style={{color: 'red'}}>{error}</p>}
            <label htmlFor="name">Name</label>
            <Input id="name" type="text" name="name" value={formData.name} onChange={handleInputChange} placeholder="Enter bundle name" />
            <label htmlFor="description">Description</label>
            <TextArea id="description" name="description" value={formData.description} onChange={handleInputChange} placeholder="Enter bundle description" />
            <label htmlFor="imageUrl">Image URL</label>
            <Input id="imageUrl" type="text" name="imageUrl" value={formData.imageUrl} onChange={handleInputChange} placeholder="Enter image URL" />
            <label htmlFor="startDate">Start Date</label>
            <Input id="startDate" type="date" name="startDate" value={formData.startDate} onChange={handleInputChange} />
            <label htmlFor="endDate">End Date</label>
            <Input id="endDate" type="date" name="endDate" value={formData.endDate} onChange={handleInputChange} />
            <label htmlFor="price">Price</label>
            <Input id="price" type="number" step="0.01" name="price" value={formData.price} onChange={handleInputChange} placeholder="Enter price" />
            <label htmlFor="discountPercentage">Discount %</label>
            <Input id="discountPercentage" type="number" step="0.01" name="discountPercentage" value={formData.discountPercentage} onChange={handleInputChange} placeholder="Enter discount percentage" />
            <label htmlFor="discountPrice">Discount Price</label>
            <Input id="discountPrice" type="number" step="0.01" name="discountPrice" value={formData.discountPrice} onChange={handleInputChange} placeholder="Enter discount price" />
            <label>Games</label>
            <div style={{ maxHeight: '200px', overflowY: 'auto', marginBottom: '1rem' }}>
              {allGames.map(game => (
                <div key={game.id}>
                  <input
                    type="checkbox"
                    id={`game-${game.id}`}
                    value={game.id}
                    checked={formData.gameIds.includes(game.id)}
                    onChange={handleGameSelection}
                  />
                  <label htmlFor={`game-${game.id}`} style={{ marginLeft: '8px' }}>{game.title}</label>
                </div>
              ))}
            </div>
            <label>
              <input type="checkbox" name="isActive" checked={formData.isActive} onChange={handleInputChange} /> Active
            </label>
            <ModalActions>
              <button onClick={closeModal} className="button-secondary">Cancel</button>
              <button onClick={handleSave} className="button-primary">{currentBundle ? 'Save Changes' : 'Create Bundle'}</button>
            </ModalActions>
          </ModalContent>
        </ModalOverlay>
      )}
    </ManagementContainer>
  );
};

export default BundlesManagement; 