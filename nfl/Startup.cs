using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NFL.Models;

namespace NFL
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddRouting(options => options.LowercaseUrls = true);
      services.AddControllersWithViews().AddNewtonsoftJson();

      services.AddMemoryCache();
      services.AddSession();
      //services.AddSession(options => {
      //  options.IdleTimeout = TimeSpan.FromSeconds(60 * 5);
      //  options.Cookie.HttpOnly = false;
      //  options.Cookie.IsEssential = true;
      //});

      services.AddDbContext<TeamContext>(options =>
          options.UseSqlServer(Configuration.GetConnectionString("TeamContext")));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseDeveloperExceptionPage();
      app.UseStaticFiles();

      app.UseSession();

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
          name: "",
          pattern: "{controller=Home}/{action=Index}/conf/{activeConf}/div/{activeDiv}");
        
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
