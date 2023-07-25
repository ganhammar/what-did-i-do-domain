import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useClickOutside, useKeyPress, useWindowSize } from '@wdid/shared';
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
    position: absolute;
    top: calc(50% - 14px);
    right: ${({ theme }) => theme.spacing.s};
    content: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24px' height='24px' fill='currentColor'><path d='M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z' /></svg>");
    ${({ isOpen }) =>
      isOpen &&
      `
      top: calc(50% - 26px);
      rotate: 180deg;
    `}
  }
  ${({ theme, isOpen }) =>
    isOpen &&
    `
    border-color: ${theme.palette.primary.main};
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
const Value = styled.div<{ hasTags: boolean }>`
  cursor: pointer;
  min-height: 32px;
  display: flex;
  flex-direction: row;
  max-width: 100%;
  flex-wrap: wrap;
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
  top: ${({ top, height }) => `${top + height + 1}px`};
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
}: SelectProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [hovered, setIsHovered] = useState<string>();
  const [filter, setFilter] = useState<string | null>(null);
  const size = useWindowSize();
  const selectBoxRef = useRef<HTMLDivElement>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const filterRef = useRef<HTMLInputElement>(null);
  const [wrapperRect, setWrapperRect] = useState<DOMRect>();

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
    setIsHovered(undefined);

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
      newValue = selected;
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

  const getFilterValue = () => {
    if (filter === null && !Array.isArray(value)) {
      return value;
    }

    return filter ?? '';
  };

  const getOptions = () =>
    options.filter(({ value }) => !filter || value.indexOf(filter) !== -1);

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
    if (wrapperRef.current) {
      setWrapperRect(wrapperRef.current.getBoundingClientRect());
    }
  }, [wrapperRef, size, value]);

  useEffect(() => {
    if (isOpen) {
      filterRef.current!.focus();
    }
  }, [isOpen, filterRef]);

  return (
    <OuterWrapper className={className}>
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
        >
          {Array.isArray(value) &&
            options.filter(isSelected).map(({ title, value }) => (
              <ValueTag key={value}>
                {title}
                <StyledRemove onClick={() => callback(value)} />
              </ValueTag>
            ))}
          {!Array.isArray(value) && !isOpen && value}
          {isOpen && (
            <Input
              type="text"
              value={getFilterValue()}
              onChange={(event) => setFilter(event.target.value)}
              ref={filterRef}
            />
          )}
        </Value>
        <ActualSelect
          value={value}
          onChange={(event) => onChange(event.target.value)}
          multiple={Array.isArray(value)}
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
            top={wrapperRect?.top ?? 0}
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
                  {option.value}
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
