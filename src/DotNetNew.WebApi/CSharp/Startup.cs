#if (FrameworkNetCoreApp21)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif
#if (FrameworkNet50)
using Microsoft.OpenApi.Models;
#endif
#if (CloudHystrix)
using Steeltoe.CircuitBreaker.Hystrix;
#endif
#if (RabbitMqConnector)
#if (Steeltoe2)
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
#else
using Steeltoe.Connector.RabbitMQ;
#endif
#endif
#if (EurekaClient)
using Steeltoe.Discovery.Client;
#endif
#if (CloudFoundryHosting)
using Steeltoe.Extensions.Configuration.CloudFoundry;
#endif
#if (ManagementEndpoints)
#if (Steeltoe2)
using Steeltoe.Management.CloudFoundry;
#else
using Steeltoe.Management.Endpoint;
#endif
#endif

namespace Company.WebApplication1
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
#if (CloudFoundryHosting)
            services.ConfigureCloudFoundryOptions(Configuration);
#endif
#if (EurekaClient)
            services.AddDiscoveryClient(Configuration);
#endif
#if (RabbitMqConnector)
            services.AddRabbitMQConnection(Configuration);
#endif
#if (CloudHystrix)
            services.AddHystrixCommand<HelloHystrixCommand>("MyCircuitBreakers", Configuration);
            services.AddHystrixMetricsStream(Configuration);
#endif
#if (ManagementEndpoints)
#if (Steeltoe2)
            services.AddCloudFoundryActuators(Configuration);
#else
            services.AddAllActuators(Configuration);
#endif
#endif
#if (FrameworkNetCoreApp21)
            services.AddMvc();
#else
            services.AddControllers();
#endif
#if (FrameworkNet50)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.WebApplication1", Version = "v1" });
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#if (FrameworkNetCoreApp21)
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

#if (EurekaClient)
            app.UseDiscoveryClient();
#endif
#if (CloudHystrix)
            app.UseHystrixRequestContext();
            app.UseHystrixMetricsStream();
#endif
#if (ManagementEndpoints)
            app.UseCloudFoundryActuators();
#endif
            app.UseMvc();
        }
#else
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if (FrameworkNet50)
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company.WebApplication1 v1"));
#endif
            }

#if (EurekaClient)
            app.UseDiscoveryClient();
#endif
#if (CloudHystrix)
            app.UseHystrixRequestContext();
            app.UseHystrixMetricsStream();
#endif
#if (ManagementEndpoints && Steeltoe2)
            app.UseCloudFoundryActuators();
#endif
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
#endif
    }
}
