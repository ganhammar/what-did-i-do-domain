import { Component, ErrorInfo, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

const Wrapper = styled.div`
  text-align: center;
`;

const Icon = styled.div`
  font-size: 7rem;
`;

export class ErrorBoundry extends Component<Props, State> {
  public state: State = {
    hasError: false,
  };

  public static getDerivedStateFromError(_: Error): State {
    return { hasError: true };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Uncaught error:', error, errorInfo);
  }

  public render() {
    if (this.state.hasError) {
      return (
        <Wrapper>
          <h2>Sorry, this wasn't expected..</h2>
          <Icon>😔</Icon>
          <Link to={-1 as any}>Go back...</Link>
        </Wrapper>
      );
    }

    return this.props.children;
  }
}
