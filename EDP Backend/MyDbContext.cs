using EDP_Backend.Hubs;
using EDP_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
namespace EDP_Backend
{
    public class MyDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ActionsHub> _hubContext;
        public MyDbContext(
            IConfiguration configuration,
            IHubContext<ActionsHub> hubContext
        )
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activity>()
                        .OwnsOne(e => e.Pictures, sa =>
                        {
                            sa.Property(p => p.Items)
                              .HasColumnName("StringArray")
                              .HasConversion(
                                  v => string.Join(',', v),
                                  v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
                        });
            modelBuilder.Entity<Review>()
                        .OwnsOne(e => e.Pictures, sa =>
                        {
                            sa.Property(p => p.Items)
                              .HasColumnName("StringArray")
                              .HasConversion(
                                  v => string.Join(',', v),
                                  v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
                        });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder
        optionsBuilder)
        {
            string? connectionString = _configuration.GetConnectionString(
            "MyConnection");
            if (connectionString != null)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public async void SendNotification(int userId, string title, string subtitle, string type, string action, string actionUrl)
        {
            // get user
            var user = Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return;
            }

            var notification = new Notification
            {
                User = user,
                Title = title,
                Subtitle = subtitle,
                Type = type,
                Action = action,
                ActionUrl = actionUrl
            };

            
            Notifications.Add(notification);
            SaveChanges();
            ChangeTracker.Clear();
            notification.User = null;
            await _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("notification", notification);


        }
    }
}