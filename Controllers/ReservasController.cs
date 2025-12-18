using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoGestionEventos.Models;
using ProyectoGestionEventos.Helpers;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ProyectoGestionEventos.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            
            var reservas = _context.Reservas
                .Include(r => r.IdEspacioNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.EstadoReserva != "Cancelada");
            
            if (!string.IsNullOrEmpty(searchString))
            {
                reservas = reservas.Where(r => 
                    r.IdEspacioNavigation!.Nombre.Contains(searchString) || 
                    (r.IdUsuarioNavigation!.Nombre + " " + r.IdUsuarioNavigation.Apellido).Contains(searchString) ||
                    r.IdEventoNavigation!.Titulo.Contains(searchString) ||
                    r.EstadoReserva!.Contains(searchString));
            }
            
            return View(await PaginatedList<Reserva>.CreateAsync(reservas, pageNumber ?? 1, pageSize));
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewData["IdEspacio"] = new SelectList(_context.Espacios.Where(e => e.Estado == true), "IdEspacio", "Nombre");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios.Where(u => u.Estado == true), "IdUsuario", "Nombre");
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Estado == true), "IdEvento", "Titulo");
            ViewData["EstadoReserva"] = new SelectList(new[] { "Pendiente", "Aprobada", "Rechazada" });
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEspacio,IdUsuario,IdEvento,FechaInicio,FechaFin,CantidadAsistentes,EstadoReserva,Observaciones")] Reserva reserva)
        {
            Console.WriteLine("=== INICIO POST Create ===");
            Console.WriteLine($"IdEspacio: {reserva.IdEspacio}");
            Console.WriteLine($"IdUsuario: {reserva.IdUsuario}");
            Console.WriteLine($"IdEvento: {reserva.IdEvento}");
            Console.WriteLine($"FechaInicio: {reserva.FechaInicio}");
            Console.WriteLine($"FechaFin: {reserva.FechaFin}");
            Console.WriteLine($"CantidadAsistentes: {reserva.CantidadAsistentes}");
            Console.WriteLine($"EstadoReserva: {reserva.EstadoReserva}");
            
            // Remover validaciones de propiedades de navegación que no se envían desde el formulario
            ModelState.Remove("IdEspacioNavigation");
            ModelState.Remove("IdEventoNavigation");
            ModelState.Remove("IdUsuarioNavigation");
            
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            // Mostrar errores del ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ERRORES DE VALIDACIÓN ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state != null && state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Campo: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }
            }
            
            // Validación de fecha inicio no puede ser pasada
            if (reserva.FechaInicio < DateTime.Now)
            {
                ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior a la fecha actual");
            }
            
            // Validación de fecha fin debe ser posterior a fecha inicio
            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
            }
            
            // Validación de cantidad de asistentes
            if (reserva.CantidadAsistentes <= 0)
            {
                ModelState.AddModelError("CantidadAsistentes", "Debe haber al menos 1 asistente");
            }
            
            // Validación de disponibilidad del espacio
            if (!VerificarDisponibilidad(reserva.IdEspacio, reserva.FechaInicio, reserva.FechaFin))
            {
                ModelState.AddModelError("", "El espacio no está disponible en el rango de fechas seleccionado");
            }
            
            // Validación de capacidad del espacio
            var espacio = await _context.Espacios.FindAsync(reserva.IdEspacio);
            if (espacio != null && reserva.CantidadAsistentes > espacio.Capacidad)
            {
                ModelState.AddModelError("CantidadAsistentes", $"La cantidad de asistentes no puede superar la capacidad del espacio ({espacio.Capacidad} personas)");
            }
            
            if (ModelState.IsValid)
            {
                Console.WriteLine("=== GUARDANDO RESERVA ===");
                reserva.FechaSolicitud = DateTime.Now;
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Reserva guardada con ID: {reserva.IdReserva}");
                return RedirectToAction(nameof(Index));
            }
            
            Console.WriteLine("=== MODELO INVÁLIDO - Retornando a la vista ===");
            ViewData["IdEspacio"] = new SelectList(_context.Espacios.Where(e => e.Estado == true), "IdEspacio", "Nombre", reserva.IdEspacio);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios.Where(u => u.Estado == true), "IdUsuario", "Nombre", reserva.IdUsuario);
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Estado == true), "IdEvento", "Titulo", reserva.IdEvento);
            ViewData["EstadoReserva"] = new SelectList(new[] { "Pendiente", "Aprobada", "Rechazada" }, reserva.EstadoReserva);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["IdEspacio"] = new SelectList(_context.Espacios.Where(e => e.Estado == true), "IdEspacio", "Nombre", reserva.IdEspacio);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios.Where(u => u.Estado == true), "IdUsuario", "Nombre", reserva.IdUsuario);
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Estado == true), "IdEvento", "Titulo", reserva.IdEvento);
            ViewData["EstadoReserva"] = new SelectList(new[] { "Pendiente", "Aprobada", "Rechazada" }, reserva.EstadoReserva);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,IdEspacio,IdUsuario,IdEvento,FechaInicio,FechaFin,CantidadAsistentes,EstadoReserva,MotivoRechazo,Observaciones,FechaSolicitud,FechaRespuesta")] Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return NotFound();
            }

            // Remover validaciones de propiedades de navegación que no se envían desde el formulario
            ModelState.Remove("IdEspacioNavigation");
            ModelState.Remove("IdEventoNavigation");
            ModelState.Remove("IdUsuarioNavigation");

            // Validación de fecha inicio no puede ser pasada
            if (reserva.FechaInicio < DateTime.Now)
            {
                ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior a la fecha actual");
            }
            
            // Validación de fecha fin debe ser posterior a fecha inicio
            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
            }
            
            // Validación de cantidad de asistentes
            if (reserva.CantidadAsistentes <= 0)
            {
                ModelState.AddModelError("CantidadAsistentes", "Debe haber al menos 1 asistente");
            }
            
            // Validación de capacidad del espacio
            var espacio = await _context.Espacios.FindAsync(reserva.IdEspacio);
            if (espacio != null && reserva.CantidadAsistentes > espacio.Capacidad)
            {
                ModelState.AddModelError("CantidadAsistentes", $"La cantidad de asistentes no puede superar la capacidad del espacio ({espacio.Capacidad} personas)");
            }
            
            // Validación de motivo de rechazo si el estado es Rechazada
            if (reserva.EstadoReserva == "Rechazada" && string.IsNullOrWhiteSpace(reserva.MotivoRechazo))
            {
                ModelState.AddModelError("MotivoRechazo", "Debe especificar el motivo del rechazo");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (reserva.EstadoReserva == "Aprobada" || reserva.EstadoReserva == "Rechazada")
                    {
                        reserva.FechaRespuesta = DateTime.Now;
                    }
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReserva))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEspacio"] = new SelectList(_context.Espacios.Where(e => e.Estado == true), "IdEspacio", "Nombre", reserva.IdEspacio);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios.Where(u => u.Estado == true), "IdUsuario", "Nombre", reserva.IdUsuario);
            ViewData["IdEvento"] = new SelectList(_context.Eventos.Where(e => e.Estado == true), "IdEvento", "Titulo", reserva.IdEvento);
            ViewData["EstadoReserva"] = new SelectList(new[] { "Pendiente", "Aprobada", "Rechazada" }, reserva.EstadoReserva);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                reserva.EstadoReserva = "Cancelada";
                _context.Update(reserva);
                await _context.SaveChangesAsync();
            }
            return Json("ok");
        }

        public bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }

        public bool VerificarDisponibilidad(int idEspacio, DateTime fechaInicio, DateTime fechaFin)
        {
            return !_context.Reservas.Any(r => 
                r.IdEspacio == idEspacio && 
                r.EstadoReserva == "Aprobada" &&
                ((fechaInicio >= r.FechaInicio && fechaInicio < r.FechaFin) ||
                 (fechaFin > r.FechaInicio && fechaFin <= r.FechaFin) ||
                 (fechaInicio <= r.FechaInicio && fechaFin >= r.FechaFin)));
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string searchString)
        {
            var reservas = _context.Reservas
                .Include(r => r.IdEspacioNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.EstadoReserva != "Cancelada");
            
            if (!string.IsNullOrEmpty(searchString))
            {
                reservas = reservas.Where(r => 
                    r.IdEspacioNavigation!.Nombre.Contains(searchString) || 
                    (r.IdUsuarioNavigation!.Nombre + " " + r.IdUsuarioNavigation.Apellido).Contains(searchString) ||
                    r.IdEventoNavigation!.Titulo.Contains(searchString) ||
                    r.EstadoReserva!.Contains(searchString));
            }
            
            var reservasList = await reservas.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reservas");
                
                // Encabezados
                worksheet.Cell(1, 1).Value = "Espacio";
                worksheet.Cell(1, 2).Value = "Usuario";
                worksheet.Cell(1, 3).Value = "Evento";
                worksheet.Cell(1, 4).Value = "Fecha Inicio";
                worksheet.Cell(1, 5).Value = "Fecha Fin";
                worksheet.Cell(1, 6).Value = "Cantidad Asistentes";
                worksheet.Cell(1, 7).Value = "Estado";
                worksheet.Cell(1, 8).Value = "Observaciones";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var reserva in reservasList)
                {
                    worksheet.Cell(row, 1).Value = reserva.IdEspacioNavigation?.Nombre ?? "";
                    worksheet.Cell(row, 2).Value = $"{reserva.IdUsuarioNavigation?.Nombre} {reserva.IdUsuarioNavigation?.Apellido}";
                    worksheet.Cell(row, 3).Value = reserva.IdEventoNavigation?.Titulo ?? "";
                    worksheet.Cell(row, 4).Value = reserva.FechaInicio.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 5).Value = reserva.FechaFin.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 6).Value = reserva.CantidadAsistentes;
                    worksheet.Cell(row, 7).Value = reserva.EstadoReserva;
                    worksheet.Cell(row, 8).Value = reserva.Observaciones ?? "";
                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"Reservas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string searchString)
        {
            var reservas = _context.Reservas
                .Include(r => r.IdEspacioNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.EstadoReserva != "Cancelada");
            
            if (!string.IsNullOrEmpty(searchString))
            {
                reservas = reservas.Where(r => 
                    r.IdEspacioNavigation!.Nombre.Contains(searchString) || 
                    (r.IdUsuarioNavigation!.Nombre + " " + r.IdUsuarioNavigation.Apellido).Contains(searchString) ||
                    r.IdEventoNavigation!.Titulo.Contains(searchString) ||
                    r.EstadoReserva!.Contains(searchString));
            }
            
            var reservasList = await reservas.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("Lista de Reservas\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Fecha de generación
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                document.Add(date);

                // Tabla
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 15, 15, 15, 12, 12, 8, 10, 13 });

                // Encabezados
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                var headerBg = new BaseColor(173, 216, 230);
                
                string[] headers = { "Espacio", "Usuario", "Evento", "Fecha Inicio", "Fecha Fin", 
                                   "Asistentes", "Estado", "Observaciones" };
                
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = headerBg;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Datos
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                foreach (var reserva in reservasList)
                {
                    table.AddCell(new Phrase(reserva.IdEspacioNavigation?.Nombre ?? "", cellFont));
                    table.AddCell(new Phrase($"{reserva.IdUsuarioNavigation?.Nombre} {reserva.IdUsuarioNavigation?.Apellido}", cellFont));
                    table.AddCell(new Phrase(reserva.IdEventoNavigation?.Titulo ?? "", cellFont));
                    table.AddCell(new Phrase(reserva.FechaInicio.ToString("dd/MM/yyyy HH:mm"), cellFont));
                    table.AddCell(new Phrase(reserva.FechaFin.ToString("dd/MM/yyyy HH:mm"), cellFont));
                    table.AddCell(new Phrase(reserva.CantidadAsistentes.ToString(), cellFont));
                    table.AddCell(new Phrase(reserva.EstadoReserva ?? "", cellFont));
                    table.AddCell(new Phrase(reserva.Observaciones ?? "", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"Reservas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
