import { Navigate, Route, Routes } from 'react-router-dom';
import { Dashboard } from 'src/Dashboard';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/account/dashboard" element={<Dashboard />} />
      <Route path="/*" element={<Navigate to="/account/dashboard" />} />
    </Routes>
  );
}
