import { RecoilRoot, useRecoilValue } from 'recoil';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import {
  appTheme, ErrorBoundry, Layout, Loader,
} from '@wdid/shared';
import useUser from '../Auth/currentUserSelector';
import { Suspense } from 'react';

function AppLayout() {
  const user = useRecoilValue(useUser);

  let links = [
    { to: '/login/register', title: 'Register', serverSide: false },
  ];

  if (user) {
    links = [
      { to: '/account/dashboard', title: 'Dashboard', serverSide: true },
    ];
  }

  return (
    <Layout isLoggedIn={Boolean(user)} links={links}>
      <ErrorBoundry>
        <AppRoutes />
      </ErrorBoundry>
    </Layout>
  );
}

export function App() {
  return (
    <RecoilRoot>
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <Suspense fallback={<Loader />}>
            <AppLayout />
          </Suspense>
        </ThemeProvider>
      </BrowserRouter>
    </RecoilRoot>
  );
}
