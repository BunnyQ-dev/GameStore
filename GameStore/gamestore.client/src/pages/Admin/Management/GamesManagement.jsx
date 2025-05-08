import React, { useState, useEffect, useCallback } from 'react';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa'; 
import { Link, useNavigate } from 'react-router-dom'; 
import styled from 'styled-components';

const ManagementContainer = styled.div` /* ... */ background-color: var(--bg-color-secondary); border-radius: 8px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); `;
const ManagementHeader = styled.div` /* ... */ display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; h2 { margin: 0; } button { padding: 8px 16px; } `;
const DataTable = styled.table` /* ... */ width: 100%; border-collapse: collapse; th, td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bg-color-tertiary); } th { font-weight: bold; color: var(--text-color-secondary); } `;
const ButtonGroup = styled.div` /* ... */ display: flex; gap: 8px; a, button { padding: 6px 12px; font-size: 0.8rem; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; justify-content: center; } `;

const GamesManagement = () => {
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const fetchGames = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get('/api/admin/games'); 
      setGames(response.data || []);
    } catch (err) {
      console.error("Error fetching games:", err);
      setError('Failed to load games.');
      dispatch(addNotification({ message: 'Failed to load games.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [dispatch]);

  useEffect(() => {
    fetchGames();
  }, [fetchGames]);

  const handleCreate = () => {
    navigate('/admin/games/create');
  };
  const handleDelete = async (id) => {
      if (!window.confirm('Are you sure you want to delete this game?')) return;
      try {
        await axios.delete(`/api/admin/games/${id}`); 
        dispatch(addNotification({ message: 'Game deleted successfully', type: 'success' }));
        fetchGames(); 
      } catch (err) {
         const errorMsg = err.response?.data?.message || 'Error deleting game.';
         dispatch(addNotification({ message: errorMsg, type: 'error' }));
      }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <p>Error loading games: {error}</p>;

  return (
    <ManagementContainer>
      <ManagementHeader>
        <h2>Games Management</h2>
        <button onClick={handleCreate}><FaPlus /> Add New Game</button>
      </ManagementHeader>

      <DataTable>
        <thead>
          <tr>
            <th>ID</th>
            <th>Title</th>
            <th>Price</th>
            {}
            <th>Status</th> 
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {games.length > 0 ? (
            games.map(game => (
              <tr key={game.id}>
                <td>{game.id}</td>
                <td>{game.title}</td>
                <td>${game.price?.toFixed(2)}</td> 
                <td>{game.isActive ? 'Active' : 'Inactive'}</td> 
                <td>
                  <ButtonGroup>
                    <Link to={`/admin/games/edit/${game.id}`} className="button-secondary"><FaEdit /></Link>
                    <button onClick={() => handleDelete(game.id)} className="button-danger"><FaTrash /></button> 
                  </ButtonGroup>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="5">No games found</td>
            </tr>
          )}
        </tbody>
      </DataTable>
    </ManagementContainer>
  );
};

export default GamesManagement; 