import { OrderType } from '../enums/order-type.enum';
import { OrderStatus } from '../enums/order-status.enum';

export interface Order {
  folio: string;
  partida: string;
  fechaPedido: string;
  tipo: OrderType;
  iniciales: string;
  proveedor: string;
  rfc: string;
  clavePresupuestal: string;
  nombrePartida: string;
  cantidad: number;
  cantidadSurtida: number;
  precio: number;
  unidad: string;
  observaciones: string;
  descripcion: string;
  estado: OrderStatus;
  contacto: string;
  telefono: string;
  email: string;
}

export interface OrderFilters {
  year: string;
  folio: string;
  supplier: string;
  budgetKey: string;
  type: string;
  dateFrom: string;
  dateTo: string;
  status: string;
}

export interface OrderStats {
  totalOrders: number;
  totalAmount: number;
  completedOrders: number;
  pendingOrders: number;
}
