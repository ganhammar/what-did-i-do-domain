import { Navigate, Route, Routes } from 'react-router-dom';
import Auth from '../Auth';
import { Register, Edit } from '../User';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login/register" element={<Register />} />
      <Route
        path="*"
        element={
          <Auth>
            <Routes>
              <Route path="/login/user" element={<Edit />} />
              <Route path="/*" element={<Navigate to="/login/user" />} />
            </Routes>
          </Auth>
        }
      />
    </Routes>
  );
}
