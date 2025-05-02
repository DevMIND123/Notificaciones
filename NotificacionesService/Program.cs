using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NotificacionesService.Data;
using NotificacionesService.Models;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 🔓 CORS: permitir frontend Angular en localhost:4200
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 📦 DB Context
builder.Services.AddDbContext<NotificacionesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");

// ✅ Endpoint GET /notificaciones con email extraído del token
app.MapGet("/api.retochimba.com/notificaciones", async (HttpContext http, NotificacionesDbContext db) =>
{
    string? authHeader = http.Request.Headers["Authorization"];
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        return Results.Unauthorized();

    string token = authHeader.Substring("Bearer ".Length).Trim();

    try
    {
        var handler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
        var claims = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        }, out _);

        var email = claims.Identity?.Name;
        if (email == null) return Results.Unauthorized();

        Console.WriteLine($"📧 Email extraído del token: {email}");

        var notis = await db.Notificaciones
            .Where(n => n.EmailUsuario == email)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();

        return Results.Ok(notis);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error validando token JWT: {ex.Message}");
        return Results.Unauthorized();
    }
});

// 🚀 Consumidor Kafka
var config = new ConsumerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
    GroupId = builder.Configuration["Kafka:GroupId"],
    AutoOffsetReset = AutoOffsetReset.Earliest
};

_ = Task.Run(() =>
{
    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe(new[] {
        builder.Configuration["Kafka:Topic"],
        "ciclo-registrado"
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificacionesDbContext>();

    db.Database.EnsureCreated();
    Console.WriteLine("🟢 Escuchando eventos Kafka...");

    while (true)
    {
        try
        {
            var cr = consumer.Consume();
            Console.WriteLine($"📨 Evento recibido: {cr.Message.Value}");

            if (cr.Topic == "usuario-creado")
            {
                var data = JsonSerializer.Deserialize<EventoKafkaDTO>(cr.Message.Value);
                var noti = new Notificacion
                {
                    EmailUsuario = data!.EmailUsuario,
                    Mensaje = $"🎉 ¡Bienvenido {data.Nombre} a Reto Chimba!",
                    TipoUsuario = data.Tipo
                };
                db.Notificaciones.Add(noti);
            }
            else if (cr.Topic == "ciclo-registrado")
            {
                var data = JsonSerializer.Deserialize<EventoCicloKafkaDTO>(cr.Message.Value);
                var noti = new Notificacion
                {
                    EmailUsuario = data!.EmailUsuario,
                    Mensaje = "📅 Tu nuevo ciclo menstrual ha sido registrado correctamente.",
                    TipoUsuario = "CLIENTE"
                };
                db.Notificaciones.Add(noti);
            }

            db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR al procesar evento Kafka: {ex.Message}");
        }
    }
});

app.Run();
