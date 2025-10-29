import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Order, OrderFilters } from '../models';
import { PedidoApiItem, PedidoApiResponse } from '../models/interfaces/pedido-api.interface';
import { PEDIDOS_ENDPOINTS } from './pedidos-api.config';
import { PedidoMapper } from '../mapper/pedido.mapper';

@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly http = inject(HttpClient);

  searchOrders(filters: OrderFilters): Observable<Order[]> {
    let params = new HttpParams();

    params = params.set('startRowIndex', '1');
    params = params.set('maximumRows', '100');

    const whereConditions: string[] = [];

    if (filters.folio) {
      whereConditions.push(`pedido.folio LIKE '%${filters.folio}%'`);
    }

    if (filters.supplier) {
      whereConditions.push(`pedido.id_proveedor=${filters.supplier}`);
    }

    if (filters.budgetKey) {
      whereConditions.push(`pedido.numero_contrato='${filters.budgetKey}'`);
    }

    if (filters.type) {
      whereConditions.push(`pedido.id_tipo_pedido=${filters.type}`);
    }

    if (filters.status) {
      whereConditions.push(`pedido.id_estado_pedido=${filters.status}`);
    }

    if (filters.dateFrom && filters.dateTo) {
      whereConditions.push(`pedido.fecha_pedido BETWEEN '${filters.dateFrom}' AND '${filters.dateTo}'`);
    } else if (filters.dateFrom) {
      whereConditions.push(`pedido.fecha_pedido >= '${filters.dateFrom}'`);
    } else if (filters.dateTo) {
      whereConditions.push(`pedido.fecha_pedido <= '${filters.dateTo}'`);
    }

    if (whereConditions.length > 0) {
      params = params.set('Where', whereConditions.join(' AND '));
    }

    params = params.set('OrderBy', 'pedido.id_pedido DESC');

    const fullUrl = `${PEDIDOS_ENDPOINTS.list}?${params.toString()}`;

    return this.http.get<PedidoApiResponse>(
      PEDIDOS_ENDPOINTS.list,
      { params }
    ).pipe(
      map(response => {
        return response?.pedidos ? PedidoMapper.toOrders(response.pedidos) : [];
      })
    );
  }

  getOrderById(id: number): Observable<Order | null> {
    const params = new HttpParams().set('id', id.toString());

    return this.http.get<PedidoApiItem>(PEDIDOS_ENDPOINTS.getById, { params }).pipe(
      map(response => response ? PedidoMapper.toOrder(response) : null)
    );
  }
}
