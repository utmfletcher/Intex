using Intex.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Intex.Models;
using Microsoft.AspNetCore.Builder;



namespace Intex
{
    public class Program
    {
        public static void Main(string[] args)
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
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            });

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

            app.MapControllerRoute("PageNumSizeType", "ProductDisplay/{pageNum}/{pageSize}/{productType}", new { Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5 });

            app.MapControllerRoute("PageNumSize", "ProductDisplay/{pageNum}/{pageSize}", new { Controller = "Home", action = "ProductDisplay" });
            
            app.MapControllerRoute("pagenumandtype", "ProductDisplay/{productType}/{pageNum}", new { Controller = "Home", action = "ProductDisplay", pageSize = 5 });


            app.MapControllerRoute("pagination", "ProductDisplay/{pageNum}", new {Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5});
            app.MapControllerRoute("productType", "ProductDisplay/{productType}", new { Controller = "Home", action = "ProductDisplay", pageNum = 1, pageSize = 5 });
 

            app.MapDefaultControllerRoute();



            app.MapRazorPages();

            app.Run();
        }
    }
}
