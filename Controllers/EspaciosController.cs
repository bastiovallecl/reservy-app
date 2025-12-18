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
    public class EspaciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EspaciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Espacios
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            
            var espacios = _context.Espacios.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                espacios = espacios.Where(e => 
                    e.Nombre.Contains(searchString) || 
                    e.TipoEspacio.Contains(searchString) ||
                    e.Ubicacion.Contains(searchString));
            }
            
            return View(await PaginatedList<Espacio>.CreateAsync(espacios, pageNumber ?? 1, pageSize));
        }

        // GET: Espacios/Create
        public IActionResult Create()
        {
            ViewData["TipoEspacio"] = new SelectList(new[] { "Aula", "Auditorio", "Laboratorio", "Sala de Reuniones", "Cancha", "Patio", "Otro" });
            return View();
        }

        // POST: Espacios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Capacidad,Ubicacion,Descripcion,TipoEspacio")] Espacio espacio)
        {
            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(espacio.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(espacio.Ubicacion))
            {
                ModelState.AddModelError("Ubicacion", "La ubicación no puede contener solo espacios");
            }
            
            // Validación de nombre duplicado
            if (NombreExists(espacio.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un espacio con este nombre");
            }
            
            // Validación de capacidad
            if (espacio.Capacidad <= 0)
            {
                ModelState.AddModelError("Capacidad", "La capacidad debe ser mayor a 0");
            }
            
            if (ModelState.IsValid)
            {
                espacio.Estado = true;
                espacio.FechaCreacion = DateTime.Now;
                _context.Add(espacio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TipoEspacio"] = new SelectList(new[] { "Aula", "Auditorio", "Laboratorio", "Sala de Reuniones", "Cancha", "Patio", "Otro" });
            return View(espacio);
        }

        // GET: Espacios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var espacio = await _context.Espacios.FindAsync(id);
            if (espacio == null)
            {
                return NotFound();
            }
            ViewData["TipoEspacio"] = new SelectList(new[] { "Aula", "Auditorio", "Laboratorio", "Sala de Reuniones", "Cancha", "Patio", "Otro" }, espacio.TipoEspacio);
            return View(espacio);
        }

        // POST: Espacios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEspacio,Nombre,Capacidad,Ubicacion,Descripcion,TipoEspacio,Estado,FechaCreacion")] Espacio espacio)
        {
            if (id != espacio.IdEspacio)
            {
                return NotFound();
            }

            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(espacio.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(espacio.Ubicacion))
            {
                ModelState.AddModelError("Ubicacion", "La ubicación no puede contener solo espacios");
            }
            
            // Validación de nombre duplicado (excluyendo el actual)
            if (_context.Espacios.Any(e => e.Nombre == espacio.Nombre && e.Estado == true && e.IdEspacio != espacio.IdEspacio))
            {
                ModelState.AddModelError("Nombre", "Ya existe un espacio con este nombre");
            }
            
            // Validación de capacidad
            if (espacio.Capacidad <= 0)
            {
                ModelState.AddModelError("Capacidad", "La capacidad debe ser mayor a 0");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(espacio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EspacioExists(espacio.IdEspacio))
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
            ViewData["TipoEspacio"] = new SelectList(new[] { "Aula", "Auditorio", "Laboratorio", "Sala de Reuniones", "Cancha", "Patio", "Otro" }, espacio.TipoEspacio);
            return View(espacio);
        }

        // GET: Espacios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var espacio = await _context.Espacios.FindAsync(id);
            if (espacio != null)
            {
                espacio.Estado = false;
                _context.Update(espacio);
                await _context.SaveChangesAsync();
            }
            return Json("ok");
        }

        public bool EspacioExists(int id)
        {
            return _context.Espacios.Any(e => e.IdEspacio == id);
        }

        public bool NombreExists(string nombre)
        {
            return _context.Espacios.Any(e => e.Nombre == nombre && e.Estado == true);
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string searchString)
        {
            var espacios = _context.Espacios.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                espacios = espacios.Where(e => 
                    e.Nombre.Contains(searchString) || 
                    e.TipoEspacio.Contains(searchString) ||
                    e.Ubicacion.Contains(searchString));
            }
            
            var espaciosList = await espacios.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Espacios");
                
                // Encabezados
                worksheet.Cell(1, 1).Value = "Nombre";
                worksheet.Cell(1, 2).Value = "Capacidad";
                worksheet.Cell(1, 3).Value = "Ubicación";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Descripción";
                worksheet.Cell(1, 6).Value = "Fecha Creación";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var espacio in espaciosList)
                {
                    worksheet.Cell(row, 1).Value = espacio.Nombre;
                    worksheet.Cell(row, 2).Value = espacio.Capacidad;
                    worksheet.Cell(row, 3).Value = espacio.Ubicacion;
                    worksheet.Cell(row, 4).Value = espacio.TipoEspacio;
                    worksheet.Cell(row, 5).Value = espacio.Descripcion;
                    worksheet.Cell(row, 6).Value = espacio.FechaCreacion?.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"Espacios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string searchString)
        {
            var espacios = _context.Espacios.Where(e => e.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                espacios = espacios.Where(e => 
                    e.Nombre.Contains(searchString) || 
                    e.TipoEspacio.Contains(searchString) ||
                    e.Ubicacion.Contains(searchString));
            }
            
            var espaciosList = await espacios.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("Lista de Espacios\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Fecha de generación
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                document.Add(date);

                // Tabla
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 20, 10, 20, 15, 25, 15 });

                // Encabezados
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var headerBg = new BaseColor(173, 216, 230);
                
                string[] headers = { "Nombre", "Capacidad", "Ubicación", "Tipo", "Descripción", "Fecha Creación" };
                
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
                foreach (var espacio in espaciosList)
                {
                    table.AddCell(new Phrase(espacio.Nombre ?? "", cellFont));
                    table.AddCell(new Phrase(espacio.Capacidad.ToString(), cellFont));
                    table.AddCell(new Phrase(espacio.Ubicacion ?? "", cellFont));
                    table.AddCell(new Phrase(espacio.TipoEspacio ?? "", cellFont));
                    table.AddCell(new Phrase(espacio.Descripcion ?? "", cellFont));
                    table.AddCell(new Phrase(espacio.FechaCreacion?.ToString("dd/MM/yyyy HH:mm") ?? "", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"Espacios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
