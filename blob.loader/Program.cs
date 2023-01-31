
namespace blob.loader;

using Azure.Identity;
using Azure.Storage.Blobs;
using blob.loader.Interfaces;
using blob.loader.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using System.Net;

public class Program
{
    //public static void Main(string[] args)
    //{
    //    var builder = Host.CreateDefaultBuilder(args)
    //        .ConfigureWebHostDefaults(webBuilder =>
    //        {
    //            webBuilder.ConfigureKestrel((context, options) =>
    //            {
    //                // Handle requests up to 500 MB
    //                options.Limits.MaxRequestBodySize = 502428800;
    //                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    //            })
    //            .UseStartup<Startup>();
    //        })
    //        .Build();

    //    builder.Run();
    //}

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.WebHost.UseKestrel().ConfigureKestrel((context, options) =>
        {
            // Handle requests up to 500 MB
            options.Limits.MaxRequestBodySize = 502428800;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
        })
        ;

        builder.Services.AddRazorPages();

        builder.Services.AddTransient<IStreamFileUploadService, StreamFileUploadService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthorization();

        app.MapRazorPages();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}