using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ��������� ����������� � MSSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // ��������� ����� � ������
    options.Password.RequiredLength = 6; // ����������� ����� ������
    options.Password.RequireLowercase = true; // ��������� �������� �����
    options.Password.RequireUppercase = true; // ��������� ��������� �����
    options.Password.RequireNonAlphanumeric = false; // �� ��������� ����������� �������
    options.Lockout.AllowedForNewUsers = true; // ��������� ���������� ��� ����� �������������
    options.Lockout.MaxFailedAccessAttempts = 5; // ������������ ���������� ��������� ������� �����
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // ����� ����������
    options.SignIn.RequireConfirmedAccount = true; // ��������� ������������� ��������
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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

app.Run();
