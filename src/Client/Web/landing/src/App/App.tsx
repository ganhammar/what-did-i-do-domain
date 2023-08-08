import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { appTheme, ErrorBoundry, Layout } from '@wdid/shared';

export function App() {
  const links = [
    { to: '/login/register', title: 'Register', serverSide: true },
  ];

  return (
    <BrowserRouter>
      <ThemeProvider theme={appTheme}>
        <Layout isLoggedIn={false} links={links}>
          <ErrorBoundry>
            <AppRoutes />
          </ErrorBoundry>
        </Layout>
      </ThemeProvider>
    </BrowserRouter>
  );
}
