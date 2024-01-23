﻿using EDP_Backend.Hubs;
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
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            notification.User = null;
            await _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("notification", notification);


        }
    }
}