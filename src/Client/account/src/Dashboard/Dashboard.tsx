import { useRecoilValue } from 'recoil';
import { Navigate } from 'react-router-dom';
import styled from 'styled-components';
import { Header, Modal } from '@wdid/shared';
import { currentAccountAtom } from '../Account';
import { Create, Log } from 'src/Event';
import { useState } from 'react';

const Wrapper = styled.div`
  margin: ${({ theme }) => `${theme.spacing.xl} 0`};
`;
const HeaderWrapper = styled.div`
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  justify-content: space-between;
`;
const Button = styled.div`
  height: 50px;
  width: 50px;
  border-radius: 25px;
  background-color: ${({ theme }) => theme.palette.success.main};
  cursor: pointer;
  text-align: center;
  &:hover {
    box-shadow: 1px 1px 3px rgba(0, 0, 0, 0.3) inset;
    opacity: 0.8;
  }
`;
const Svg = styled.svg`
  margin-top: 5px;
`;

export function Dashboard() {
  const account = useRecoilValue(currentAccountAtom);
  const [isCreating, setIsCreating] = useState(false);

  if (!account) {
    return <Navigate to="/account/select" />;
  }

  const toggle = () => setIsCreating(!isCreating);

  return (
    <>
      <Header size="H2">Welcome! 👋</Header>
      <p>Showing data for the account "{account.name}".</p>
      <Wrapper>
        <HeaderWrapper>
          <Header size="H3">Last Events</Header>
          <Button onClick={toggle}>
            <Svg
              xmlns="http://www.w3.org/2000/svg"
              width="40px"
              height="40px"
              viewBox="0 0 24 24"
              fill="none"
            >
              <g>
                <path
                  d="M6 12H12M12 12H18M12 12V18M12 12V6"
                  stroke="#f9f9f9"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </g>
            </Svg>
          </Button>
        </HeaderWrapper>
        <Log />
      </Wrapper>
      <Modal isOpen={isCreating} onClose={toggle}>
        <Create onCreate={toggle} />
      </Modal>
    </>
  );
}
