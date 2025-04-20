using Microsoft.EntityFrameworkCore;
using NotificacionesService.Models;

namespace NotificacionesService.Data;

public class NotificacionesDbContext : DbContext
{
    public NotificacionesDbContext(DbContextOptions<NotificacionesDbContext> options)
        : base(options) { }

    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
}
