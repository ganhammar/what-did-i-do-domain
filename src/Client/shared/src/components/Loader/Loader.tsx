import styled, { keyframes } from 'styled-components';

const LoadingIndicatorWrapper = styled.div<{ partial?: boolean }>`
  ${({ partial }) =>
    partial !== true &&
    `
    position: fixed;
    top: 50%;
    left: 50%;
    margin-left: -20px;
    margin-top: -20px;
  `}
  display: flex;
  flex-direction: row;
`;

const fade = keyframes`
  0% {
    -webkit-transform: scale(0);
  }
  100% {
    -webkit-transform: scale(1.0);
    opacity: 0;
  }
`;

const LoadingIndicator = styled.div<{ size: 'small' | 'normal' }>`
  width: ${({ size }) => (size === 'normal' ? '40px' : '24px')};
  height: ${({ size }) => (size === 'normal' ? '40px' : '24px')};
  border-radius: ${({ size }) => (size === 'normal' ? '20px' : '12px')};
  background-color: ${({ theme }) => theme.palette.primary.main};
  animation: ${fade} 1s infinite ease-in-out;
`;

interface LoaderProps {
  partial?: boolean;
  size?: 'small' | 'normal';
  className?: string;
}

export function Loader({ partial, className, size }: LoaderProps) {
  return (
    <LoadingIndicatorWrapper partial={partial} className={className}>
      <LoadingIndicator size={size ?? 'normal'} />
    </LoadingIndicatorWrapper>
  );
}
