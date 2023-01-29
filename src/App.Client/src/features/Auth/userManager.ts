import { UserManager } from 'oidc-client-ts';

const userManager = new UserManager({
  client_id: process.env.REACT_APP_CLIENT_ID as string,
  authority: process.env.REACT_APP_AUTH_URL as string,
  redirect_uri: `${window.location.origin}/signin/callback`,
  post_logout_redirect_uri: `${window.location.origin}/signout/callback`,
  response_type: 'code',
  scope: 'openid profile email',
  stopCheckSessionOnError: false,
  automaticSilentRenew: true,
  silent_redirect_uri: `${window.location.origin}/signin/silent-renew`,
});

export default userManager;
