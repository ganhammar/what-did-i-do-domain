import { useEffect, useMemo, useState } from 'react';
import { useRecoilRefresher_UNSTABLE, useRecoilValue } from 'recoil';
import styled from 'styled-components';
import {
  Button,
  Checkbox,
  TextInput,
  isEmail,
  useAsyncError,
} from '@wdid/shared';
import { UserService } from '../User/UserService';
import useUser from './currentUserSelector';

const Form = styled.form`
  display: flex;
  flex-direction: column;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: 0.5rem;
`;

export function Login() {
  const throwError = useAsyncError();
  const user = useRecoilValue(useUser);
  const refresh = useRecoilRefresher_UNSTABLE(useUser);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const params = new URLSearchParams(window.location.search);
  const returnUrl = params.get('ReturnUrl');

  const userService = useMemo(() => new UserService(), []);

  const submit = async () => {
    try {
      setIsLoading(true);

      const response = await userService.login({
        email,
        password,
        rememberMe,
      });

      if (response.success && response.result?.succeeded) {
        if (returnUrl) {
          window.location.href = returnUrl;
        } else {
          refresh();
        }
      } else {
        console.log(response);
      }
    } catch (error) {
      throwError(error);
    }
  };

  useEffect(() => {
    if (user) {
      window.location.href = '/account/dashboard';
    }
  }, [user]);

  return (
    <Form>
      <TextInput
        title="Email"
        type="text"
        value={email}
        onChange={setEmail}
        hasError={Boolean(email) && !isEmail(email)}
        errorTip="Must be a valid email address"
      />
      <TextInput
        title="Password"
        type="password"
        value={password}
        onChange={setPassword}
      />
      <Checkbox
        title="Remember me"
        onChange={() => setRememberMe(!rememberMe)}
        isChecked={rememberMe}
        position="right"
      />
      <Submit
        color="success"
        onClick={submit}
        isDisabled={!isEmail(email) || !Boolean(password)}
        isLoading={isLoading}
        isAsync
      >
        Login
      </Submit>
    </Form>
  );
}
