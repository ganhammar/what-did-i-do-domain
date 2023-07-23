import { useRecoilValue, useResetRecoilState, useSetRecoilState } from 'recoil';
import { eventsAtom } from './';
import styled from 'styled-components';
import { useIntl } from 'react-intl';
import { Loader, Select, timeFromNow } from '@wdid/shared';
import { eventListParamtersAtom } from './eventListParamtersAtom';
import { Suspense, useEffect, useState } from 'react';
import { Tag } from './Tag';

const Filters = styled.div`
  padding: ${({ theme }) => `${theme.spacing.s} ${theme.spacing.m}`};
  background-color: ${({ theme }) => theme.palette.paper.main};
  color: ${({ theme }) => theme.palette.paper.contrastText};
  border-radius: ${({ theme }) => theme.borderRadius};
  border-bottom: 2px solid ${({ theme }) => theme.palette.background.main};
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: ${({ theme }) => theme.spacing.s};
`;
const ListWrapper = styled.div`
  background-color: ${({ theme }) => theme.palette.paper.main};
  color: ${({ theme }) => theme.palette.paper.contrastText};
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
const Tags = styled.div`
  display: flex;
  flex-direction: row;
  margin-top: ${({ theme }) => theme.spacing.xs};
`;

const LogList = () => {
  const events = useRecoilValue(eventsAtom);
  const intl = useIntl();

  return (
    <List>
      {(events.result?.length ?? 0) > 0 &&
        events.result?.map(({ id, title, date, description, tags }) => (
          <Item key={id}>
            <Title>{title}</Title>
            <Time>{timeFromNow(new Date(date), intl)}</Time>
            <Description>{description}</Description>
            {tags && (
              <Tags>
                {tags.map((tag) => (
                  <Tag key={tag}>{tag}</Tag>
                ))}
              </Tags>
            )}
          </Item>
        ))}
      {events.result?.length === 0 && (
        <Item>
          <em>No events during the selected period</em>
        </Item>
      )}
    </List>
  );
};

export const Log = () => {
  const setParameters = useSetRecoilState(eventListParamtersAtom);
  const reset = useResetRecoilState(eventsAtom);
  const [timePeriod, setTimePeriod] = useState('day');
  const [limit, setLimit] = useState('20');
  const limitOpptions = [
    { value: '10', title: '10' },
    { value: '20', title: '20' },
    { value: '30', title: '30' },
    { value: '50', title: '50' },
    { value: '100', title: '100' },
    { value: '200', title: '200' },
  ];
  const timePeriodOptions = [
    { value: 'day', title: 'Day' },
    { value: 'two-days', title: 'Two days' },
    { value: 'three-days', title: 'Three days' },
    { value: 'week', title: 'Week' },
    { value: 'month', title: 'Month' },
  ];

  useEffect(() => {
    const to = new Date();
    const from = new Date();

    switch (timePeriod) {
      case 'day':
        from.setDate(to.getDate() - 1);
        break;
      case 'two-days':
        from.setDate(to.getDate() - 2);
        break;
      case 'three-days':
        from.setDate(to.getDate() - 3);
        break;
      case 'week':
        from.setDate(to.getDate() - 7);
        break;
      case 'month':
        from.setDate(to.getDate() - 30);
        break;
    }

    setParameters({
      limit: parseInt(limit, 10),
      fromDate: from.toISOString(),
      toDate: to.toISOString(),
    });
    reset();
  }, [timePeriod, setParameters, limit, reset]);

  return (
    <>
      <Filters>
        <Select
          value={limit}
          options={limitOpptions}
          onChange={setLimit}
          label="Limit"
        />
        <Select
          value={timePeriod}
          options={timePeriodOptions}
          onChange={setTimePeriod}
          label="Show data for last"
        />
      </Filters>
      <ListWrapper>
        <Suspense fallback={<StyledLoader partial />}>
          <LogList />
        </Suspense>
      </ListWrapper>
    </>
  );
};
