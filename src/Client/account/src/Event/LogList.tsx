import { useRecoilValue } from 'recoil';
import { eventsAtom } from '.';
import styled from 'styled-components';
import { useIntl } from 'react-intl';
import { Loader, Remove, timeFromNow, useAsyncError } from '@wdid/shared';
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

  return (
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
                  <Remove onClick={() => setRemoveInitiated(true)} />
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
  );
};
