using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGestionEventos.Models;
using ProyectoGestionEventos.Helpers;

namespace ProyectoGestionEventos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ActividadHelper _actividadHelper;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ActividadHelper actividadHelper)
        {
            _logger = logger;
            _context = context;
            _actividadHelper = actividadHelper;
        }

        //Authorize sirve para cualquier usuario logeado
        [Authorize]
        public IActionResult Index()
        {
            // Métricas básicas
            ViewBag.TotalUsuarios = _context.Usuarios.Count(u => u.Estado == true);
            ViewBag.TotalEspacios = _context.Espacios.Count(e => e.Estado == true);
            ViewBag.TotalEventos = _context.Eventos.Count(e => e.Estado == true);
            ViewBag.TotalRecursos = _context.Recursos.Count(r => r.Estado == true);
            
            // Métricas de reservas
            var reservas = _context.Reservas.Where(r => r.EstadoReserva != "Cancelada").ToList();
            ViewBag.TotalReservas = reservas.Count;
            ViewBag.ReservasPendientes = _context.Reservas.Count(r => r.EstadoReserva == "Pendiente");
            ViewBag.ReservasAprobadas = _context.Reservas.Count(r => r.EstadoReserva == "Aprobada");
            ViewBag.ReservasRechazadas = _context.Reservas.Count(r => r.EstadoReserva == "Rechazada");
            
            // Reservas próximas (próximos 7 días)
            var hoy = DateTime.Now;
            var proximaSemana = hoy.AddDays(7);
            ViewBag.ReservasProximas = _context.Reservas
                .Count(r => r.EstadoReserva == "Aprobada" && r.FechaInicio >= hoy && r.FechaInicio <= proximaSemana);
            
            // Reservas hoy
            var inicioDia = hoy.Date;
            var finDia = inicioDia.AddDays(1);
            ViewBag.ReservasHoy = _context.Reservas
                .Count(r => r.EstadoReserva == "Aprobada" && r.FechaInicio >= inicioDia && r.FechaInicio < finDia);
            
            // Distribución de reservas por estado
            ViewBag.PorcentajeAprobadas = ViewBag.TotalReservas > 0 
                ? Math.Round((double)ViewBag.ReservasAprobadas / ViewBag.TotalReservas * 100, 1) 
                : 0;
            ViewBag.PorcentajePendientes = ViewBag.TotalReservas > 0 
                ? Math.Round((double)ViewBag.ReservasPendientes / ViewBag.TotalReservas * 100, 1) 
                : 0;
            ViewBag.PorcentajeRechazadas = ViewBag.TotalReservas > 0 
                ? Math.Round((double)ViewBag.ReservasRechazadas / ViewBag.TotalReservas * 100, 1) 
                : 0;
            
            // Eventos por tipo
            var eventosPorTipo = _context.Eventos
                .Where(e => e.Estado == true)
                .GroupBy(e => e.TipoEvento)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToList();
            ViewBag.EventosPorTipo = eventosPorTipo;
            
            // Top 5 espacios más reservados
            var espaciosMasReservados = _context.Reservas
                .Where(r => r.EstadoReserva == "Aprobada")
                .GroupBy(r => r.IdEspacio)
                .Select(g => new { 
                    IdEspacio = g.Key, 
                    Cantidad = g.Count() 
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();
            
            var espaciosTop = espaciosMasReservados.Select(e => new {
                Nombre = _context.Espacios.FirstOrDefault(es => es.IdEspacio == e.IdEspacio)?.Nombre ?? "Desconocido",
                Cantidad = e.Cantidad
            }).ToList();
            ViewBag.EspaciosTop = espaciosTop;
            
            // Reservas del mes actual
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);
            ViewBag.ReservasMesActual = _context.Reservas
                .Count(r => r.FechaInicio >= inicioMes && r.FechaInicio < finMes);
            
            // Tasa de ocupación (reservas aprobadas vs total de reservas)
            ViewBag.TasaAprobacion = ViewBag.TotalReservas > 0 
                ? Math.Round((double)ViewBag.ReservasAprobadas / ViewBag.TotalReservas * 100, 1) 
                : 0;
            
            // Recursos más utilizados
            var recursosMasUsados = _context.ReservaRecursos
                .GroupBy(rr => rr.IdRecurso)
                .Select(g => new { 
                    IdRecurso = g.Key, 
                    Cantidad = g.Sum(x => x.CantidadSolicitada) 
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();
            
            var recursosTop = recursosMasUsados.Select(r => new {
                Nombre = _context.Recursos.FirstOrDefault(rec => rec.IdRecurso == r.IdRecurso)?.Nombre ?? "Desconocido",
                Cantidad = r.Cantidad
            }).ToList();
            ViewBag.RecursosTop = recursosTop;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //AllowAnonymous indica que puede ser accedido por cualquier persona
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe ingresar email y/o contraseña";
                return View();
            }
            var user = await _context.Usuarios.Include(x => x.IdRolNavigation).FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Email y/o contraseña incorrectas.";
                return View();
            }
            // utilizamos claims para guardar la información del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.IdRolNavigation.Rol1),
            };
            //creamos identidad y principal para las coockies
            var claimsIdentify = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentify);
            // Iniciar sesión con autenticación por cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            //asignamos a una variable sesión el nombre de usuario ingresado
            HttpContext.Session.SetString("nombre", user.Nombre + " " + user.Apellido);
            TempData["nombre"] = HttpContext.Session.GetString("nombre");
            
            // Registrar actividad de login
            await _actividadHelper.RegistrarActividad("Login", "Sistema", $"Inicio de sesión exitoso");
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            // Registrar actividad de logout antes de cerrar sesión
            await _actividadHelper.RegistrarActividad("Logout", "Sistema", "Cierre de sesión");
            
            // cerrar la sesión 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.Clear();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Búsqueda Global
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> BusquedaGlobal(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return Json(new List<ResultadoBusquedaGlobal>());
            }

            var resultados = new List<ResultadoBusquedaGlobal>();
            termino = termino.Trim();

            // Buscar en Reservas
            var reservas = await _context.Reservas
                .Include(r => r.IdEspacioNavigation)
                .Include(r => r.IdEventoNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Where(r => r.IdReserva.ToString().Contains(termino) ||
                           r.IdEspacioNavigation!.Nombre.Contains(termino) ||
                           r.IdEventoNavigation!.Titulo.Contains(termino) ||
                           r.EstadoReserva!.Contains(termino))
                .Take(5)
                .ToListAsync();

            foreach (var reserva in reservas)
            {
                resultados.Add(new ResultadoBusquedaGlobal
                {
                    Tipo = "Reserva",
                    Id = reserva.IdReserva,
                    Titulo = $"Reserva #{reserva.IdReserva}",
                    Subtitulo = reserva.IdEspacioNavigation?.Nombre,
                    Descripcion = $"{reserva.IdEventoNavigation?.Titulo} - {reserva.EstadoReserva}",
                    UrlDetalle = $"/Reservas/Edit/{reserva.IdReserva}",
                    Icono = "fa-calendar-check",
                    ColorBadge = reserva.EstadoReserva == "Aprobada" ? "success" : 
                                reserva.EstadoReserva == "Pendiente" ? "warning" : "danger"
                });
            }

            // Buscar en Espacios
            var espacios = await _context.Espacios
                .Where(e => e.Estado == true &&
                           (e.Nombre.Contains(termino) ||
                            e.Ubicacion.Contains(termino) ||
                            e.TipoEspacio.Contains(termino)))
                .Take(5)
                .ToListAsync();

            foreach (var espacio in espacios)
            {
                resultados.Add(new ResultadoBusquedaGlobal
                {
                    Tipo = "Espacio",
                    Id = espacio.IdEspacio,
                    Titulo = espacio.Nombre,
                    Subtitulo = espacio.TipoEspacio,
                    Descripcion = $"{espacio.Ubicacion} - Capacidad: {espacio.Capacidad}",
                    UrlDetalle = $"/Espacios/Edit/{espacio.IdEspacio}",
                    Icono = "fa-building",
                    ColorBadge = "primary"
                });
            }

            // Buscar en Eventos
            var eventos = await _context.Eventos
                .Where(e => e.Estado == true &&
                           (e.Titulo.Contains(termino) ||
                            (e.Descripcion ?? "").Contains(termino) ||
                            e.TipoEvento.Contains(termino)))
                .Take(5)
                .ToListAsync();

            foreach (var evento in eventos)
            {
                resultados.Add(new ResultadoBusquedaGlobal
                {
                    Tipo = "Evento",
                    Id = evento.IdEvento,
                    Titulo = evento.Titulo,
                    Subtitulo = evento.TipoEvento,
                    Descripcion = evento.Descripcion,
                    UrlDetalle = $"/Eventos/Edit/{evento.IdEvento}",
                    Icono = "fa-calendar-alt",
                    ColorBadge = "info"
                });
            }

            // Buscar en Usuarios (solo si es admin)
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Administrador" || userRole == "SuperAdministrador")
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Estado == true &&
                               (u.Nombre.Contains(termino) ||
                                u.Apellido.Contains(termino) ||
                                u.Email.Contains(termino) ||
                                u.Run.Contains(termino)))
                    .Take(5)
                    .ToListAsync();

                foreach (var usuario in usuarios)
                {
                    resultados.Add(new ResultadoBusquedaGlobal
                    {
                        Tipo = "Usuario",
                        Id = usuario.IdUsuario,
                        Titulo = $"{usuario.Nombre} {usuario.Apellido}",
                        Subtitulo = usuario.TipoUsuario,
                        Descripcion = usuario.Email,
                        UrlDetalle = $"/Usuarios/Edit/{usuario.IdUsuario}",
                        Icono = "fa-user",
                        ColorBadge = "secondary"
                    });
                }
            }

            // Buscar en Recursos
            var recursos = await _context.Recursos
                .Where(r => r.Estado == true &&
                           (r.Nombre.Contains(termino) ||
                            (r.Descripcion ?? "").Contains(termino)))
                .Take(5)
                .ToListAsync();

            foreach (var recurso in recursos)
            {
                resultados.Add(new ResultadoBusquedaGlobal
                {
                    Tipo = "Recurso",
                    Id = recurso.IdRecurso,
                    Titulo = recurso.Nombre,
                    Subtitulo = $"Disponible: {recurso.CantidadDisponible}",
                    Descripcion = recurso.Descripcion,
                    UrlDetalle = $"/Recursos/Edit/{recurso.IdRecurso}",
                    Icono = "fa-box",
                    ColorBadge = "warning"
                });
            }

            return Json(resultados.Take(10));
        }

        // Configuración de Usuario
        [Authorize]
        public async Task<IActionResult> Configuracion()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);
            
            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            var configuracion = await _context.ConfiguracionesUsuario
                .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (configuracion == null)
            {
                configuracion = new ConfiguracionUsuario
                {
                    IdUsuario = userId,
                    Tema = "claro",
                    NotificacionesEmail = true,
                    NotificacionesReservas = true,
                    NotificacionesEventos = true,
                    Idioma = "es",
                    ElementosPorPagina = 10,
                    FechaActualizacion = DateTime.Now
                };
                _context.ConfiguracionesUsuario.Add(configuracion);
                await _context.SaveChangesAsync();
            }

            ViewBag.Usuario = usuario;
            return View(configuracion);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Configuracion(ConfiguracionUsuario model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var configuracion = await _context.ConfiguracionesUsuario
                .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (configuracion != null)
            {
                configuracion.Tema = model.Tema;
                configuracion.NotificacionesEmail = model.NotificacionesEmail;
                configuracion.NotificacionesReservas = model.NotificacionesReservas;
                configuracion.NotificacionesEventos = model.NotificacionesEventos;
                configuracion.Idioma = model.Idioma;
                configuracion.ElementosPorPagina = model.ElementosPorPagina;
                configuracion.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();
                await _actividadHelper.RegistrarActividad("Actualizar", "Configuración", "Configuración actualizada");
                
                TempData["mensaje"] = "Configuración actualizada correctamente";
            }

            return RedirectToAction("Configuracion");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CambiarPassword(string passwordActual, string passwordNueva, string passwordConfirmar)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado";
                return RedirectToAction("Configuracion");
            }

            if (usuario.Password != passwordActual)
            {
                TempData["error"] = "La contraseña actual es incorrecta";
                return RedirectToAction("Configuracion");
            }

            if (passwordNueva != passwordConfirmar)
            {
                TempData["error"] = "Las contraseñas nuevas no coinciden";
                return RedirectToAction("Configuracion");
            }

            if (passwordNueva.Length < 6)
            {
                TempData["error"] = "La contraseña debe tener al menos 6 caracteres";
                return RedirectToAction("Configuracion");
            }

            usuario.Password = passwordNueva;
            await _context.SaveChangesAsync();
            await _actividadHelper.RegistrarActividad("Cambio Password", "Seguridad", "Contraseña actualizada");

            TempData["mensaje"] = "Contraseña actualizada correctamente";
            return RedirectToAction("Configuracion");
        }

        // Registro de Actividad
        [Authorize]
        public async Task<IActionResult> RegistroActividad(int pagina = 1)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<ActividadUsuario> query = _context.ActividadesUsuario
                .Include(a => a.Usuario);

            // Si no es admin, solo mostrar sus propias actividades
            if (userRole != "Administrador" && userRole != "SuperAdministrador")
            {
                query = query.Where(a => a.IdUsuario == userId);
            }

            var elementosPorPagina = 20;
            var totalActividades = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalActividades / (double)elementosPorPagina);

            var actividades = await query
                .OrderByDescending(a => a.FechaHora)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalActividades = totalActividades;

            return View(actividades);
        }
    }
}

