import { OrderFilters } from '../models';

export function getInitialFilters(): OrderFilters {
  const currentYear = new Date().getFullYear().toString();

  return {
    year: currentYear,
    folio: '',
    supplier: '',
    budgetKey: '',
    type: '',
    dateFrom: '',
    dateTo: '',
    status: ''
  };
}

export const DEFAULT_FILTERS_CONFIG = {
  year: '2025',
  dateFrom: '2025-01-01',
  dateTo: '2025-12-31'
} as const;
