import styled from 'styled-components';

const Main = styled.main`
  text-align: center;
`;
const Paragraph = styled.p`
  font-size: 1.6rem;
  line-height: 4rem;
`;
const SadParagraph = styled(Paragraph)`
  font-size: 3rem;
`;

export function NotFound() {
  return (
    <Main>
      <h2>Not Found?</h2>
      <Paragraph>
        It seems like I can't help you, weird..
      </Paragraph>
      <SadParagraph>😓</SadParagraph>
    </Main>
  );
}
