using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;
using Microsoft.EntityFrameworkCore;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Data
{
    /// <summary>
    /// Контекст базы данных для управления пользователями.
    /// </summary>
    public class UserManagementDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserManagementDbContext"/>.
        /// </summary>
        /// <param name="options">Опции для конфигурации контекста.</param>
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Коллекция пользователей в базе данных.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Конфигурирует модель базы данных при ее создании.
        /// </summary>
        /// <param name="modelBuilder">Строитель для конструирования модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }
    }
}