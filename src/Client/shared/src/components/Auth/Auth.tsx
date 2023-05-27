import { useCallback, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useRecoilValue } from 'recoil';
import { userManager } from './userManager';
import { currentUserAtom } from './currentUserAtom';
import { Loader } from '../Loader';

interface State {
  from: string;
}

interface LoginProps {
  defaultView: string;
}

interface Props extends LoginProps {
  children: JSX.Element;
}

function RenderIfLoggedIn({ children, defaultView }: Props) {
  const user = useRecoilValue(currentUserAtom);
  const { pathname } = useLocation();
  const navigate = useNavigate();

  const login = useCallback(() => {
    new Promise((resolve, reject) => {
      if (!user || user.expired === true) {
        reject(new Error('login_required'));
      } else {
        userManager.signinSilent()
          .then(resolve)
          .catch(reject);
      }
    }).then(() => {
      if (pathname === '/login') {
        navigate(defaultView);
      } else {
        navigate(pathname);
      }
    }).catch(({ error, message }) => {
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
    login();
  }, [login]);

  if (user && children) {
    return children;
  }

  return <Loader />;
}

function LoginCallback({ defaultView }: LoginProps) {
  const navigate = useNavigate();

  useEffect(() => {
    userManager.signinRedirectCallback()
      .then((response) => {
        navigate((response.state as State).from || defaultView);
      });
  }, [navigate, defaultView]);

  return <Loader />;
}

function LogoutRedirect() {
  const navigate = useNavigate();

  useEffect(() => {
    userManager.getUser()
      .then((result) => {
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

export function Auth({ children, defaultView }: Props) {
  const { pathname } = useLocation();
  console.log(pathname);

  switch(pathname) {
    case '/login/callback':
      console.log('/login/callback');
      return <LoginCallback defaultView={defaultView} />;
    case '/logout/callback':
      console.log('/logout/callback');
      return <LogoutCallbackRedirect />;
    case '/logout':
      console.log('/logout');
      return <LogoutRedirect />;
    default:
      console.log('default');
      return (
        <RenderIfLoggedIn defaultView={defaultView}>
          {children}
        </RenderIfLoggedIn>
      );
  }
}
