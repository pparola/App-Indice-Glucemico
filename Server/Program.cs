using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.Cookies;
using App_Indice_Glucemico.Shared;
using App_Indice_Glucemico.Server.Repositories;
using App_Indice_Glucemico.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddHttpContextAccessor();

// Registrar repositorios
builder.Services.AddScoped<IAlimentoRepository, AlimentoRepository>();
builder.Services.AddScoped<IRegistroRepository, RegistroRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar servicio de inicialización de base de datos
builder.Services.AddSingleton<DatabaseInitializer>();

// Registrar servicio de email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Registrar servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Inicializar la base de datos si no existe
try
{
    var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al inicializar la base de datos. La aplicación continuará, pero puede que algunas funcionalidades no funcionen correctamente.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
