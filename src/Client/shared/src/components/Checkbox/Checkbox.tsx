import styled from 'styled-components';

const Fieldset = styled.fieldset`
  cursor: pointer;
  border: none;
`;

const CheckboxElement = styled.input``;

const Label = styled.label`
  cursor: pointer;
`;

interface Props {
  title: string;
  isChecked: boolean;
  onChange: () => void;
  position: 'left' | 'right';
}

export function Checkbox({
  title,
  isChecked,
  onChange,
  position,
}: Props) {
  return (
    <Fieldset onClick={onChange}>
      <CheckboxElement type="checkbox" checked={isChecked} onChange={onChange} />
      <Label>{title}</Label>
    </Fieldset>
  );
}
