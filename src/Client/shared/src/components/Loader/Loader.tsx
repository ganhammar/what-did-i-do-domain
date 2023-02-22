import styled, { keyframes } from 'styled-components';

const LoadingIndicatorWrapper = styled.div`
  position: fixed;
  top: 50%;
  left: 50%;
  margin-left: -20px;
  margin-top: -20px;
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

const LoadingIndicator = styled.div`
  width: 40px;
  height: 40px;
  border-radius: 20px;
  background-color: ${({ theme }) => theme.palette.primary.main};
  animation: ${fade} 1.0s infinite ease-in-out;
`;

export function Loader() {
  return (
    <LoadingIndicatorWrapper>
      <LoadingIndicator />
    </LoadingIndicatorWrapper>
  );
}
