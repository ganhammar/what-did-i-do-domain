import { useRecoilValue } from 'recoil';
import { eventsSelector } from './';
import styled from 'styled-components';
import { useIntl } from 'react-intl';
import { timeFromNow } from '@wdid/shared';

const Wrapper = styled.div`
  margin: 2rem 0;
`;
const Header = styled.h3`
  margin-bottom: 1rem;
`;
const List = styled.div`
  border: 1px solid ${({ theme }) => theme.palette.divider.main};
  border-radius: ${({ theme }) => theme.borderRadius};
`;
const Item = styled.div`
  padding: 1rem;
  border-bottom: 1px solid ${({ theme }) => theme.palette.divider.main};
  &:last-child {
    border: none;
  }
`;
const Title = styled.p`
  font-weight: bold;
`;
const Time = styled.p`
  font-size: 0.8rem;
`;
const Description = styled.p``;

export const Log = () => {
  const events = useRecoilValue(eventsSelector);
  const intl = useIntl();

  return (
    <Wrapper>
      <Header>Last Events</Header>
      <List>
        {events.result?.map(({ id, title, date, description }) => (
          <Item key={id}>
            <Title>{title}</Title>
            <Time>{timeFromNow(new Date(date), intl)}</Time>
            <Description>{description}</Description>
          </Item>
        ))}
      </List>
    </Wrapper>
  );
};
