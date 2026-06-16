using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;

namespace SistemaVenta.DAL.Repositorios

{
    public class VentaRepository: GenericRepository<Venta>,IVentaRepository
    {
        private readonly DbventangularContext _dbcontext;

        public VentaRepository(DbventangularContext dbcontext):base(dbcontext) 
        {
            _dbcontext = dbcontext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();

            using(var transaction = _dbcontext.Database.BeginTransaction())
            {
                try 
                {
                    foreach (DetalleVenta dv in modelo.DetalleVenta)
                    {

                        if (dv.Cantidad <= 0)
                            throw new Exception("Debe ingresar cantidad mayor o igual a 1");

                        var producto_encontrado = await _dbcontext.Productos
                            .Where(p => p.IdProducto == dv.IdProducto)
                            .FirstOrDefaultAsync()
                            ?? throw new Exception("Producto no encontrado");

                        if (producto_encontrado.Stock < dv.Cantidad)
                            throw new Exception("Stock insuficiente");

                        producto_encontrado.Stock -= dv.Cantidad;
                        _dbcontext.Productos.Update(producto_encontrado);

                    }
                    await _dbcontext.SaveChangesAsync();

                    var correlativo = await _dbcontext.NumeroDocumentos.FirstOrDefaultAsync()
                        ?? throw new Exception("Numero de documento no configurado");

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaRegistro = DateTime.Now;

                    _dbcontext.NumeroDocumentos.Update(correlativo);
                    await _dbcontext.SaveChangesAsync();

                    //0001 para generar este formato
                    int cantidadDigitos = 4;
                    string ceros = string.Concat(Enumerable.Repeat("0", cantidadDigitos));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();

                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - cantidadDigitos, cantidadDigitos);
                    modelo.NumeroDocumento = numeroVenta;
                    modelo.FechaRegistro = DateTime.Now;

                    await _dbcontext.Venta.AddAsync(modelo);
                    await _dbcontext.SaveChangesAsync();

                    ventaGenerada = modelo;
                    transaction.Commit();

                } 
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return ventaGenerada;
            }
        }
    }
}
