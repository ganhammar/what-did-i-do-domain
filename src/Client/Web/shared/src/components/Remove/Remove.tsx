import styled from 'styled-components';

const Svg = styled.svg`
  width: 24px;
  height: 24px;
  cursor: pointer;
  &:hover {
    opacity: 0.6;
  }
`;

interface RemoveProps {
  onClick?: () => void;
  className?: string;
}

export const Remove = ({ onClick, className }: RemoveProps) => (
  <Svg
    xmlns="http://www.w3.org/2000/svg"
    fill="currentColor"
    viewBox="-3.5 0 19 19"
    onClick={onClick}
    className={className}
  >
    <path d="M11.383 13.644A1.03 1.03 0 0 1 9.928 15.1L6 11.172 2.072 15.1a1.03 1.03 0 1 1-1.455-1.456l3.928-3.928L.617 5.79a1.03 1.03 0 1 1 1.455-1.456L6 8.261l3.928-3.928a1.03 1.03 0 0 1 1.455 1.456L7.455 9.716z" />
  </Svg>
);
