import { ReactElement } from 'react';
import { Route, Routes } from 'react-router-dom';
import { useRecoilState } from 'recoil';
import { Loader } from '../../components/Loader';
import { userAtom } from '../User';
import { Login } from './';

interface AuthProps {
  children: ReactElement;
}

function RenderIfLoggedIn({ children }: AuthProps) {
  const [user] = useRecoilState(userAtom);

  if (user && children) {
    return children;
  }

  return <Loader />;
}

function Logout() {
  return <>Logout</>;
}

export function Auth({ children }: AuthProps) {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/logout" element={<Logout />} />
      <Route
        path="*"
        element={<RenderIfLoggedIn>{children}</RenderIfLoggedIn>}
      />
    </Routes>
  );
}
