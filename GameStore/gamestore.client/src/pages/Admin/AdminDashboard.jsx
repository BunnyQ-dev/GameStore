import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { addNotification } from '../../store/slices/uiSlice';
import axios from '../../utils/axios-config';
import styled from 'styled-components';
import GenresManagement from './Management/GenresManagement';
import PlatformsManagement from './Management/PlatformsManagement';
import DevelopersManagement from './Management/DevelopersManagement';
import PublishersManagement from './Management/PublishersManagement';
import AchievementsManagement from './Management/AchievementsManagement';
import GamesManagement from './Management/GamesManagement';
import UsersManagement from './Management/UsersManagement';
import OrdersManagement from './Management/OrdersManagement';
import BundlesManagement from './Management/BundlesManagement';

const AdminDashboard = () => {
  const { user, isAuthenticated } = useSelector(state => state.auth);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  
  const [stats, setStats] = useState({
    totalGames: 0,
    totalUsers: 0,
    totalOrders: 0,
    totalRevenue: 0
  });
  
  const [recentOrders, setRecentOrders] = useState([]);
  const [activeTab, setActiveTab] = useState('dashboard');
  const [isLoadingUser, setIsLoadingUser] = useState(true);

  useEffect(() => {
    // Check if user is authenticated
    if (!isAuthenticated) {
      navigate('/login');
      dispatch(addNotification({
        message: 'You must be logged in to access the admin area',
        type: 'error'
      }));
      return;
    }

    // Перевіряємо, чи user вже завантажений. Якщо ні, то не перевіряємо ролі одразу
    if (!user) {
        setIsLoadingUser(true); // Можливо, дані ще завантажуються
        return; 
    } else if (isLoadingUser) {
        setIsLoadingUser(false); 
    }

    // Тепер перевіряємо ролі, тільки якщо user НЕ null
    if (!user.roles?.includes('Admin')) {
      navigate('/');
      dispatch(addNotification({
        message: 'You do not have permission to access the admin area',
        type: 'error'
      }));
      return;
    }
    
    // Fetch dashboard data
    const fetchDashboardData = async () => {
      try {
        const statsResponse = await axios.get('/api/admin/stats');
        setStats(statsResponse.data);
        
        const ordersResponse = await axios.get('/api/admin/recent-orders');
        setRecentOrders(ordersResponse.data);
      } catch (error) {
        console.error('Failed to fetch admin data:', error);
        dispatch(addNotification({
          message: 'Failed to load admin dashboard data',
          type: 'error'
        }));
      }
    };
    
    fetchDashboardData();
  }, [isAuthenticated, user, dispatch, navigate]);

  // Додаємо відображення завантаження, поки user не завантажиться
  if (isLoadingUser && isAuthenticated) {
      return <AdminContainer><div>Loading user data...</div></AdminContainer>; 
  }

  const renderActiveTab = () => {
    switch (activeTab) {
      case 'dashboard':
        return <DashboardContent stats={stats} recentOrders={recentOrders} />;
      case 'games':
        return <GamesManagement />;
      case 'platforms':
        return <PlatformsManagement />;
      case 'users':
        return <UsersManagement />;
      case 'orders':
        return <OrdersManagement />;
      case 'genres':
        return <GenresManagement />;
      case 'developers':
        return <DevelopersManagement />;
      case 'publishers':
        return <PublishersManagement />;
      case 'achievements':
        return <AchievementsManagement />;
      case 'bundles':
        return <BundlesManagement />;
      default:
        return <DashboardContent stats={stats} recentOrders={recentOrders} />;
    }
  };

  return (
    <AdminContainer>
      <AdminSidebar>
        <SidebarHeader>
          <h2>Admin Panel</h2>
        </SidebarHeader>
        <SidebarMenu>
          <SidebarItem 
            $active={activeTab === 'dashboard'} 
            onClick={() => setActiveTab('dashboard')}
          >
            Dashboard
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'games'} 
            onClick={() => setActiveTab('games')}
          >
            Games Management
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'platforms'} 
            onClick={() => setActiveTab('platforms')}
          >
            Platforms Management
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'users'} 
            onClick={() => setActiveTab('users')}
          >
            Users Management
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'orders'} 
            onClick={() => setActiveTab('orders')}
          >
            Orders
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'genres'} 
            onClick={() => setActiveTab('genres')}
          >
            Genres
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'developers'} 
            onClick={() => setActiveTab('developers')}
          >
            Developers
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'publishers'} 
            onClick={() => setActiveTab('publishers')}
          >
            Publishers
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'achievements'} 
            onClick={() => setActiveTab('achievements')}
          >
            Achievements
          </SidebarItem>
          <SidebarItem 
            $active={activeTab === 'bundles'} 
            onClick={() => setActiveTab('bundles')}
          >
            Bundles
          </SidebarItem>
          <SidebarItem as={Link} to="/">
            Back to Store
          </SidebarItem>
        </SidebarMenu>
      </AdminSidebar>
      
      <AdminContent>
        <ContentHeader>
          <h1>{activeTab.charAt(0).toUpperCase() + activeTab.slice(1)}</h1>
        </ContentHeader>
        {activeTab === 'users' ? <UsersManagement /> : renderActiveTab()}
      </AdminContent>
    </AdminContainer>
  );
};

// Dashboard Tab Content
const DashboardContent = ({ stats, recentOrders }) => {
  return (
    <DashboardContainer>
      <StatsGrid>
        <StatCard>
          <StatTitle>Total Games</StatTitle>
          <StatValue>{stats.totalGames}</StatValue>
        </StatCard>
        
        <StatCard>
          <StatTitle>Total Users</StatTitle>
          <StatValue>{stats.totalUsers}</StatValue>
        </StatCard>
        
        <StatCard>
          <StatTitle>Total Orders</StatTitle>
          <StatValue>{stats.totalOrders}</StatValue>
        </StatCard>
        
        <StatCard>
          <StatTitle>Total Revenue</StatTitle>
          <StatValue>${stats.totalRevenue.toFixed(2)}</StatValue>
        </StatCard>
      </StatsGrid>
      
      <RecentOrdersSection>
        <h2>Recent Orders</h2>
        <OrdersTable>
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
            {recentOrders.length > 0 ? (
              recentOrders.map(order => (
                <tr key={order.id}>
                  <td>#{order.id}</td>
                  <td>{order.userName}</td>
                  <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                  <td>${order.total.toFixed(2)}</td>
                  <td>
                    <OrderStatus $status={order.status}>{order.status}</OrderStatus>
                  </td>
                  <td>
                    <button>View</button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="6">No recent orders found</td>
              </tr>
            )}
          </tbody>
        </OrdersTable>
      </RecentOrdersSection>
    </DashboardContainer>
  );
};

// Styled Components
const AdminContainer = styled.div`
  display: flex;
  min-height: calc(100vh - var(--header-height));
  background-color: var(--bg-color);
`;

const AdminSidebar = styled.div`
  width: 250px;
  background-color: var(--bg-color-secondary);
  border-right: 1px solid var(--bg-color-tertiary);
  display: flex;
  flex-direction: column;
`;

const SidebarHeader = styled.div`
  padding: 20px;
  border-bottom: 1px solid var(--bg-color-tertiary);
  
  h2 {
    margin: 0;
    color: var(--accent-color);
    font-size: 1.5rem;
  }
`;

const SidebarMenu = styled.div`
  display: flex;
  flex-direction: column;
  padding: 20px 0;
`;

const SidebarItem = styled.button`
  background: ${props => props.$active ? 'var(--bg-color-tertiary)' : 'transparent'};
  border: none;
  text-align: left;
  padding: 12px 20px;
  color: ${props => props.$active ? 'var(--accent-color)' : 'var(--text-color)'};
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.2s;
  
  &:hover {
    background-color: var(--bg-color-tertiary);
    color: var(--accent-color);
  }
`;

const AdminContent = styled.div`
  flex: 1;
  padding: 20px;
  overflow-y: auto;
`;

const ContentHeader = styled.div`
  margin-bottom: 20px;
  
  h1 {
    font-size: 2rem;
    margin: 0;
  }
`;

const DashboardContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 30px;
`;

const StatsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 20px;
`;

const StatCard = styled.div`
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
`;

const StatTitle = styled.h3`
  font-size: 1rem;
  color: var(--text-color-secondary);
  margin: 0 0 10px 0;
`;

const StatValue = styled.div`
  font-size: 2rem;
  font-weight: bold;
  color: var(--accent-color);
`;

const RecentOrdersSection = styled.div`
  background-color: var(--bg-color-secondary);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  
  h2 {
    margin-top: 0;
    margin-bottom: 20px;
    color: var(--text-color);
  }
`;

const OrdersTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  
  th, td {
    padding: 12px;
    text-align: left;
    border-bottom: 1px solid var(--bg-color-tertiary);
  }
  
  th {
    font-weight: bold;
    color: var(--text-color-secondary);
  }
  
  button {
    padding: 6px 12px;
    font-size: 0.8rem;
  }
`;

const OrderStatus = styled.span`
  display: inline-block;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: bold;
  
  ${props => {
    switch (props.$status) {
      case 'Completed':
        return `
          background-color: rgba(76, 175, 80, 0.2);
          color: #4caf50;
        `;
      case 'Processing':
        return `
          background-color: rgba(33, 150, 243, 0.2);
          color: #2196f3;
        `;
      case 'Pending':
        return `
          background-color: rgba(255, 152, 0, 0.2);
          color: #ff9800;
        `;
      case 'Cancelled':
        return `
          background-color: rgba(244, 67, 54, 0.2);
          color: #f44336;
        `;
      default:
        return `
          background-color: rgba(158, 158, 158, 0.2);
          color: #9e9e9e;
        `;
    }
  }}
`;

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
  
  h2 {
    margin: 0;
  }
  
  button {
    padding: 8px 16px;
  }
`;

const DataTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  
  th, td {
    padding: 12px;
    text-align: left;
    border-bottom: 1px solid var(--bg-color-tertiary);
  }
  
  th {
    font-weight: bold;
    color: var(--text-color-secondary);
  }
`;

const ButtonGroup = styled.div`
  display: flex;
  gap: 8px;
  
  button {
    padding: 6px 12px;
    font-size: 0.8rem;
  }
`;

export default AdminDashboard; 