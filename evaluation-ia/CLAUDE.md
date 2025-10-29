
# Angular Frontend Best Practices (Arquitectura Feature + Signals)

> Opinadas para Angular 18+ / 19+ / 20+, usando **standalone components**, **signals**, **computed**, **effects**, TanStack Query, y arquitectura limpia por feature.  
> Objetivo: código mantenible, escalable y fácil de testear.

---

## 1. Estructura de carpetas por feature

Cada módulo funcional de negocio vive en su propia carpeta `feature/` con límite claro.  
Ejemplo para una feature `customers`:

```text
src/
 ├─ app/
 │   ├─ core/                       # Infra transversal: auth, http, api-clients, guards, ui-core
 │   │   ├─ auth/
 │   │   ├─ http/
 │   │   ├─ logging/
 │   │   └─ translation/
 │   │
 │   ├─ shared/                     # Componentes UI reusables (inputs, tablas, etc.)
 │   │   ├─ ui/
 │   │   ├─ forms/
 │   │   └─ pipes/
 │   │
 │   ├─ features/
 │   │   ├─ customers/
 │   │   │   ├─ service/                # Facade HTTP y queries remotas
 │   │   │   │   ├─ customer.service.ts
 │   │   │   │   └─ customer.query.ts
 │   │   │   │
 │   │   │   ├─ state/              # signals para UI state de esta feature
 │   │   │   │   ├─ customer.store.ts
 │   │   │   │   └─ customer.types.ts
 │   │   │   │
 │   │   │   ├─ components/         # Presentational / smart components standalone
 │   │   │   │   ├─ customer-list/
 │   │   │   │   │   ├─ customer-list.component.ts
 │   │   │   │   │   ├─ customer-list.component.html
 │   │   │   │   │   └─ customer-list.component.scss
 │   │   │   │   ├─ customer-detail-form/
 │   │   │   │   │   ├─ customer-detail-form.component.ts
 │   │   │   │   │   ├─ customer-detail-form.component.html
 │   │   │   │   │   └─ customer-detail-form.component.scss
 │   │   │   │   └─ ...
 │   │   │   │
 │   │   │   ├─ pages/              # Rutas (screens) standalone
 │   │   │   │   ├─ customers-page.component.ts
 │   │   │   │   └─ customer-edit-page.component.ts
 │   │   │   │
 │   │   │   ├─ customers.routes.ts # Rutas lazy de la feature
 │   │   │   └─ index.ts            # Barrel: export público controlado
 │   │   │
 │   │   └─ other-feature/
 │   │
 │   ├─ app.routes.ts               # Rutas raíz (lazy load features)
 │   └─ main.ts
 │
 └─ environments/
     ├─ environment.ts
     └─ environment.prod.ts
```

### Reglas:
- Nada de `shared/utils/service-that-knows-everything`.
- Cada feature es dueña de su estado, de su API y de sus páginas.
- `core/` es solo infraestructura reusable (auth, http client, logging, interceptors, translations, etc.).
- `shared/` son componentes UI puros (inputs, tablas, chips). **Sin lógica de negocio.**

---

## 2. Señales (signals) en vez de `@Input` mutables o `BehaviorSubject`

### Por qué signals
- Reemplazan `BehaviorSubject`, `EventEmitter` interno, `@Output` de spam.
- Son síncronas, fáciles de testear y más seguras que streams sueltos.
- `computed` asegura valores derivados sin recalcular manualmente.
- `effect` reacciona a cambios de estado (mostrar toast, habilitar botón, etc.), NO para lógica de negocio core.

---

## 3. State store por feature

Cada feature tiene su propio "store" con signals puros (sin NgRx pesado, sin services dios).

**Archivo:** `customer.store.ts`

```ts
import { signal, computed, effect, Injectable } from '@angular/core';
import { Customer } from './customer.types';
import { CustomerApi } from '../api/customer.api';
import { LoggingService } from '@app/core/logging/logging.service';

@Injectable({ providedIn: 'root' })
export class CustomerStore {
  // === STATE BASE ===
  private readonly _items = signal<Customer[]>([]);
  private readonly _selectedId = signal<string | null>(null);
  private readonly _isSaving = signal(false);
  private readonly _errorMessage = signal<string | null>(null);

  // === DERIVADOS / COMPUTED ===
  readonly items = computed(() => this._items());
  readonly isSaving = computed(() => this._isSaving());
  readonly errorMessage = computed(() => this._errorMessage());

  readonly selectedCustomer = computed<Customer | null>(() => {
    const id = this._selectedId();
    if (!id) return null;
    return this._items().find(c => c.id === id) ?? null;
  });

  readonly hasChanges = signal(false); // puede ser manejado con form service centralizado

  constructor(
    private readonly api: CustomerApi,
    private readonly logger: LoggingService,
  ) {
    // Reacción de UI (side effect): cuando hay error, mostrar notificación
    effect(() => {
      const message = this._errorMessage();
      if (message) {
        this.logger.showError(message);
      }
    });
  }

  // === ACCIONES PÚBLICAS (INTENCIONES DE NEGOCIO) ===
  // Regla: los métodos describen intención, no eventos de UI.
  // Prohibido prefijos tipo 'onClick', 'onChange', 'ngOnInitHandler'.
  // Deben sonar a "qué hace el caso de uso".
  loadAllCustomers(): void {
    this.api.getAll().then(result => {
      this._items.set(result);
    }).catch(err => {
      this._errorMessage.set('No se pudieron cargar los clientes');
      console.error(err);
    });
  }

  selectCustomer(id: string | null): void {
    this._selectedId.set(id);
    this.hasChanges.set(false);
  }

  updateCustomerDraft(partial: Partial<Customer>): void {
    // Actualiza en memoria el item seleccionado sin guardar en backend
    const current = this.selectedCustomer();
    if (!current) return;
    const updated = { ...current, ...partial };

    this._items.update(list =>
      list.map(c => (c.id === current.id ? updated : c)),
    );

    this.hasChanges.set(true);
  }

  async saveSelectedCustomer(): Promise<void> {
    const current = this.selectedCustomer();
    if (!current) return;

    this._isSaving.set(true);
    this._errorMessage.set(null);

    try {
      const saved = await this.api.update(current.id, current);
      // Sincronizar store con lo que devolvió el backend:
      this._items.update(list =>
        list.map(c => (c.id === saved.id ? saved : c)),
      );
      this.hasChanges.set(false);
      this.logger.showSuccess('Cliente actualizado');
    } catch (err) {
      this._errorMessage.set('No se pudo guardar el cliente');
      console.error(err);
    } finally {
      this._isSaving.set(false);
    }
  }
}
```

### Buenas prácticas aplicadas:
- Signals privados con `_` + nombre.
- Exponer `readonly` computed en vez de exponer directamente los signals privados.
- Métodos con intención de negocio:
  - ✅ `loadAllCustomers`
  - ✅ `selectCustomer`
  - ✅ `saveSelectedCustomer`
  - ❌ `onRowClick`
  - ❌ `handleSaveClick`
  - ❌ `ngOnInitLoad`

---

## 4. Los nombres de las funciones

### Reglas de nombres:
1. **Nada de prefijos de UI como `on`, `handle`, `btn`, `click`, `change`.**  
   - ❌ `onCustomerClick()`
   - ✅ `selectCustomer()`

2. **Los métodos describen la intención, no el evento:**  
   - ❌ `onSaveButton()`  
   - ✅ `saveSelectedCustomer()`

3. **Usa verbos claros al inicio:**  
   - `load...`, `select...`, `create...`, `remove...`, `reset...`, `toggle...`, `apply...`, `submit...`

4. **Evita abreviaturas innecesarias:**  
   - ❌ `updCust()`  
   - ✅ `updateCustomerDraft()`

5. **Los computed NO usan `get` en el nombre.**  
   - ❌ `getSelectedCustomer` como `computed`  
   - ✅ `selectedCustomer` como `computed`

6. **Los signals booleanos deben sonar a estado, no a acción:**  
   - ✅ `isSaving`, `isOpen`, `hasChanges`, `isLoadingList`
   - ❌ `saving`, `loadingList`, `openState`

---

## 5. Componentes Standalone

Los componentes consumen el store vía inyección directa y NO manejan estado duplicado.  
Nada de `customers: Customer[] = [];` en el componente, usa signals/computed.

**Archivo:** `customer-list.component.ts`

```ts
import { Component, inject, computed, signal } from '@angular/core';
import { CustomerStore } from '../../state/customer.store';
import { NgIf, NgFor } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss'],
  imports: [NgIf, NgFor],
})
export class CustomerListComponent {
  private readonly store = inject(CustomerStore);

  // Signals locales SOLO para UI interna del componente
  private readonly _searchTerm = signal('');
  readonly searchTerm = computed(() => this._searchTerm());

  // Estado proyectado/derivado para la vista
  readonly filteredCustomers = computed(() => {
    const term = this._searchTerm().toLowerCase().trim();
    if (!term) return this.store.items();
    return this.store.items().filter(c =>
      c.fullName.toLowerCase().includes(term) ||
      c.email.toLowerCase().includes(term),
    );
  });

  readonly isSaving = this.store.isSaving;
  readonly selectedCustomer = this.store.selectedCustomer;

  constructor() {
    // Cargar data al montar
    this.store.loadAllCustomers();
  }

  setSearchTerm(value: string): void {
    this._searchTerm.set(value);
  }

  selectRow(customerId: string): void {
    this.store.selectCustomer(customerId);
  }

  saveChanges(): void {
    this.store.saveSelectedCustomer();
  }
}
```

### Reglas aplicadas:
- `constructor()` puede llamar flujo inicial como `this.store.loadAllCustomers()` (esto reemplaza el típico `ngOnInit()`).
- El componente NO conoce HTTP, solo el store.
- Los métodos del componente igual siguen naming de intención:
  - ✅ `setSearchTerm`
  - ✅ `selectRow`
  - ✅ `saveChanges`
  - ❌ `onRowClick`
  - ❌ `handleSearchChange`

---

## 6. Rutas por feature (lazy)

Cada feature expone sus páginas standalone y se carga lazy vía `loadChildren`.

**Archivo:** `customers.routes.ts`

```ts
import { Routes } from '@angular/router';
import { CustomersPageComponent } from './pages/customers-page.component';
import { CustomerEditPageComponent } from './pages/customer-edit-page.component';

export const CUSTOMERS_ROUTES: Routes = [
  {
    path: '',
    component: CustomersPageComponent,
  },
  {
    path: ':id',
    component: CustomerEditPageComponent,
  },
];
```

**Archivo raíz:** `app.routes.ts`

```ts
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: 'customers',
    loadChildren: () =>
      import('./features/customers/customers.routes').then(m => m.CUSTOMERS_ROUTES),
  },
  // otras features...
];
```

### Buenas prácticas aplicadas:
- Cada feature controla sus rutas.
- `app.routes.ts` solo sabe que `customers` existe, no sus internals.
- Los pages son componentes standalone que orquestan layout + subcomponentes.

---

## 7. API / Facade HTTP de la feature

Nunca pegues `HttpClient` directo en el componente.  
Cada feature tiene su `Api` y allí se centraliza la comunicación con backend:

**Archivo:** `customer.api.ts`

```ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Customer } from '../state/customer.types';

@Injectable({ providedIn: 'root' })
export class CustomerApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/customers';

  getAll(): Promise<Customer[]> {
    return this.http.get<Customer[]>(this.baseUrl).toPromise();
  }

  getById(id: string): Promise<Customer> {
    return this.http.get<Customer>(`${this.baseUrl}/${id}`).toPromise();
  }

  update(id: string, payload: Customer): Promise<Customer> {
    return this.http.put<Customer>(`${this.baseUrl}/${id}`, payload).toPromise();
  }

  create(payload: Customer): Promise<Customer> {
    return this.http.post<Customer>(this.baseUrl, payload).toPromise();
  }
}
```

### Buenas prácticas aplicadas:
- `CustomerApi` es la única puerta HTTP de la feature.
- Fácil de mockear en tests.
- El store llama esta capa, no el componente.

---

## 8. Form state con signals (sin `ngModel` suelto)

Ejemplo de un formulario standalone que edita el cliente seleccionado usando `FormGroup` pero trackeando cambios con signals.

**Archivo:** `customer-detail-form.component.ts`

```ts
import { Component, inject, computed, effect } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomerStore } from '../../state/customer.store';
import { NgIf } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-customer-detail-form',
  templateUrl: './customer-detail-form.component.html',
  styleUrls: ['./customer-detail-form.component.scss'],
  imports: [ReactiveFormsModule, NgIf],
})
export class CustomerDetailFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(CustomerStore);

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    isActive: [true],
  });

  // computed: el cliente activo
  readonly model = this.store.selectedCustomer;

  // computed: el botón Guardar solo se habilita si hay cambios, no se está guardando y el form es válido
  readonly canSave = computed(() => {
    return (
      this.form.valid &&
      this.store.hasChanges() &&
      !this.store.isSaving()
    );
  });

  constructor() {
    // Cuando cambia el cliente seleccionado, se actualiza el form
    effect(() => {
      const current = this.model();
      if (!current) return;
      this.form.patchValue(
        {
          fullName: current.fullName,
          email: current.email,
          phone: current.phone ?? '',
          isActive: current.isActive,
        },
        { emitEvent: false },
      );

      this.store.hasChanges.set(false);
    });

    // Cuando el usuario edita algo en el form => marcar hasChanges y actualizar draft del store
    this.form.valueChanges.subscribe(val => {
      this.store.updateCustomerDraft({
        fullName: val.fullName ?? '',
        email: val.email ?? '',
        phone: val.phone ?? '',
        isActive: !!val.isActive,
      });
    });
  }

  saveCustomer(): void {
    if (!this.canSave()) return;
    this.store.saveSelectedCustomer();
  }
}
```

### Reglas aplicadas:
- `canSave` es `computed`, no un getter de clase.
- `saveCustomer()` (intención) en vez de `onSaveClick()`.
- El form NO guarda solo; la acción final la maneja el store.

---

## 9. Resumen de reglas duras

1. **Arquitectura por feature**  
   - Cada feature tiene `api/`, `state/`, `components/`, `pages/`, `*.routes.ts`.

2. **Signals en vez de Subjects**  
   - Estado UI -> `signal`.
   - Valores derivados -> `computed`.
   - Reacciones de UI/hud/toasts -> `effect`.

3. **Nombres de métodos claros (intención)**  
   - Siempre verbo + intención de negocio.  
   - Nada con `on`, `handle`, `click`, `change`, `init`, `ngOnInitX`.

4. **El componente no toca HTTP ni guarda directamente en backend**  
   - El componente habla con el store.
   - El store habla con la API.
   - La API habla con el backend.

5. **El store es dueño del estado**  
   - `CustomerStore` decide qué es seleccionado, si hay cambios, etc.
   - Evita estado duplicado en varios componentes.

6. **No exportes todo**  
   - Usa `index.ts` dentro de la feature para exponer solo lo que otro módulo necesita.

7. **UI reusable vive en `shared/`**  
   - Inputs, tablas, chips, badges, etc.
   - Ninguna referencia directa a negocio (`Customer`, `Order`, etc.) en `shared/`.

8. **Efecft solo para side effects visibles**  
   - Mostrar toast, sync form, escuchar cambios de selección.
   - Nunca meter lógica de dominio pesada en un `effect`.

---

## 10. Mini guía express de nombres correctos

### ✅ Correcto
- `loadAllCustomers`
- `selectCustomer`
- `saveSelectedCustomer`
- `setSearchTerm`
- `updateCustomerDraft`
- `resetFormState`
- `toggleSidebarVisibility`
- `applyFilters`
- `createCustomer`
- `removeCustomer`

### ❌ Incorrecto
- `onRowClick`
- `handleSaveButton`
- `onSearchChange`
- `ngOnInitLoadCustomers`
- `saveBtn`
- `processData`
- `doStuff`

---

## 11. Conclusión

- Usa **signals/computed/effect** para TODO el estado de UI y derivaciones.
- El store de la feature es tu “fuente de la verdad”.
- Componentes = presentación + orquestación mínima.
- Nombra las funciones como intención, no como evento.
- Las carpetas cuentan una historia clara: `api/`, `state/`, `components/`, `pages/`, `*.routes.ts`.

Listo. Esto es la base para que tu Angular se vea serio, entendible y listo para equipo grande. 🚀


## 12. NOMENGLATURA CORRECTA
- No utilizar ngfor para eso utilizamos @for
- No utilizar ngif par eso utilizamos @if
