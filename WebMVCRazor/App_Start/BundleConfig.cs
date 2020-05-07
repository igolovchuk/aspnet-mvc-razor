using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using System.Web;
using System.Web.Optimization;

namespace WebMVCRazor
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            bundles.UseCdn = true;
            var cssTransformer = new CssTransformer();
            var jsTransformer = new JsTransformer();
            var nullOrderer = new NullOrderer();

            var cssBundle = new CustomStyleBundle("~/bundles/css");
            cssBundle.Include("~/Content/Site.less", "~/Content/bootstrap.css");
            cssBundle.Transforms.Add(cssTransformer);
            cssBundle.Orderer = nullOrderer;
            bundles.Add(cssBundle);

            var momemtBundle = new CustomScriptBundle("~/bundles/moment");
            momemtBundle.Include("~/Scripts/moment.js","~/Scripts/moment-with-locales.js");
            momemtBundle.Transforms.Add(jsTransformer);
            momemtBundle.Orderer = nullOrderer;
            bundles.Add(momemtBundle);

            

            bundles.Add(new StyleBundle("~/Content/GridCss").Include(                                
                                  "~/Content/WebGrid.css")
                                 );

            bundles.Add(new StyleBundle("~/Content/PrintStyles").Include(
                                  "~/Content/PrintStyles.css"));

            bundles.Add(new ScriptBundle("~/Scripts/ShowHide").Include(
                                  "~/Scripts/ShowHide.js"));

            bundles.Add(new ScriptBundle("~/Scripts/RedRows").Include(
                              "~/Scripts/RedRows.js"));

            var jqueryBundle = new CustomScriptBundle("~/bundles/jquery");
            jqueryBundle.Include("~/Scripts/jquery-{version}.js");
            jqueryBundle.Transforms.Add(jsTransformer);
            jqueryBundle.Orderer = nullOrderer;
            bundles.Add(jqueryBundle);

            var jqueryvalBundle = new CustomScriptBundle("~/bundles/jqueryval");
            jqueryvalBundle.Include("~/Scripts/jquery.validate*");
            jqueryvalBundle.Transforms.Add(jsTransformer);
            jqueryvalBundle.Orderer = nullOrderer;
            bundles.Add(jqueryvalBundle);

            

            var modernizrBundle = new CustomScriptBundle("~/bundles/modernizr");
            modernizrBundle.Include("~/Scripts/modernizr-*");
            modernizrBundle.Transforms.Add(jsTransformer);
            modernizrBundle.Orderer = nullOrderer;
            bundles.Add(modernizrBundle);


            var bootstrapBundle = new CustomScriptBundle("~/bundles/bootstrap");
            bootstrapBundle.Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js");
            bootstrapBundle.Transforms.Add(jsTransformer);
            bootstrapBundle.Orderer = nullOrderer;
            bundles.Add(bootstrapBundle);


        }
    }
}
