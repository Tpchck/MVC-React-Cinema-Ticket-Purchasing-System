import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { Navbar } from './components/Navbar';
import { Footer } from './components/Footer';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useInitializeAuth } from './hooks/useInitializeAuth';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ScreeningsPage } from './pages/ScreeningsPage';
import { SeatMapPage } from './pages/SeatMapPage';
import { ProfilePage } from './pages/ProfilePage';
import { AdminDashboardPage } from './pages/AdminDashboardPage';
import { AdminEditUserPage } from './pages/AdminEditUserPage';
import './index.css';

const NotFound = () => <div className="container mt-5"><h1>404 - Page Not Found</h1></div>;

function App() {
  useInitializeAuth();

  return (
    <BrowserRouter>
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 3500,
          success: { iconTheme: { primary: '#198754', secondary: '#fff' } },
          error:   { iconTheme: { primary: '#dc3545', secondary: '#fff' } },
        }}
      />
      <Navbar />
      <Routes>
        <Route path="/" element={<ScreeningsPage />} />
        <Route path="/screenings/:id/seats" element={<SeatMapPage />} />
        <Route path="/auth/login" element={<LoginPage />} />
        <Route path="/auth/register" element={<RegisterPage />} />
        <Route
          path="/profile"
          element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin"
          element={
            <ProtectedRoute requireAdmin>
              <AdminDashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/users/:id"
          element={
            <ProtectedRoute requireAdmin>
              <AdminEditUserPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<NotFound />} />
      </Routes>
      <Footer />
    </BrowserRouter>
  );
}

export default App;
