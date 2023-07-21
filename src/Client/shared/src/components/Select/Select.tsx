import { useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useClickOutside } from '@wdid/shared';
import styled from 'styled-components';

interface SelectOption {
  value: string;
  title: string;
}

interface SelectProps {
  options: SelectOption[];
  value: string;
  label: string;
  className?: string;
  onChange: (value: string) => void;
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

const OuterWrapper = styled.div``;
const Wrapper = styled.div<WrapperProps>`
  border: none;
  display: flex;
  flex-direction: column;
  cursor: pointer;
  padding: ${({ theme }) => theme.spacing.xs};
  margin-top: ${({ theme }) => theme.spacing.s};
  box-sizing: border-box;
  border-bottom: 1px solid ${({ theme }) => theme.palette.divider.main};
  position: relative;
  &:after {
    position: absolute;
    top: calc(50% - 12px);
    right: ${({ theme }) => theme.spacing.s};
    content: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24px' height='24px' fill='currentColor'><path d='M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z' /></svg>");
    ${({ isOpen }) =>
      isOpen &&
      `
      top: calc(50% - 24px);
      rotate: 180deg;
    `}
  }
  ${({ theme, isOpen }) =>
    isOpen &&
    `
    border-color: ${theme.palette.primary.main};
  `}
`;
const Label = styled.label`
  font-size: 0.8rem;
  height: 22px;
  display: block;
  cursor: pointer;
  position: absolute;
  top: -14px;
  left: ${({ theme }) => theme.spacing.xs};
  background: ${({ theme }) => theme.palette.paper.main};
`;
const ActualSelect = styled.select`
  display: none;
`;
const Value = styled.div`
  cursor: pointer;
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
`;
const Options = styled.div``;
const Option = styled.div`
  border-top: 2px solid ${({ theme }) => theme.palette.background.main};
  cursor: pointer;
  padding: ${({ theme }) => theme.spacing.xs};
  &:hover {
    background-color: ${({ theme }) => theme.palette.background.main};
  }
`;

export const Select = ({
  options,
  value,
  label,
  className,
  onChange,
}: SelectProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const selectBoxRef = useRef<HTMLDivElement>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);

  const toggle = () => {
    setIsOpen(!isOpen);
  };

  const callback = (value: string) => {
    setIsOpen(false);
    onChange(value);
  };

  useClickOutside([wrapperRef, selectBoxRef], () => isOpen && toggle());

  return (
    <OuterWrapper className={className}>
      <Wrapper ref={wrapperRef} isOpen={isOpen} onClick={toggle}>
        <Label>{label}</Label>
        <Value>{options.find((option) => option.value === value)?.title}</Value>
        <ActualSelect
          value={value}
          onChange={(event) => onChange(event.target.value)}
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
            left={wrapperRef.current!.offsetLeft}
            top={wrapperRef.current!.offsetTop}
            width={wrapperRef.current!.clientWidth}
            height={wrapperRef.current!.clientHeight}
            ref={selectBoxRef}
          >
            <Options>
              {options.map(({ value, title }) => (
                <Option key={value} onClick={() => callback(value)}>
                  {title}
                </Option>
              ))}
            </Options>
          </SelectBox>,
          document.body
        )}
    </OuterWrapper>
  );
};
