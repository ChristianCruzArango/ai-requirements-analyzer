import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrdersApi } from '../../service/orders.service';
import { Order, OrderFilters, OrderStats, OrderStatus } from '../../models';
import { getInitialFilters } from '../../config/filters.config';
import {
  AVAILABLE_YEARS,
  SUPPLIER_OPTIONS,
  BUDGET_KEY_OPTIONS,
  ORDER_TYPE_OPTIONS,
  ORDER_STATUS_OPTIONS
} from '../../config/select-options.config';

@Component({
  standalone: true,
  selector: 'app-orders',
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.html',
  styleUrls: ['./orders.scss']
})
export class OrdersComponent {

  private readonly api = inject(OrdersApi);

  private readonly _items = signal<Order[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _filters = signal<OrderFilters>(getInitialFilters());

  readonly orders = computed(() => this._items());
  readonly isLoading = computed(() => this._isLoading());
  readonly errorMessage = computed(() => this._errorMessage());
  readonly filters = computed(() => this._filters());
  readonly hasResults = computed(() => this._items().length > 0);

  readonly stats = computed<OrderStats>(() => {
    const orders = this._items();
    const completedOrders = orders.filter(o => o.estado === OrderStatus.COMPLETO).length;
    const pendingOrders = orders.filter(o => o.estado === OrderStatus.PENDIENTE).length;
    const totalAmount = orders.reduce((sum, order) => sum + (order.cantidad * order.precio), 0);

    return {
      totalOrders: orders.length,
      totalAmount,
      completedOrders,
      pendingOrders
    };
  });

  selectedOrderForDetail: Order | null = null;
  showAdvancedFiltersModal = false;

  readonly availableYears = AVAILABLE_YEARS;
  readonly supplierOptions = SUPPLIER_OPTIONS;
  readonly budgetKeyOptions = BUDGET_KEY_OPTIONS;
  readonly orderTypeOptions = ORDER_TYPE_OPTIONS;
  readonly orderStatusOptions = ORDER_STATUS_OPTIONS;

  constructor() {}

  searchOrders(): void {
    const currentFilters = this._filters();

    this._isLoading.set(true);
    this._errorMessage.set(null);
    this._items.set([]);

    this.api.searchOrders(currentFilters).subscribe({
      next: (results) => {
        this._items.set(results);

        if (results.length === 0) {
          this._errorMessage.set('No se encontraron resultados con los filtros aplicados.');
        }
        this._isLoading.set(false);
      },
      error: (error) => {
        this._errorMessage.set('Error al buscar pedidos. Intente nuevamente.');
        this._isLoading.set(false);
      }
    });
  }

  clearFilters(): void {
    this._filters.set(getInitialFilters());
    this._items.set([]);
    this._errorMessage.set(null);
  }

  updateFilter(field: string, value: string): void {
    this._filters.update(current => ({ ...current, [field]: value }));
  }

  showOrderDetail(order: Order): void {
    this.selectedOrderForDetail = order;
  }

  closeOrderDetail(): void {
    this.selectedOrderForDetail = null;
  }

  toggleAdvancedFilters(): void {
    this.showAdvancedFiltersModal = !this.showAdvancedFiltersModal;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-MX', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
  }

  calculateTotal(order: Order): number {
    return order.cantidad * order.precio;
  }

  calculateSubtotal(order: Order): number {
    return order.cantidad * order.precio;
  }

  calculateIVA(order: Order): number {
    return this.calculateSubtotal(order) * 0.16;
  }

  calculateTotalWithIVA(order: Order): number {
    return this.calculateSubtotal(order) + this.calculateIVA(order);
  }

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.COMPLETO:
        return 'status-completed';
      case OrderStatus.PENDIENTE:
        return 'status-pending';
      case OrderStatus.PARCIAL:
        return 'status-partial';
      default:
        return '';
    }
  }

  exportToExcel(): void {
    alert('Funcionalidad de exportación a Excel en desarrollo');
  }

  exportToPDF(): void {
    alert('Funcionalidad de exportación a PDF en desarrollo');
  }

  printReport(): void {
    window.print();
  }

  printOrderDetail(): void {
    window.print();
  }
}
