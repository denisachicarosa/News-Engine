using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PlatformaDeStiri.Startup))]
namespace PlatformaDeStiri
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
