import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa';
import styled from 'styled-components';

const ManagementContainer = styled.div` /* ... */ background-color: var(--bg-color-secondary); border-radius: 8px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); `;
const ManagementHeader = styled.div` /* ... */ display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; h2 { margin: 0; } button { padding: 8px 16px; } `;
const DataTable = styled.table` /* ... */ width: 100%; border-collapse: collapse; th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); } th { font-weight: bold; color: var(--text-color-secondary); } td img { max-width: 30px; max-height: 30px; vertical-align: middle; margin-right: 5px; } `;
const ButtonGroup = styled.div` /* ... */ display: flex; gap: 8px; button { padding: 6px 12px; font-size: 0.8rem; cursor: pointer; } `;
const ModalOverlay = styled.div` position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0,0,0,0.6); display: flex; justify-content: center; align-items: center; z-index: 1000; `;
const ModalContent = styled.div` background-color: var(--bg-color-secondary); padding: 30px; border-radius: 8px; min-width: 450px; box-shadow: 0 5px 15px rgba(0,0,0,0.2); h3 { margin-top: 0; margin-bottom: 20px; color: var(--accent-color); } `;
const Input = styled.input` width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid var(--bg-color-tertiary); background-color: var(--bg-color); color: var(--text-color); border-radius: 4px; `;
const TextArea = styled.textarea` width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid var(--bg-color-tertiary); background-color: var(--bg-color); color: var(--text-color); border-radius: 4px; min-height: 80px; `;
const ModalActions = styled.div` display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; button { padding: 8px 16px; } `;

const AchievementsManagement = () => {
  const [achievements, setAchievements] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentAchievement, setCurrentAchievement] = useState(null);
  const [formData, setFormData] = useState({ name: '', description: '', iconUrl: '', gameId: 0 });
  const dispatch = useDispatch();

  const fetchAchievements = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get('/api/admin/manage/achievements');
      setAchievements(response.data || []);
    } catch (err) {
      console.error("Error fetching achievements:", err);
      setError('Failed to load achievements.');
      dispatch(addNotification({ message: 'Failed to load achievements.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => {
    fetchAchievements();
  }, [fetchAchievements]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'gameId' ? parseInt(value, 10) || 0 : value
    }));
  };

  const openModalForCreate = () => {
    setCurrentAchievement(null);
    setFormData({ name: '', description: '', iconUrl: '', gameId: 0 });
    setIsModalOpen(true);
  };

  const openModalForEdit = (achievement) => {
    setCurrentAchievement(achievement);
    setFormData({ name: achievement.name, description: achievement.description, iconUrl: achievement.iconUrl, gameId: achievement.gameId });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setCurrentAchievement(null);
    setFormData({ name: '', description: '', iconUrl: '', gameId: 0 });
    setError(null);
  };

  const handleSave = async () => {
    setError(null);
    const url = currentAchievement ? `/api/admin/manage/achievements/${currentAchievement.id}` : '/api/admin/manage/achievements';
    try {
      if (currentAchievement) {
        await axios.put(url, formData);
        dispatch(addNotification({ message: 'Achievement successfully updated', type: 'success' }));
      } else {
        await axios.post(url, formData);
        dispatch(addNotification({ message: 'Achievement successfully created', type: 'success' }));
      }
      closeModal();
      fetchAchievements();
    } catch (err) {
      console.error("Error saving achievement:", err);
      const errorMsg = err.response?.data?.message || (currentAchievement ? 'Error updating achievement.' : 'Error creating achievement.');
      setError(errorMsg);
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this achievement? This may not be possible if it has already been unlocked by users.')) {
      return;
    }
    try {
      await axios.delete(`/api/admin/manage/achievements/${id}`);
      dispatch(addNotification({ message: 'Achievement successfully deleted', type: 'success' }));
      fetchAchievements();
    } catch (err) {
      console.error("Error deleting achievement:", err);
      const errorMsg = err.response?.data?.message || 'Error deleting achievement.';
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error && achievements.length === 0) return <p>Error: {error}</p>;

  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Achievements Management</h2>
        <button onClick={openModalForCreate}><FaPlus /> Add New Achievement</button>
      </ManagementHeader>

      <DataTable>
        <thead>
          <tr>
            <th>ID</th>
            <th>Icon</th>
            <th>Name</th>
            <th>Description</th>
            <th>Actions</th>
            {}
          </tr>
        </thead>
        <tbody>
          {achievements.length > 0 ? (
            achievements.map(ach => (
              <tr key={ach.id}>
                <td>{ach.id}</td>
                <td>{ach.iconUrl && <img src={ach.iconUrl} alt="icon" />}</td>
                <td>{ach.name}</td>
                <td>{ach.description}</td>
                {}
                <td>
                  <ButtonGroup>
                    <button onClick={() => openModalForEdit(ach)}><FaEdit /></button>
                    <button onClick={() => handleDelete(ach.id)}><FaTrash /></button>
                  </ButtonGroup>
                </td>
              </tr>
            ))
          ) : (
            <tr><td colSpan="5">No achievements found</td></tr>
          )}
        </tbody>
      </DataTable>

      {isModalOpen && (
        <ModalOverlay onClick={closeModal}>
          <ModalContent onClick={(e) => e.stopPropagation()}>
            <h3>{currentAchievement ? 'Edit Achievement' : 'Create New Achievement'}</h3>
            {error && <p style={{color: 'red'}}>{error}</p>}
            <Input
              type="text"
              placeholder="Achievement Name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
            />
            <TextArea
              placeholder="Description"
              name="description"
              value={formData.description}
              onChange={handleInputChange}
            />
            <Input
              type="text"
              placeholder="Icon URL"
              name="iconUrl"
              value={formData.iconUrl}
              onChange={handleInputChange}
            />
            <Input
              type="number"
              placeholder="Game ID"
              name="gameId"
              value={formData.gameId}
              onChange={handleInputChange}
            />
            {}
            <ModalActions>
              <button onClick={closeModal} className="button-secondary">Cancel</button>
              <button onClick={handleSave} className="button-primary">
                {currentAchievement ? 'Save Changes' : 'Create Achievement'}
              </button>
            </ModalActions>
          </ModalContent>
        </ModalOverlay>
      )}
    </ManagementContainer>
  );
};

export default AchievementsManagement; 