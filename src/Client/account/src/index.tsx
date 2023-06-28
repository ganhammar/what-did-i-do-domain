import React from 'react';
import ReactDOM from 'react-dom/client';
import { App } from './App';
import { reportWebVitals } from '@wdid/shared';
import { userManager } from '@wdid/shared/src/components/Auth/userManager';

if (window.location.pathname.replace(`/account`, '') === '/login/silent-renew') {
  userManager.signinSilentCallback();
} else {
  const root = ReactDOM.createRoot(
    document.getElementById('root') as HTMLElement
  );
  root.render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );

  // If you want to start measuring performance in your app, pass a function
  // to log results (for example: reportWebVitals(console.log))
  // or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
  reportWebVitals();
}
