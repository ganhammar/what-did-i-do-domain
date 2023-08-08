import { Navigate, Route, Routes } from 'react-router-dom';
import { Create, Select } from 'src/Account';
import { Dashboard } from 'src/Dashboard';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/account/dashboard" element={<Dashboard />} />
      <Route path="/account/create" element={<Create />} />
      <Route path="/account/select" element={<Select />} />
      <Route path="/*" element={<Navigate to="/account/dashboard" />} />
    </Routes>
  );
}
