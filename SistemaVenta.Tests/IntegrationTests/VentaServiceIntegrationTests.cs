using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaVenta.BLL.Servicios;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;
using SistemaVenta.Tests.IntegrationTests;
using SistemaVenta.Utility;

namespace SistemaVenta.Tests
{
    public class VentaServiceIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly IMapper _mapper;

        public VentaServiceIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;

            var cfg = new MapperConfigurationExpression();
            cfg.AddProfile<AutoMapperProfile>();
            var config = new MapperConfiguration(cfg, NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();
        }

        private VentaService CrearServicio(DbventangularContext context)
        {
            var ventaRepo = new VentaRepository(context);
            var detalleRepo = new GenericRepository<DetalleVenta>(context);
            return new VentaService(ventaRepo, detalleRepo, _mapper);
        }

        [Fact]
        public async Task Registrar_VentaExitosa_RetornaVentaDTO()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                TipoPago = "Efectivo",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 1, Cantidad = 2, PrecioTexto = "100,50", TotalTexto = "201,00" }
                }
            };

            var result = await service.Registrar(dto);

            Assert.NotNull(result);
            Assert.True(result.IdVenta > 0);
            Assert.Equal("Efectivo", result.TipoPago);
        }

        [Fact]
        public async Task Registrar_SinTipoPago_LanzaExcepcion()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 1, Cantidad = 1, PrecioTexto = "100,50", TotalTexto = "100,50" }
                }
            };

            var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => service.Registrar(dto));
            Assert.Contains("tipo de pago", ex.Message);
        }

        [Fact]
        public async Task Registrar_StockInsuficiente_LanzaExcepcion()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                TipoPago = "Efectivo",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 3, Cantidad = 10, PrecioTexto = "999,00", TotalTexto = "9990,00" }
                }
            };

            await Assert.ThrowsAsync<Exception>(() => service.Registrar(dto));
        }

        [Fact]
        public async Task Registrar_StockSeReduceCorrectamente()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                TipoPago = "Tarjeta",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 1, Cantidad = 3, PrecioTexto = "100,50", TotalTexto = "301,50" }
                }
            };

            await service.Registrar(dto);

            var producto = await context.Productos.FindAsync(1);
            Assert.NotNull(producto);
            Assert.Equal(7, producto.Stock);
        }

        [Fact]
        public async Task Registrar_GeneraNumeroDocumento()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                TipoPago = "Efectivo",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 2, Cantidad = 1, PrecioTexto = "50,25", TotalTexto = "50,25" }
                }
            };

            var result = await service.Registrar(dto);

            Assert.NotNull(result.NumeroDocumento);
            Assert.Matches(@"^\d{4}$", result.NumeroDocumento);
        }

        [Fact]
        public async Task Historial_PorNumeroDocumento_RetornaVenta()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            var dto = new VentaDTO
            {
                TipoPago = "Efectivo",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 2, Cantidad = 1, PrecioTexto = "50,25", TotalTexto = "50,25" }
                }
            };

            var creada = await service.Registrar(dto);

            var historial = await service.Historial("numero", creada.NumeroDocumento!, "", "");

            Assert.NotEmpty(historial);
            Assert.Equal(creada.NumeroDocumento, historial[0].NumeroDocumento);
        }

        [Fact]
        public async Task Historial_PorFecha_RetornaVentas()
        {
            using var context = _fixture.CreateContext();
            var service = CrearServicio(context);

            await service.Registrar(new VentaDTO
            {
                TipoPago = "Efectivo",
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new() { IdProducto = 1, Cantidad = 1, PrecioTexto = "100,50", TotalTexto = "100,50" }
                }
            });

            var hoy = DateTime.Now.ToString("dd/MM/yyyy");
            var manana = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");

            var historial = await service.Historial("fecha", "", hoy, manana);

            Assert.NotEmpty(historial);
        }
    }
}
