// styled.d.ts
import 'styled-components';

interface IPalette {
  main: string;
  contrastText: string;
}

declare module 'styled-components' {
  export interface DefaultTheme {
    borderRadius: string;
    palette: {
      common: {
        black: string;
        white: string;
      };
      background: IPalette;
      backgroundHighlight: IPalette;
      primary: IPalette;
      secondary: IPalette;
      divider: IPalette;
      success: IPalette;
      warning: IPalette;
    };
    typography: {
      fontFamily: string;
      fontSize: string;
      lineHeight: string;
      h1: string;
      h2: string;
      h3: string;
    };
    spacing: {
      xs: string;
      s: string;
      m: string;
      l: string;
      xl: string;
    };
    shadows: [string, string, string, string];
  }
}
