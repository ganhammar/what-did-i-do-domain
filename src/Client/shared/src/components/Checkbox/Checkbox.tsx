import styled from 'styled-components';

const Fieldset = styled.fieldset`
  cursor: pointer;
  border: none;
  display: flex;
  flex-direction: row;
  height: 2rem;
  align-items: center;
`;

const CheckboxElement = styled.input`
  width: 1.2rem;
  height: 1.2rem;
  border: none;
  margin-right: 0.6rem;
  appearance: none;
  &:after {
    content: '';
    width: 1.2em;
    height: 1.2em;
    display: block;
    border-radius: ${({ theme }) => theme.borderRadius};
    cursor: pointer;
    border: 2px solid ${({ theme }) => theme.palette.divider.main};
    background-color: ${({ theme }) => theme.palette.paper.main};
    box-sizing: border-box;
    color: ${({ theme }) => theme.palette.primary.contrastText};
    transition:
      background-color 0.2s,
      border-color 0.2s;
  }
  &:checked:after {
    content: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' viewBox='2 8 24 24' width='16px' height='16px' fill='white'><path d='M5 16.577l2.194-2.195 5.486 5.484L24.804 7.743 27 9.937l-14.32 14.32z' /></svg>");
    border-color: ${({ theme }) => theme.palette.primary.main};
    background-color: ${({ theme }) => theme.palette.primary.main};
  }
`;

const Label = styled.label`
  cursor: pointer;
`;

interface Props {
  title: string;
  isChecked: boolean;
  onChange: () => void;
  position: 'left' | 'right';
}

export function Checkbox({ title, isChecked, onChange, position }: Props) {
  return (
    <Fieldset onClick={onChange}>
      <CheckboxElement
        type="checkbox"
        checked={isChecked}
        onChange={onChange}
      />
      <Label>{title}</Label>
    </Fieldset>
  );
}
