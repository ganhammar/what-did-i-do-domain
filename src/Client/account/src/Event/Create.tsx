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

const TITLE_MIN_LENGTH = 3;

export const Create = ({ onCreate }: CreateProps) => {
  const throwError = useAsyncError();
  const account = useRecoilValue(currentAccountAtom);
  const eventService = useRecoilValue(eventServiceSelector);
  const existingTags = useRecoilValue(tagsAtom);
  const addEvent = useAddEvent();
  const syncTags = useSyncTags();
  const [tagOptions, setTagOptions] = useState<SelectOption[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);

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
      });

      if (event.result) {
        addEvent(event.result);
        syncTags(tags);
      }

      setIsLoading(false);

      setTitle('');
      setDescription('');
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
