import {
  Button,
  TextInput,
  useAsyncError,
  Header,
  Select,
  SelectOption,
} from '@wdid/shared';
import { useEffect, useState } from 'react';
import { useRecoilValue } from 'recoil';
import DatePicker from 'react-datepicker';
import { currentAccountAtom } from 'src/Account';
import styled from 'styled-components';
import { eventServiceSelector } from './eventServiceSelector';
import { useAddEvent } from './useAddEvent';
import { tagsAtom, useSyncTags } from 'src/Tag';

import 'react-datepicker/dist/react-datepicker.css';

interface CreateProps {
  onCreate: () => void;
}

const Form = styled.form`
  display: flex;
  flex-direction: column;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: ${({ theme }) => theme.spacing.l};
`;
const DatePickerStyles = styled.div`
  box-shadow: ${({ theme }) => theme.shadows[3]};
  border-radius: ${({ theme }) => theme.borderRadius};
  border: none;
  .react-datepicker__header {
    background-color: ${({ theme }) => theme.palette.paperHighlight.main};
    color: ${({ theme }) => theme.palette.paperHighlight.contrastText};
  }
  .react-datepicker-time__header {
    line-height: 63px;
    background-color: ${({ theme }) => theme.palette.paperHighlight.main};
    color: ${({ theme }) => theme.palette.paperHighlight.contrastText};
  }
  .react-datepicker__day--selected,
  .react-datepicker__time-container
    .react-datepicker__time
    .react-datepicker__time-box
    ul.react-datepicker__time-list
    li.react-datepicker__time-list-item--selected {
    background-color: ${({ theme }) => theme.palette.primary.main};
    color: ${({ theme }) => theme.palette.primary.contrastText};
    font-weight: normal;
  }
  .react-datepicker__day--keyboard-selected {
    background-color: ${({ theme }) => theme.palette.secondary.main};
    color: ${({ theme }) => theme.palette.secondary.contrastText};
    font-weight: normal;
  }
  .react-datepicker__day--selected {
    border-radius: ${({ theme }) => theme.borderRadius};
  }
  .react-datepicker__time-container
    .react-datepicker__time
    .react-datepicker__time-box
    ul.react-datepicker__time-list
    li.react-datepicker__time-list-item {
    height: auto;
    padding: ${({ theme }) => theme.spacing.xxs} 0;
  }
  .react-datepicker__navigation--next--with-time:not(
      .react-datepicker__navigation--next--with-today-button
    ) {
    right: 100px;
    top: 10px;
  }
  .react-datepicker__navigation--previous {
    top: 10px;
  }
  .react-datepicker__time-container,
  .react-datepicker__time-container
    .react-datepicker__time
    .react-datepicker__time-box {
    width: 100px;
  }
  .react-datepicker__time-container,
  .react-datepicker__header {
    border-color: ${({ theme }) => theme.palette.divider.main};
  }
`;
const Fieldset = styled.fieldset<{ isFocused: boolean }>`
  border: none;
  position: relative;
  margin-bottom: 1.8rem;
  &:after {
    content: '';
    height: 3px;
    width: 0;
    position: absolute;
    bottom: 0;
    left: 0;
    background-color: ${({ theme: { palette } }) => palette.primary.main};
    z-index: 1;
    transition:
      width 0.5s,
      background-color 0.5s;
  }
  ${({ isFocused }) =>
    isFocused &&
    `
    &:after {
      width: 100%;
    }
  `}
  .react-datepicker-wrapper {
    width: 100%;
  }
  .react-datepicker-popper {
    padding-top: ${({ theme }) => theme.spacing.xs};
    z-index: 10;
  }
`;
const Label = styled.label<{ isFocused: boolean; hasValue: boolean }>`
  position: absolute;
  left: ${({ theme }) => theme.spacing.xs};
  top: 24px;
  transition:
    top 0.5s,
    font-size 0.5s;
  ${({ isFocused, hasValue }) =>
    (isFocused || hasValue) &&
    `
    top: 0;
    font-size: 0.8rem;
  `}
`;
const Input = styled.input`
  border: none;
  border-bottom: 1px solid ${({ theme: { palette } }) => palette.divider.main};
  background: none;
  padding: ${({ theme }) =>
    `0 ${theme.spacing.xs} ${theme.spacing.xs} ${theme.spacing.xs}`};
  width: 100%;
  margin: 1.1rem 0 0 0;
  transition: border-bottom-color 0.5s;
  &:focus {
    outline: none;
  }
`;

const TITLE_MIN_LENGTH = 3;

export const Create = ({ onCreate }: CreateProps) => {
  const throwError = useAsyncError();
  const account = useRecoilValue(currentAccountAtom);
  const eventService = useRecoilValue(eventServiceSelector);
  const existingTags = useRecoilValue(tagsAtom);
  const addEvent = useAddEvent();
  const syncTags = useSyncTags();
  const [tagOptions, setTagOptions] = useState<SelectOption[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [dateIsFocused, setDateIsFocused] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [date, setDate] = useState<Date | null>(new Date());

  useEffect(() => {
    if (existingTags.result?.length) {
      setTagOptions(
        existingTags.result.map(({ value }) => ({
          value,
          title: value,
        }))
      );
    }
  }, [existingTags]);

  const submit = async () => {
    try {
      setIsLoading(true);

      const event = await eventService.create({
        accountId: account.id,
        title,
        description,
        tags,
        date: date?.toISOString(),
      });

      if (event.result) {
        addEvent(event.result);
        syncTags(tags);
      }

      setIsLoading(false);

      setTitle('');
      setDescription('');
      setDate(new Date());
      setTags([]);

      onCreate();
    } catch (error) {
      throwError(error);
    }
  };

  const onAddNewTag = (value: string) => {
    setTagOptions([...tagOptions, { value, title: value }]);

    setTags([...tags, value]);
  };

  return (
    <Form>
      <Header size="H3">Create Event</Header>
      <TextInput
        title="Title"
        type="text"
        value={title}
        onChange={setTitle}
        hasError={Boolean(title) && title.length < TITLE_MIN_LENGTH}
        errorTip="The title must have at least three characters"
      />
      <TextInput
        title="Description"
        type="textarea"
        value={description}
        onChange={setDescription}
      />
      <Fieldset isFocused={dateIsFocused}>
        <Label isFocused={dateIsFocused} hasValue={Boolean(date)}>
          Date
        </Label>
        <DatePicker
          selected={date}
          onChange={setDate}
          showTimeSelect
          calendarContainer={DatePickerStyles}
          onCalendarOpen={() => setDateIsFocused(true)}
          onCalendarClose={() => setDateIsFocused(false)}
          dateFormat="Pp"
          customInput={<Input type="text" />}
        />
      </Fieldset>
      <Select
        value={tags}
        options={tagOptions}
        onChange={(value) => setTags(value as string[])}
        onAddNew={onAddNewTag}
        label="Tags"
      />
      <Submit
        color="success"
        onClick={submit}
        isDisabled={title.length < TITLE_MIN_LENGTH}
        isLoading={isLoading}
        isAsync
      >
        Create
      </Submit>
    </Form>
  );
};
