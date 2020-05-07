using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebMVCRazor.Startup))]
namespace WebMVCRazor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
