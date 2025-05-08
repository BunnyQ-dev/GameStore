import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa';
import styled from 'styled-components';

// Стилі, копіюємо з GenresManagement
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
  button { padding: 8px 16px; }
`;

const DataTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); }
  th { font-weight: bold; color: var(--text-color-secondary); }
`;

const ButtonGroup = styled.div`
  display: flex;
  gap: 8px;
  button { padding: 6px 12px; font-size: 0.8rem; }
`;

// Модальні стилі
const ModalOverlay = styled.div`
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(0,0,0,0.6);
  display: flex; justify-content: center; align-items: center;
  z-index: 1000;
`;

const ModalContent = styled.div`
  background-color: var(--bg-color-secondary);
  padding: 30px;
  border-radius: 8px;
  min-width: 350px;
  box-shadow: 0 5px 15px rgba(0,0,0,0.2);
  h3 { margin-top: 0; margin-bottom: 20px; color: var(--accent-color); }
`;

const Input = styled.input`
  width: 100%;
  padding: 10px;
  margin-bottom: 15px;
  border: 1px solid var(--bg-color-tertiary);
  background-color: var(--bg-color);
  color: var(--text-color);
  border-radius: 4px;
`;

const ModalActions = styled.div`
  display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px;
  button { padding: 8px 16px; }
`;

const PlatformsManagement = () => {
  const [platforms, setPlatforms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentPlatform, setCurrentPlatform] = useState(null);
  const [platformName, setPlatformName] = useState('');
  const dispatch = useDispatch();

  const fetchPlatforms = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get('/api/admin/manage/platforms');
      setPlatforms(response.data || []);
    } catch (err) {
      console.error('Error fetching platforms:', err);
      setError('Failed to load platforms.');
      dispatch(addNotification({ message: 'Failed to load platforms.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => { fetchPlatforms(); }, [fetchPlatforms]);

  const openModalForCreate = () => { setCurrentPlatform(null); setPlatformName(''); setIsModalOpen(true); };
  const openModalForEdit = (p) => { setCurrentPlatform(p); setPlatformName(p.name); setIsModalOpen(true); };
  const closeModal = () => { setIsModalOpen(false); setCurrentPlatform(null); setPlatformName(''); };

  const handleSave = async () => {
    setError(null);
    if (!platformName.trim()) { setError('Platform name cannot be empty.'); return; }
    try {
      if (currentPlatform) {
        await axios.put(`/api/admin/manage/platforms/${currentPlatform.id}`, { name: platformName });
        dispatch(addNotification({ message: 'Platform successfully updated', type: 'success' }));
      } else {
        await axios.post('/api/admin/manage/platforms', { name: platformName });
        dispatch(addNotification({ message: 'Platform successfully created', type: 'success' }));
      }
      closeModal();
      fetchPlatforms();
    } catch (err) {
      console.error('Error saving platform:', err);
      const msg = err.response?.data?.message || (currentPlatform ? 'Error updating platform.' : 'Error creating platform.');
      setError(msg);
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this platform?')) return;
    try {
      await axios.delete(`/api/admin/manage/platforms/${id}`);
      dispatch(addNotification({ message: 'Platform successfully deleted', type: 'success' }));
      fetchPlatforms();
    } catch (err) {
      console.error('Error deleting platform:', err);
      const msg = err.response?.data?.message || 'Error deleting platform.';
      dispatch(addNotification({ message: msg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Platforms Management</h2>
        <button onClick={openModalForCreate}><FaPlus /> Add New Platform</button>
      </ManagementHeader>
      <DataTable>
        <thead>
          <tr><th>ID</th><th>Name</th><th>Actions</th></tr>
        </thead>
        <tbody>
          {platforms.length ? platforms.map(p => (
            <tr key={p.id}>
              <td>{p.id}</td><td>{p.name}</td>
              <td>
                <ButtonGroup>
                  <button onClick={() => openModalForEdit(p)}><FaEdit /></button>
                  <button onClick={() => handleDelete(p.id)}><FaTrash /></button>
                </ButtonGroup>
              </td>
            </tr>
          )) : <tr><td colSpan={3}>No platforms found</td></tr>}
        </tbody>
      </DataTable>
      {isModalOpen && (
        <ModalOverlay>
          <ModalContent>
            <h3>{currentPlatform ? 'Edit Platform' : 'Add Platform'}</h3>
            {error && <p style={{color: 'red'}}>{error}</p>}
            <Input type="text" value={platformName} onChange={e => setPlatformName(e.target.value)} placeholder="Platform Name" />
            <ModalActions>
              <button onClick={closeModal}>Cancel</button>
              <button onClick={handleSave}>{currentPlatform ? 'Save' : 'Create'}</button>
            </ModalActions>
          </ModalContent>
        </ModalOverlay>
      )}
    </ManagementContainer>
  );
};

export default PlatformsManagement; 