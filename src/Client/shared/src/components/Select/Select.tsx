import styled from 'styled-components';

interface SelectOption {
  value: string;
  title: string;
}

interface SelectProps {
  options: SelectOption[];
  value: string;
  onChange: (value: string) => void;
}

const Element = styled.select`
  background: none;
  border: none;
`;

export const Select = ({ options, value, onChange }: SelectProps) => (
  <Element value={value} onChange={(event) => onChange(event.target.value)}>
    {options.map(({ value, title }) => (
      <option value={value} key={value}>
        {title}
      </option>
    ))}
  </Element>
);
