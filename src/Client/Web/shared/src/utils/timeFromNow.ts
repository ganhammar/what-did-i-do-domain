import { FormatRelativeTimeOptions, IntlShape } from 'react-intl';

const WEEK_IN_MILLIS = 6.048e8,
  DAY_IN_MILLIS = 8.64e7,
  HOUR_IN_MILLIS = 3.6e6,
  MIN_IN_MILLIS = 6e4,
  SEC_IN_MILLIS = 1e3;

const getUTCTime = (date: Date) =>
  date.getTime() - date.getTimezoneOffset() * 60000;

const getCurrentUTCTime = () => getUTCTime(new Date());

const defaultFormatOptions: FormatRelativeTimeOptions = {
  style: 'long',
};

export const timeFromNow = (
  date: Date,
  intl: IntlShape,
  options = defaultFormatOptions
) => {
  const millis =
      typeof date === 'string' ? getUTCTime(new Date(date)) : getUTCTime(date),
    diff = millis - getCurrentUTCTime();
  if (Math.abs(diff) > WEEK_IN_MILLIS)
    return intl.formatRelativeTime(
      Math.trunc(diff / WEEK_IN_MILLIS),
      'week',
      options
    );
  else if (Math.abs(diff) > DAY_IN_MILLIS)
    return intl.formatRelativeTime(
      Math.trunc(diff / DAY_IN_MILLIS),
      'day',
      options
    );
  else if (Math.abs(diff) > HOUR_IN_MILLIS)
    return intl.formatRelativeTime(
      Math.trunc((diff % DAY_IN_MILLIS) / HOUR_IN_MILLIS),
      'hour',
      options
    );
  else if (Math.abs(diff) > MIN_IN_MILLIS)
    return intl.formatRelativeTime(
      Math.trunc((diff % HOUR_IN_MILLIS) / MIN_IN_MILLIS),
      'minute',
      options
    );
  else
    return intl.formatRelativeTime(
      Math.trunc((diff % MIN_IN_MILLIS) / SEC_IN_MILLIS),
      'second',
      options
    );
};
