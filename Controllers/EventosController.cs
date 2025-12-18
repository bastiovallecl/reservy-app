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
    public class EventosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Eventos
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            
            var eventos = _context.Eventos.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                eventos = eventos.Where(e => 
                    e.Titulo.Contains(searchString) || 
                    e.TipoEvento.Contains(searchString));
            }
            
            return View(await PaginatedList<Evento>.CreateAsync(eventos, pageNumber ?? 1, pageSize));
        }

        // GET: Eventos/Create
        public IActionResult Create()
        {
            ViewData["TipoEvento"] = new SelectList(new[] { "Académico", "Cultural", "Deportivo", "Social", "Reunión", "Conferencia", "Taller", "Otro" });
            return View();
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titulo,Descripcion,TipoEvento")] Evento evento)
        {
            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(evento.Titulo))
            {
                ModelState.AddModelError("Titulo", "El título no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(evento.Descripcion))
            {
                ModelState.AddModelError("Descripcion", "La descripción no puede contener solo espacios");
            }
            
            // Validación de título duplicado
            if (TituloExists(evento.Titulo))
            {
                ModelState.AddModelError("Titulo", "Ya existe un evento con este título");
            }
            
            if (ModelState.IsValid)
            {
                evento.Estado = true;
                evento.FechaCreacion = DateTime.Now;
                _context.Add(evento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TipoEvento"] = new SelectList(new[] { "Académico", "Cultural", "Deportivo", "Social", "Reunión", "Conferencia", "Taller", "Otro" });
            return View(evento);
        }

        // GET: Eventos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }
            ViewData["TipoEvento"] = new SelectList(new[] { "Académico", "Cultural", "Deportivo", "Social", "Reunión", "Conferencia", "Taller", "Otro" }, evento.TipoEvento);
            return View(evento);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEvento,Titulo,Descripcion,TipoEvento,Estado,FechaCreacion")] Evento evento)
        {
            if (id != evento.IdEvento)
            {
                return NotFound();
            }

            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(evento.Titulo))
            {
                ModelState.AddModelError("Titulo", "El título no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(evento.Descripcion))
            {
                ModelState.AddModelError("Descripcion", "La descripción no puede contener solo espacios");
            }
            
            // Validación de título duplicado (excluyendo el actual)
            if (_context.Eventos.Any(e => e.Titulo == evento.Titulo && e.Estado == true && e.IdEvento != evento.IdEvento))
            {
                ModelState.AddModelError("Titulo", "Ya existe un evento con este título");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(evento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(evento.IdEvento))
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
            ViewData["TipoEvento"] = new SelectList(new[] { "Académico", "Cultural", "Deportivo", "Social", "Reunión", "Conferencia", "Taller", "Otro" }, evento.TipoEvento);
            return View(evento);
        }

        // GET: Eventos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                evento.Estado = false;
                _context.Update(evento);
                await _context.SaveChangesAsync();
            }
            return Json("ok");
        }

        public bool EventoExists(int id)
        {
            return _context.Eventos.Any(e => e.IdEvento == id);
        }

        public bool TituloExists(string titulo)
        {
            return _context.Eventos.Any(e => e.Titulo == titulo && e.Estado == true);
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string searchString)
        {
            var eventos = _context.Eventos.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                eventos = eventos.Where(e => 
                    e.Titulo.Contains(searchString) || 
                    e.TipoEvento.Contains(searchString));
            }
            
            var eventosList = await eventos.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Eventos");
                
                // Encabezados
                worksheet.Cell(1, 1).Value = "Título";
                worksheet.Cell(1, 2).Value = "Tipo";
                worksheet.Cell(1, 3).Value = "Descripción";
                worksheet.Cell(1, 4).Value = "Fecha Creación";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var evento in eventosList)
                {
                    worksheet.Cell(row, 1).Value = evento.Titulo;
                    worksheet.Cell(row, 2).Value = evento.TipoEvento;
                    worksheet.Cell(row, 3).Value = evento.Descripcion;
                    worksheet.Cell(row, 4).Value = evento.FechaCreacion?.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"Eventos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string searchString)
        {
            var eventos = _context.Eventos.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                eventos = eventos.Where(e => 
                    e.Titulo.Contains(searchString) || 
                    e.TipoEvento.Contains(searchString));
            }
            
            var eventosList = await eventos.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("Lista de Eventos\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Fecha de generación
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                document.Add(date);

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 25, 15, 40, 20 });

                // Encabezados
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var headerBg = new BaseColor(173, 216, 230);
                
                string[] headers = { "Título", "Tipo", "Descripción", "Fecha Creación" };
                
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = headerBg;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Datos
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var evento in eventosList)
                {
                    table.AddCell(new Phrase(evento.Titulo ?? "", cellFont));
                    table.AddCell(new Phrase(evento.TipoEvento ?? "", cellFont));
                    table.AddCell(new Phrase(evento.Descripcion ?? "", cellFont));
                    table.AddCell(new Phrase(evento.FechaCreacion?.ToString("dd/MM/yyyy HH:mm") ?? "", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"Eventos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
