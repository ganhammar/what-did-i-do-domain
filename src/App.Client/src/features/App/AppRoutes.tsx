import { Route, Routes } from 'react-router-dom';
import Dashboard from '../Dashboard';
import Landing from '../Landing';
import Auth from '../Auth';
import { Register } from '../User';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/register" element={<Register />} />
      <Route
        path="*"
        element={
          <Auth>
            <Route path="/register" element={<Dashboard />} />
          </Auth>
        }
      />
    </Routes>
  );
}
