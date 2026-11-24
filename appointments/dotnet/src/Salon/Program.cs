using System.Diagnostics;
using Salon.Booking;
using Salon.Common;
using Salon.Data;
using Salon.Shops;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddDbContext<SalonDbContext>(options =>
    SalonDbContext.Configure(options, builder.Configuration.GetConnectionString("Salon")!));
builder.Services.AddScoped<ShopRepository>();
builder.Services.AddScoped<ShopsService>();
builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<AppointmentsService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = HttpErrorHandler.Validation);
builder.Services.AddExceptionHandler<HttpErrorHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
var httpLog = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("HTTP");

app.Use(async (context, next) =>
{
    var startedAt = Stopwatch.GetTimestamp();
    await next();
    var ms = (int)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
    httpLog.LogInformation("{Method} {Path}{Query} {StatusCode} {Ms}ms",
        context.Request.Method, context.Request.Path, context.Request.QueryString, context.Response.StatusCode, ms);
});
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
