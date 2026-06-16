using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.Tests.IntegrationTests
{
    public class TestDbventangularContext : DbventangularContext
    {
        public TestDbventangularContext(DbContextOptions<DbventangularContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.GetDefaultValueSql() == "(getdate())"))
                {
                    property.SetDefaultValueSql("datetime('now')");
                }
            }
        }
    }

    public class DatabaseFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public DatabaseFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<DbventangularContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new TestDbventangularContext(options);
            context.Database.EnsureCreated();
            SeedData(context);
        }

        private static void SeedData(DbventangularContext context)
        {
            context.Categoria.Add(new Model.Categoria
            {
                Nombre = "Electrónicos",
                EsActivo = true,
                FechaRegistro = DateTime.Now
            });
            context.SaveChanges();

            context.Productos.AddRange(
                new Model.Producto
                {
                    Nombre = "Producto A",
                    IdCategoria = 1,
                    Stock = 10,
                    Precio = 100.50m,
                    EsActivo = true,
                    FechaRegistro = DateTime.Now
                },
                new Model.Producto
                {
                    Nombre = "Producto B",
                    IdCategoria = 1,
                    Stock = 5,
                    Precio = 50.25m,
                    EsActivo = true,
                    FechaRegistro = DateTime.Now
                },
                new Model.Producto
                {
                    Nombre = "Producto C (stock bajo)",
                    IdCategoria = 1,
                    Stock = 1,
                    Precio = 999m,
                    EsActivo = true,
                    FechaRegistro = DateTime.Now
                }
            );
            context.SaveChanges();

            context.NumeroDocumentos.Add(new Model.NumeroDocumento
            {
                UltimoNumero = 0,
                FechaRegistro = DateTime.Now
            });
            context.SaveChanges();
        }

        public DbventangularContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DbventangularContext>()
                .UseSqlite(_connection)
                .Options;

            return new TestDbventangularContext(options);
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
