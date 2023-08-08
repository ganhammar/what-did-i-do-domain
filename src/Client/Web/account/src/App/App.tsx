import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { Auth, appTheme, ErrorBoundry, Layout, Loader } from '@wdid/shared';
import { RecoilRoot } from 'recoil';
import { Suspense } from 'react';
import { IntlProvider } from 'react-intl';

export function App() {
  const links = [
    { to: '/account/dashboard', title: 'Dashboard', serverSide: false },
  ];

  return (
    <IntlProvider locale={navigator.language}>
      <RecoilRoot>
        <BrowserRouter>
          <ThemeProvider theme={appTheme}>
            <Auth defaultView="/account/dashboard" pathPrefix="account">
              <Layout isLoggedIn={true} links={links}>
                <ErrorBoundry>
                  <Suspense fallback={<Loader />}>
                    <AppRoutes />
                  </Suspense>
                </ErrorBoundry>
              </Layout>
            </Auth>
          </ThemeProvider>
        </BrowserRouter>
      </RecoilRoot>
    </IntlProvider>
  );
}
