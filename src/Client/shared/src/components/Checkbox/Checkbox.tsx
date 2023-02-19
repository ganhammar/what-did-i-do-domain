import styled from 'styled-components';

const Fieldset = styled.fieldset`
  cursor: pointer;
  border: none;
`;

const CheckboxElement = styled.input``;

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
      <CheckboxElement type="checkbox" checked={isChecked} />
      <label>{title}</label>
    </Fieldset>
  );
}
