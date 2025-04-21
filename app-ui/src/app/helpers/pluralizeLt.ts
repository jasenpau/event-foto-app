export interface PluralDefinition {
  singular: string;
  smallPlural: string;
  largePlural: string;
}

export const pluralizeLt = (count: number, definition: PluralDefinition) => {
  if (count === 0) return `${count} ${definition.largePlural}`;
  if (count === 1) return `${count} ${definition.singular}`;
  if (count > 1 && count < 10) return `${count} ${definition.smallPlural}`;
  else return `${count} ${definition.largePlural}`;
};
