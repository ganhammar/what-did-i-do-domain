import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { Auth, appTheme, ErrorBoundry, Layout } from '@wdid/shared';
import { RecoilRoot } from 'recoil';

export function App() {
  const links = [
    { to: '/account/dashboard', title: 'Dashboard', serverSide: false },
  ];

  return (
    <RecoilRoot>
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <Auth defaultView="/account/dashboard">
            <Layout isLoggedIn={true} links={links}>
              <ErrorBoundry>
                <AppRoutes />
              </ErrorBoundry>
            </Layout>
          </Auth>
        </ThemeProvider>
      </BrowserRouter>
    </RecoilRoot>
  );
}
