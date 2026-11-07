using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using PosPia.Modelos;

namespace PosPia
{
    public static class DatabaseHelper
    {

        #region Métodos de Infraestructura (Sin Cambios)
        private static string dbPath = string.Empty;

        private static string GetDatabasePath()
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appFolder = Path.Combine(documentsFolder, "RicosJochos");
                Directory.CreateDirectory(appFolder);
                dbPath = Path.Combine(appFolder, "ricosjochos.db");
            }
            return dbPath;
        }

        private static SqliteConnection GetConnection()
        {
            string connectionString = $"Data Source={GetDatabasePath()}";
            return new SqliteConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                CreateTables(connection);
            }
        }

        private static void CreateTables(SqliteConnection connection)
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS Ingredientes (
                IdIngrediente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL UNIQUE,
                Descripcion TEXT,
                Stock REAL NOT NULL DEFAULT 0,
                UnidadMedida TEXT NOT NULL 
            );");

            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS Productos (
                IdProducto INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL UNIQUE,
                Descripcion TEXT,
                Categoria TEXT NOT NULL,
                Precio REAL NOT NULL
            );");

           
            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS ProductoIngredientes (
                IdProducto INTEGER NOT NULL,
                IdIngrediente INTEGER NOT NULL,
                Cantidad REAL NOT NULL,
                PRIMARY KEY (IdProducto, IdIngrediente),
                FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
                FOREIGN KEY (IdIngrediente) REFERENCES Ingredientes(IdIngrediente)
            );");

           
            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS Clientes (
                IdCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Empresa TEXT,
                Telefono TEXT,
                Direccion TEXT,
                RFC TEXT UNIQUE
            );");

            
            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS Ventas (
                IdVenta INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha TEXT NOT NULL,
                IdCliente INTEGER,
                Subtotal REAL NOT NULL,
                IVA REAL NOT NULL,
                Total REAL NOT NULL,
                TipoPago TEXT,
                FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente)
            );");

            
            sqlBuilder.AppendLine(@"
            CREATE TABLE IF NOT EXISTS VentaDetalle (
                IdVentaDetalle INTEGER PRIMARY KEY AUTOINCREMENT,
                IdVenta INTEGER NOT NULL,
                IdProducto INTEGER NOT NULL,
                Cantidad INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL, 
                SubtotalProducto REAL NOT NULL,
                FOREIGN KEY (IdVenta) REFERENCES Ventas(IdVenta),
                FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
            );");

            var createCommand = connection.CreateCommand();
            createCommand.CommandText = sqlBuilder.ToString();
            createCommand.ExecuteNonQuery();
        }
        #endregion

        #region Métodos de Poblado de Datos (Sin Cambios)

        public static void SeedInitialData()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;

                        command.CommandText = GetIngredientesData();
                        command.ExecuteNonQuery();

                        command.CommandText = GetProductosData();
                        command.ExecuteNonQuery();

                        command.CommandText = GetRecetasData();
                        command.ExecuteNonQuery();

                        command.CommandText = GetClientesData();
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error poblando datos: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }

        private static string GetIngredientesData()
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT OR IGNORE INTO Ingredientes (Nombre, Stock, UnidadMedida, Descripcion) VALUES ('Pan para hot dog', 80, 'pz', 'Pan suave para jocho');");
            sb.AppendLine("INSERT OR IGNORE INTO Ingredientes (Nombre, Stock, UnidadMedida, Descripcion) VALUES ('Salchicha de res', 40, 'pz', 'Salchicha clasica de res');");
            sb.AppendLine("INSERT OR IGNORE INTO Ingredientes (Nombre, Stock, UnidadMedida, Descripcion) VALUES ('Salchicha de pavo', 30, 'pz', 'Salchicha de pavo');");
            sb.AppendLine("INSERT OR IGNORE INTO Ingredientes (Nombre, Stock, UnidadMedida, Descripcion) VALUES ('Salchicha jumbo', 20, 'pz', 'Salchicha grande');");
            sb.AppendLine("INSERT OR IGNORE INTO Ingredientes (Nombre, Stock, UnidadMedida, Descripcion) VALUES ('Queso americano', 25, 'pz', 'Rebanada de queso');");
            return sb.ToString();
        }

        private static string GetProductosData()
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT OR IGNORE INTO Productos (Nombre, Categoria, Precio, Descripcion) VALUES ('Jocho clasico', 'Jocho', 35.00, 'Pan y salchicha de res.');");
            sb.AppendLine("INSERT OR IGNORE INTO Productos (Nombre, Categoria, Precio, Descripcion) VALUES ('Jocho de pavo', 'Jocho', 38.00, 'Pan y salchicha de pavo.');");
            sb.AppendLine("INSERT OR IGNORE INTO Productos (Nombre, Categoria, Precio, Descripcion) VALUES ('Jocho con queso', 'Jocho', 42.00, 'Pan, salchicha de res y queso.');");
            sb.AppendLine("INSERT OR IGNORE INTO Productos (Nombre, Categoria, Precio, Descripcion) VALUES ('Jocho jumbo', 'Jocho', 48.00, 'Pan y salchicha jumbo.');");
            return sb.ToString();
        }

        private static string GetRecetasData()
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho clasico'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Pan para hot dog'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho clasico'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Salchicha de res'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho de pavo'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Pan para hot dog'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho de pavo'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Salchicha de pavo'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho con queso'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Pan para hot dog'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho con queso'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Salchicha de res'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho con queso'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Queso americano'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho jumbo'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Pan para hot dog'), 1);");
            sb.AppendLine("INSERT OR IGNORE INTO ProductoIngredientes (IdProducto, IdIngrediente, Cantidad) VALUES ((SELECT IdProducto FROM Productos WHERE Nombre = 'Jocho jumbo'), (SELECT IdIngrediente FROM Ingredientes WHERE Nombre = 'Salchicha jumbo'), 1);");
            return sb.ToString();
        }

        private static string GetClientesData()
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT OR IGNORE INTO Clientes (Nombre, Empresa, Telefono, Direccion, RFC) VALUES ('Publico General', 'N/A', '0000000000', 'Mostrador', 'XAXX010101000');");
            return sb.ToString();
        }
        #endregion


        public static List<Modelos.Producto> GetAllProductos()
        {
            var productos = new List<Modelos.Producto>();

            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
                    SELECT
                        p.IdProducto,
                        p.Nombre,
                        p.Descripcion,
                        p.Categoria,
                        p.Precio,
                        CASE
                            WHEN EXISTS (
                                SELECT 1
                                FROM ProductoIngredientes pi
                                JOIN Ingredientes i ON pi.IdIngrediente = i.IdIngrediente
                                WHERE pi.IdProducto = p.IdProducto AND i.Stock < pi.Cantidad
                            )
                            THEN 0
                            ELSE 1
                        END AS EstaDisponible
                    FROM
                        Productos p
                    ORDER BY
                        p.Categoria, p.Nombre";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var producto = new Modelos.Producto
                        {
                            IdProducto = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.GetString(2),
                            Categoria = reader.GetString(3),
                            Precio = reader.GetDouble(4),
                            EstaDisponible = reader.GetInt32(5) == 1
                        };
                        productos.Add(producto);
                    }
                }
            }
            return productos;
        }

        public static List<string> GetAllCategorias()
        {
            var categorias = new List<string>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT Categoria FROM Productos ORDER BY Categoria";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categorias.Add(reader.GetString(0));
                    }
                }
            }
            return categorias;
        }

        public static List<Cliente> GetAllClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();


                command.CommandText = "SELECT IdCliente, Nombre, Empresa, Telefono, Direccion, RFC FROM Clientes ORDER BY Nombre";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {

                            IdCliente = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Empresa = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Direccion = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            RFC = reader.IsDBNull(5) ? "" : reader.GetString(5)      
                        });
                    }
                }
            }
            return clientes;
        }

        public static Modelos.CorteDeCaja GetCorteDeCajaHoy()
        {
            Modelos.CorteDeCaja corte = new Modelos.CorteDeCaja();

            using (var connection = GetConnection())
            {
                connection.Open();

                var commandResumen = connection.CreateCommand();
                commandResumen.CommandText = @"
                    SELECT
                        COUNT(IdVenta) AS CantidadDeVentas,
                        IFNULL(SUM(Total), 0) AS TotalVendido,
                        IFNULL(SUM(CASE WHEN TipoPago = 'Efectivo' THEN Total ELSE 0 END), 0) AS TotalEfectivo,
                        IFNULL(SUM(CASE WHEN TipoPago = 'Transferencia' THEN Total ELSE 0 END), 0) AS TotalTransferencia
                    FROM 
                        Ventas
                    WHERE 
                        DATE(Fecha) = DATE('now', 'localtime');";

                using (var reader = commandResumen.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        corte.CantidadTickets = reader.GetInt32(0);
                        corte.TotalVendido = reader.GetDouble(1);
                        corte.TotalEfectivo = reader.GetDouble(2);
                        corte.TotalTransferencia = reader.GetDouble(3);
                    }
                }

                var commandDetalle = connection.CreateCommand();
                commandDetalle.CommandText = @"
                    SELECT 
                        p.Nombre, 
                        SUM(vd.Cantidad) AS CantidadTotal, 
                        SUM(vd.SubtotalProducto) AS SubtotalTotal,
                        IFNULL(c.Nombre, 'Público General') AS NombreCliente
                    FROM 
                        VentaDetalle vd
                    JOIN 
                        Productos p ON vd.IdProducto = p.IdProducto
                    JOIN 
                        Ventas v ON vd.IdVenta = v.IdVenta
                    LEFT JOIN 
                        Clientes c ON v.IdCliente = c.IdCliente
                    WHERE 
                        DATE(v.Fecha) = DATE('now', 'localtime')
                    GROUP BY 
                        p.Nombre, NombreCliente
                    ORDER BY 
                        NombreCliente, p.Nombre;";

                using (var reader = commandDetalle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var itemVendido = new VentaDetalladaItem
                        {
                            NombreProducto = reader.GetString(0),
                            Cantidad = reader.GetInt32(1), // Usamos SUM() así que es GetInt32
                            Subtotal = reader.GetDouble(2), // Usamos SUM() así que es GetDouble
                            NombreCliente = reader.GetString(3)
                        };
                        corte.VentasDetalladas.Add(itemVendido);
                    }
                }
            }

            return corte;
        }

        public static int GuardarVenta(double subtotal, double iva, double total, string tipoPago, ObservableCollection<CarritoItem> carrito, int? idCliente)
        {
            if (carrito == null || carrito.Count == 0)
            {
                return -1;
            }

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Guardar la Venta (Maestro)
                        var ventaCmd = connection.CreateCommand();
                        ventaCmd.Transaction = transaction;
                        ventaCmd.CommandText = @"
                            INSERT INTO Ventas (Fecha, Subtotal, IVA, Total, TipoPago, IdCliente)
                            VALUES (@Fecha, @Subtotal, @IVA, @Total, @TipoPago, @IdCliente);
                            SELECT last_insert_rowid();";

                        ventaCmd.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        ventaCmd.Parameters.AddWithValue("@Subtotal", subtotal);
                        ventaCmd.Parameters.AddWithValue("@IVA", iva);
                        ventaCmd.Parameters.AddWithValue("@Total", total);
                        ventaCmd.Parameters.AddWithValue("@TipoPago", tipoPago);


                        ventaCmd.Parameters.AddWithValue("@IdCliente", (object)idCliente ?? DBNull.Value);

                        int nuevaVentaId = Convert.ToInt32(ventaCmd.ExecuteScalar());

                        if (nuevaVentaId <= 0)
                        {
                            throw new Exception("No se pudo obtener el IdVenta.");
                        }

                        // 2. Guardar el VentaDetalle
                        var detalleCmd = connection.CreateCommand();
                        detalleCmd.Transaction = transaction;
                        detalleCmd.CommandText = @"
                            INSERT INTO VentaDetalle (IdVenta, IdProducto, Cantidad, PrecioUnitario, SubtotalProducto)
                            VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @SubtotalProducto);";

                        foreach (var item in carrito)
                        {
                            detalleCmd.Parameters.Clear();
                            detalleCmd.Parameters.AddWithValue("@IdVenta", nuevaVentaId);
                            detalleCmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                            detalleCmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                            detalleCmd.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                            detalleCmd.Parameters.AddWithValue("@SubtotalProducto", item.Subtotal);

                            detalleCmd.ExecuteNonQuery();
                        }

                        // 3. (Fase 2) Descontar del inventario
                        var stockCmd = connection.CreateCommand();
                        stockCmd.Transaction = transaction;

                        foreach (var item in carrito)
                        {
                            stockCmd.CommandText = @"SELECT IdIngrediente, Cantidad FROM ProductoIngredientes WHERE IdProducto = @IdProducto";
                            stockCmd.Parameters.Clear();
                            stockCmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);

                            var recetaItems = new List<(int IdIngrediente, double CantidadReceta)>();
                            using (var recetaReader = stockCmd.ExecuteReader())
                            {
                                while (recetaReader.Read())
                                {
                                    recetaItems.Add((recetaReader.GetInt32(0), recetaReader.GetDouble(1)));
                                }
                            }

                            foreach (var (idIngrediente, cantidadReceta) in recetaItems)
                            {
                                double totalADescontar = cantidadReceta * item.Cantidad;

                                stockCmd.CommandText = @"
                                    UPDATE Ingredientes 
                                    SET Stock = Stock - @TotalADescontar 
                                    WHERE IdIngrediente = @IdIngrediente";
                                stockCmd.Parameters.Clear();
                                stockCmd.Parameters.AddWithValue("@TotalADescontar", totalADescontar);
                                stockCmd.Parameters.AddWithValue("@IdIngrediente", idIngrediente);

                                stockCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return nuevaVentaId;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar venta: {ex.Message}");
                        transaction.Rollback();
                        return -1;
                    }
                }
            }
        }

        public static List<Ingrediente> GetAllIngredientes()
        {
            var ingredientes = new List<Ingrediente>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT IdIngrediente, Nombre, Descripcion, Stock, UnidadMedida FROM Ingredientes ORDER BY Nombre";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ingredientes.Add(new Ingrediente
                        {
                            IdIngrediente = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Stock = reader.GetDouble(3),
                            UnidadMedida = reader.GetString(4)
                        });
                    }
                }
            }
            return ingredientes;
        }


        public static Ingrediente AgregarIngrediente(Ingrediente nuevoIngrediente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Ingredientes (Nombre, Descripcion, Stock, UnidadMedida)
                    VALUES (@Nombre, @Descripcion, @Stock, @UnidadMedida);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@Nombre", nuevoIngrediente.Nombre);
                command.Parameters.AddWithValue("@Descripcion", (object)nuevoIngrediente.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("@Stock", nuevoIngrediente.Stock);
                command.Parameters.AddWithValue("@UnidadMedida", nuevoIngrediente.UnidadMedida);

                // Ejecuta y obtiene el nuevo ID
                int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                nuevoIngrediente.IdIngrediente = nuevoId;

                return nuevoIngrediente;
            }
        }


        public static void ActualizarIngrediente(Ingrediente ingrediente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Ingredientes
                    SET Nombre = @Nombre,
                        Descripcion = @Descripcion,
                        Stock = @Stock,
                        UnidadMedida = @UnidadMedida
                    WHERE IdIngrediente = @IdIngrediente";

                command.Parameters.AddWithValue("@Nombre", ingrediente.Nombre);
                command.Parameters.AddWithValue("@Descripcion", (object)ingrediente.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("@Stock", ingrediente.Stock);
                command.Parameters.AddWithValue("@UnidadMedida", ingrediente.UnidadMedida);
                command.Parameters.AddWithValue("@IdIngrediente", ingrediente.IdIngrediente);

                command.ExecuteNonQuery();
            }
        }


        public static void EliminarIngrediente(int idIngrediente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Ingredientes WHERE IdIngrediente = @IdIngrediente";
                command.Parameters.AddWithValue("@IdIngrediente", idIngrediente);

                command.ExecuteNonQuery();
            }
        }



        public static Modelos.Cliente AgregarCliente(Modelos.Cliente cliente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Clientes (Nombre, Empresa, Telefono, Direccion, RFC)
                    VALUES (@Nombre, @Empresa, @Telefono, @Direccion, @RFC);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                command.Parameters.AddWithValue("@Empresa", (object)cliente.Empresa ?? DBNull.Value);
                command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("@RFC", (object)cliente.RFC ?? DBNull.Value);

                // Ejecutamos y obtenemos el nuevo ID
                long newId = (long)command.ExecuteScalar();
                cliente.IdCliente = (int)newId;
                return cliente;
            }
        }

        public static void ActualizarCliente(Modelos.Cliente cliente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Clientes SET
                        Nombre = @Nombre,
                        Empresa = @Empresa,
                        Telefono = @Telefono,
                        Direccion = @Direccion,
                        RFC = @RFC
                    WHERE IdCliente = @IdCliente";

                command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                command.Parameters.AddWithValue("@Empresa", (object)cliente.Empresa ?? DBNull.Value);
                command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("@RFC", (object)cliente.RFC ?? DBNull.Value);
                command.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);

                command.ExecuteNonQuery();
            }
        }

        public static void EliminarCliente(int idCliente)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Clientes WHERE IdCliente = @IdCliente";
                command.Parameters.AddWithValue("@IdCliente", idCliente);
                command.ExecuteNonQuery();
            }
        }

        public static int GetMaximaCantidadParaProducto(int idProducto)
        {
            int maximoPosible = int.MaxValue;

            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Consulta para obtener la receta y el stock actual de CADA ingrediente
                command.CommandText = @"
                    SELECT 
                        pi.Cantidad, 
                        i.Stock
                    FROM 
                        ProductoIngredientes pi
                    JOIN 
                        Ingredientes i ON pi.IdIngrediente = i.IdIngrediente
                    WHERE 
                        pi.IdProducto = @IdProducto";

                command.Parameters.AddWithValue("@IdProducto", idProducto);

                bool tieneReceta = false;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tieneReceta = true;
                        double cantidadRequerida = reader.GetDouble(0);
                        double stockActual = reader.GetDouble(1);

                        if (cantidadRequerida <= 0) continue;

                        int posibleConEsteIngrediente = (int)Math.Floor(stockActual / cantidadRequerida);

                        if (posibleConEsteIngrediente < maximoPosible)
                        {
                            maximoPosible = posibleConEsteIngrediente;
                        }
                    }
                }

                if (!tieneReceta)
                {
                    return 999; // Límite arbitrario alto para productos sin receta
                }

                return maximoPosible;
            }
        }
    }
}
