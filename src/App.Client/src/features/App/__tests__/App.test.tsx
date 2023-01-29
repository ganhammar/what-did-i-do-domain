import { render, screen } from '@testing-library/react';
import { App } from '../App';

test('renders application title', () => {
  render(<App />);
  const elements = screen.getAllByText(/What Did I Do\\?/i);
  expect(elements.length).toBeGreaterThan(0);
  expect(elements[0]).toBeInTheDocument();
});
