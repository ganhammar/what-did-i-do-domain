import { render, screen } from '@testing-library/react';
import { Dashboard } from '../Dashboard';

test('renders application title', async () => {
  render(<Dashboard />);

  const elements = screen.getAllByText(/Dashboard\\?/i);
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
