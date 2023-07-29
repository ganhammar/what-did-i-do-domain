import { useRecoilValue, useResetRecoilState, useSetRecoilState } from 'recoil';
import { LogList, eventsAtom } from './';
import styled from 'styled-components';
import { Loader, Select } from '@wdid/shared';
import { eventListParamtersAtom } from './eventListParamtersAtom';
import { Suspense, useEffect, useState } from 'react';
import { tagsAtom } from 'src/Tag';

const Filters = styled.div`
  padding: ${({ theme }) => `${theme.spacing.s} ${theme.spacing.m}`};
  background-color: ${({ theme }) => theme.palette.paper.main};
  color: ${({ theme }) => theme.palette.paper.contrastText};
  border-radius: ${({ theme }) => theme.borderRadius};
  border-bottom: 2px solid ${({ theme }) => theme.palette.background.main};
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
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
  overflow: hidden;
`;
const StyledLoader = styled(Loader)`
  margin: ${({ theme }) => theme.spacing.xl};
`;

export const Log = () => {
  const existingTags = useRecoilValue(tagsAtom);
  const setParameters = useSetRecoilState(eventListParamtersAtom);
  const reset = useResetRecoilState(eventsAtom);
  const [timePeriod, setTimePeriod] = useState('day');
  const [limit, setLimit] = useState('20');
  const [tag, setTag] = useState<string>('');
  const limitOpptions = [
    { value: '10', title: '10' },
    { value: '20', title: '20' },
    { value: '30', title: '30' },
    { value: '50', title: '50' },
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
      tag: tag,
    });
    reset();
  }, [timePeriod, setParameters, limit, tag, reset]);

  return (
    <>
      <Filters>
        <Select
          value={limit}
          options={limitOpptions}
          onChange={(value) => setLimit(value as string)}
          label="Events per page"
        />
        <Select
          value={timePeriod}
          options={timePeriodOptions}
          onChange={(value) => setTimePeriod(value as string)}
          label="Show data for last"
        />
        <Select
          value={tag}
          options={
            existingTags.result?.map(({ value }) => ({
              value,
              title: value,
            })) ?? []
          }
          label="Filter by tag"
          onChange={(value) => setTag(value as string)}
          allowClear
          condense
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
