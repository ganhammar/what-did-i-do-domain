import { ReactNode } from 'react';
import styled from 'styled-components';

interface HeaderProps {
  size: 'H2' | 'H3';
  children: ReactNode;
}

const H2 = styled.h2`
  margin-bottom: ${({ theme }) => theme.spacing.m};
`;

const H3 = styled.h3`
  margin-bottom: ${({ theme }) => theme.spacing.l};
`;

export const Header = ({ size, children }: HeaderProps) => {
  switch (size) {
    case 'H2':
      return <H2>{children}</H2>;
    case 'H3':
      return <H3>{children}</H3>;
  }
};
