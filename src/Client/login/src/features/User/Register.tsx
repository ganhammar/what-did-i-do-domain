import { useState } from 'react';
import styled from 'styled-components';
import { Button, TextInput, isEmail, useAsyncError } from '@wdid/shared';
import { UserService } from './';
import { useNavigate } from 'react-router-dom';

const MIN_PASSWORD_LENGTH = 8;
const RETURN_URL = '/dashboard';

const Form = styled.form`
  display: flex;
  flex-direction: column;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: 0.5rem;
`;

export function Register() {
  const throwError = useAsyncError();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');

  const userService = new UserService();
  const submit = async () => {
    try {
      setIsLoading(true);

      await userService.register({
        email,
        password,
        returnUrl: `${window.location.origin}${RETURN_URL}`,
      });

      setIsLoading(false);

      navigate('/account/dashboard');
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
        hasError={Boolean(password) && password.length < MIN_PASSWORD_LENGTH}
        errorTip="At least eight characters"
      />
      <Submit
        color="success"
        onClick={submit}
        isDisabled={!isEmail(email) || password.length < MIN_PASSWORD_LENGTH}
        isLoading={isLoading}
        isAsync
      >
        Register
      </Submit>
    </Form>
  );
}
