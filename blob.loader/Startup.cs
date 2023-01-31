
using blob.loader.Interfaces;
using blob.loader.Services;
using blob.loader.Unused.Attributes;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace blob.loader;

public class Startup
{
    // ConfigureServices is where you register dependencies. This gets
    // called by the runtime before the ConfigureContainer method, below.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the collection. Don't build or return
        // any IServiceProvider or the ConfigureContainer method
        // won't get called. Don't create a ContainerBuilder
        // for Autofac here, and don't call builder.Populate() - that
        // happens in the AutofacServiceProviderFactory for you.
        //services.AddLogging(builder =>
        //{
        //    builder.ClearProviders();
        //    builder.AddSimpleConsole();
        //});

        //services.AddMvc().AddRazorPagesOptions(options =>
        //{
        //    options.Conventions.AddPageRoute("/Pages/Loader", "");
        //});

        services.AddControllersWithViews();
        services.AddRazorPages(options =>
        {
            options.Conventions
                .AddPageApplicationModelConvention("/StreamFileUpload",
                    model =>
                    {
                        //model.Filters.Add(
                        //    new GenerateAntiforgeryTokenCookieAttribute());
                        model.Filters.Add(
                            new DisableFormValueModelBindingAttribute());
                    });
        });
        services.AddTransient<IStreamFileUploadService, StreamFileUploadService>();
    }

    //// ConfigureContainer is where you can register things directly
    //// with Autofac. This runs after ConfigureServices so the things
    //// here will override registrations made in ConfigureServices.
    //// Don't build the container; that gets done for you by the factory.
    //public void ConfigureContainer(ContainerBuilder builder)
    //{
    //    // Register your own things directly with Autofac here. Don't
    //    // call builder.Populate(), that happens in AutofacServiceProviderFactory
    //    // for you.
    //    //builder.RegisterModule(new MyApplicationModule());
    //}

    // Configure is where you add middleware. This is called after
    // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
    // here if you need to resolve things from the container.
    public void Configure(
        IApplicationBuilder app,
        ILoggerFactory loggerFactory)
    {
        //// If, for some reason, you need a reference to the built container, you
        // can use the convenience extension method GetAutofacRoot.
        app.UseExceptionHandler("/Home/Error");

        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}