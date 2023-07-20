import { useRecoilState, useRecoilValue } from 'recoil';
import { eventsSelector } from './';
import styled from 'styled-components';
import { useIntl } from 'react-intl';
import { Header, Loader, Select, timeFromNow } from '@wdid/shared';
import { eventListParamtersAtom } from './eventListParamtersAtom';
import { Suspense } from 'react';

const Wrapper = styled.div`
  margin: ${({ theme }) => `${theme.spacing.xl} 0`};
`;
const Filters = styled.div`
  padding: ${({ theme }) => `${theme.spacing.s} ${theme.spacing.m}`};
  background-color: ${({ theme }) => theme.palette.backgroundHighlight.main};
  color: ${({ theme }) => theme.palette.backgroundHighlight.contrastText};
  border-radius: ${({ theme }) => theme.borderRadius};
  border-bottom: 2px solid ${({ theme }) => theme.palette.background.main};
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
`;
const Fieldset = styled.fieldset`
  border: none;
`;
const Label = styled.label`
  margin-right: ${({ theme }) => theme.spacing.s};
`;
const ListWrapper = styled.div`
  background-color: ${({ theme }) => theme.palette.backgroundHighlight.main};
  color: ${({ theme }) => theme.palette.backgroundHighlight.contrastText};
  border-radius: ${({ theme }) => theme.borderRadius};
  border-top-left-radius: 0;
  border-top-right-radius: 0;
  display: flex;
  flex-direction: row;
  justify-content: center;
`;
const StyledLoader = styled(Loader)`
  margin: ${({ theme }) => theme.spacing.xl};
`;
const List = styled.div`
  flex-grow: 1;
`;
const Item = styled.div`
  padding: ${({ theme }) => theme.spacing.m};
  border-bottom: 2px solid ${({ theme }) => theme.palette.background.main};
  &:last-child {
    border: none;
  }
`;
const Title = styled.p`
  font-size: 1.1rem;
  font-weight: bold;
`;
const Time = styled.p`
  font-size: 0.8rem;
`;
const Description = styled.p``;

const LogList = () => {
  const events = useRecoilValue(eventsSelector);
  const intl = useIntl();

  return (
    <List>
      {events.result?.map(({ id, title, date, description }) => (
        <Item key={id}>
          <Title>{title}</Title>
          <Time>{timeFromNow(new Date(date), intl)}</Time>
          <Description>{description}</Description>
        </Item>
      ))}
    </List>
  );
};

export const Log = () => {
  const [parameters, setParameters] = useRecoilState(eventListParamtersAtom);
  const options = [
    { value: '10', title: '10' },
    { value: '20', title: '20' },
    { value: '30', title: '30' },
    { value: '50', title: '50' },
    { value: '100', title: '100' },
    { value: '200', title: '200' },
  ];

  const updateLimit = (limit: string) => {
    setParameters({
      ...parameters,
      limit: parseInt(limit, 10),
    });
  };

  return (
    <Wrapper>
      <Header size="H3">Last Events</Header>
      <Filters>
        <Fieldset>
          <Label>Limit</Label>
          <Select
            value={parameters.limit.toString()}
            options={options}
            onChange={updateLimit}
          />
        </Fieldset>
      </Filters>
      <ListWrapper>
        <Suspense fallback={<StyledLoader partial />}>
          <LogList />
        </Suspense>
      </ListWrapper>
    </Wrapper>
  );
};
