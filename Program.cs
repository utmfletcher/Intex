using Intex.Data;
using Intex.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Intex.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Security.Policy;

namespace Intex
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("PostgresConnection") ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(connectionString));

            builder.Services.AddScoped<IProductRepository, EFProductRepository>();


            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true) 
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            });
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();
            builder.Services.AddScoped<Cart>(sp=>SessionCart.GetCart(sp));  
            builder.Services.AddSingleton
                <IHttpContextAccessor, HttpContextAccessor>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {   
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
         
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            //app.MapControllerRoute(
            //    "default",
            //    "{controller=Home}/{action=Index}"
            //);

            //app.MapControllerRoute(
            //    "productTypeWithPage",
            //    "{productType}/{pageNum:int}",
            //    new { controller = "Products", action = "Index" }
            //);

            //app.MapControllerRoute(
            //    "pagination",
            //    "products/page/{pageNum:int}",
            //    new { controller = "Products", action = "List" }
            //);

            //app.MapControllerRoute(
            //    "type",
            //    "products/{productType}",
            //    new { controller = "Products", action = "ByType" }
            //);
            app.MapControllerRoute( "ProductDisplayWithColour", "ProductDisplay/{pageNum}/{pageSize}/{productType}/{productColour}", new { Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5, productType = (string?)null, productColour = (string?)null }
);

            app.MapControllerRoute("PageNumSizeType", "ProductDisplay/{pageNum}/{pageSize}/{productType}", new { Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5 });

            app.MapControllerRoute("PageNumSize", "ProductDisplay/{pageNum}/{pageSize}", new { Controller = "Home", action = "ProductDisplay" });
            
            app.MapControllerRoute("pagenumandtype", "ProductDisplay/{productType}/{pageNum}", new { Controller = "Home", action = "ProductDisplay", pageSize = 5 });

           

            app.MapControllerRoute("pagination", "ProductDisplay/{pageNum}", new {Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5});
            app.MapControllerRoute("productType", "ProductDisplay/{productType}", new { Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5 });
            
            app.MapControllerRoute("productId", "ProductDetails/{productId}", new { Controller = "Home", action = "ProductDetails"});

            


            app.MapDefaultControllerRoute();
            app.UseSession();

            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                ctx.Response.Headers.Add("Referrer-Policy", "no-referrer");
                ctx.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://app.termly.io; " + // Add https://app.termly.io here
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' https://images.brickset.com https://www.lego.com https://*.amazonaws.com https://*.googleusercontent.com https://m.media-amazon.com https://www.brickeconomy.com data:; " +
                    "font-src 'self'; " +
                    "frame-src 'self'; " +
                    "object-src 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'; " +
                    "connect-src 'self';");

                await next();
            });



            app.MapRazorPages();
            using(var scope = app.Services.CreateScope())
            {
                var roleManager 
                    = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Manager","User" };

                foreach (var role in roles)
                {

                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            using (var scope = app.Services.CreateScope())
            {
                var userManager
                    = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string email = "admin@admin.com";
                string password = "Legoisfun1975!";

                //if(await userManager.FindByEmailAsync(email)== null)
                //{
                //    var user = new IdentityUser();
                //    user.UserName = email;
                //    user.Email = email;
                //    user.EmailConfirmed = true;

                //    await userManager.CreateAsync(user, password);

                //    await userManager.AddToRoleAsync(user, "Admin");

                //}

                
            }

            app.Run();
        }
    }
}
