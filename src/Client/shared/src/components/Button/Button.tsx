import { ReactNode, useEffect, useRef, useState } from 'react';
import styled, { css, DefaultTheme, keyframes } from 'styled-components';

const MINIMUM_LOADING_INDICATION = 800;

type colors = 'primary' | 'secondary' | 'warning' | 'success' | 'divider';

interface ElementProps {
  color: colors;
  isDisabled: boolean;
  isLoading: boolean;
  buttonWidth: number;
  hasClicked: boolean;
}

const initLoading = (theme: DefaultTheme, buttonWidth: number) => keyframes`
  0% {
    max-width: ${buttonWidth}px;
    max-height: auto;
    overflow: hidden;
  }
  20% {
    color: transparent;
    overflow: hidden;
    border-radius: ${theme.borderRadius};
  }
  50% {
    max-width: calc(1.6em + ${theme.spacing.xs} * 2);
    max-height: calc(1.6em + ${theme.spacing.xs} * 2);
    border-radius: 50%;
    color: transparent;
    overflow: hidden;
  }
  100% {
    max-width: calc(1.6em + ${theme.spacing.xs} * 2);
    max-height: calc(1.6em + ${theme.spacing.xs} * 2);
    border-radius: 100%;
    color: transparent;
    overflow: hidden;
  }
`;

const exitLoading = (theme: DefaultTheme, buttonWidth: number) => keyframes`
  0% {
    max-width: calc(1.6em + ${theme.spacing.xs} * 2);
    max-height: calc(1.6em + ${theme.spacing.xs} * 2);
    border-radius: 100%;
    color: transparent;
    overflow: hidden;
  }
  20% {
    max-width: calc(1.6em + ${theme.spacing.xs} * 2);
    max-height: calc(1.6em + ${theme.spacing.xs} * 2);
    border-radius: 50%;
    color: transparent;
    overflow: hidden;
  }
  50% {
    color: transparent;
    overflow: hidden;
    border-radius: ${theme.borderRadius};
  }
  100% {
    max-width: ${buttonWidth}px;
    max-height: auto;
    overflow: hidden;
  }
`;

const pulse = keyframes`
  0% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
  100% {
    opacity: 1;
  }
`;

const Element = styled.button<ElementProps>`
  background-color: ${({ theme, color, isDisabled }) =>
    isDisabled ? theme.palette.divider.main : theme.palette[color].main};
  color: ${({ theme, color, isDisabled }) =>
    isDisabled
      ? theme.palette.divider.contrastText
      : theme.palette[color].contrastText};
  border: none;
  padding: ${({ theme }) => theme.spacing.xs} ${({ theme }) => theme.spacing.m};
  border-radius: ${({ theme }) => theme.borderRadius};
  box-shadow: ${({ theme, isDisabled }) =>
    isDisabled ? theme.shadows[0] : theme.shadows[1]};
  cursor: ${({ isDisabled }) => (isDisabled ? 'not-allowed' : 'pointer')};
  transition:
    box-shadow 0.5s,
    opacity 0.5s;
  &:hover {
    box-shadow: ${({ theme }) => theme.shadows[0]};
    opacity: 0.9;
  }
  ${({ theme, buttonWidth, isLoading }) =>
    isLoading &&
    css`
      animation:
        ${initLoading(theme, buttonWidth)} 0.3s forwards,
        ${pulse} 1s infinite;
      cursor: not-allowed;
    `}
  ${({ theme, buttonWidth, isLoading, hasClicked }) =>
    hasClicked &&
    !isLoading &&
    css`
      animation: ${exitLoading(theme, buttonWidth)} 0.3s forwards;
    `}
`;

interface Props {
  children: ReactNode;
  color: colors;
  className?: string;
  isDisabled?: boolean;
  isLoading?: boolean;
  isAsync?: boolean;
  onClick: () => void;
}

export function Button({
  children,
  className,
  color,
  isDisabled,
  isLoading,
  isAsync,
  onClick,
}: Props) {
  const buttonRef = useRef<HTMLButtonElement>(null);
  const [buttonWidth, setButtonWidth] = useState(0);
  const [hasClicked, setHasClicked] = useState(false);
  const [showLoadingAnimation, setShowLoadingAnimation] = useState(false);

  const submit = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();

    if (isDisabled || isLoading) {
      return;
    }

    setHasClicked(true);

    if (isAsync) {
      setShowLoadingAnimation(true);
      setTimeout(() => {
        setShowLoadingAnimation(false);
      }, MINIMUM_LOADING_INDICATION);
    }

    onClick();
  };

  useEffect(() => {
    if (buttonRef.current) {
      const { width } = buttonRef.current.getBoundingClientRect();

      if (buttonWidth === 0) {
        setButtonWidth(width);
      }
    }
  }, [buttonRef, buttonWidth]);

  return (
    <Element
      isDisabled={isDisabled ?? false}
      color={color}
      className={className}
      onClick={submit}
      isLoading={isLoading || showLoadingAnimation}
      buttonWidth={buttonWidth}
      ref={buttonRef}
      hasClicked={hasClicked}
    >
      {children}
    </Element>
  );
}
