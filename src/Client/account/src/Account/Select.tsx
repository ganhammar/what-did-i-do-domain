import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRecoilState, useRecoilValue } from 'recoil';
import accountsSelector from 'src/Dashboard/accountsSelector';
import { currentAccountAtom } from 'src/Dashboard/currentAccountAtom';

export function Select() {
  const navigate = useNavigate();
  const [currentAccount, setCurrentAccount] =
    useRecoilState(currentAccountAtom);
  const accounts = useRecoilValue(accountsSelector);

  useEffect(() => {
    if (currentAccount) {
      navigate('/account/dashboard');
    } else if (accounts.result?.length === 0) {
      navigate('/account/create');
    } else if (accounts.result?.length === 1) {
      setCurrentAccount(accounts.result[0]);
    }
  }, [accounts, currentAccount, setCurrentAccount, navigate]);

  return <>Select Account</>;
}
