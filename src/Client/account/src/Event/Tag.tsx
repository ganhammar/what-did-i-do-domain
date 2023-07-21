import { ReactNode } from 'react';
import styled from 'styled-components';

interface TagProps {
  children: ReactNode;
  className?: string;
}

const Element = styled.div`
  background-color: ${({ theme }) => theme.palette.background.main};
  color: ${({ theme }) => theme.palette.background.contrastText};
  padding: ${({ theme }) => `0 ${theme.spacing.xs}`};
  height: 28px;
  line-height: 28px;
  font-size: 0.7rem;
  border-radius: 14px;
  margin-right: ${({ theme }) => theme.spacing.xs};
`;

export const Tag = ({ children, className }: TagProps) => (
  <Element className={className}>{children}</Element>
);
