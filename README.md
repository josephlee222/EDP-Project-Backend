# NTUC UPlay - Back-end
*Enterprise Development Project 2024*

### Theme
NTUC UPlay Website

### About  
UPlay, powered by NTUC Club, is a phygital (physical + digital) concierge of curatorial recreation experiences to enhance the social well-being of all workers.

More than just a booking platform, UPlay aspires to connect people from all walks of life, forging new relationships over time as they find a common thread through shared interests. Union and companies can also join us in creating fun and engaging communities while cultivating deep connections and lifelong relationships.

### Features
- Re-designed user interface with modern web framework
- User management & profile
- Activities with booking timeslots and daily pricing
- Cart with wallet system (Payment by Stripe)
- Group discussions /w realtime chat
- Discount coupons
- Real-time notifications

### Credits
- **General Design, User Management, Cart + Checkout, Groups (Real-time chat)** - Joseph Lee
- **Activity Management, Availabilities Slots, Bookings, Coupons** - Toby Benedict
- **Groups (Back-end)** - Jing Hao

### Technologies
- **Front-end** - ReactJS, Material UI, SignalR Client, Stripe Payment & Elements, Google & Facebook OAuth
- **Back-end** - ASP.NET Core API, SignalR Server, fido2-net-lib, Microsoft SQL & Entity Framework

### Hosting
- **Front-end** - Powered by Vercel
- **Back-end** - Powered by Azure App Service with SQL Server by Azure

## Back-End Usage

1. Clone Repo & open project
2. Under tools > Nuget Package Manager Console, open it
3. Run `database-update`
4. Create `.env` file in project root (contact me for details)
5. Run Project
6. (Optional but recommended) In Swagger, run the `User/Init` route to initialise the first admin user
