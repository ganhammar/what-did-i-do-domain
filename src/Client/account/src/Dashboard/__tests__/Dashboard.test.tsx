import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { createRecoilMockWrapper } from 'recoil-mock';
import { Dashboard } from '../Dashboard';
import { currentAccountAtom } from '../../Account';
import { eventsAtom } from 'src/Event';
import { IntlProvider } from 'react-intl';
import { ThemeProvider } from 'styled-components';
import { appTheme, flushPromisesAndTimers } from '@wdid/shared';
import { Suspense } from 'react';
import { tagsSelector } from 'src/Tag';

jest.mock('@wdid/shared/src/components/Auth/userManager', () => ({
  userManager: {
    getUser: jest.fn(() =>
      Promise.resolve({
        expired: false,
        access_token: 'test',
      })
    ),
  },
}));

test('renders application title', async () => {
  const name = 'test-account-name';
  const { context, wrapper } = createRecoilMockWrapper();
  context.set(currentAccountAtom, {
    name,
    id: name,
    createDate: new Date().toISOString(),
  });
  context.set(eventsAtom, {
    success: true,
    result: [],
  });
  context.set(tagsSelector, {
    success: true,
    result: [],
  });

  render(
    <IntlProvider locale={navigator.language}>
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <Suspense fallback={<div />}>
            <Dashboard />
          </Suspense>
        </ThemeProvider>
      </BrowserRouter>
    </IntlProvider>,
    { wrapper }
  );

  await flushPromisesAndTimers();

  const elements = screen.getAllByText(new RegExp(name));
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
