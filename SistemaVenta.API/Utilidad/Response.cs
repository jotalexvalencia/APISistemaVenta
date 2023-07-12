using Microsoft.Identity.Client;

namespace SistemaVenta.API.Utilidad
{
    // como respuesta a todas las solicitudes de nuestras apis
    public class Response<T>
    {
        public bool status {  get; set; } 
        public T Value { get; set; }
        public string msg { get; set; }
    }
}
