import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRecoilState, useRecoilValue } from 'recoil';
import { accountsSelector, currentAccountAtom } from 'src/Dashboard';
import styled from 'styled-components';

const List = styled.div`
  margin: 2rem 0;
`;
const Item = styled.div`
  line-height: 3rem;
  cursor: pointer;
  border-bottom: 1px solid ${({ theme }) => theme.palette.divider.main};
  &:hover {
    color: ${({ theme }) => theme.palette.primary.main};
    border-bottom: 2px solid ${({ theme }) => theme.palette.primary.main};
    margin-bottom: -1px;
  }
`;

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

  const selectAccount = (account: Account) => {
    setCurrentAccount(account);
    navigate('/account/dashboard');
  };

  return (
    <>
      <h2>Select Account</h2>
      <p>
        Seems like you have access to more than one account, which account do
        you want to use?
      </p>
      <List>
        {accounts.result?.map((account) => (
          <Item onClick={() => selectAccount(account)} key={account.id}>
            {account.name}
          </Item>
        ))}
      </List>
    </>
  );
}
