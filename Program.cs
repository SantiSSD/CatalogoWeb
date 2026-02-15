using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoWebContext") ?? throw new InvalidOperationException("Connection string 'CatalogoWebContext' not found.")));

builder.Services.AddScoped<IPedidoService, PedidoService>();
// Add services to the container.
builder.Services.AddSession();

builder.Services.AddControllersWithViews();
builder.Services.AddValidation();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
