import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useClickOutside, useKeyPress } from '@wdid/shared';
import styled, { keyframes } from 'styled-components';

interface SelectOption {
  value: string;
  title: string;
}

interface SelectProps {
  options: SelectOption[];
  value: string | string[];
  label: string;
  className?: string;
  onChange: (value: string | string[]) => void;
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
  padding: ${({ theme }) => theme.spacing.xs};
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
const Label = styled.label<{ hasSelected: boolean }>`
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
  ${({ hasSelected }) =>
    hasSelected &&
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
const Value = styled.div`
  cursor: pointer;
  min-height: 32px;
  display: flex;
  flex-direction: row;
`;
const ValueTag = styled.div`
  border: 1px solid ${({ theme }) => theme.palette.divider.main};
  border-radius: ${({ theme }) => theme.borderRadius};
  padding: ${({ theme }) => `0 ${theme.spacing.xs}`};
  margin-right: ${({ theme }) => theme.spacing.xs};
  background: ${({ theme }) => theme.palette.paperHighlight.main};
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

export const Select = ({
  options,
  value,
  label,
  className,
  onChange,
}: SelectProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [hovered, setIsHovered] = useState<string>();
  const selectBoxRef = useRef<HTMLDivElement>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const [wrapperRect, setWrapperRect] = useState<DOMRect>();

  const toggle = () => {
    if (isOpen) {
      setIsHovered(undefined);
    }

    setIsOpen(!isOpen);
  };

  const callback = (selected: string) => {
    let newValue;

    if (Array.isArray(value)) {
      newValue = [...value];

      if (newValue.indexOf(selected) !== -1) {
        newValue.splice(newValue.indexOf(selected), 1);
      } else {
        newValue.push(selected);
      }
    } else {
      setIsHovered(undefined);
      setIsOpen(false);
      newValue = selected;
    }

    onChange(newValue);
  };

  const isSelected = (option: SelectOption) => {
    if (Array.isArray(value)) {
      return value.indexOf(option.value) !== -1;
    }

    return value === option.value;
  };

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
  }, [wrapperRef]);

  return (
    <OuterWrapper className={className}>
      <Wrapper ref={wrapperRef} isOpen={isOpen} onClick={toggle}>
        <Label hasSelected={Boolean(value.length)}>{label}</Label>
        <Value>
          {Array.isArray(value) &&
            options
              .filter(isSelected)
              .map(({ title, value }) => (
                <ValueTag key={value}>{title}</ValueTag>
              ))}
          {!Array.isArray(value) && value}
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
              {options.map((option) => (
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
            </Options>
          </SelectBox>,
          document.body
        )}
    </OuterWrapper>
  );
};
