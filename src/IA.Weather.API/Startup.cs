﻿using System.Web.Http;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;
using Swashbuckle.Application;
using Microsoft.Owin.Cors;
using Serilog;

namespace IA.Weather.API
{
    public abstract class Startup
    {
        private readonly HttpConfiguration _configuration;
        private readonly Container _container;

        protected Startup()
        {
            _configuration = new HttpConfiguration();
            _container = new Container();
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            //You'd usually lock this down a bit more by restricting the origin
            appBuilder.UseCors(CorsOptions.AllowAll);

            Container();
            Routes();
            Swagger();

            appBuilder.UseWebApi(_configuration);

        }

        protected abstract void Bindings(Container container);

        private void Routes()
        {
            _configuration.MapHttpAttributeRoutes();

            _configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private void Swagger()
        {
            _configuration
                .EnableSwagger(c => { c.SingleApiVersion("v1", "IA.Weather.API"); })
                .EnableSwaggerUi();
        }

        private void Container()
        {
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            _container.RegisterWebApiControllers(_configuration);
            _container.Register(() => Log.Logger, Lifestyle.Singleton);

            Bindings(_container);

            _container.Verify();
            _configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(_container);


        }
    }
}
