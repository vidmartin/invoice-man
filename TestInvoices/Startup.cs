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

            //testovací faktura na zaèátek
            var testInvoice = new Invoice()
            {
                Seller = new Company()
                {
                    Address = "Blekotalova 13",
                    City = "Praha 70",
                    IÈO = "12345678",
                    DIÈ = "CZ123456789",
                    Name = "Frgály s.r.o.",
                    Phone = "606 888 999",
                    PSÈ = "123 45"
                },
                Buyer = new Company()
                {
                    Address = "Žblebtalova 23",
                    City = "Praha -7",
                    IÈO = "56565646",
                    DIÈ = "CZ987654321",
                    Name = "Spojení traktoristé a.s.",
                    Phone = "777 888 777",
                    PSÈ = "555 55"
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
                return service; //správce faktur inicializovaný s jednou fakturou
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
