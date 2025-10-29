import { Order, OrderType, OrderStatus } from '../models';
import { PedidoApiItem } from '../models/interfaces/pedido-api.interface';

export class PedidoMapper {
  static toOrder(item: PedidoApiItem): Order {
    return {
      folio: item.folio || '',
      partida: item.consecutivo_completo || '',
      fechaPedido: item.fecha_pedido || '',
      tipo: this.mapTipo(item.id_tipo_pedido_cat_tipo_pedido?.descripcion),
      iniciales: item.iniciales || '',
      proveedor: item.id_proveedor_cat_proveedor?.razon_social || '',
      rfc: item.id_proveedor_cat_proveedor?.rfc || '',
      clavePresupuestal: item.numero_contrato || '',
      nombrePartida: item.destinatario_factura || '',
      cantidad: 1,
      cantidadSurtida: this.mapCantidadSurtida(item.id_estado_surtido),
      precio: item.monto_total || 0,
      unidad: 'PZA',
      observaciones: item.observaciones || '',
      descripcion: item.direccion_entrega || '',
      estado: this.mapEstado(item.id_estado_pedido_cat_estado_pedido?.descripcion),
      contacto: item.id_proveedor_cat_proveedor?.nombre_contacto || '',
      telefono: item.id_proveedor_cat_proveedor?.telefono || '',
      email: item.id_proveedor_cat_proveedor?.correo_electronico || ''
    };
  }

  static toOrders(items: PedidoApiItem[]): Order[] {
    return items.map(item => this.toOrder(item));
  }

  private static mapTipo(descripcion?: string): OrderType {
    if (!descripcion) return OrderType.NORMAL;

    const desc = descripcion.toLowerCase();
    if (desc.includes('urgente')) return OrderType.URGENTE;
    if (desc.includes('programado')) return OrderType.PROGRAMADO;
    if (desc.includes('especial')) return OrderType.ESPECIAL;

    return OrderType.NORMAL;
  }

  private static mapEstado(descripcion?: string): OrderStatus {
    if (!descripcion) return OrderStatus.PENDIENTE;

    const desc = descripcion.toLowerCase();
    if (desc.includes('completo') || desc.includes('finalizado')) return OrderStatus.COMPLETO;
    if (desc.includes('parcial')) return OrderStatus.PARCIAL;

    return OrderStatus.PENDIENTE;
  }

  private static mapCantidadSurtida(idEstadoSurtido: number): number {
    if (idEstadoSurtido === 3) return 1;
    if (idEstadoSurtido === 2) return 0.5;
    return 0;
  }
}
