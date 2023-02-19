import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { AppRoutes } from './AppRoutes';
import { appTheme, ErrorBoundry, Layout } from '@wdid/shared';

export function App() {
  const links = [
    { to: '/register', title: 'Register' },
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
