using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoGestionEventos.Models;
using ProyectoGestionEventos.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    //IdleTimeout sirve para definir el tiempo de inacatividad de la sesión
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    //cookie.httponly sirve para que la cookie pueda ser accedido por js
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "GestionEventos.Session";
    //la cookie no se ve afectada por las políticas de cookies
    options.Cookie.IsEssential = true;

});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/Index";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost\\SQLEXPRESS;Database=gestionEventosDb;Trusted_Connection=True;TrustServerCertificate=True;"));

// Registrar servicios personalizados
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ActividadHelper>();

builder.Services.AddAuthorization();

var app = builder.Build();

//permite usar las variables session
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//se agrega autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
