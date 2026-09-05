import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Interceptor HTTP funcional para inyectar automáticamente las cabeceras de trazabilidad
 * en cada solicitud saliente hacia el Backend:
 * - systemid: Identificador del cliente consumidor ('inter-rapidisimo-web')
 * - uuid: Identificador único por petición (UUID v4)
 * - timestamp: Marca de tiempo ISO-8601 UTC de despacho
 */
export const requestHeadersInterceptor: HttpInterceptorFn = (req, next) => {
  const clonedRequest = req.clone({
    setHeaders: {
      systemid: 'inter-rapidisimo-web',
      uuid: crypto.randomUUID(),
      timestamp: new Date().toISOString()
    }
  });

  return next(clonedRequest);
};
