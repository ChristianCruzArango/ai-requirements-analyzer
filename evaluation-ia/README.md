# Evaluation IA - Frontend Angular

Sistema de evaluación y análisis de especificaciones de software usando Inteligencia Artificial. Aplicación web desarrollada con Angular 20.3 que permite consultar pedidos y analizar especificaciones de software con IA.

## 📋 Descripción

Este proyecto frontend está dividido en dos ejercicios principales:

### Ejercicio 1: Consulta de Pedidos
Sistema de consulta y gestión de pedidos de licitaciones. Permite filtrar, visualizar y analizar pedidos de proveedores con estadísticas en tiempo real.

**Video demostración**: [Ver en YouTube](https://youtu.be/ARs44lQqi2w)

**Características:**
- Búsqueda avanzada de pedidos con múltiples filtros
- Visualización de estadísticas (total de pedidos, monto total, pedidos completados/pendientes)
- Detalle completo de cada pedido con cálculos de IVA
- Filtros por año, proveedor, clave presupuestal, tipo y estado
- Interfaz responsive con Bootstrap 5

### Ejercicio 2: Análisis de Especificaciones con IA
Herramienta que usa Inteligencia Artificial para analizar especificaciones de software y generar automáticamente procesos, subprocesos y casos de uso.

**Video demostración**: [Ver en YouTube](https://youtu.be/V-VEs5XVrx8)

**Características:**
- Análisis automático de especificaciones usando IA (OpenRouter)
- 3 tipos de análisis: Detallado, Solo Procesos/Subprocesos, Básico
- Visualización jerárquica de procesos, subprocesos y casos de uso
- Modales de detalle para cada elemento
- Integración con backend .NET para procesamiento con IA

## 🛠️ Tecnologías

- **Angular 20.3** - Framework frontend
- **TypeScript 5.9** - Lenguaje de programación
- **RxJS 7.8** - Programación reactiva
- **Bootstrap 5** - Framework CSS
- **Font Awesome** - Iconos
- **Signals** - Gestión de estado reactiva de Angular
- **Standalone Components** - Arquitectura moderna de Angular

## 📁 Estructura del Proyecto

```
src/
├── app/
│   ├── core/                    # Servicios core (auth, HTTP)
│   │   ├── auth/               # Autenticación y tokens
│   │   └── http/               # Interceptores HTTP
│   │
│   ├── shared/                  # Componentes compartidos
│   │   └── layout/             # Layout principal
│   │
│   └── features/               # Módulos funcionales
│       ├── ejercicio1/         # Consulta de Pedidos
│       │   ├── components/    # Componentes UI
│       │   ├── service/       # API service
│       │   ├── models/        # Interfaces y enums
│       │   └── config/        # Configuración
│       │
│       └── ejercicio2/         # Análisis con IA
│           ├── components/    # Componentes UI
│           ├── service/       # API service
│           └── models/        # Interfaces y enums
│
└── environments/               # Configuración de entornos
```

## 🚀 Instalación y Ejecución

### Requisitos Previos
- Node.js 18+
- npm 9+
- Angular CLI 20.3+

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
cd evaluation-ia
```

2. **Instalar dependencias**
```bash
npm install
```

3. **Configurar variables de entorno**

Editar `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://108.60.201.12/WebApiLicitacionesCaasim',  // API de pedidos
  analysisApiUrl: 'http://localhost:5000/api',                   // API de análisis IA
  auth: {
    username: 'admin',
    password: 'admin'
  }
};
```

4. **Ejecutar en desarrollo**
```bash
npm start
```

La aplicación estará disponible en `http://localhost:4200`

5. **Compilar para producción**
```bash
npm run build
```

Los archivos compilados estarán en `dist/`

## 🔧 Configuración

### API de Pedidos (Ejercicio 1)
- **URL**: Configurada en `environment.apiBaseUrl`
- **Autenticación**: OAuth 2.0 con username/password
- **Interceptor**: Agrega automáticamente el token Bearer a todas las peticiones

### API de Análisis IA (Ejercicio 2)
- **URL**: Configurada en `environment.analysisApiUrl`
- **Backend**: .NET 9 API (ver proyecto `evaluation-api`)
- **Sin autenticación**: CORS configurado para desarrollo

## 📦 Dependencias Principales

```json
{
  "@angular/core": "^20.3.0",
  "@angular/common": "^20.3.0",
  "@angular/forms": "^20.3.0",
  "@angular/router": "^20.3.0",
  "rxjs": "~7.8.0"
}
```

## 🏗️ Arquitectura

### Patrón Feature-Based
Cada ejercicio está organizado como una feature independiente con su propia estructura:
- **Components**: Componentes standalone UI
- **Service**: Servicios para comunicación con API
- **Models**: Interfaces TypeScript y enums
- **Config**: Archivos de configuración

### Gestión de Estado
- **Signals**: Estado reactivo nativo de Angular
- **Computed**: Valores derivados calculados automáticamente
- **Sin NgRx**: Arquitectura simple y mantenible

### Comunicación HTTP
- **HttpClient**: Cliente HTTP nativo de Angular
- **Interceptores**: Autenticación automática con tokens
- **Observables**: Manejo reactivo de peticiones asíncronas

## 🎨 Estilos

- **Bootstrap 5**: Framework CSS principal
- **SCSS**: Preprocesador CSS
- **Font Awesome**: Librería de iconos
- **Responsive Design**: Mobile-first approach

## 📝 Scripts Disponibles

```bash
npm start          # Ejecutar en desarrollo (puerto 4200)
npm run build      # Compilar para producción
npm run watch      # Compilar con watch mode
npm test           # Ejecutar tests unitarios
```

## 🔐 Autenticación

El sistema implementa autenticación OAuth 2.0:

1. Al iniciar la aplicación, se solicita automáticamente un token
2. El token se almacena en memoria y localStorage
3. Todas las peticiones al API de pedidos incluyen el token Bearer
4. El interceptor agrega automáticamente el header Authorization

## 🌐 Rutas

```typescript
/                          # Home / Dashboard
/ejercicio1               # Consulta de Pedidos
/ejercicio2               # Análisis con IA
```

## 📊 Features del Ejercicio 1

- Filtros avanzados (año, proveedor, clave, tipo, estado)
- Búsqueda en tiempo real
- Estadísticas dinámicas
- Detalle de pedido con modal
- Cálculo automático de IVA (16%)
- Exportación (en desarrollo)

## 🤖 Features del Ejercicio 2

- 3 modos de análisis:
  - **Detallado**: Procesos + Subprocesos + Casos de Uso
  - **Procesos**: Solo Procesos y Subprocesos
  - **Básico**: Solo Procesos principales
- Visualización jerárquica
- Modales de detalle para cada elemento
- Integración con OpenRouter AI
- Exportación (en desarrollo)

## 🔗 Integración con Backend

Este frontend se integra con dos backends:

1. **API de Licitaciones** (`apiBaseUrl`)
   - Gestión de pedidos
   - Requiere autenticación

2. **API de Análisis IA** (`analysisApiUrl`)
   - Backend .NET 9 (ver `evaluation-api`)
   - Procesamiento con OpenRouter AI
   - Sin autenticación

## 🐛 Troubleshooting

### El frontend no puede conectarse al backend de IA
- Verificar que `evaluation-api` esté corriendo en puerto 5000
- Revisar configuración CORS en el backend
- Verificar `environment.analysisApiUrl`

### Error de autenticación en Ejercicio 1
- Verificar credenciales en `environment.auth`
- Revisar que la API de licitaciones esté disponible
- Verificar conexión de red

### Puerto 4200 ocupado
```bash
# Usar otro puerto
ng serve --port 4201
```

## 👨‍💻 Desarrollo

### Agregar nueva feature
1. Crear carpeta en `src/app/features/nueva-feature`
2. Crear componentes standalone
3. Crear service para API
4. Definir modelos/interfaces
5. Agregar rutas en `app.routes.ts`

### Buenas prácticas
- Usar signals para estado reactivo
- Componentes standalone (sin NgModules)
- Servicios con `providedIn: 'root'`
- Interfaces TypeScript para todos los modelos
- Nombres descriptivos (no abreviaturas)

## 📄 Licencia

Proyecto de evaluación técnica - Uso educativo
