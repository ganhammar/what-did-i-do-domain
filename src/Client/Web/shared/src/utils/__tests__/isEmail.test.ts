import { isEmail } from '../isEmail';

describe('isEmail', () => {
  it('is email', () => {
    expect(isEmail('hello@wdid.fyi')).toBeTruthy();
  });

  it('is not email', () => {
    expect(isEmail('hello wdid.fyi')).toBeFalsy();
  });
});
