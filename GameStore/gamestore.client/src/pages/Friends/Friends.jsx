import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import './Friends.css';

const Friends = () => {
  const [friends, setFriends] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [sentRequests, setSentRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState('friends'); // 'friends', 'pending', 'sent'
  
  useEffect(() => {
    const fetchFriends = async () => {
      setLoading(true);
      try {
        const [friendsRes, pendingRes, sentRes] = await Promise.all([
          axios.get('/api/friends'),
          axios.get('/api/friends/requests/incoming'),
          axios.get('/api/friends/requests/outgoing')
        ]);
        
        setFriends(friendsRes.data || []);
        setPendingRequests(pendingRes.data || []);
        setSentRequests(sentRes.data || []);
        setError(null);
      } catch (err) {
        console.error('Error fetching friends:', err);
        setError('Не вдалось завантажити список друзів. Спробуйте пізніше.');
      } finally {
        setLoading(false);
      }
    };

    fetchFriends();
  }, []);

  const handleAcceptRequest = async (requestId) => {
    try {
      await axios.post(`/api/friends/request/${requestId}/accept`);
      
      setPendingRequests(prev => prev.filter(request => request.id !== requestId));
    } catch (err) {
      console.error('Error accepting friend request:', err);
      setError('Не вдалось прийняти запит. Спробуйте пізніше.');
    }
  };

  const handleDeclineRequest = async (requestId) => {
    try {
      await axios.post(`/api/friends/request/${requestId}/decline`);
      
      setPendingRequests(prev => prev.filter(request => request.id !== requestId));
    } catch (err) {
      console.error('Error rejecting friend request:', err);
      setError('Не вдалось відхилити запит. Спробуйте пізніше.');
    }
  };

  const handleCancelRequest = async (requestId) => {
    try {
      await axios.post(`/api/friends/request/${requestId}/cancel`);
      
      setSentRequests(prev => prev.filter(request => request.id !== requestId));
    } catch (err) {
      console.error('Error canceling friend request:', err);
      setError('Не вдалось скасувати запит. Спробуйте пізніше.');
    }
  };

  const handleRemoveFriend = async (friendIdToRemove) => {
    try {
      await axios.delete(`/api/friends/${friendIdToRemove}`);
      
      setFriends(prev => prev.filter(friend => friend.friendId !== friendIdToRemove));
      setError(null);
    } catch (err) {
      console.error('Error removing friend:', err);
      setError('Не вдалось видалити друга. Спробуйте пізніше.');
    }
  };

  const filteredItems = () => {
    if (!friends || !pendingRequests || !sentRequests) return [];
    const query = searchQuery.toLowerCase();
    
    switch (activeTab) {
      case 'friends':
        return friends.filter(friend => 
          (friend.friendName?.toLowerCase().includes(query) ||
           friend.friendDisplayName?.toLowerCase().includes(query))
        );
      case 'pending':
        return pendingRequests.filter(request => 
          request.senderName?.toLowerCase().includes(query)
        );
      case 'sent':
        return sentRequests.filter(request => 
          request.receiverName?.toLowerCase().includes(query)
        );
      default:
        return [];
    }
  };

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  return (
    <div className="friends-page-container">
      <div className="friends-content">
        <h1 className="friends-title">Friends</h1>
        
        {error && <div className="friends-error">{error}</div>}
        
        {}
        <div className="friends-search-panel">
          <input
            type="text"
            placeholder="Search for friends..."
            className="friends-search-input"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        
        {}
        <div className="friends-tabs">
          <button
            className={`friends-tab-button ${
              activeTab === 'friends' 
                ? 'active' 
                : ''
            }`}
            onClick={() => setActiveTab('friends')}
          >
            Friends {friends.length > 0 && `(${friends.length})`}
          </button>
          <button
            className={`friends-tab-button ${
              activeTab === 'pending' 
                ? 'active' 
                : ''
            }`}
            onClick={() => setActiveTab('pending')}
          >
            Incoming requests {pendingRequests.length > 0 && `(${pendingRequests.length})`}
          </button>
          <button
            className={`friends-tab-button ${
              activeTab === 'sent' 
                ? 'active' 
                : ''
            }`}
            onClick={() => setActiveTab('sent')}
          >
            Sent requests {sentRequests.length > 0 && `(${sentRequests.length})`}
          </button>
        </div>
        
        {}
        <div className="friends-list-container">
          {filteredItems().length === 0 ? (
            <div className="friends-empty-state">
              <p className="friends-empty-text">
                {searchQuery 
                  ? `Нічого не знайдено за запитом "${searchQuery}"` 
                  : activeTab === 'friends'
                    ? 'You have no friends'
                    : activeTab === 'pending'
                      ? 'No incoming friend requests'
                      : 'No sent friend requests'}
              </p>
              {activeTab === 'friends' && friends.length === 0 && (
                <p className="friends-empty-subtext">
                  Add friends to play together and follow their achievements
                </p>
              )}
            </div>
          ) : (
            <div className="friends-grid">
              {filteredItems().map(user => {
                const userData = activeTab === 'friends' 
                    ? { id: user.friendId, name: user.friendName, displayName: user.friendDisplayName, avatar: user.friendAvatar, status: user.friendStatus, isOnline: user.friendIsOnline }
                    : { id: activeTab === 'pending' ? user.senderId : user.receiverId, name: activeTab === 'pending' ? user.senderName : user.receiverName, avatar: activeTab === 'pending' ? user.senderAvatar : user.receiverAvatar, requestId: user.id };
                
                return (
                  <div
                    key={userData.id || userData.requestId}
                    className="friend-card"
                  >
                    <img 
                      src={userData.avatar ? `https://localhost:7297${userData.avatar}` : '/img/default-avatar.jpg'} 
                      alt={userData.name}
                      className="friend-avatar"
                    />
                    
                    <div className="friend-info">
                      <Link to={`/profile/id/${userData.id}`} className="friend-link" title={userData.displayName || userData.name}>
                        {userData.displayName || userData.name} 
                      </Link>
                      {/* Статус друга/запиту */} 
                      {activeTab === 'friends' ? (
                        <p className={`friend-status ${userData.isOnline ? 'online' : ''} ${userData.status?.startsWith('In game:') ? 'in-game' : ''}`}>
                          {userData.status || (userData.isOnline ? 'Online' : 'Offline')}
                        </p>
                      ) : (
                        <p className="friend-status">
                          {activeTab === 'pending' ? 'Waiting for your response' : 'Request sent'}
                        </p>
                      )}
                    </div>
                    
                    <div className="friend-actions">
                      {activeTab === 'friends' && (
                        <>
                          <Link 
                            to={`/profile/id/${userData.id}`}
                            className="friend-action-button button-profile"
                          >
                            Profile
                          </Link>
                          <button 
                            className="friend-action-button button-remove"
                            onClick={() => handleRemoveFriend(userData.id)}
                          >
                            Remove
                          </button>
                        </>
                      )}
                      
                      {activeTab === 'pending' && (
                        <>
                          <button 
                            className="friend-action-button button-accept"
                            onClick={() => handleAcceptRequest(userData.requestId)}
                          >
                            Accept
                          </button>
                          <button 
                            className="friend-action-button button-decline"
                            onClick={() => handleDeclineRequest(userData.requestId)}
                          >
                            Decline
                          </button>
                        </>
                      )}
                      
                      {activeTab === 'sent' && (
                        <button 
                          className="friend-action-button button-cancel"
                          onClick={() => handleCancelRequest(userData.requestId)}
                        >
                          Cancel request
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Friends; 