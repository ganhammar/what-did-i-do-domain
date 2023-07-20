import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { createRecoilMockWrapper } from 'recoil-mock';
import { Dashboard } from '../Dashboard';
import { currentAccountAtom } from '../../Account';

test('renders application title', async () => {
  const name = 'test-account-name';
  const { context, wrapper } = createRecoilMockWrapper();
  context.set(currentAccountAtom, {
    name,
    id: name,
    createDate: new Date().toISOString(),
  });

  render(
    <BrowserRouter>
      <Dashboard />
    </BrowserRouter>,
    { wrapper }
  );

  const elements = screen.getAllByText(new RegExp(name));
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
