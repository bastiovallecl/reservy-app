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
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation).Where(u => u.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                usuarios = usuarios.Where(u => 
                    u.Nombre.Contains(searchString) || 
                    u.Apellido.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.Run.Contains(searchString));
            }
            
            return View(await PaginatedList<Usuario>.CreateAsync(usuarios, pageNumber ?? 1, pageSize));
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1");
            ViewData["TipoUsuario"] = new SelectList(new[] { "Estudiante", "Docente", "Administrativo", "Coordinador" });
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Run,Nombre,Apellido,Email,Telefono,TipoUsuario,Cargo,Password,IdRol")] Usuario usuario)
        {
            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                ModelState.AddModelError("Apellido", "El apellido no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(usuario.Run))
            {
                ModelState.AddModelError("Run", "El RUN no puede contener solo espacios");
            }
            
            // Validación de RUN duplicado
            if (RunExists(usuario.Run))
            {
                ModelState.AddModelError("Run", "Ya existe un usuario con este RUN");
            }
            
            // Validación de email duplicado
            if (EmailExists(usuario.Email))
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con este email");
            }
            
            // Validación de longitud mínima del RUN
            if (usuario.Run != null && usuario.Run.Replace("-", "").Length < 8)
            {
                ModelState.AddModelError("Run", "El RUN debe tener al menos 8 dígitos sin contar el guión");
            }
            
            if (ModelState.IsValid)
            {
                usuario.Estado = true;
                usuario.FechaRegistro = DateTime.Now;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            ViewData["TipoUsuario"] = new SelectList(new[] { "Estudiante", "Docente", "Administrativo", "Coordinador" });
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            ViewData["TipoUsuario"] = new SelectList(new[] { "Estudiante", "Docente", "Administrativo", "Coordinador" }, usuario.TipoUsuario);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Run,Nombre,Apellido,Email,Telefono,TipoUsuario,Cargo,Password,IdRol,Estado,FechaRegistro")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            // Validación de solo espacios
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(usuario.Apellido))
            {
                ModelState.AddModelError("Apellido", "El apellido no puede contener solo espacios");
            }
            if (string.IsNullOrWhiteSpace(usuario.Run))
            {
                ModelState.AddModelError("Run", "El RUN no puede contener solo espacios");
            }
            
            // Validación de RUN duplicado (excluyendo el actual)
            if (_context.Usuarios.Any(e => e.Run == usuario.Run && e.IdUsuario != usuario.IdUsuario))
            {
                ModelState.AddModelError("Run", "Ya existe un usuario con este RUN");
            }
            
            // Validación de Email duplicado (excluyendo el actual)
            if (_context.Usuarios.Any(e => e.Email == usuario.Email && e.IdUsuario != usuario.IdUsuario))
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con este email");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            ViewData["TipoUsuario"] = new SelectList(new[] { "Estudiante", "Docente", "Administrativo", "Coordinador" }, usuario.TipoUsuario);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Estado = false;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            return Json("ok");
        }

        public bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }

        public bool RunExists(string run)
        {
            return _context.Usuarios.Any(e => e.Run == run);
        }

        public bool EmailExists(string email)
        {
            return _context.Usuarios.Any(e => e.Email == email);
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string searchString)
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Where(u => u.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                usuarios = usuarios.Where(u => 
                    u.Nombre.Contains(searchString) || 
                    u.Apellido.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.Run.Contains(searchString));
            }
            
            var usuariosList = await usuarios.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Usuarios");
                
                // Encabezados
                worksheet.Cell(1, 1).Value = "RUN";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "Apellido";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Teléfono";
                worksheet.Cell(1, 6).Value = "Tipo Usuario";
                worksheet.Cell(1, 7).Value = "Rol";
                worksheet.Cell(1, 8).Value = "Cargo";
                worksheet.Cell(1, 9).Value = "Fecha Registro";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Datos
                int row = 2;
                foreach (var usuario in usuariosList)
                {
                    worksheet.Cell(row, 1).Value = usuario.Run;
                    worksheet.Cell(row, 2).Value = usuario.Nombre;
                    worksheet.Cell(row, 3).Value = usuario.Apellido;
                    worksheet.Cell(row, 4).Value = usuario.Email;
                    worksheet.Cell(row, 5).Value = usuario.Telefono;
                    worksheet.Cell(row, 6).Value = usuario.TipoUsuario;
                    worksheet.Cell(row, 7).Value = usuario.IdRolNavigation?.Rol1;
                    worksheet.Cell(row, 8).Value = usuario.Cargo;
                    worksheet.Cell(row, 9).Value = usuario.FechaRegistro?.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string searchString)
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Where(u => u.Estado == true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                usuarios = usuarios.Where(u => 
                    u.Nombre.Contains(searchString) || 
                    u.Apellido.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.Run.Contains(searchString));
            }
            
            var usuariosList = await usuarios.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("Lista de Usuarios\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Fecha de generación
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                document.Add(date);

                // Tabla
                PdfPTable table = new PdfPTable(9);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10, 12, 12, 15, 10, 12, 10, 12, 12 });

                // Encabezados
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                var headerBg = new BaseColor(173, 216, 230);
                
                string[] headers = { "RUN", "Nombre", "Apellido", "Email", "Teléfono", 
                                   "Tipo Usuario", "Rol", "Cargo", "Fecha Registro" };
                
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
                foreach (var usuario in usuariosList)
                {
                    table.AddCell(new Phrase(usuario.Run ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.Nombre ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.Apellido ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.Email ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.Telefono ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.TipoUsuario ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.IdRolNavigation?.Rol1 ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.Cargo ?? "", cellFont));
                    table.AddCell(new Phrase(usuario.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
