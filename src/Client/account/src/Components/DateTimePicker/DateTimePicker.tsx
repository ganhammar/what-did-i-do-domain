import { useState } from 'react';
import styled from 'styled-components';
import DatePicker, { registerLocale } from 'react-datepicker';
import sv from 'date-fns/locale/sv';

import 'react-datepicker/dist/react-datepicker.css';

registerLocale('sv', sv);

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
const Fieldset = styled.fieldset<{ isFocused: boolean; isDisabled: boolean }>`
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
  ${({ isDisabled }) =>
    isDisabled &&
    `
    cursor: not-allowed;
    opacity: 0.8;
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
const Input = styled.input<{ isDisabled: boolean }>`
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
  ${({ isDisabled }) =>
    isDisabled &&
    `
    cursor: not-allowed;
  `}
`;

interface DateTimePickerProps {
  date: Date | null;
  showTimeSelect?: boolean;
  isDisabled?: boolean;
  onChange: (date: Date | null) => void;
}

export const DateTimePicker = ({
  date,
  showTimeSelect,
  isDisabled,
  onChange,
}: DateTimePickerProps) => {
  const [dateIsFocused, setDateIsFocused] = useState(false);

  return (
    <Fieldset isFocused={dateIsFocused} isDisabled={isDisabled ?? false}>
      <Label isFocused={dateIsFocused} hasValue={Boolean(date)}>
        Date
      </Label>
      <DatePicker
        selected={date}
        onChange={onChange}
        showTimeSelect={showTimeSelect}
        calendarContainer={DatePickerStyles}
        onCalendarOpen={() => setDateIsFocused(true)}
        onCalendarClose={() => setDateIsFocused(false)}
        dateFormat={showTimeSelect ? 'Pp' : 'P'}
        customInput={<Input type="text" isDisabled={isDisabled ?? false} />}
        disabled={isDisabled ?? false}
      />
    </Fieldset>
  );
};
