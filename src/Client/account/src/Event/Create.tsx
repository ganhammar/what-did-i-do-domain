import { Button, TextInput, useAsyncError, Header } from '@wdid/shared';
import { useState } from 'react';
import { useRecoilValue } from 'recoil';
import { currentAccountAtom } from 'src/Account';
import styled from 'styled-components';
import { eventServiceSelector } from './eventServiceSelector';
import { useAddEvent } from './useAddEvent';

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
  const addEvent = useAddEvent();
  const [title, setTitle] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const submit = async () => {
    try {
      setIsLoading(true);

      const event = await eventService.create({
        accountId: account.id,
        title,
      });

      if (event.result) {
        addEvent(event.result);
      }

      setIsLoading(false);

      setTitle('');

      onCreate();
    } catch (error) {
      throwError(error);
    }
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
