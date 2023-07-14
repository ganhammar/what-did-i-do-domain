import { ReactElement, useEffect, useMemo } from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useRecoilRefresher_UNSTABLE, useRecoilValue } from 'recoil';
import { Loader } from '@wdid/shared';
import { UserService } from '../User';
import { Login } from '.';
import currentUserSelector from './currentUserSelector';

interface AuthProps {
  children: ReactElement;
}

function RenderIfLoggedIn({ children }: AuthProps) {
  const user = useRecoilValue(currentUserSelector);

  if (user && children) {
    return children;
  }

  return <Navigate to="/login" />;
}

function Logout() {
  const user = useRecoilValue(currentUserSelector);
  const refresh = useRecoilRefresher_UNSTABLE(currentUserSelector);
  const userService = useMemo(() => new UserService(), []);
  const navigate = useNavigate();

  useEffect(() => {
    const logout = async () => {
      const response = await userService.logout();

      if (response.success) {
        refresh();
        navigate('/');
      }
    };

    if (user) {
      logout();
    }
  }, [userService, navigate, refresh, user]);

  if (!user) {
    return <Navigate to="/login" />;
  }

  return <Loader />;
}

export function Auth({ children }: AuthProps) {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/login/logout" element={<Logout />} />
      <Route
        path="*"
        element={<RenderIfLoggedIn>{children}</RenderIfLoggedIn>}
      />
    </Routes>
  );
}
