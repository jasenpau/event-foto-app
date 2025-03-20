export const formatLithuanianDate = (date: Date) => {
  return date.toLocaleDateString('lt-LT', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }) + ' ' + date.toLocaleTimeString('lt-LT', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  });
}
