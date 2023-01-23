import { useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';
import Button from '../../components/Button';
import Checkbox from '../../components/Checkbox';
import TextInput from '../../components/TextInput';
import isEmail from '../../utils/isEmail';
import useAsyncError from '../../utils/useAsyncError';
import { useDebounce } from '../hooks/useDebounce';
import { ApplicationService } from '../User/ApplicationService';
import { UserService } from '../User/UserService';

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
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [domain, setDomain] = useState('');
  const [domainVerified, setDomainVerified] = useState(false);
  const [requiredLoginProvider, setRequiredLoginProvider] = useState<
    string | null
  >(null);

  const userService = useMemo(() => new UserService(), []);
  const applicationService = useMemo(() => new ApplicationService(), []);

  const checkDomain = useDebounce(async () => {
    const requirements = await applicationService.domainRequirements(domain);
    setRequiredLoginProvider(
      requirements.result?.requiredOidcProviderId ?? null
    );
    setDomainVerified(true);
  }, 500);

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

  useEffect(() => {
    if (isEmail(email)) {
      const currentDomain = email.split('@').pop() as string;
      if (currentDomain !== domain) {
        setDomain(currentDomain);
        setDomainVerified(false);
      }
    }
  }, [email, domain]);
  useEffect(() => {
    if (domain && !domainVerified) {
      checkDomain();
    }
  }, [domain, checkDomain, domainVerified]);

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
        isDisabled={Boolean(requiredLoginProvider)}
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
        isDisabled={!isEmail(email) || !Boolean(password) || !domainVerified}
        isLoading={isLoading}
        isAsync
      >
        {requiredLoginProvider ? 'Login with SSO' : 'Login'}
      </Submit>
    </Form>
  );
}
