import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import {
  useClickOutside,
  useKeyPress,
  useScroll,
  useWindowSize,
} from '@wdid/shared';
import { Remove } from '@wdid/shared/src/components/Remove';
import styled, { keyframes } from 'styled-components';

export interface SelectOption {
  value: string;
  title: string;
}

interface SelectProps {
  options: SelectOption[];
  value: string | string[];
  label: string;
  className?: string;
  onChange: (value: string | string[]) => void;
  onAddNew?: (value: string) => void;
  allowClear?: boolean;
  condense?: boolean;
}

interface WrapperProps {
  isOpen: boolean;
}

interface SelectBoxProps {
  left: number;
  top: number;
  width: number;
  height: number;
}

interface OptionProps {
  isSelected: boolean;
  isHovered: boolean;
}

const OuterWrapper = styled.div``;
const Wrapper = styled.div<WrapperProps>`
  border: none;
  display: flex;
  flex-direction: column;
  cursor: pointer;
  padding: ${({ theme }) =>
    `${theme.spacing.xs} 44px ${theme.spacing.xs} ${theme.spacing.xs}`};
  margin-top: 22px;
  box-sizing: border-box;
  border-bottom: 1px solid ${({ theme }) => theme.palette.divider.main};
  position: relative;
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
  ${({ theme, isOpen }) =>
    isOpen &&
    `
    border-color: ${theme.palette.primary.main};
    &:after {
      width: 100%;
    }
  `}
`;
const Arrow = styled.div<WrapperProps>`
  position: absolute;
  top: calc(50% - 24px);
  right: 0;
  content: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24px' height='24px' fill='currentColor'><path d='M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z' /></svg>");
  padding: ${({ theme }) => `10px ${theme.spacing.s} 10px 4px`};
  ${({ theme, isOpen }) =>
    isOpen &&
    `
      top: calc(50% - 26px);
      rotate: 180deg;
      padding: ${`10px 4px 10px ${theme.spacing.s}`};
    `}
`;
const Label = styled.label<{ hasSelected: boolean; isOpen: boolean }>`
  height: 22px;
  display: block;
  cursor: pointer;
  position: absolute;
  left: ${({ theme }) => theme.spacing.xs};
  top: 0;
  background: ${({ theme }) => theme.palette.paper.main};
  transition:
    top 0.5s,
    font-size 0.5s;
  ${({ hasSelected, isOpen }) =>
    (hasSelected || isOpen) &&
    `
    font-size: 0.8rem;
    top: -22px;
  `}
`;
const ActualSelect = styled.select`
  width: 0;
  height: 0;
  overflow: hidden;
`;
const Value = styled.div<{
  hasTags: boolean;
  width: number;
  condense: boolean;
}>`
  cursor: pointer;
  min-height: 32px;
  display: flex;
  flex-direction: row;
  max-width: ${({ width }) => width - 52}px;
  flex-wrap: ${({ condense }) => (condense ? 'nowrap' : 'wrap')};
  ${({ hasTags }) =>
    hasTags &&
    `
    margin-bottom: -9px;
  `}
`;
const Input = styled.input`
  background: none;
  border: none;
  flex-grow: 1;
  &:focus {
    outline: none;
  }
`;
const ValueTag = styled.div`
  box-sizing: border-box;
  border: 1px solid ${({ theme }) => theme.palette.divider.main};
  border-radius: ${({ theme }) => theme.borderRadius};
  padding: ${({ theme }) => `0 ${theme.spacing.xs}`};
  margin: ${({ theme }) => `0 ${theme.spacing.xs} ${theme.spacing.xs} 0`};
  background: ${({ theme }) => theme.palette.paperHighlight.main};
  display: flex;
  align-items: center;
  white-space: nowrap;
`;
const StyledRemove = styled(Remove)`
  height: 12px;
  width: 12px;
  margin-left: 4px;
`;

const fade = keyframes`
  0% {
    opacity: 0;
    margin-top: -20px;
  }
  100% {
    opacity: 1;
    margin-top: 0;
  }
`;

const SelectBox = styled.div<SelectBoxProps>`
  position: absolute;
  left: ${({ left }) => `${left}px`};
  top: ${({ top, height }) => `${top + height}px`};
  width: ${({ width }) => `${width}px`};
  background-color: ${({ theme }) => theme.palette.paperHighlight.main};
  color: ${({ theme }) => theme.palette.paperHighlight.contrastText};
  box-shadow: ${({ theme }) => theme.shadows[3]};
  border-radius: ${({ theme }) => theme.borderRadius};
  animation: ${fade} 200ms forwards ease-in-out;
`;
const Options = styled.div``;
const Option = styled.div<OptionProps>`
  border-top: 2px solid ${({ theme }) => theme.palette.background.main};
  cursor: pointer;
  padding: ${({ theme }) => theme.spacing.xs};
  ${({ theme, isHovered }) =>
    isHovered &&
    `
    background-color: ${theme.palette.paper.main};
    color: ${theme.palette.paper.contrastText};
  `}
  ${({ theme, isSelected }) =>
    isSelected &&
    `
    background-color: ${theme.palette.background.main};
    color: ${theme.palette.background.contrastText};
  `}
`;
const AddOption = styled.p`
  padding: ${({ theme }) => theme.spacing.xs};
  cursor: pointer;
  &:hover {
    background-color: ${({ theme }) => theme.palette.paper.main};
    color: ${({ theme }) => theme.palette.paper.contrastText};
  }
`;
const NoOptions = styled.p`
  font-size: 0.8em;
  font-style: italic;
  padding: ${({ theme }) => theme.spacing.xs};
`;

export const Select = ({
  options,
  value,
  label,
  className,
  onChange,
  onAddNew,
  allowClear,
  condense,
}: SelectProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [hovered, setIsHovered] = useState<string>();
  const [filter, setFilter] = useState<string | null>(null);
  const size = useWindowSize();
  const outerWrapperRef = useRef<HTMLDivElement>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const selectBoxRef = useRef<HTMLDivElement>(null);
  const filterRef = useRef<HTMLInputElement>(null);
  const [wrapperRect, setWrapperRect] = useState<DOMRect>();
  const [, scrollY] = useScroll();

  const toggle = () => {
    if (isOpen) {
      setIsHovered(undefined);
      setFilter(null);
    }

    setIsOpen(!isOpen);
  };

  const callback = (selected: string) => {
    let newValue;
    setFilter(null);

    if (Array.isArray(value)) {
      filterRef.current?.focus();

      newValue = [...value];

      if (newValue.indexOf(selected) !== -1) {
        newValue.splice(newValue.indexOf(selected), 1);
      } else {
        newValue.push(selected);
      }
    } else {
      setIsOpen(false);
      setIsHovered(undefined);

      if ((selected === value || selected.length === 0) && allowClear) {
        newValue = '';
      } else {
        newValue = selected;
      }
    }

    onChange(newValue);
  };

  const addNewCallback = () => {
    if (!onAddNew) {
      return;
    }

    setIsHovered(undefined);

    if (Array.isArray(value)) {
      filterRef.current?.focus();
    } else {
      setIsOpen(false);
    }

    onAddNew(filter as string);
    setFilter(null);
  };

  const isSelected = (option: SelectOption) => {
    if (Array.isArray(value)) {
      return value.indexOf(option.value) !== -1;
    }

    return value === option.value;
  };

  const getTitle = (value: string) =>
    options.find((option) => option.value === value)?.title;

  const getFilterValue = () => {
    if (filter === null && !Array.isArray(value)) {
      return getTitle(value) ?? '';
    }

    return filter ?? '';
  };

  const getOptions = () =>
    options.filter(({ value, title }) => {
      if (!filter) {
        return true;
      }

      const lowerFilter = filter.toLowerCase();

      if (
        value.toLowerCase().indexOf(lowerFilter) !== -1 ||
        title.toLowerCase().indexOf(lowerFilter) !== -1
      ) {
        return true;
      }

      return false;
    });

  useClickOutside([wrapperRef, selectBoxRef], () => isOpen && toggle());
  useKeyPress(['Escape', 'Enter', 'ArrowDown', 'ArrowUp'], (key) => {
    if (!isOpen) {
      return;
    }

    const hoveredIndex = options.findIndex(
      (option) => option.value === hovered
    );

    switch (key) {
      case 'Escape':
        toggle();
        break;
      case 'ArrowDown':
        if (!hovered) {
          setIsHovered(options.at(-1)?.value);
        }

        setIsHovered(options.at(hoveredIndex + 1)?.value);
        break;
      case 'ArrowUp':
        if (!hovered) {
          setIsHovered(options.at(1)?.value);
        }

        setIsHovered(options.at(hoveredIndex - 1)?.value);
        break;
      case 'Enter':
        if (hovered) {
          callback(hovered);
        }
        break;
    }
  });

  useEffect(() => {
    if (isOpen && wrapperRef.current) {
      setWrapperRect(wrapperRef.current.getBoundingClientRect());
    }
  }, [isOpen, wrapperRef, size, value, scrollY]);

  useEffect(() => {
    if (isOpen) {
      filterRef.current!.focus();
    }
  }, [isOpen, filterRef]);

  return (
    <OuterWrapper className={className} ref={outerWrapperRef}>
      <Wrapper ref={wrapperRef} isOpen={isOpen}>
        <Label
          hasSelected={Boolean(value.length)}
          isOpen={isOpen}
          onClick={toggle}
        >
          {label}
        </Label>
        <Value
          hasTags={Array.isArray(value) && value.length > 0}
          onClick={() => setIsOpen(true)}
          width={outerWrapperRef.current?.clientWidth ?? 0}
          condense={condense ?? false}
        >
          {!condense &&
            Array.isArray(value) &&
            options.filter(isSelected).map(({ title, value }) => (
              <ValueTag key={value}>
                {title}
                <StyledRemove onClick={() => callback(value)} />
              </ValueTag>
            ))}
          {condense && Array.isArray(value) && value.length > 0 && (
            <ValueTag>
              {value.length}{' '}
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="2 5 24 24"
                width="16px"
                height="16px"
                fill="currentColor"
              >
                <path d="M5 16.577l2.194-2.195 5.486 5.484L24.804 7.743 27 9.937l-14.32 14.32z" />
              </svg>
            </ValueTag>
          )}
          {!Array.isArray(value) && !isOpen && getTitle(value)}
          {isOpen && (
            <Input
              type="text"
              value={getFilterValue()}
              onChange={(event) => setFilter(event.target.value)}
              ref={filterRef}
            />
          )}
        </Value>
        <Arrow isOpen={isOpen} onClick={toggle} />
        <ActualSelect
          value={value}
          onChange={(event) => onChange(event.target.value)}
          multiple={Array.isArray(value)}
          onFocus={() => toggle()}
        >
          {options.map(({ value, title }) => (
            <option value={value} key={value}>
              {title}
            </option>
          ))}
        </ActualSelect>
      </Wrapper>
      {isOpen &&
        createPortal(
          <SelectBox
            left={wrapperRect?.left ?? 0}
            top={(wrapperRect?.top ?? 0) + scrollY}
            width={wrapperRect?.width ?? 0}
            height={wrapperRect?.height ?? 0}
            ref={selectBoxRef}
            onMouseLeave={() => setIsHovered(undefined)}
          >
            <Options>
              {getOptions().map((option) => (
                <Option
                  key={option.value}
                  onClick={() => callback(option.value)}
                  onMouseEnter={() => setIsHovered(option.value)}
                  isSelected={isSelected(option)}
                  isHovered={hovered === option.value}
                >
                  {option.title}
                </Option>
              ))}
              {getOptions().length === 0 && (
                <>
                  {Boolean(onAddNew) && filter?.length && (
                    <AddOption onClick={addNewCallback}>
                      Add "{filter}"
                    </AddOption>
                  )}
                  {!Boolean(onAddNew) && (
                    <NoOptions>No matching options</NoOptions>
                  )}
                </>
              )}
            </Options>
          </SelectBox>,
          document.body
        )}
    </OuterWrapper>
  );
};
