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
  button { padding: 6px 12px; font-size: 0.8rem; cursor: pointer; }
`;

const ModalOverlay = styled.div`
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex; justify-content: center; align-items: center;
  z-index: 1000;
`;

const ModalContent = styled.div`
  background-color: var(--bg-color-secondary);
  padding: 30px;
  border-radius: 8px;
  min-width: 350px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
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

const GenresManagement = () => {
  const [genres, setGenres] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentGenre, setCurrentGenre] = useState(null); 
  const [genreName, setGenreName] = useState('');
  const dispatch = useDispatch();

  const fetchGenres = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get('/api/admin/manage/genres');
      setGenres(response.data || []);
    } catch (err) {
      console.error("Error fetching genres:", err);
      setError('Failed to load genres.');
      dispatch(addNotification({ message: 'Failed to load genres.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => {
    fetchGenres();
  }, [fetchGenres]);

  const openModalForCreate = () => {
    setCurrentGenre(null);
    setGenreName('');
    setIsModalOpen(true);
  };

  const openModalForEdit = (genre) => {
    setCurrentGenre(genre);
    setGenreName(genre.name);
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setCurrentGenre(null);
    setGenreName('');
  };

  const handleSave = async () => {
    setError(null);
    const dto = { name: genreName };
    const method = currentGenre ? 'put' : 'post';
    const url = currentGenre ? `/api/admin/manage/genres/${currentGenre.id}` : '/api/admin/manage/genres';

    try {
      if (currentGenre) {
        await axios.put(url, dto);
        dispatch(addNotification({ message: 'Genre successfully updated', type: 'success' }));
      } else {
        await axios.post(url, dto);
        dispatch(addNotification({ message: 'Genre successfully created', type: 'success' }));
      }
      closeModal();
      fetchGenres(); 
    } catch (err) {
      console.error("Error saving genre:", err);
      const errorMsg = err.response?.data?.message || (currentGenre ? 'Error updating genre.' : 'Error creating genre.');
      setError(errorMsg); 
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this genre? This may not be possible if it is being used.')) {
      return;
    }
    setError(null);
    try {
      await axios.delete(`/api/admin/manage/genres/${id}`);
      dispatch(addNotification({ message: 'Genre deleted successfully', type: 'success' }));
      fetchGenres(); 
    } catch (err) {
      console.error("Error deleting genre:", err);
      const errorMsg = err.response?.data?.message || 'Error deleting genre.';
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error && genres.length === 0) return <p>Error: {error}</p>; 

  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Genres Management</h2>
        <button onClick={openModalForCreate}><FaPlus /> Add New Genre</button>
      </ManagementHeader>

      <DataTable>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {genres.length > 0 ? (
            genres.map(genre => (
              <tr key={genre.id}>
                <td>{genre.id}</td>
                <td>{genre.name}</td>
                <td>
                  <ButtonGroup>
                    <button onClick={() => openModalForEdit(genre)}><FaEdit /></button>
                    <button onClick={() => handleDelete(genre.id)}><FaTrash /></button>
                  </ButtonGroup>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="3">No genres found</td>
            </tr>
          )}
        </tbody>
      </DataTable>

      {isModalOpen && (
        <ModalOverlay onClick={closeModal}>
          <ModalContent onClick={(e) => e.stopPropagation()}>
            <h3>{currentGenre ? 'Edit Genre' : 'Create New Genre'}</h3>
            {error && <p style={{color: 'red'}}>{error}</p>} 
            <Input
              type="text"
              placeholder="Genre Name"
              value={genreName}
              onChange={(e) => setGenreName(e.target.value)}
            />
            <ModalActions>
              <button onClick={closeModal} className="button-secondary">Cancel</button>
              <button onClick={handleSave} className="button-primary">
                {currentGenre ? 'Save Changes' : 'Create Genre'}
              </button>
            </ModalActions>
          </ModalContent>
        </ModalOverlay>
      )}
    </ManagementContainer>
  );
};

export default GenresManagement; 