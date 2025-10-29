import { SelectOption } from '../models';

export function getAvailableYears(): string[] {
  const currentYear = new Date().getFullYear();
  const years: string[] = [];

  for (let i = 0; i < 6; i++) {
    years.push((currentYear - i).toString());
  }

  return years;
}

export const AVAILABLE_YEARS = getAvailableYears();

export const SUPPLIER_OPTIONS: SelectOption[] = [
  { value: '', label: 'Todos los proveedores' },
  { value: '1', label: 'COMPUTADORAS Y TECNOLOGIA DEL NORTE SA DE CV' },
  { value: '2', label: 'Suministros Técnicos Hidalgo S.A. de C.V.' },
  { value: '3', label: 'Comercializadora de Equipos Gubernamentales' },
  { value: '4', label: 'Papelería y Suministros Oficina Total' },
  { value: '5', label: 'Constructora y Servicios Múltiples del Estado' }
];

export const BUDGET_KEY_OPTIONS: SelectOption[] = [
  { value: '', label: 'Todas las claves' },
  { value: '2110', label: '2110 - Materiales de Administración' },
  { value: '2120', label: '2120 - Materiales y Artículos de Construcción' },
  { value: '2140', label: '2140 - Materiales y Artículos Metálicos' },
  { value: '2150', label: '2150 - Material Eléctrico y Electrónico' },
  { value: '2160', label: '2160 - Material de Limpieza' },
  { value: '2170', label: '2170 - Materiales y Útiles de Impresión' }
];

export const ORDER_TYPE_OPTIONS: SelectOption[] = [
  { value: '', label: 'Todos los tipos' },
  { value: '1', label: 'Normal' },
  { value: '2', label: 'Urgente' },
  { value: '3', label: 'Programado' },
  { value: '4', label: 'Especial' }
];

export const ORDER_STATUS_OPTIONS: SelectOption[] = [
  { value: '', label: 'Todos los estados' },
  { value: '1', label: 'Borrador' },
  { value: '2', label: 'Pendiente' },
  { value: '3', label: 'Completo' },
  { value: '4', label: 'Parcial' }
];
