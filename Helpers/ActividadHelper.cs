using Microsoft.AspNetCore.Http;
using ProyectoGestionEventos.Models;
using System.Security.Claims;

namespace ProyectoGestionEventos.Helpers
{
    public class ActividadHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActividadHelper(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarActividad(string tipoAccion, string modulo, string? descripcion = null)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return;

                var actividad = new ActividadUsuario
                {
                    IdUsuario = int.Parse(userId),
                    TipoAccion = tipoAccion,
                    Modulo = modulo,
                    Descripcion = descripcion,
                    FechaHora = DateTime.Now,
                    DireccionIP = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
                };

                _context.ActividadesUsuario.Add(actividad);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Silenciosamente fallar para no interrumpir el flujo de la aplicación
            }
        }

        public static string FormatearAccion(string entidad, string accion, string? detalle = null)
        {
            var descripcion = $"{accion} {entidad}";
            if (!string.IsNullOrEmpty(detalle))
            {
                descripcion += $": {detalle}";
            }
            return descripcion;
        }
    }
}
