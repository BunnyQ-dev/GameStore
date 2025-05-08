import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import axios from '../../utils/axios-config';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import './ProfilePage.css'; 
import { FaUser, FaImage, FaSave, FaTimes, FaPencilAlt, FaCheckCircle, FaTrophy } from 'react-icons/fa';
import { FaUserFriends } from 'react-icons/fa'; 
import { logout } from '../../store/slices/authSlice'; 
import { addNotification } from '../../store/slices/uiSlice';

const serverUrl = 'https://localhost:7297';

const ProfilePage = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const { userId: routeUserId } = useParams(); 
  
  const { user: currentUser, isAuthenticated, token } = useSelector((state) => state.auth); 
  
  const currentUserId = currentUser?.userId || currentUser?.id;
  const isOwnProfile = !routeUserId || String(routeUserId) === String(currentUserId);

  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [achievements, setAchievements] = useState([]);
  const [loadingAchievements, setLoadingAchievements] = useState(true);
  const [achievementsError, setAchievementsError] = useState(null);

  const fetchProfile = useCallback(async (userIdToFetch) => {
    const isFetchingMe = userIdToFetch === 'me';
    const endpoint = isFetchingMe ? '/api/users/me' : `/api/users/profile/${userIdToFetch}`;

    if (!isFetchingMe && (!userIdToFetch || userIdToFetch === 'unknown')) {
       setError('Unable to load profile. Invalid ID.');
       setLoading(false);
       return;
    }

    if (isFetchingMe && !isAuthenticated) {
         setError('Please login to view your profile.');
         setLoading(false);
         return;
    }


    setLoading(true);
    setError(null);
    try {
      const response = await axios.get(endpoint);
      console.log("API Response Data:", response.data); 
      setProfile(response.data);
      setIsEditing(false);
    } catch (err) {
      console.error(`Error fetching profile (${endpoint}):`, err);
       if (err.response?.status === 401 || err.response?.status === 403) {
            setError('Authorization error. Please login again.');
            // Можливо, dispatch(logout()) або navigate('/login');
       } else if (err.response?.status === 404) {
           setError('User not found.');
       } else {
            setError('Failed to load profile.');
       }
    } finally {
      console.log("Setting loading to false"); 
      setLoading(false);
    }
  }, [isAuthenticated]); 

  // useEffect для fetchProfile
  useEffect(() => {

    const idToFetch = routeUserId ? routeUserId : (isAuthenticated ? 'me' : null);

    if (idToFetch) {
        fetchProfile(idToFetch);
    } else if (!routeUserId && !isAuthenticated) {
        setError("Please login to view your profile.");
        setLoading(false);
    }
  }, [routeUserId, isAuthenticated, fetchProfile]);


  // useEffect для fetchAchievements
  useEffect(() => {
    const fetchAchievementsInternal = async (userIdToFetch) => {
      if (!userIdToFetch || userIdToFetch === 'unknown') return;
      setLoadingAchievements(true);
      setAchievementsError(null);
      try {
        const response = await axios.get(`/api/users/${userIdToFetch}/achievements`);
        setAchievements(response.data || []);
      } catch (err) {
        console.error('Error fetching achievements:', err);
        if (!loading && !error) { 
             setAchievementsError('User has no achievements.');
        }
      } finally {
        setLoadingAchievements(false);
      }
    };

    if (profile?.userId) {
      fetchAchievementsInternal(profile.userId);
    } else {
      setAchievements([]);
      setLoadingAchievements(false);
    }
  }, [profile?.userId, loading, error]);
  

  const handleEditToggle = () => setIsEditing(prev => !prev);
  const handleSaveProfile = async (dataToSend) => {
    setError(null);
    try {
      const response = await axios.put('/api/users/me', dataToSend, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setProfile(response.data);
      setIsEditing(false);
      dispatch(addNotification({ message: 'Profile saved successfully!', type: 'success' })); 
    } catch (err) {
      console.error('Error saving profile:', err);
      let message = 'Failed to save profile.';
      if (err.response?.data) {
        if (typeof err.response.data === 'string') message = err.response.data;
        else if (err.response.data.message) message = err.response.data.message;
        else if (err.response.data.errors?.[0]?.description) message = err.response.data.errors[0].description;
        else if (err.response.data.errors?.[0]) message = JSON.stringify(err.response.data.errors[0]);
      } else if (err.message) message = err.message;
      setError(message);
      dispatch(addNotification({ message: message, type: 'error' })); 
      throw new Error(message);
    }
  };
  const handleAddFriend = async () => {
    const targetUserId = profile?.userId;
    if (!targetUserId || targetUserId === 'unknown') return;
    setError(null);
    setProfile(prev => ({ ...prev, isLoadingFriendAction: true }));
    try {
      await axios.post(`/api/friends/request/${targetUserId}`);
      dispatch(addNotification({ message: 'Friend request sent.', type: 'success' }));
      fetchProfile(targetUserId);
    } catch (err) {
      console.error('Error adding friend:', err);
      const message = err.response?.data?.message || 'Failed to send friend request.';
      setError(message);
      dispatch(addNotification({ message: message, type: 'error' }));
      setProfile(prev => ({ ...prev, isLoadingFriendAction: false }));
    } 
  };
  const handleRemoveFriend = async () => { 
    const targetUserId = profile?.userId;
    if (!targetUserId || targetUserId === 'unknown') return;
    setError(null);
    setProfile(prev => ({ ...prev, isLoadingFriendAction: true }));
    try {
      await axios.delete(`/api/friends/${targetUserId}`);
      dispatch(addNotification({ message: 'Friend removed.', type: 'success' }));
      fetchProfile(targetUserId); 
    } catch (err) {
      console.error('Error removing friend:', err);
      const message = err.response?.data?.message || 'Failed to remove friend.';
      setError(message);
      dispatch(addNotification({ message: message, type: 'error' }));
      setProfile(prev => ({ ...prev, isLoadingFriendAction: false }));
    }
  };

  console.log("ProfilePage Render State:", { loading, error, profile }); 

  if (loading) {
    console.log("Rendering: Loading Spinner");
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    console.log("Rendering: Error Message");
    return (
      <div className="profile-container error-container">
        <h2>Error</h2>
        <p>{error}</p>
        <Link to="/" className="button-primary">Home</Link>
      </div>
    );
  }

  if (!profile) {
     console.log("Rendering: Profile Not Found / Not Logged In Message");
     return (
       <div className="profile-container error-container">
         <h2>Profile Not Found</h2>
         <p>Unable to load profile. You may need to log in.</p>
         <Link to="/login" className="button-primary">Login</Link>
       </div>
     );
  }

  console.log("Rendering: Profile Content");
  const avatarUrl = profile?.profilePictureUrl
                    ? `${serverUrl}${profile.profilePictureUrl}`
                    : '/img/default-avatar.jpg';

  const renderFriendshipButton = () => {
      if (!isAuthenticated || isOwnProfile) return null; 
      
      const status = profile?.friendshipStatus;
      const isLoadingFriendAction = profile?.isLoadingFriendAction;

      switch (status) {
          case 'accepted':
              return (
                  <div className="friendship-status-container">
                      <span className="friendship-status-indicator accepted">
                          <FaCheckCircle /> Friends
                      </span>
                      <button onClick={handleRemoveFriend} className="button-secondary button-small" disabled={isLoadingFriendAction}>Remove</button>
                  </div>
              );
          case 'pending_sent':
              return <button disabled className="button-secondary">Request Sent</button>;
          case 'pending_received':
               // TODO: Додати функцію прийняття/відхилення запиту
               return <button onClick={handleAddFriend} className="button-primary" disabled={isLoadingFriendAction}>Accept Request</button>; 
          case 'none':
          default:
              return <button onClick={handleAddFriend} className="button-primary" disabled={isLoadingFriendAction}>
                         {isLoadingFriendAction ? <LoadingSpinner size="small" /> : 'Add Friend'}
                      </button>;
      }
  };
  
  const ProfileEditForm = ({ initialData, onSave, onCancel }) => {
      const [formData, setFormData] = useState({
          displayName: initialData.displayName || '',
          bio: initialData.bio || '',
      });
      const [selectedFile, setSelectedFile] = useState(null);
      const initialAvatarUrl = initialData.profilePictureUrl
                               ? `${serverUrl}${initialData.profilePictureUrl}`
                               : '/img/default-avatar.jpg';
      const [previewUrl, setPreviewUrl] = useState(initialAvatarUrl);
      const [isSaving, setIsSaving] = useState(false);
      const [formError, setFormError] = useState(null);

      const handleChange = (e) => {
          const { name, value } = e.target;
          setFormData(prev => ({ ...prev, [name]: value }));
      };
      
      const handleFileChange = (e) => {
          const file = e.target.files[0];
          if (file) {
              if (!file.type.startsWith('image/')) {
                  setFormError('Please select an image file.');
                  setSelectedFile(null);
                  setPreviewUrl(initialAvatarUrl);
                  return;
              }
              const maxSize = 5 * 1024 * 1024; 
              if (file.size > maxSize) {
                   setFormError('File size too large. Maximum 5MB.');
                   setSelectedFile(null);
                   setPreviewUrl(initialAvatarUrl);
                   return;
              }
              setSelectedFile(file);
              setFormError(null);
              const reader = new FileReader();
              reader.onloadend = () => {
                  setPreviewUrl(reader.result); 
              };
              reader.readAsDataURL(file);
          } else {
              setSelectedFile(null);
              setFormError(null);
              setPreviewUrl(initialAvatarUrl);
          }
      };
  
      const handleSubmit = async (e) => {
          e.preventDefault();
          setIsSaving(true);
          setFormError(null);
          
          const dataToSend = new FormData();
          dataToSend.append('DisplayName', formData.displayName);
          dataToSend.append('Bio', formData.bio);
          if (selectedFile) {
              dataToSend.append('AvatarFile', selectedFile);
          }
          
          try {
             await onSave(dataToSend);
          } catch (error) {

          } finally {
              setIsSaving(false);
          }
      };
  
      return (
          <form onSubmit={handleSubmit} className="profile-edit-form">
              <h2>Edit Profile</h2>
              
              {/* Помилка тепер обробляється глобально в setError */}
              {/* {formError && (
                  <div className="server-error-message" role="alert">
                      {formError}
                  </div>
              )} */} 

              {/* Аватар */} 
              <div className="form-group avatar-edit-group">
                  <label>Avatar</label>
                  <div className="avatar-current-preview">
                       <img src={previewUrl} alt="Поточний аватар" />
                  </div>
                   <input 
                        type="file" 
                        id="avatarFile" 
                        name="avatarFile" 
                        accept="image/*" 
                        onChange={handleFileChange} 
                        className="form-file-input" 
                    />
                    {formError && <small className="form-error-text">{formError}</small>}
                    <small className="form-text text-muted">Select an image file (up to 5MB).</small>
              </div>
              
              {/* Відображуване ім'я */} 
              <div className="form-group">
                  <label htmlFor="displayName">Display Name</label>
                   <div className="input-icon-wrapper">
                        <FaUser className="input-icon" />
                        <input 
                            type="text" 
                            id="displayName" 
                            name="displayName" 
                            value={formData.displayName} 
                            onChange={handleChange} 
                            className="form-input" 
                            placeholder="Your name or nickname"
                            maxLength={50} 
                        />
                   </div>
              </div>
              
              {/* Про себе */} 
              <div className="form-group">
                  <label htmlFor="bio">About</label>
                  <textarea 
                    id="bio" 
                    name="bio" 
                    value={formData.bio} 
                    onChange={handleChange} 
                    rows="5" 
                    className="form-textarea" 
                    placeholder="Tell us a bit about yourself..."
                    maxLength={1000} 
                ></textarea>
              </div>
              
              {/* Кнопки */} 
              <div className="form-actions">
                  <button type="submit" className="button-primary" disabled={isSaving}> 
                      {isSaving ? <LoadingSpinner size="small" color="white" /> : <><FaSave /> Save</>}
                  </button>
                  <button type="button" onClick={onCancel} className="button-secondary" disabled={isSaving}>
                      <FaTimes /> Cancel
                  </button>
              </div>
          </form>
      );
  };

  const renderAchievements = () => {
    if (loadingAchievements) return <p>Loading achievements...</p>;
    if (achievementsError) return <p style={{ color: 'orange' }}>{achievementsError}</p>;
    if (achievements.length === 0) return <p>No achievements to display.</p>;

    return (
      <div className="achievements-list">
        {achievements.map(ach => (
          <div key={ach.id} className="achievement-card" title={`Unlocked: ${new Date(ach.unlockedAt).toLocaleDateString()}`}>
            <img src={ach.iconUrl || '/img/default-achievement.png'} alt={ach.name} className="achievement-icon" />
            <div className="achievement-info">
              <span className="achievement-name">{ach.name}</span>
              <span className="achievement-description">{ach.description}</span>
            </div>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className="profile-container">
      {/* Глобальна помилка для профілю */} 
      {error && <div className="server-error-message profile-error">{error}</div>} 

      {isEditing ? (
           <ProfileEditForm 
                initialData={profile} // Передаємо завантажений профіль
                onSave={handleSaveProfile} 
                onCancel={handleEditToggle} 
            />
      ) : (
        // Показуємо основний контент тільки якщо профіль завантажено
        profile && (
          <>
            <header className="profile-header">
              <img 
                src={avatarUrl} // Використовуємо згенерований URL
                alt={`${profile.displayName || profile.username}'s avatar`} 
                className="profile-avatar"
                onError={(e) => { e.target.onerror = null; e.target.src='/img/default-avatar.jpg'; }} // Обробка помилки завантаження аватара
              />
              <div className="profile-header-info">
                <h1>{profile.displayName || profile.username}</h1>
                <p className="username">@{profile.username}</p>
                <p className="registration-date">Member since: {new Date(profile.registrationDate || profile.createdAt).toLocaleDateString()}</p>
                 {/* Кнопка Редагувати або кнопки Дружби */}
                 {isOwnProfile ? (
                      <button onClick={handleEditToggle} className="button-secondary edit-profile-button">
                          <FaPencilAlt style={{ marginRight: '8px' }} /> Edit Profile
                      </button>
                  ) : (
                      renderFriendshipButton()
                  )}
              </div>
              <div className="profile-stats">
                <div className="stat-item">
                  <span className="stat-value">{profile.gamesOwned ?? profile.gamesCount ?? 0}</span>
                  <span className="stat-label">Games</span>
                </div>
                <div className="stat-item">
                  <span className="stat-value">{profile.friendsCount ?? 0}</span>
                  <span className="stat-label">Friends</span>
                </div>
                <div className="stat-item">
                  <span className="stat-value">{profile.wishlistCount ?? 0}</span>
                  <span className="stat-label">Wishlist</span>
                </div>
              </div>
            </header>

            <section className="profile-bio">
              <h2>About</h2>
              <p>{profile.bio || 'User has not shared anything about themselves yet.'}</p>
            </section>

            {/* Секція друзів */}
            {profile.friendsPreview && profile.friendsPreview.length > 0 && (
              <section className="profile-friends-preview">
                <h2>Friends ({profile.friendsCount ?? 0})</h2>
                <div className="friends-grid">
                  {profile.friendsPreview.map(friend => {
                    const friendStatus = friend.isOnline ? 'Online' : 'Offline';
                    const statusClass = friend.isOnline ? 'online' : '';

                    // Перевірка, чи є у друга ID (userId або id)
                    const friendKey = friend.userId || friend.id; 
                    if (!friendKey) {
                        console.warn("Friend object missing key:", friend);
                        return null; // Не рендеримо друга без ключа
                    }

                    return (
                      // Додаємо key={friendKey}
                      <Link key={friendKey} to={`/profile/id/${friendKey}`} className="friend-item" title={`${friend.displayName || friend.userName}`}> 
                        <img 
                          src={friend.profilePictureUrl ? `${serverUrl}${friend.profilePictureUrl}` : '/img/default-avatar.jpg'} 
                          alt={friend.userName}
                          className="friend-avatar"
                          onError={(e) => { e.target.onerror = null; e.target.src='/img/default-avatar.jpg'; }} // Обробка помилки
                        />
                        <div className="friend-info-preview">
                          <span className="friend-name">{friend.displayName || friend.userName}</span>
                          <span className={`friend-status-preview ${statusClass}`}>{friendStatus}</span>
                        </div>
                      </Link>
                    );
                  })}
                </div>
                {profile.friendsCount > profile.friendsPreview.length && (
                   <Link to={`/friends/${profile?.userId}`} className="view-all-friends-link">
                      View All Friends
                   </Link>
                )}
              </section>
            )}

            {/* --- Секція Досягнень --- */}
            <div className="profile-section profile-achievements">
              <h3><FaTrophy /> Achievements</h3>
              {renderAchievements()}
            </div>
          </>
        )
      )}
      {/* Показуємо завантажувач, якщо profile ще не завантажено */}
      {loading && !profile && <LoadingSpinner fullScreen />} 
    </div>
  );
};

export default ProfilePage; 