import { useRecoilValue } from 'recoil';
import { Navigate } from 'react-router-dom';
import { currentAccountAtom } from '../Account';
import { Log } from 'src/Event';
import { Header } from '@wdid/shared';

export function Dashboard() {
  const account = useRecoilValue(currentAccountAtom);

  if (!account) {
    return <Navigate to="/account/select" />;
  }

  return (
    <>
      <Header size="H2">Welcome! 👋</Header>
      <p>Showing data for the account "{account.name}".</p>
      <Log />
    </>
  );
}
