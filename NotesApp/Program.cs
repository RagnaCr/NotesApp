using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Настройка подключения к MSSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    options.SignIn.RequireConfirmedAccount = false; // Требовать подтверждение аккаунта
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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

app.Run();
