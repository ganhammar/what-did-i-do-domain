import { Route, Routes } from 'react-router-dom';
import Landing from '../Landing';
import NotFound from '../NotFound';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/*" element={<NotFound />} />
    </Routes>
  );
}
