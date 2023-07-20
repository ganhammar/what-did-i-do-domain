import { Button, TextInput, useAsyncError } from '@wdid/shared';
import { useEffect, useState } from 'react';
import { useRecoilRefresher_UNSTABLE, useRecoilValue } from 'recoil';
import styled from 'styled-components';
import { accountServiceSelector, accountsSelector } from './';
import { currentUserAtom } from '@wdid/shared/src/components/Auth/currentUserAtom';
import { useNavigate } from 'react-router-dom';

const Form = styled.form`
  display: flex;
  flex-direction: column;
  margin-top: 2rem;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: 0.5rem;
`;

const ACCOUNT_NAME_MIN_LENGTH = 3;

export function Create() {
  const throwError = useAsyncError();
  const navigate = useNavigate();
  const accounts = useRecoilValue(accountsSelector);
  const refresh = useRecoilRefresher_UNSTABLE(accountsSelector);
  const user = useRecoilValue(currentUserAtom);
  const [isLoading, setIsLoading] = useState(false);
  const [name, setName] = useState<string>('');

  const accountService = useRecoilValue(accountServiceSelector);
  const submit = async () => {
    try {
      setIsLoading(true);

      await accountService.create(name);

      refresh();
    } catch (error) {
      throwError(error);
    }
  };

  useEffect(() => {
    if (accounts.result?.length) {
      navigate('/account/select');
    }
  }, [accounts, navigate]);

  useEffect(() => {
    if (user && user.profile.email) {
      setName(user.profile.email.split('@')[0]);
    }
  }, [user]);

  return (
    <>
      <h2>Create Account</h2>
      <p>Seems like you don't have an account, let's create one!</p>
      <Form>
        <TextInput
          title="Name"
          type="text"
          value={name}
          onChange={setName}
          hasError={name.length < ACCOUNT_NAME_MIN_LENGTH}
          errorTip="The name must have at least three characters"
        />
        <Submit
          color="success"
          onClick={submit}
          isDisabled={name.length < ACCOUNT_NAME_MIN_LENGTH}
          isLoading={isLoading}
          isAsync
        >
          Create
        </Submit>
      </Form>
    </>
  );
}
