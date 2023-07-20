import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { createRecoilMockWrapper } from 'recoil-mock';
import Dashboard from '../Dashboard';
import { currentAccountAtom } from '../../Account';
import { eventsSelector } from 'src/Event';
import { IntlProvider } from 'react-intl';
import { ThemeProvider } from 'styled-components';
import { appTheme } from '@wdid/shared';

test('renders application title', async () => {
  const name = 'test-account-name';
  const { context, wrapper } = createRecoilMockWrapper();
  context.set(currentAccountAtom, {
    name,
    id: name,
    createDate: new Date().toISOString(),
  });
  context.set(eventsSelector, {
    success: true,
    result: [],
  });

  render(
    <IntlProvider locale={navigator.language}>
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <Dashboard />
        </ThemeProvider>
      </BrowserRouter>
    </IntlProvider>,
    { wrapper }
  );

  const elements = screen.getAllByText(new RegExp(name));
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
