import { UserManager } from 'oidc-client-ts';

export const userManager = new UserManager({
  client_id: process.env.REACT_APP_CLIENT_ID ?? '',
  authority: process.env.REACT_APP_AUTH_URL ?? '',
  redirect_uri: `${process.env.REACT_APP_BASE_URL}/login/callback`,
  post_logout_redirect_uri: `${process.env.REACT_APP_BASE_URL}/logout/callback`,
  response_type: 'code',
  scope: `openid profile email ${process.env.REACT_APP_SCOPES ?? ''}`,
  stopCheckSessionOnError: false,
  automaticSilentRenew: true,
  silent_redirect_uri: `${process.env.REACT_APP_BASE_URL}/login/silent-renew`,
});
