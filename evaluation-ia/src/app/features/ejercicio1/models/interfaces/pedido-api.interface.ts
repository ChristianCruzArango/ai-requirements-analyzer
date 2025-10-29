export interface PedidoApiItem {
  id_pedido: number;
  folio: string;
  consecutivo_completo: string;
  id_tipo_documento_pedido: number;
  id_tipo_pedido: number;
  id_proveedor: number;
  id_procedimiento_adquisicion: number;
  fecha_pedido: string;
  numero_contrato: string;
  destinatario_factura: string;
  direccion_entrega: string;
  fecha_entrega: string;
  tiempo_entrega: string;
  persona_elaboro: string;
  persona_autorizo: string;
  iniciales: string;
  subtotal: number;
  total_iva: number;
  total_retenciones: number;
  monto_total: number;
  id_estado_pedido: number;
  id_estado_surtido: number;
  observaciones: string;
  fecha_registro: string;
  hora_registro: string;
  id_usuario_registro: number;
  fecha_modifica: string;
  hora_modifica: string;
  id_usuario_modifica: number;
  fecha_aprueba: string;
  hora_aprueba: string;
  id_usuario_aprueba: number;
  id_archivo_firma: number | null;
  id_tipo_pedido_cat_tipo_pedido?: {
    id_tipo_pedido: number;
    descripcion: string;
    activo: boolean | null;
    Id: number;
  };
  id_proveedor_cat_proveedor?: {
    id_proveedor: number;
    razon_social: string;
    rfc: string | null;
    tipo_persona: string | null;
    nombre_contacto: string | null;
    correo_electronico: string | null;
    telefono: string | null;
    direccion: string | null;
    activo: boolean | null;
    fecha_registro: string | null;
    id_usuario_registro: number | null;
    Id: number;
  };
  id_estado_pedido_cat_estado_pedido?: {
    id_estado_pedido: number;
    descripcion: string;
    color_badge: string | null;
    Id: number;
  };
  id_estado_surtido_cat_estado_surtido?: {
    id_estado_surtido: number;
    descripcion: string;
    color_badge: string | null;
    Id: number;
  };
}

export interface PedidoApiResponse {
  pedidos: PedidoApiItem[];
}
