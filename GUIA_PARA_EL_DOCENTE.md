# Ricos Jochos — Guía rápida para explicar el PIA

App de escritorio **WinUI 3 (.NET 8)** para punto de venta de hot dogs.  
Caso de negocio: **Ricos Jochos**.

---

## 1. Estructura del proyecto

```
Pia_Apps.sln
└── Punto_de_venta/          ← proyecto WinUI (un solo exe)
    ├── App.xaml / App.xaml.cs     ← arranque de la app
    ├── Login.xaml(+.cs)           ← acceso con usuarios
    ├── MenuPrincipal.xaml(+.cs)   ← menú según rol
    ├── Inventario.xaml(+.cs)      ← CRUD de ingredientes
    ├── Clientes.xaml(+.cs)        ← CRUD de clientes
    ├── users.json                 ← usuarios y roles (archivo local)
    └── PosPia/                    ← lógica de venta / BD
        ├── DatabaseHelper.cs      ← SQLite (tablas, consultas, ventas)
        ├── MainWindow.xaml(+.cs)  ← pantalla de venta / carrito
        ├── *Dialog.xaml(+.cs)     ← pago, ticket, corte, cliente
        ├── Modelos/               ← clases de datos (Producto, Cliente…)
        └── *Converter.cs          ← conversión para bindings XAML
```

**Idea clave:** la UI está en XAML; la lógica en C# (code-behind).  
`PosPia` concentra la venta y la base de datos; `Punto_de_venta` el acceso y módulos de gestión.

---

## 2. Funcionamiento general (flujo)

1. **App** inicia → crea/abre BD SQLite → abre **Login**.
2. **Login** lee `users.json`, valida usuario/contraseña.
3. Si es correcto → **Menú principal** (pasa el usuario/rol).
4. Desde el menú:
   - **Venta** → `PosPia.MainWindow` (productos, carrito, cobro, corte).
   - **Inventario** → solo si el rol es **Gerente**.
   - **Clientes** → catálogo CRUD.
5. En venta: elegir productos → carrito → (opcional) cliente → **Cobrar** → diálogo de pago → ticket / vista previa.
6. **Corte del día** consulta ventas del día en SQLite y muestra totales.

Base de datos:  
`Documentos\RicosJochos\ricosjochos.db`

Usuarios de prueba:
| Usuario | Contraseña | Rol     |
|---------|------------|---------|
| admin   | admin      | Gerente |
| caja    | caja       | Cajero  |

---

## 3. Sintaxis / elementos de código que se ven en clase

### C# (code-behind)
- **Namespaces:** `Punto_de_venta`, `PosPia`, `PosPia.Modelos`.
- **Clases `partial`:** la mitad está en `.xaml.cs` y la otra la genera el compilador XAML.
- **Eventos:** `Click="OnLoginClick"` en XAML → método `OnLoginClick(...)` en C#.
- **Colecciones:** `List<T>`, `ObservableCollection<T>` para listas ligadas a la UI.
- **SQLite:** `Microsoft.Data.Sqlite` — `SqliteConnection`, `CreateCommand`, `ExecuteNonQuery` / `ExecuteReader`.
- **JSON:** `System.Text.Json` para deserializar `users.json`.
- **async / await:** diálogos (`ContentDialog.ShowAsync`) y operaciones que no bloquean la UI.

Ejemplo típico (login):
```csharp
var user = _users.FirstOrDefault(u =>
    u.Username == usuario && u.Password == contrasena);
```

Ejemplo típico (BD):
```csharp
using (var connection = GetConnection())
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT ...";
    // ExecuteReader / ExecuteNonQuery
}
```

### XAML (interfaz)
- Controles: `Window`, `Grid`, `StackPanel`, `Button`, `TextBox`, `ListView`, `GridView`, `ContentDialog`.
- **Binding:** `{Binding Nombre}`, `{x:Bind ...}` para mostrar datos del modelo.
- **x:Name:** para referenciar controles desde C# (`UsernameTextBox.Text`).
- **Converters:** clases que transforman datos para la UI (precio a texto, bool a opacidad).

---

## 4. Módulos y responsabilidad (qué hace cada parte)

| Archivo / carpeta        | Qué explicar |
|--------------------------|--------------|
| `App.xaml.cs`            | Punto de entrada: inicia BD y muestra Login |
| `Login`                  | Autenticación con JSON + roles |
| `Menu_principal`         | Navegación; oculta Inventario si no es Gerente |
| `DatabaseHelper`         | Crea tablas, consultas, inserta venta, descuenta stock |
| `MainWindow` (venta)     | Catálogo, carrito, totales, IVA, cobro |
| Diálogos (`PagoDialog`…) | Flujos modales: pago, ticket, corte, elegir cliente |
| `Modelos`                | POCO / entidades (Producto, Cliente, CarritoItem…) |
| `Inventario` / `Clientes`| Altas, bajas, cambios sobre tablas SQLite |

---

## 5. Puntos clave preguntables (prepárate esto)

1. **¿Qué es WinUI 3?** Framework de UI moderna para apps de escritorio Windows (XAML + C#).
2. **¿Dónde se guardan los datos?** En SQLite (`ricosjochos.db` en Documentos), no solo en memoria.
3. **¿Cómo funciona el login?** Lee `users.json`, compara usuario/contraseña, según **Role** muestra u oculta Inventario.
4. **¿Qué es un code-behind?** El `.xaml.cs` que responde a eventos de la interfaz definida en XAML.
5. **¿Qué es un modelo?** Clase que representa un dato del negocio (ej. `Producto` con Nombre, Precio, Categoria).
6. **¿Cómo se calcula el total de venta?** Suma de subtotales del carrito + IVA (16%).
7. **¿Qué pasa al cobrar?** Se abre `PagoDialog` (efectivo/transferencia); luego se registra la venta en BD y se puede mostrar ticket.
8. **¿Para qué sirve `DatabaseHelper`?** Centraliza acceso a SQLite (una sola clase estática con métodos de lectura/escritura).
9. **¿Qué es un `ContentDialog`?** Ventana modal dentro de la app (pago, ticket, corte).
10. **¿Por qué el menú de productos puede salir vacío?** En este avance el *seed* de datos está vacío a propósito; las tablas existen, faltan cargar productos reales de Ricos Jochos.
11. **¿Qué es binding?** Enlazar propiedades del objeto (modelo) a controles XAML para que la UI se actualice.
12. **¿Diferencia Gerente vs Cajero?** Gerente ve Inventario; Cajero no (restricción por rol en `ApplyRoleRestrictions`).

---

## 6. Cómo hablarlo en 1 minuto (guion corto)

> “Es un punto de venta WinUI para mi negocio Ricos Jochos. Al iniciar abre login con usuarios en JSON y roles. El menú lleva a venta, clientes e inventario (solo gerente). La venta usa SQLite: productos, carrito, cobro con diálogo, ticket y corte de caja. La UI está en XAML y la lógica en C#, con una clase `DatabaseHelper` para la base de datos.”

---

## 7. Compilar / demostrar

- Visual Studio → solución `Pia_Apps.sln`
- Plataforma **x64** → **F5**
- Login `admin` / `admin`
