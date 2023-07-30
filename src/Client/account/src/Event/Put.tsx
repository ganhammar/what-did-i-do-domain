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
import { currentAccountAtom } from 'src/Account';
import styled from 'styled-components';
import { eventServiceSelector } from './eventServiceSelector';
import { useAddEvent } from './useAddEvent';
import { tagsAtom, useSyncTags } from 'src/Tag';
import { DateTimePicker } from 'src/Components';
import { useUpdateEvent } from './useUpdateEvent';

interface PutProps {
  onPut: () => void;
  event?: Event;
}

const Form = styled.form`
  display: flex;
  flex-direction: column;
`;
const Submit = styled(Button)`
  margin-left: auto;
  margin-top: ${({ theme }) => theme.spacing.l};
`;

const TITLE_MIN_LENGTH = 3;

export const Put = ({ onPut, event }: PutProps) => {
  const throwError = useAsyncError();
  const account = useRecoilValue(currentAccountAtom);
  const eventService = useRecoilValue(eventServiceSelector);
  const existingTags = useRecoilValue(tagsAtom);
  const addEvent = useAddEvent();
  const updateEvent = useUpdateEvent();
  const syncTags = useSyncTags();
  const [tagOptions, setTagOptions] = useState<SelectOption[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [date, setDate] = useState<Date | null>(new Date());

  useEffect(() => {
    if (event) {
      setTitle(event.title);
      setDescription(event.description ?? '');
      setTags(event.tags ?? []);
      setDate(new Date(event.date));
    }
  }, [event]);

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

      if (Boolean(event)) {
        const editEvent = await eventService.edit({
          id: event!.id,
          title,
          description,
          tags,
        });

        if (editEvent.result) {
          updateEvent(event!.id, editEvent.result);
          syncTags(tags);
        }
      } else {
        const newEvent = await eventService.create({
          accountId: account.id,
          title,
          description,
          tags,
          date: date?.toISOString(),
        });

        if (newEvent.result) {
          addEvent(newEvent.result);
          syncTags(tags);
        }
      }

      setIsLoading(false);

      setTitle('');
      setDescription('');
      setDate(new Date());
      setTags([]);

      onPut();
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
      <Header size="H3">
        {Boolean(event) ? 'Edit Event' : 'Create Event'}
      </Header>
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
      <DateTimePicker
        date={date}
        onChange={setDate}
        showTimeSelect
        isDisabled={Boolean(event)}
      />
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
        {Boolean(event) ? 'Edit' : 'Create'}
      </Submit>
    </Form>
  );
};
