import { useRecoilValue } from 'recoil';
import { Put, eventsAtom } from '.';
import styled from 'styled-components';
import { useIntl } from 'react-intl';
import {
  Loader,
  Modal,
  Remove,
  timeFromNow,
  useAsyncError,
} from '@wdid/shared';
import { useEffect, useState } from 'react';
import { Tag } from './Tag';
import { eventServiceSelector } from './eventServiceSelector';
import { useRemoveEvent } from './useRemoveEvent';
import { useLoadMoreEvents } from './useLoadMoreEvents';

interface ItemProps {
  isHovered: boolean;
}

const List = styled.div`
  flex-grow: 1;
`;
const Item = styled.div<ItemProps>`
  padding: ${({ theme }) => theme.spacing.m};
  border-bottom: 2px solid ${({ theme }) => theme.palette.background.main};
  position: relative;
  &:last-child {
    border: none;
  }
  ${({ theme, isHovered }) =>
    isHovered &&
    `
    background-color: ${theme.palette.paperHighlight.main};
    color: ${theme.palette.paperHighlight.contrastText};
  `}
`;
const LoadMoreItem = styled(Item)`
  cursor: pointer;
  font-weight: bold;
  display: flex;
  justify-content: center;
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
const DeleteWrapper = styled.div`
  position: absolute;
  right: ${({ theme }) => theme.spacing.m};
  top: 50%;
  margin-top: -12px;
  text-align: right;
`;
const Edit = styled.svg`
  width: 19px;
  height: 19px;
  cursor: pointer;
  float: left;
  margin-top: 2px;
  margin-right: 5px;
  &:hover {
    opacity: 0.6;
  }
`;
const ConfirmActions = styled.div`
  background-color: ${({ theme }) => theme.palette.paperHighlight.main};
  padding: ${({ theme }) => `0 0 ${theme.spacing.xs} ${theme.spacing.xs}`};
`;
const ConfirmText = styled.p`
  font-size: 0.7em;
  line-height: 0.8em;
  background-color: ${({ theme }) => theme.palette.paperHighlight.main};
  padding: ${({ theme }) => `0 0 0 ${theme.spacing.xs}`};
`;
const AbortLink = styled.p`
  cursor: pointer;
  font-size: 0.7em;
  line-height: 0.8em;
  display: inline;
  margin-left: ${({ theme }) => theme.spacing.xs};
  &:hover {
    text-decoration: underline;
  }
`;
const ConfirmLink = styled(AbortLink)`
  color: ${({ theme }) => theme.palette.warning.main};
`;

export const LogList = () => {
  const throwError = useAsyncError();
  const events = useRecoilValue(eventsAtom);
  const intl = useIntl();
  const [hovered, setHovered] = useState<string>();
  const [removeInitiated, setRemoveInitiated] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const eventService = useRecoilValue(eventServiceSelector);
  const removeEvent = useRemoveEvent();
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const loadMoreEvents = useLoadMoreEvents();
  const [editingEvent, setEditingEvent] = useState<Event>();

  useEffect(() => {
    setRemoveInitiated(false);
  }, [hovered]);

  const onRemoveEvent = async (id: string) => {
    try {
      setIsLoading(true);

      await eventService.remove(id);
      removeEvent(id);

      setRemoveInitiated(false);
      setIsLoading(false);
    } catch (error) {
      throwError(error);
    }
  };

  const changeHovered = (id?: string) => {
    if (!isLoading && !removeInitiated) {
      setHovered(id);
    }
  };

  const onLoadMoreEvents = async () => {
    if (isLoadingMore) {
      return;
    }

    setIsLoadingMore(true);
    await loadMoreEvents();
    setIsLoadingMore(false);
  };

  const onEditEvent = (id: string) => {
    const event = events.result?.items.find((e) => e.id === id);

    if (event) {
      setEditingEvent(event);
    }
  };

  return (
    <>
      <List>
        {(events.result?.items.length ?? 0) > 0 &&
          events.result?.items.map(({ id, title, date, description, tags }) => (
            <Item
              key={id}
              onMouseEnter={() => changeHovered(id)}
              onMouseLeave={() => changeHovered(undefined)}
              isHovered={hovered === id}
            >
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
              {hovered === id && (
                <DeleteWrapper>
                  {!isLoading && !removeInitiated && (
                    <>
                      <Edit
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 18 18"
                        fill="currentColor"
                        onClick={() => onEditEvent(id)}
                      >
                        <path d="M2.25,12.9378906 L2.25,15.75 L5.06210943,15.75 L13.3559575,7.45615192 L10.5438481,4.64404249 L2.25,12.9378906 L2.25,12.9378906 L2.25,12.9378906 Z M15.5306555,5.28145396 C15.8231148,4.98899458 15.8231148,4.5165602 15.5306555,4.22410082 L13.7758992,2.46934454 C13.4834398,2.17688515 13.0110054,2.17688515 12.718546,2.46934454 L11.3462366,3.84165394 L14.1583461,6.65376337 L15.5306555,5.28145396 L15.5306555,5.28145396 L15.5306555,5.28145396 Z"></path>
                      </Edit>
                      <Remove onClick={() => setRemoveInitiated(true)} />
                    </>
                  )}
                  {!isLoading && removeInitiated && (
                    <>
                      <ConfirmText>Remove event?</ConfirmText>
                      <ConfirmActions>
                        <ConfirmLink onClick={() => onRemoveEvent(id)}>
                          Yes
                        </ConfirmLink>
                        <AbortLink onClick={() => setRemoveInitiated(false)}>
                          No
                        </AbortLink>
                      </ConfirmActions>
                    </>
                  )}
                  {isLoading && <Loader partial size="small" />}
                </DeleteWrapper>
              )}
            </Item>
          ))}
        {events.result?.items.length === 0 && (
          <Item isHovered={false}>
            <em>No events during the selected period</em>
          </Item>
        )}
        {Boolean(events.result?.paginationToken) && (
          <LoadMoreItem
            onMouseEnter={() => changeHovered('load-more')}
            onMouseLeave={() => changeHovered(undefined)}
            isHovered={hovered === 'load-more'}
            onClick={onLoadMoreEvents}
          >
            {!isLoadingMore && <p>Load more events</p>}
            {isLoadingMore && <Loader partial />}
          </LoadMoreItem>
        )}
      </List>
      <Modal
        isOpen={Boolean(editingEvent)}
        onClose={() => setEditingEvent(undefined)}
      >
        <Put onPut={() => setEditingEvent(undefined)} event={editingEvent} />
      </Modal>
    </>
  );
};
