import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { appTheme, ErrorBoundry, Layout } from '@wdid/shared';

export function App() {
  const links = [
    { to: '/account/dashboard', title: 'Dashboard', serverSide: false },
  ];

  return (
    <BrowserRouter>
      <ThemeProvider theme={appTheme}>
        <Layout isLoggedIn={true} links={links}>
          <ErrorBoundry>
            <AppRoutes />
          </ErrorBoundry>
        </Layout>
      </ThemeProvider>
    </BrowserRouter>
  );
}
