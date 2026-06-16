using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepositorio; 
        private readonly IGenericRepository<DetalleVenta> _detalleVentaRepositorio;
        private readonly IMapper _mapper;

        public VentaService(IVentaRepository ventaRepositorio, 
            IGenericRepository<DetalleVenta> detalleVentaRepositorio, 
            IMapper mapper)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _mapper = mapper;
        }

        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            if (modelo.DetalleVenta is null || modelo.DetalleVenta.Count == 0)
                throw new TaskCanceledException("Debe incluir al menos un producto en la venta");
            if (string.IsNullOrWhiteSpace(modelo.TipoPago))
                throw new TaskCanceledException("Debe especificar el tipo de pago");
            if (modelo.DetalleVenta.Any(d => d.IdProducto is null or 0))
                throw new TaskCanceledException("Debe especificar un producto válido en cada detalle");

            var ventaGenerada = await _ventaRepositorio.Registrar(_mapper.Map<Venta>(modelo));
            if (ventaGenerada.IdVenta == 0)
                throw new TaskCanceledException("No se pudo crear la venta");
            return _mapper.Map<VentaDTO>(ventaGenerada);
        }

        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _ventaRepositorio.Consultar();
            var ListaResultado = new List<Venta>();

            if(buscarPor =="fecha")
            {
                DateTime fech_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-CO"));
                DateTime fech_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-CO"));

                ListaResultado = await query.Where(v =>
                    v.FechaRegistro.Value.Date >= fech_Inicio.Date && 
                    v.FechaRegistro.Value.Date <= fech_Fin.Date
                ).Include(dv => dv.DetalleVenta)
                .ThenInclude(p=>p.IdProductoNavigation)
                .ToListAsync();
            }
            else
            {
                ListaResultado = await query.Where(v => v.NumeroDocumento == numeroVenta
                ).Include(dv => dv.DetalleVenta)
                .ThenInclude(p => p.IdProductoNavigation)
                .ToListAsync();
            }

            return _mapper.Map<List<VentaDTO>>(ListaResultado);
        }

        public async Task<List<ReporteDTO>> Reporte(string fechaInicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detalleVentaRepositorio.Consultar();
            var ListaResultado = new List<DetalleVenta>();
            DateTime fech_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-CO"));
            DateTime fech_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-CO"));

            ListaResultado = await query
                .Include(p => p.IdProductoNavigation)
                .Include(v => v.IdVentaNavigation)
                .Where(dv =>
                dv.IdVentaNavigation.FechaRegistro.Value.Date >= fech_Inicio.Date &&
                dv.IdVentaNavigation.FechaRegistro.Value.Date <= fech_Fin.Date
                ).ToListAsync();

            return _mapper.Map<List<ReporteDTO>>(ListaResultado); 
        }
    }
}
