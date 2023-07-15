import { useCallback, useEffect, useRef } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useRecoilValueLoadable, useSetRecoilState } from 'recoil';
import { userManager } from './userManager';
import { currentUserAtom } from './currentUserAtom';
import { Loader } from '../Loader';

interface State {
  from: string;
}

interface LoginProps {
  defaultView: string;
}

interface LoginPropsWithChildren extends LoginProps {
  children: JSX.Element;
}

interface Props extends LoginPropsWithChildren {
  pathPrefix: string;
}

function RenderIfLoggedIn({ children, defaultView }: LoginPropsWithChildren) {
  const initialized = useRef(false); // Avoid running twice in dev (strictMode)
  const user = useRecoilValueLoadable(currentUserAtom);
  const { pathname } = useLocation();
  const navigate = useNavigate();

  const login = useCallback(() => {
    new Promise((resolve, reject) => {
      const fiveMinuteAgo = new Date(
        new Date().getTime() - 5 * 60000
      ).getTime();

      if (!user.contents || user.contents.expired === true) {
        reject(new Error('login_required'));
      } else if (
        new Date(user.contents.expires_at * 1000).getTime() < fiveMinuteAgo
      ) {
        userManager.signinSilent().then(resolve).catch(reject);
      }
    })
      .then(() => {
        if (pathname === '/login') {
          navigate(defaultView);
        } else {
          navigate(pathname);
        }
      })
      .catch(({ error, message }) => {
        if (error === 'login_required' || message === 'login_required') {
          userManager.signinRedirect({
            state: { from: pathname || defaultView },
          });
        } else {
          throw new Error(message);
        }
      });
  }, [user, defaultView, pathname, navigate]);

  useEffect(() => {
    if (user.state === 'hasValue' && !initialized.current) {
      initialized.current = true;
      login();
    }
  }, [login, user]);

  if (user.state === 'hasValue' && user.contents?.expired === false) {
    return children;
  }

  return <Loader />;
}

function LoginCallback({ defaultView }: LoginProps) {
  const initialized = useRef(false); // Avoid running twice in dev (strictMode)
  const setCurrentUser = useSetRecoilState(currentUserAtom);
  const navigate = useNavigate();

  useEffect(() => {
    if (!initialized.current) {
      initialized.current = true;

      userManager.signinRedirectCallback().then(async (response) => {
        const user = await userManager.getUser();

        if (user) {
          setCurrentUser(user);
        }

        navigate((response.state as State).from || defaultView);
      });
    }
  }, [navigate, defaultView, setCurrentUser]);

  return <Loader />;
}

function LogoutRedirect() {
  const navigate = useNavigate();

  useEffect(() => {
    userManager.getUser().then((result) => {
      if (result && result.expired === false) {
        userManager.signoutRedirect();
      } else {
        navigate('/');
      }
    });
  }, [navigate]);

  return <Loader />;
}

function LogoutCallbackRedirect() {
  const navigate = useNavigate();

  useEffect(() => {
    userManager.signoutRedirectCallback().then(() => {
      navigate('/');
    });
  }, [navigate]);

  return <Loader />;
}

export function Auth({ children, defaultView, pathPrefix }: Props) {
  const { pathname } = useLocation();

  switch (pathname.replace(`/${pathPrefix}`, '')) {
    case '/login/callback':
      return <LoginCallback defaultView={defaultView} />;
    case '/logout/callback':
      return <LogoutCallbackRedirect />;
    case '/logout':
      return <LogoutRedirect />;
    default:
      return (
        <RenderIfLoggedIn defaultView={defaultView}>
          {children}
        </RenderIfLoggedIn>
      );
  }
}
