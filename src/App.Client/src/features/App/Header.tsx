import { NavLink } from 'react-router-dom';
import { useRecoilValue } from 'recoil';
import styled from 'styled-components';
import useUser from '../Auth/useUser';

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

export function Header() {
  const user = useRecoilValue(useUser);

  return (
    <Wrapper>
      <Title>What Did I Do?</Title>
      <Nav>
        {!user && (
          <>
            <StyledNavLink
              className={(isActive) => (isActive ? 'active' : '')}
              to="/"
            >
              Home
            </StyledNavLink>
            <StyledNavLink
              className={(isActive) => (isActive ? 'active' : '')}
              to="/register"
            >
              Register
            </StyledNavLink>
            <StyledNavLink
              className={(isActive) => (isActive ? 'active' : '')}
              to="/login"
            >
              Login
            </StyledNavLink>
          </>
        )}
        {user && (
          <>
            <StyledNavLink
              className={(isActive) => (isActive ? 'active' : '')}
              to="/dashboard"
            >
              Dashboard
            </StyledNavLink>
            <StyledNavLink
              className={(isActive) => (isActive ? 'active' : '')}
              to="/logout"
            >
              Logout
            </StyledNavLink>
          </>
        )}
      </Nav>
    </Wrapper>
  );
}
