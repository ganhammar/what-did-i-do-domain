import { ReactNode } from 'react';
import styled, { createGlobalStyle } from 'styled-components';
import { Header } from './Header';

const GlobalStyle = createGlobalStyle`
  * {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
    color: ${({ theme }) => theme.palette.background.contrastText};
    font-family: ${({ theme }) => theme.typography.fontFamily};
    font-size: ${({ theme }) => theme.typography.fontSize};
    line-height: ${({ theme }) => theme.typography.lineHeight};
    transition: color 0.5s, background-color 0.5s;
    text-rendering: optimizeLegibility;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
  }
  body {
    background: ${({ theme }) => theme.palette.background.main};
  }
  h1 {
    font-size: ${({ theme }) => theme.typography.h1};
  }
  h2 {
    font-size: ${({ theme }) => theme.typography.h2};
  }
  h3 {
    font-size: ${({ theme }) => theme.typography.h3};
  }
`;

const App = styled.div`
  width: 800px;
  padding: 2rem;
  font-weight: normal;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
`;

interface Props {
  children: ReactNode;
  links: { to: string; title: string; serverSide: boolean }[];
  isLoggedIn: boolean;
}

export function Layout({ children, links, isLoggedIn }: Props) {
  return (
    <>
      <GlobalStyle />
      <App>
        <Header links={links} isLoggedIn={isLoggedIn} />
        {children}
      </App>
    </>
  );
}
