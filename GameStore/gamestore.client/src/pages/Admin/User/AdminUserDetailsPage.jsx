import React, { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from '../../../utils/axios-config';
import { useDispatch } from 'react-redux';
import { addNotification } from '../../../store/slices/uiSlice';
import styled from 'styled-components';
import { Link } from 'react-router-dom';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';

const BackLink = styled(Link)`
  display: inline-block;
  margin-bottom: 15px;
  color: var(--accent-color);
  text-decoration: none;
`;

const AchievementsSection = styled.div`
  margin-top: 30px;
  h3 {
    color: var(--accent-color);
    margin-bottom: 15px;
  }
`;

const AchievementItem = styled.div`
  display: flex;
  align-items: center;
  background-color: var(--bg-color-tertiary);
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 10px;
  img {
    width: 50px;
    height: 50px;
    border-radius: 50%;
    margin-right: 15px;
  }
  p {
    margin: 0;
    color: var(--text-color);
  }
  span {
    font-size: 0.8rem;
    color: var(--text-color-secondary);
    margin-left: auto;
  }
`;

const AddAchievementButton = styled.button`
  background-color: var(--button-bg-color);
  color: var(--button-text-color);
  padding: 8px 15px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
  &:hover {
    background-color: var(--button-bg-color-hover);
  }
`;

const Container = styled.div`
  padding: 20px;
  max-width: 900px;
  margin: 20px auto;
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
`;

const AdminUserDetailsPage = () => {
  const { userId } = useParams();
  const [user, setUser] = useState(null);
  const [achievements, setAchievements] = useState([]);
  const [allAchievements, setAllAchievements] = useState([]);
  const [selectedAchievement, setSelectedAchievement] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const dispatch = useDispatch();

  const fetchUserDetails = useCallback(async () => {
    setLoading(true);
    try {
      const userRes = await axios.get(`/api/admin/manage/users/${userId}`);
      setUser(userRes.data);
      // Fetch user's achievements
      const userAchievementsRes = await axios.get(`/api/admin/manage/users/${userId}/achievements`);
      setAchievements(userAchievementsRes.data || []);
      // Fetch all available achievements for dropdown
      const allAchievementsRes = await axios.get('/api/admin/manage/achievements');
      setAllAchievements(allAchievementsRes.data || []);
    } catch (err) {
      console.error('Error fetching user details:', err);
      setError('Failed to load user details.');
      dispatch(addNotification({ message: 'Failed to load user details.', type: 'error' }));
    } finally {
      setLoading(false);
    }
  }, [userId, dispatch]);

  useEffect(() => {
    fetchUserDetails();
  }, [fetchUserDetails]);

  const handleAddAchievement = async () => {
    if (!selectedAchievement) {
      dispatch(addNotification({ message: 'Please select an achievement to add.', type: 'warning' }));
      return;
    }
    try {
      await axios.post(`/api/admin/manage/users/${userId}/achievements`, { achievementId: parseInt(selectedAchievement) });
      dispatch(addNotification({ message: 'Achievement added successfully.', type: 'success' }));
      setSelectedAchievement('');
      fetchUserDetails(); // Refresh user details to show new achievement
    } catch (err) {
      console.error('Error adding achievement:', err);
      const errorMsg = err.response?.data?.message || 'Failed to add achievement.';
      dispatch(addNotification({ message: errorMsg, type: 'error' }));
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <Container><p style={{ color: 'red' }}>{error}</p></Container>;
  if (!user) return null;

  return (
    <Container>
      <BackLink to="/admin?tab=users">← Back to Users</BackLink>
      <h1>User Details</h1>
      <p><strong>Username:</strong> {user.userName}</p>
      <p><strong>Email:</strong> {user.email}</p>
      <p><strong>Roles:</strong> {user.roles.join(', ')}</p>
      <p><strong>Registration Date:</strong> {new Date(user.registrationDate).toLocaleDateString()}</p>
      <p><strong>Active:</strong> {user.isActive ? 'Yes' : 'No'}</p>

      <AchievementsSection>
        <h3>User Achievements ({achievements.length})</h3>
        {achievements.length > 0 ? (
          achievements.map(ach => (
            <AchievementItem key={ach.id}>
              <img src={ach.iconUrl || '/img/default-achievement.png'} alt={ach.name} />
              <div>
                <p>{ach.name}</p>
                <span style={{fontSize: '0.75rem'}}>{ach.description}</span>
              </div>
              <span>Unlocked: {new Date(ach.unlockedAt).toLocaleDateString()}</span>
            </AchievementItem>
          ))
        ) : (
          <p>This user has no achievements yet.</p>
        )}
        
        <div style={{marginTop: '20px'}}>
            <h4>Add Achievement</h4>
            <select 
                value={selectedAchievement} 
                onChange={e => setSelectedAchievement(e.target.value)}
                style={{padding: '8px', marginRight: '10px'}}
            >
                <option value="">Select Achievement</option>
                {allAchievements
                    .filter(ach => !achievements.some(userAch => userAch.id === ach.id)) // Filter out already owned achievements
                    .map(ach => (
                        <option key={ach.id} value={ach.id}>{ach.name} (ID: {ach.id})</option>
                ))}
            </select>
            <AddAchievementButton onClick={handleAddAchievement} disabled={!selectedAchievement}>
                Add to User
            </AddAchievementButton>
        </div>
      </AchievementsSection>
    </Container>
  );
};

export default AdminUserDetailsPage; 