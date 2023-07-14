import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { Auth, appTheme, ErrorBoundry, Layout, Loader } from '@wdid/shared';
import { RecoilRoot } from 'recoil';
import { Suspense } from 'react';

export function App() {
  const links = [
    { to: '/account/dashboard', title: 'Dashboard', serverSide: false },
  ];

  return (
    <RecoilRoot>
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <Suspense fallback={<Loader />}>
            <Auth defaultView="/account/dashboard" pathPrefix="account">
              <Layout isLoggedIn={true} links={links}>
                <ErrorBoundry>
                  <AppRoutes />
                </ErrorBoundry>
              </Layout>
            </Auth>
          </Suspense>
        </ThemeProvider>
      </BrowserRouter>
    </RecoilRoot>
  );
}
