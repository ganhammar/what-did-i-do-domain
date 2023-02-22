import { NavLink } from 'react-router-dom';
import styled from 'styled-components';

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
const StyledNavLink = styled(NavLink)`
  text-decoration: none;
  margin: 0 ${({ theme }) => theme.spacing.s};
  &:last-child {
    position: absolute;
    right: ${({ theme }) => theme.spacing.s};
  }
  &:hover, &.active {
    font-weight: bold;
    color: ${({ theme }) => theme.palette.primary.main};
  }
`;

interface Props {
  links: { to: string, title: string, serverSide: boolean }[];
  isLoggedIn: boolean;
};

export function Header({ links, isLoggedIn }: Props) {
  return (
    <Wrapper>
      <Title>What Did I Do?</Title>
      <Nav>
        {links.map(({ to, title, serverSide }) => (
          <>
            {serverSide && (
              <a href={to}>{title}</a>
            )}
            {!serverSide && (
              <StyledNavLink
                className={(isActive) => (isActive ? 'active' : '')}
                to={to}
                key={to}
              >
                {title}
              </StyledNavLink>
            )}
          </>
        ))}
        {!isLoggedIn && (
            <a href="/login">Login</a>
        )}
        {isLoggedIn && (
          <a href="/login/logout">Logout</a>
        )}
      </Nav>
    </Wrapper>
  );
}
