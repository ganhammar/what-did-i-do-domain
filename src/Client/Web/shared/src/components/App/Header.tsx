import { Fragment } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import styled, { css } from 'styled-components';

const Wrapper = styled.header`
  text-align: center;
`;
const Title = styled.h1`
  font-family: Pacifico;
`;
const Nav = styled.nav`
  margin: 2rem 0;
  padding: 0.5rem 0;
  border-top: 1px solid ${({ theme }) => theme.palette.divider.main};
  border-bottom: 1px solid ${({ theme }) => theme.palette.divider.main};
  position: relative;
`;
const LinkStyle = css`
  text-decoration: none;
  margin: 0 ${({ theme }) => theme.spacing.s};
  &:last-child {
    position: absolute;
    right: ${({ theme }) => theme.spacing.s};
  }
  &:hover,
  &.active {
    font-weight: bold;
    color: ${({ theme }) => theme.palette.primary.main};
  }
`;
const StyledNavLink = styled(NavLink)`
  ${LinkStyle}
`;
const StyledALink = styled.a`
  ${LinkStyle}
`;

interface Props {
  links: { to: string; title: string; serverSide: boolean }[];
  isLoggedIn: boolean;
}

export function Header({ links, isLoggedIn }: Props) {
  const { pathname } = useLocation();

  return (
    <Wrapper>
      <Title>What Did I Do?</Title>
      <Nav>
        {links.map(({ to, title, serverSide }) => (
          <Fragment key={to}>
            {serverSide && (
              <StyledALink
                href={to}
                className={pathname.startsWith(to) ? 'active' : ''}
              >
                {title}
              </StyledALink>
            )}
            {!serverSide && (
              <StyledNavLink
                className={(isActive) => (isActive ? 'active' : '')}
                to={to}
              >
                {title}
              </StyledNavLink>
            )}
          </Fragment>
        ))}
        {!isLoggedIn && (
          <StyledALink
            href="/login"
            className={pathname === '/login' ? 'active' : ''}
          >
            Login
          </StyledALink>
        )}
        {isLoggedIn && <StyledALink href="/login/logout">Logout</StyledALink>}
      </Nav>
    </Wrapper>
  );
}
