import { useMemo, useState } from 'react';
import styled from 'styled-components';
import Button from '../../components/Button';
import Checkbox from '../../components/Checkbox';
import TextInput from '../../components/TextInput';
import isEmail from '../../utils/isEmail';
import useAsyncError from '../../utils/useAsyncError';
import { UserService } from '../User/UserService';

const Form = styled.form`
  display: flex;
  flex-direction: column;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: 0.5rem;
`;

export function Signin() {
  const throwError = useAsyncError();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const userService = useMemo(() => new UserService(), []);

  const submit = async () => {
    try {
      setIsLoading(true);

      const response = await userService.login({
        email,
        password,
        rememberMe,
      });

      console.log(response);
      setIsLoading(false);
    } catch (error) {
      throwError(error);
    }
  };

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
