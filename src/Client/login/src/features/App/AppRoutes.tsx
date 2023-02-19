import { Navigate, Route, Routes } from 'react-router-dom';
import Auth from '../Auth';
import { Register, Edit } from '../User';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/register" element={<Register />} />
      <Route
        path="*"
        element={
          <Auth>
            <Routes>
              <Route path="/user" element={<Edit />} />
              <Route path="/*" element={<Navigate to="/login" />} />
            </Routes>
          </Auth>
        }
      />
    </Routes>
  );
}
