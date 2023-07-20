import { useRecoilValue } from 'recoil';
import { Navigate } from 'react-router-dom';
import { currentAccountAtom } from '../Account';
import { Log } from 'src/Event';

export function Dashboard() {
  const account = useRecoilValue(currentAccountAtom);

  if (!account) {
    return <Navigate to="/account/select" />;
  }

  return (
    <>
      <h2>Welcome! 👋</h2>
      <p>Showing data for the account "{account.name}"</p>
      <Log />
    </>
  );
}
