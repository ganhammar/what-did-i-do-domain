import { DefaultTheme } from 'styled-components';

export const appTheme: DefaultTheme = {
  borderRadius: '6px',
  palette: {
    common: {
      black: '#333333',
      white: '#f9f9f9',
    },
    background: {
      main: '#eee',
      contrastText: '#333333',
    },
    paper: {
      main: '#f4f4f4',
      contrastText: '#444444',
    },
    paperHighlight: {
      main: '#f7f7f7',
      contrastText: '#444444',
    },
    primary: {
      main: '#5e50a1',
      contrastText: '#f9f9f9',
    },
    secondary: {
      main: '#9e2168',
      contrastText: '#f9f9f9',
    },
    divider: {
      main: '#bbb',
      contrastText: '#fff',
    },
    warning: {
      main: '#e52129',
      contrastText: '#f9f9f9',
    },
    success: {
      main: '#41a949',
      contrastText: '#f9f9f9',
    },
  },
  typography: {
    fontFamily: 'Roboto, "Helvetica Neue", sans-serif',
    fontSize: '20px',
    lineHeight: '1.6',
    h1: '4rem',
    h2: '2rem',
    h3: '1.6rem',
  },
  spacing: {
    xxs: '4px',
    xs: '8px',
    s: '16px',
    m: '24px',
    l: '32px',
    xl: '40px',
  },
  shadows: [
    'none',
    'rgb(0 0 0 / 25%) 0px 2px 1px -1px, rgb(0 0 0 / 19%) 0px 1px 1px 0px, rgb(0 0 0 / 17%) 0px 1px 3px 0px;',
    'rgb(0 0 0 / 25%) 0px 3px 1px -2px, rgb(0 0 0 / 19%) 0px 2px 2px 0px, rgb(0 0 0 / 17%) 0px 1px 5px 0px;',
    'rgb(0 0 0 / 17%) 0px 3px 3px -2px, rgb(0 0 0 / 15%) 0px 3px 4px 0px, rgb(0 0 0 / 13%) 0px 1px 8px 0px;',
  ],
};
