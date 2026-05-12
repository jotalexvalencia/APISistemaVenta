using Xunit;
using Moq;
using AutoMapper; // <--- Agregado
using SistemaVenta.BLL.Servicios;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;
using SistemaVenta.DTO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SistemaVenta.Tests
{
    /// <summary>
    /// Contains unit tests for the VentaService class, ensuring correct behavior under various conditions.
    /// </summary>
    /// <remarks>This class is designed to validate the functionality of the VentaService methods,
    /// particularly focusing on scenarios such as registering sales with insufficient stock. Each test method follows
    /// the Arrange-Act-Assert pattern to clearly define the setup, execution, and verification of outcomes.</remarks>
    public class VentaServiceTests
    {
        [Fact]
        public async Task Registrar_VentaSinStock_DebeLanzarExcepcion()
        {
            // ARRANGE (Preparar el escenario)

            // 1. Simulamos los repositorios
            var mockVentaRepo = new Mock<IVentaRepository>();
            var mockDetalleRepo = new Mock<IGenericRepository<DetalleVenta>>();
            var mockProductoRepo = new Mock<IGenericRepository<Producto>>();
            var mockMapper = new Mock<IMapper>(); // <--- Simulamos el Mapper

            // 2. Configuramos el producto con Stock 0
            mockProductoRepo
                .Setup(r => r.Obtener(It.IsAny<Expression<Func<Producto, bool>>>()))
                .ReturnsAsync(new Producto { IdProducto = 1, Stock = 0, Precio = 100, Nombre = "Producto Test" });

            // 3. Instanciamos el servicio
            var service = new VentaService(
                mockVentaRepo.Object,
                mockDetalleRepo.Object,
                mockProductoRepo.Object,
                mockMapper.Object // <--- Pasamos el simulador, NO null
            );

            var ventaDto = new VentaDTO
            {
                DetalleVenta = new List<DetalleVentaDTO>
                {
                    new DetalleVentaDTO { IdProducto = 1, Cantidad = 1 }
                }
            };

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => service.Registrar(ventaDto));
            Console.WriteLine($"El mensaje fue: {exception.Message}");
            Assert.Contains("Stock insuficiente", exception.Message);
        }
    }
}