using BillingMailer;
using BillingMailer.Data;
using BillingMailer.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddWindowsService(options =>
    options.ServiceName = "Monthly Billing Mailer");

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
