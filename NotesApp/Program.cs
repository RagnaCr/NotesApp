using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Services;
using NotesApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
//builder.Configuration.GetConnectionString("DefaultConnection") - appsettings.json (for first start)

// Настройка подключения к MSSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // Требовать цифры в пароле
    options.Password.RequiredLength = 6; // Минимальная длина пароля
    options.Password.RequireLowercase = true; // Требовать строчные буквы
    options.Password.RequireUppercase = true; // Требовать заглавные буквы
    options.Password.RequireNonAlphanumeric = false; // НЕ требовать специальные символы
    options.Lockout.AllowedForNewUsers = true; // Разрешить блокировку для новых пользователей
    options.Lockout.MaxFailedAccessAttempts = 5; // Максимальное количество неудачных попыток входа
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Время блокировки
    options.SignIn.RequireConfirmedAccount = true; // Требовать подтверждение аккаунта
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<INotesService, NotesService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.Run();
