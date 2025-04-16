export const formatLithuanianDate = (date: Date) => {
  return (
    date.toLocaleDateString('lt-LT', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }) +
    ' ' +
    date.toLocaleTimeString('lt-LT', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    })
  );
};

export const formatLithuanianDateWithSeconds = (date: Date) => {
  return (
    date.toLocaleDateString('lt-LT', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }) +
    ' ' +
    date.toLocaleTimeString('lt-LT', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
    })
  );
};

export const formatLithuanianDateOnly = (date: Date) => {
  return date.toLocaleDateString('lt-LT', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  });
};

export const formatLithuanianTimeOnly = (date: Date) => {
  return date.toLocaleTimeString('lt-LT', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  });
};
