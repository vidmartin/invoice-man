using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestInvoices.Api;
using TestInvoices.DbModels;
using TestInvoices.Managers;

namespace TestInvoices
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
            services.AddControllersWithViews();

            //testovac� faktura na za��tek
            var testInvoice = new Invoice()
            {
                Seller = new Company()
                {
                    Address = "Blekotalova 13",
                    City = "Praha 70",
                    I�O = "12345678",
                    DI� = "CZ123456789",
                    Name = "Frg�ly s.r.o.",
                    Phone = "606 888 999",
                    PS� = "123 45"
                },
                Buyer = new Company()
                {
                    Address = "�blebtalova 23",
                    City = "Praha -7",
                    I�O = "56565646",
                    DI� = "CZ987654321",
                    Name = "Spojen� traktorist� a.s.",
                    Phone = "777 888 777",
                    PS� = "555 55"
                },
                DateOfIssue = DateTime.Now,
                DueDate = DateTime.Now + TimeSpan.FromDays(7),
                Paid = false
            };

            testInvoice.Items.Add(new InvoiceItem()
            {
                Count = 2,
                PricePerOne = 500,
                Description = "Slepice"
            });

            testInvoice.Items.Add(new InvoiceItem()
            {
                Count = 3,
                PricePerOne = 100,
                Description = "Kabel"
            });

            services.AddSingleton<IInvoiceManager>(_ =>
            {
                var service = new MockInvoiceManager();
                service.Add(testInvoice);
                return service; //spr�vce faktur inicializovan� s jednou fakturou
            });

            services.AddSingleton<IApiAuthenticator, MockApiAuthenticator>();
            services.AddSingleton<IModelPatcher<Invoice, ApiEditInvoiceModel>, ApiInvoicePatcher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();                
            });
        }
    }
}
