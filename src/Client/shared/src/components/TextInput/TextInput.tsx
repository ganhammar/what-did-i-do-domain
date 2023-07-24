import { useState } from 'react';
import styled, { css } from 'styled-components';

interface TextInputStyleProps {
  isFocused: boolean;
  hasValue: boolean;
  hasError: boolean;
}

const Fieldset = styled.fieldset<TextInputStyleProps>`
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
    background-color: ${({ theme: { palette }, hasError }) =>
      hasError ? palette.warning.main : palette.primary.main};
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
`;
const Label = styled.label<TextInputStyleProps>`
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
const InputStyles = css<TextInputStyleProps>`
  border: none;
  border-bottom: 1px solid
    ${({ theme: { palette }, hasError }) =>
      hasError ? palette.warning.main : palette.divider.main};
  background: none;
  padding: ${({ theme }) =>
    `0 ${theme.spacing.xs} ${theme.spacing.xs} ${theme.spacing.xs}`};
  width: 100%;
  margin: 1.1rem 0 0 0;
  transition: border-bottom-color 0.5s;
  &:focus {
    outline: none;
  }
`;
const Input = styled.input<TextInputStyleProps>`
  ${InputStyles}
  height: 2.6rem;
`;
const Textarea = styled.textarea`
  ${InputStyles}
  margin-bottom: -8px;
  resize: none;
`;
const Tip = styled.div<TextInputStyleProps>`
  position: absolute;
  bottom: -1.5rem;
  left: ${({ theme }) => theme.spacing.xs};
  font-size: 0.8rem;
  z-index: 2;
  transition: opacity 0.5s;
  opacity: 0;
  ${({ isFocused, hasError }) =>
    isFocused &&
    hasError &&
    `
    opacity: 1;
  `}
`;

interface Props {
  type: 'text' | 'password' | 'textarea';
  value: string;
  title: string;
  onChange: (value: string) => void;
  isDisabled?: boolean;
  hasError?: boolean;
  errorTip?: string;
}

export function TextInput({
  type,
  value,
  title,
  onChange,
  isDisabled,
  hasError,
  errorTip,
}: Props) {
  const [isFocused, setIsFocused] = useState(false);

  return (
    <Fieldset
      isFocused={isFocused}
      hasValue={Boolean(value)}
      hasError={hasError ?? false}
    >
      <Label
        isFocused={isFocused}
        hasValue={Boolean(value)}
        hasError={hasError ?? false}
      >
        {title}
      </Label>
      {type === 'textarea' && (
        <Textarea
          value={value}
          onChange={({ target: { value } }) => onChange(value)}
          disabled={isDisabled}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          isFocused={isFocused}
          hasValue={Boolean(value)}
          hasError={hasError ?? false}
        />
      )}
      {type !== 'textarea' && (
        <Input
          type={type}
          value={value}
          onChange={({ target: { value } }) => onChange(value)}
          disabled={isDisabled}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          isFocused={isFocused}
          hasValue={Boolean(value)}
          hasError={hasError ?? false}
        />
      )}
      {errorTip && (
        <Tip
          isFocused={isFocused}
          hasValue={Boolean(value)}
          hasError={hasError ?? false}
        >
          {errorTip}
        </Tip>
      )}
    </Fieldset>
  );
}
