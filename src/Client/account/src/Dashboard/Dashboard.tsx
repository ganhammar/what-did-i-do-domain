import { useRecoilValue } from 'recoil';
import { Navigate } from 'react-router-dom';
import styled from 'styled-components';
import { Header } from '@wdid/shared';
import { currentAccountAtom } from '../Account';
import { Log } from 'src/Event';

const Wrapper = styled.div`
  margin: ${({ theme }) => `${theme.spacing.xl} 0`};
`;

export function Dashboard() {
  const account = useRecoilValue(currentAccountAtom);

  if (!account) {
    return <Navigate to="/account/select" />;
  }

  return (
    <>
      <Header size="H2">Welcome! 👋</Header>
      <p>Showing data for the account "{account.name}".</p>
      <Wrapper>
        <Header size="H3">Last Events</Header>
        <Log />
      </Wrapper>
    </>
  );
}
