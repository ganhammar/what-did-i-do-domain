import { useRecoilValue, useResetRecoilState, useSetRecoilState } from 'recoil';
import { LogList, eventsAtom } from '.';
import styled from 'styled-components';
import { Button, Header, Loader, Modal, Select } from '@wdid/shared';
import { eventListParamtersAtom } from './eventListParamtersAtom';
import { Suspense, useEffect, useState } from 'react';
import { tagsAtom } from 'src/Tag';
import {
  addDays,
  formatISO,
  startOfDay,
  startOfMonth,
  startOfWeek,
} from 'date-fns';
import { DateTimePicker } from 'src/Components';

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
const CustomInfo = styled.p`
  white-space: nowrap;
  font-size: 0.8em;
  font-style: italic;
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
const Submit = styled(Button)`
  float: right;
`;

export const Log = () => {
  const existingTags = useRecoilValue(tagsAtom);
  const setParameters = useSetRecoilState(eventListParamtersAtom);
  const reset = useResetRecoilState(eventsAtom);
  const [timePeriod, setTimePeriod] = useState('today');
  const [limit, setLimit] = useState('20');
  const [tag, setTag] = useState<string>('');
  const [showDateSelect, setShowDateSelect] = useState(false);
  const [startDate, setStartDate] = useState<Date | null>(null);
  const [endDate, setEndDate] = useState<Date | null>(null);
  const limitOpptions = [
    { value: '10', title: '10' },
    { value: '20', title: '20' },
    { value: '30', title: '30' },
    { value: '50', title: '50' },
  ];
  const timePeriodOptions = [
    { value: 'today', title: 'Today' },
    { value: 'yesterday', title: 'Yesterday' },
    { value: 'last-three-days', title: 'Last three days' },
    { value: 'this-week', title: 'This week' },
    { value: 'this-month', title: 'This month' },
    { value: 'last-thirty-days', title: 'Last thirty days' },
    { value: 'custom', title: 'Custom' },
  ];

  const onTimePeriodChange = (value: string | string[]) => {
    setTimePeriod(value as string);

    if (value === 'custom') {
      setShowDateSelect(true);
    } else {
      setStartDate(null);
      setEndDate(null);
    }
  };

  useEffect(() => {
    let to = new Date();
    let from = new Date();

    switch (timePeriod) {
      case 'today':
        from = startOfDay(from);
        break;
      case 'yesterday':
        to = startOfDay(to);
        from = startOfDay(addDays(from, -1));
        break;
      case 'last-three-days':
        from = startOfDay(addDays(from, -2));
        break;
      case 'this-week':
        from = startOfWeek(from);
        break;
      case 'this-month':
        from = startOfMonth(from);
        break;
      case 'last-thirty-days':
        from = addDays(from, -30);
        break;
      case 'custom':
        return;
    }

    setParameters({
      limit: parseInt(limit, 10),
      fromDate: from.toISOString(),
      toDate: to.toISOString(),
      tag: tag,
    });
    reset();
  }, [timePeriod, setParameters, limit, tag, reset]);

  const onCustomDatesChange = ([start, end]: [Date | null, Date | null]) => {
    setStartDate(start);
    setEndDate(end);
  };

  const onSubmitCustomDates = () => {
    setShowDateSelect(false);

    if (startDate && endDate) {
      setParameters({
        limit: parseInt(limit, 10),
        fromDate: startDate.toISOString(),
        toDate: endDate.toISOString(),
        tag: tag,
      });
      reset();
    }
  };

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
          onChange={onTimePeriodChange}
          label="Show data for"
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
      {startDate && endDate && (
        <Filters>
          <CustomInfo>
            Showing data between{' '}
            {`${formatISO(startDate, {
              representation: 'date',
            })} and ${formatISO(endDate, { representation: 'date' })}`}
          </CustomInfo>
        </Filters>
      )}
      <ListWrapper>
        <Suspense fallback={<StyledLoader partial />}>
          <LogList />
        </Suspense>
      </ListWrapper>
      <Modal isOpen={showDateSelect} onClose={() => setShowDateSelect(false)}>
        <Header size="H3">Show events between</Header>
        <DateTimePicker
          selectsRange
          startDate={startDate}
          endDate={endDate}
          onRangeChange={onCustomDatesChange}
        />
        <Submit
          color="success"
          onClick={onSubmitCustomDates}
          isDisabled={
            !Boolean(startDate) || !Boolean(endDate) || startDate! >= endDate!
          }
        >
          Show Events
        </Submit>
      </Modal>
    </>
  );
};
