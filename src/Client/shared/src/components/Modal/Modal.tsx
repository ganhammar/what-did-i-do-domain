import { ReactNode, useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import styled, { css, keyframes } from 'styled-components';

interface ModalProps {
  isOpen: boolean;
  children: ReactNode;
  onClose: () => void;
}

interface ModalStyleProps {
  isOpen: boolean;
  isClosing: boolean;
}

const fadeIn = keyframes`
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
`;
const fadeOut = keyframes`
  0% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
`;
const moveDown = keyframes`
  0% {
    opacity: 0;
    margin-top: -50px;
  }
  100% {
    opacity: 1;
    margin-top: 0px;
  }
`;
const moveUp = keyframes`
  0% {
    opacity: 1;
    margin-top: 0px;
  }
  100% {
    opacity: 0;
    margin-top: -50px;
  }
`;

const Wrapper = styled.div<ModalStyleProps>`
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  visibility: hidden;
  opacity: 0;
  pointer-events: none;
  ${({ isOpen }) =>
    isOpen &&
    css`
      visibility: visible;
      opacity: 1;
      pointer-events: auto;
    `}
`;
const Overlay = styled.div<ModalStyleProps>`
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.2);
  opacity: 0;
  ${({ isOpen }) =>
    isOpen &&
    css`
      animation: ${fadeIn} 200ms forwards ease-in-out;
    `}
  ${({ isClosing }) =>
    isClosing &&
    css`
      animation: ${fadeOut} 200ms forwards ease-in-out;
    `}
`;
const Window = styled.div<ModalStyleProps>`
  position: absolute;
  top: 180px;
  left: 50%;
  margin-left: -300px;
  width: 600px;
  min-height: 200px;
  background-color: ${({ theme }) => theme.palette.paper.main};
  color: ${({ theme }) => theme.palette.paper.contrastText};
  border-radius: 20px;
  box-shadow: ${({ theme }) => theme.shadows[3]};
  padding: ${({ theme }) => theme.spacing.l};
  opacity: 0;
  ${({ isOpen }) =>
    isOpen &&
    css`
      animation: ${moveDown} 200ms forwards ease-in-out;
    `}
  ${({ isClosing }) =>
    isClosing &&
    css`
      animation: ${moveUp} 200ms forwards ease-in-out;
    `}
`;

export const Modal = ({ isOpen, children, onClose }: ModalProps) => {
  const [isClosing, setIsClosing] = useState(false);

  useEffect(() => {
    if (isClosing) {
      setTimeout(() => {
        setIsClosing(false);
        onClose();
      }, 200);
    }
  }, [isClosing, onClose]);

  return createPortal(
    <Wrapper isOpen={isOpen} isClosing={isClosing}>
      <Overlay
        isOpen={isOpen}
        isClosing={isClosing}
        onClick={() => setIsClosing(true)}
      ></Overlay>
      <Window isOpen={isOpen} isClosing={isClosing}>
        {children}
      </Window>
    </Wrapper>,
    document.body
  );
};
