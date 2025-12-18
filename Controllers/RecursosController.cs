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
    public class RecursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Recursos
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            
            var recursos = _context.Recursos.Where(r => r.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                recursos = recursos.Where(r => 
                    r.Nombre.Contains(searchString) || 
                    r.TipoRecurso.Contains(searchString));
            }
            
            return View(await PaginatedList<Recurso>.CreateAsync(recursos, pageNumber ?? 1, pageSize));
        }

        // GET: Recursos/Create
        public IActionResult Create()
        {
            ViewData["TipoRecurso"] = new SelectList(new[] { "Proyector", "Computadora", "Micrófono", "Parlantes", "Pizarra", "Sillas", "Mesas", "Otro" });
            return View();
        }

        // POST: Recursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,TipoRecurso,CantidadDisponible")] Recurso recurso)
        {
            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(recurso.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            
            // Validación de nombre duplicado
            if (NombreExists(recurso.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un recurso con este nombre");
            }
            
            // Validación de cantidad
            if (recurso.CantidadDisponible < 0)
            {
                ModelState.AddModelError("CantidadDisponible", "La cantidad no puede ser negativa");
            }
            
            if (ModelState.IsValid)
            {
                recurso.Estado = true;
                recurso.FechaRegistro = DateTime.Now;
                _context.Add(recurso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TipoRecurso"] = new SelectList(new[] { "Proyector", "Computadora", "Micrófono", "Parlantes", "Pizarra", "Sillas", "Mesas", "Otro" });
            return View(recurso);
        }

        // GET: Recursos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null)
            {
                return NotFound();
            }
            ViewData["TipoRecurso"] = new SelectList(new[] { "Proyector", "Computadora", "Micrófono", "Parlantes", "Pizarra", "Sillas", "Mesas", "Otro" }, recurso.TipoRecurso);
            return View(recurso);
        }

        // POST: Recursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRecurso,Nombre,Descripcion,TipoRecurso,CantidadDisponible,Estado,FechaRegistro")] Recurso recurso)
        {
            if (id != recurso.IdRecurso)
            {
                return NotFound();
            }

            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(recurso.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            
            // Validación de nombre duplicado (excluyendo el actual)
            if (_context.Recursos.Any(e => e.Nombre == recurso.Nombre && e.Estado == true && e.IdRecurso != recurso.IdRecurso))
            {
                ModelState.AddModelError("Nombre", "Ya existe un recurso con este nombre");
            }
            
            // Validación de cantidad
            if (recurso.CantidadDisponible < 0)
            {
                ModelState.AddModelError("CantidadDisponible", "La cantidad no puede ser negativa");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recurso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecursoExists(recurso.IdRecurso))
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
            ViewData["TipoRecurso"] = new SelectList(new[] { "Proyector", "Computadora", "Micrófono", "Parlantes", "Pizarra", "Sillas", "Mesas", "Otro" }, recurso.TipoRecurso);
            return View(recurso);
        }

        // GET: Recursos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso != null)
            {
                recurso.Estado = false;
                _context.Update(recurso);
                await _context.SaveChangesAsync();
            }
            return Json("ok");
        }

        public bool RecursoExists(int id)
        {
            return _context.Recursos.Any(e => e.IdRecurso == id);
        }

        public bool NombreExists(string nombre)
        {
            return _context.Recursos.Any(e => e.Nombre == nombre && e.Estado == true);
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string searchString)
        {
            var recursos = _context.Recursos.Where(r => r.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                recursos = recursos.Where(r => 
                    r.Nombre.Contains(searchString) || 
                    r.TipoRecurso.Contains(searchString));
            }
            
            var recursosList = await recursos.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Recursos");
                
                // Encabezados
                worksheet.Cell(1, 1).Value = "Nombre";
                worksheet.Cell(1, 2).Value = "Tipo";
                worksheet.Cell(1, 3).Value = "Descripción";
                worksheet.Cell(1, 4).Value = "Cantidad Disponible";
                worksheet.Cell(1, 5).Value = "Fecha Registro";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var recurso in recursosList)
                {
                    worksheet.Cell(row, 1).Value = recurso.Nombre;
                    worksheet.Cell(row, 2).Value = recurso.TipoRecurso;
                    worksheet.Cell(row, 3).Value = recurso.Descripcion;
                    worksheet.Cell(row, 4).Value = recurso.CantidadDisponible;
                    worksheet.Cell(row, 5).Value = recurso.FechaRegistro?.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"Recursos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string searchString)
        {
            var recursos = _context.Recursos.Where(r => r.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                recursos = recursos.Where(r => 
                    r.Nombre.Contains(searchString) || 
                    r.TipoRecurso.Contains(searchString));
            }
            
            var recursosList = await recursos.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("Lista de Recursos\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Fecha de generación
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                document.Add(date);

                // Tabla
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 20, 15, 35, 15, 15 });

                // Encabezados
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var headerBg = new BaseColor(173, 216, 230);
                
                string[] headers = { "Nombre", "Tipo", "Descripción", "Cantidad", "Fecha Registro" };
                
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
                foreach (var recurso in recursosList)
                {
                    table.AddCell(new Phrase(recurso.Nombre ?? "", cellFont));
                    table.AddCell(new Phrase(recurso.TipoRecurso ?? "", cellFont));
                    table.AddCell(new Phrase(recurso.Descripcion ?? "", cellFont));
                    table.AddCell(new Phrase(recurso.CantidadDisponible.ToString(), cellFont));
                    table.AddCell(new Phrase(recurso.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"Recursos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
