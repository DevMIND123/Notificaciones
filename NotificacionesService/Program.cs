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
        var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        }, out _);

        foreach (var claim in claimsPrincipal.Claims)
        {
            Console.WriteLine($"🧩 CLAIM: {claim.Type} = {claim.Value}");
        }

        var email = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("⛔ No se encontró el claim 'email' en el token.");
            return Results.Unauthorized();
        }

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
_ = Task.Run(() =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        GroupId = builder.Configuration["Kafka:GroupId"],
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    var topics = builder.Configuration.GetSection("Kafka:Topic").Get<string[]>();
    consumer.Subscribe(topics);

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
                if (data is not null && !string.IsNullOrEmpty(data.EmailUsuario) && !string.IsNullOrEmpty(data.Mensaje))
                {
                    Console.WriteLine($"📨 Guardando notificación ciclo: {data.Mensaje}");
                    var noti = new Notificacion
                    {
                        EmailUsuario = data.EmailUsuario,
                        Mensaje = data.Mensaje,
                        TipoUsuario = "CLIENTE"
                    };
                    db.Notificaciones.Add(noti);
                }
                else
                {
                    Console.WriteLine("⛔ Datos del evento ciclo inválidos.");
                }
            }
            else if (cr.Topic == "embarazo-registrado")
            {
                var data = JsonSerializer.Deserialize<EventoEmbarazoKafkaDTO>(cr.Message.Value);
                if (data is not null && !string.IsNullOrEmpty(data.EmailUsuario) && !string.IsNullOrEmpty(data.Mensaje))
                {
                    Console.WriteLine($"📨 Guardando notificación embarazo: {data.Mensaje}");
                    var noti = new Notificacion
                    {
                        EmailUsuario = data.EmailUsuario,
                        Mensaje = data.Mensaje,
                        TipoUsuario = "CLIENTE"
                    };
                    db.Notificaciones.Add(noti);
                }
                else
                {
                    Console.WriteLine("⛔ Datos del evento embarazo inválidos.");
                }
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