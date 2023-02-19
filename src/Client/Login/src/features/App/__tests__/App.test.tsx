import { render, screen } from '@testing-library/react';
import { flushPromisesAndTimers } from '@wdid/shared';
import { App } from '../App';

test('renders application title', async () => {
  global.fetch = jest.fn(() =>
    Promise.resolve({
      ok: false,
      text: () => { },
    }),
  ) as jest.Mock;

  render(<App />);

  await flushPromisesAndTimers();

  const elements = screen.getAllByText(/What Did I Do\\?/i);
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
