import { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/layout/Layout';
import LoadingSpinner from './components/ui/LoadingSpinner';
import GameDetailsPage from './pages/GameDetailsPage';
import ProfilePage from './pages/Profile/Profile';
import AdminDashboard from './pages/Admin/AdminDashboard';
import NotFound from './pages/NotFound';
import AdminGameEditPage from './pages/Admin/Game/AdminGameEditPage';
import AdminGameCreatePage from './pages/Admin/Game/AdminGameCreatePage';
import AdminOrderDetailsPage from './pages/Admin/Order/AdminOrderDetailsPage';
import AdminUserDetailsPage from './pages/Admin/User/AdminUserDetailsPage';
import Home from './pages/Home';
import BundlesPage from './pages/BundlesPage';
import BundleDetailsPage from './pages/BundleDetailsPage';

// Ліниве завантаження компонентів для оптимізації
const Login = lazy(() => import('./pages/Auth/Login'));
const Register = lazy(() => import('./pages/Auth/Register'));
const Store = lazy(() => import('./pages/Store/Store'));
const Cart = lazy(() => import('./pages/Cart/Cart'));
const Checkout = lazy(() => import('./pages/Cart/Checkout'));
const Library = lazy(() => import('./pages/Library/Library'));
const Friends = lazy(() => import('./pages/Friends/Friends'));
const Wishlist = lazy(() => import('./pages/Wishlist/Wishlist'));

// Компонент захищеного маршруту
const PrivateRoute = ({ children }) => {
  // Реалізація захисту маршруту
  return children;
};

function App() {
  return (
    <Suspense fallback={<LoadingSpinner fullScreen />}>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="store" element={<Store />} />
          <Route path="store/search" element={<Store />} />
          <Route path="store/categories/:categoryId" element={<Store />} />
          <Route path="cart" element={<Cart />} />
          <Route path="checkout" element={<Checkout />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="profile/id/:userId" element={<ProfilePage />} />
          <Route path="library" element={<Library />} />
          <Route path="friends" element={<Friends />} />
          <Route path="wishlist" element={<Wishlist />} />
          <Route path="bundles" element={<BundlesPage />} />
          <Route path="bundles/:id" element={<BundleDetailsPage />} />
          <Route path="admin" element={
            <PrivateRoute>
              <AdminDashboard />
            </PrivateRoute>
          } />
          <Route path="admin/games/edit/:gameId" element={
            <PrivateRoute>
              <AdminGameEditPage />
            </PrivateRoute>
          } />
          <Route path="admin/games/create" element={
            <PrivateRoute>
              <AdminGameCreatePage />
            </PrivateRoute>
          } />
          <Route path="admin/orders/:orderId" element={
            <PrivateRoute>
              <AdminOrderDetailsPage />
            </PrivateRoute>
          } />
          <Route path="admin/users/:userId" element={
            <PrivateRoute>
              <AdminUserDetailsPage />
            </PrivateRoute>
          } />
          <Route path="404" element={<NotFound />} />
          <Route path="game/:id" element={<GameDetailsPage />} />
          <Route path="*" element={<Navigate replace to="/404" />} />
        </Route>
      </Routes>
    </Suspense>
  );
}

export default App;