/**
 * Envoltorio estandarizado de respuesta para las peticiones a la Web API.
 */
export interface ApiResponse<T> {
  estado: boolean;
  descripcion: string;
  data: T;
}
