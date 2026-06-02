using Microsoft.EntityFrameworkCore;
using Tasqana.Controllers;
using Tasqana.Models;


namespace Tasqana.Repositories
{
    public class TaskanaDb : DbContext
    {

        public DbSet<User> Users => Set<User>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<CheckItem> todoListItems => Set<CheckItem>();
        public DbSet<TelegramMessage> TelegamMessages => Set<TelegramMessage>();

        private readonly IConfiguration _configuration;
        private readonly ILogger<TaskanaDb> _logger;

        public TaskanaDb(
            IConfiguration configuration,
            ILogger<TaskanaDb> logger
            ) { 
            _configuration = configuration;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
            } );

            builder.Entity<Session>(entity => {
                entity.ToTable("sessions");
                entity.HasKey(u => u.Id);
                entity.HasIndex(e => e.TokenHash);
            
                entity.HasOne(e => e.User)
                    .WithMany(p => p.Sessions)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
                

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Parent)                 
                      .WithMany(p => p.SubCategories)         
                      .HasForeignKey(c => c.ParentId)          
                      .OnDelete(DeleteBehavior.Cascade);      

                entity.HasOne(c => c.User)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Todo>(entity => {
                entity.ToTable("todos");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.User)
                    .WithMany(p => p.Todos)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Category)
                    .WithMany(p => p.Todos)
                    .HasForeignKey(c => c.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TelegramMessage>(entity =>
            {
                entity.ToTable("telegram_messages");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.User)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CheckItem>(entity =>
            {
                entity.ToTable("todos_checkitems");
                entity.HasKey(c => c.Id);
              //  entity.HasIndex(c => c.TodoId);

                entity.HasOne(c => c.Todo)
                    .WithMany(c => c.CheckItems)
                    .HasForeignKey(c => c.TodoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges()
        {
            BeforeSave();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            BeforeSave();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigureConnection(optionsBuilder);
        }

        private void BeforeSave()
        {
            var entries = base.ChangeTracker.Entries()
                .Where(e => e.Entity is IBeforeSaveBehavior);

            foreach (var entry in entries)
            {
                var entity = entry.Entity as IBeforeSaveBehavior;
                entity!.BeforeSave(this);
            }
        }

        private void ConfigureConnection(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = BuildConnectionString();

            optionsBuilder.UseNpgsql(connectionString);
        }

        private string BuildConnectionString()
        {
            //var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //if (env == "Local") Console.WriteLine($"LOCAL MODE");
            //return "host=localhost port=5432 database=taskana username=taskana password=1";
            var config = _configuration.GetSection("PostgresConnectionData").GetChildren();
            var connectionString = string.Empty;
            connectionString = config
                .Where(section => !string.IsNullOrEmpty(section.Value))
                .Aggregate(connectionString, (current, setup) => current + $"{setup.Key}={setup.Value};");
            return connectionString;
        }
    }
}
