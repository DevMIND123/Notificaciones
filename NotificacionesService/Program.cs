using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NotificacionesService.Data;
using NotificacionesService.Models;

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

// 🧭 Requerido para enrutar las peticiones
app.UseRouting();

// 🧠 Activar CORS correctamente entre Routing y Endpoints
app.UseCors("AllowFrontend");

// ✅ Endpoint GET /notificaciones/{idUsuario}
app.MapGet("/api.retochimba.com/notificaciones/{idUsuario}", async (int idUsuario, NotificacionesDbContext db) =>
{
    var notis = await db.Notificaciones
        .Where(n => n.IdUsuario == idUsuario)
        .OrderByDescending(n => n.Fecha)
        .ToListAsync();
    return Results.Ok(notis);
});

// 🚀 Iniciar consumidor Kafka
var config = new ConsumerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
    GroupId = builder.Configuration["Kafka:GroupId"],
    AutoOffsetReset = AutoOffsetReset.Earliest
};

_ = Task.Run(() =>
{
    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    consumer.Subscribe(builder.Configuration["Kafka:Topic"]);

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

            var data = System.Text.Json.JsonSerializer.Deserialize<EventoKafkaDTO>(cr.Message.Value);

            Console.WriteLine($"🔍 Datos deserializados: id={data.IdUsuario}, nombre={data.Nombre}");

            var noti = new Notificacion
            {
                IdUsuario = data.IdUsuario,
                Mensaje = $"🎉 ¡Bienvenido {data.Nombre} a Reto Chimba!",
                TipoUsuario = data.Tipo
            };

            db.Notificaciones.Add(noti);
            db.SaveChanges();

            Console.WriteLine($"✅ Notificación guardada para usuario {noti.IdUsuario}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR al guardar notificación: {ex.Message}");
        }
    }
});

app.Run();
