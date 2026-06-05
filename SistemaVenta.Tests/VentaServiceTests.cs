using Xunit;
using Moq;
using AutoMapper;
using SistemaVenta.BLL.Servicios;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;
using SistemaVenta.DTO;
using System.Threading.Tasks;

namespace SistemaVenta.Tests
{
    public class VentaServiceTests
    {
        [Fact]
        public async Task Registrar_VentaExitosa_RetornaVentaDTO()
        {
            var mockVentaRepo = new Mock<IVentaRepository>();
            var mockDetalleRepo = new Mock<IGenericRepository<DetalleVenta>>();
            var mockMapper = new Mock<IMapper>();

            var ventaDto = new VentaDTO();
            var ventaModel = new Venta { IdVenta = 1 };

            mockMapper.Setup(m => m.Map<Venta>(ventaDto)).Returns(ventaModel);
            mockVentaRepo.Setup(r => r.Registrar(ventaModel)).ReturnsAsync(ventaModel);
            mockMapper.Setup(m => m.Map<VentaDTO>(ventaModel)).Returns(ventaDto);

            var service = new VentaService(mockVentaRepo.Object, mockDetalleRepo.Object, mockMapper.Object);

            var result = await service.Registrar(ventaDto);

            Assert.NotNull(result);
        }
    }
}