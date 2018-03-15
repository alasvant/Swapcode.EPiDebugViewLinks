using EPiServer.PlugIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Logging;
using Swapcode.EPiDebugViewLinks.Models;
using EPiServer;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Configuration;

namespace Swapcode.EPiDebugViewLinks.Controllers
{
    // using Administrators group because the DebugView uses it too - so if using SQL membershi etc you need to create this group and add your user to that group (after change you need to logout and log back in)
    [Authorize(Roles = "Administrators")]
    [GuiPlugIn(Area = PlugInArea.AdminMenu, UrlFromModuleFolder = "DebugViewLinks", DisplayName = "Debug links")]
    public class SwapDebugViewLinksController : Controller
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(SwapDebugViewLinksController));

        private static readonly Regex cmsRegex = new Regex("/cms/$", RegexOptions.IgnoreCase);

        // NOTE: we are using module feature to prefix our controllers (see module.config in this project), in module config the controller prefix Swap will be removed in the routing
        // GuiPlugIn: UrlFromModuleFolder = "DebugViewLinks" => our controller without the "Swap" prefix. Url in admin will be: /EPiDebugViewLinks/DebugViewLinks (module name + controller name without the prefix)

        public ActionResult Index()
        {
            try
            {
                // is the debugview enabled
                bool isEnabled = string.Compare("true", ConfigurationManager.AppSettings["EPi.DebugView.Enabled"], StringComparison.OrdinalIgnoreCase) == 0;

                DebugView viewModel = new DebugView(isEnabled);

                // Episerver built-in debug views
                // https://world.episerver.com/blogs/Per-Bjurstrom/Archive/2012/9/EPiServer-7-Startup-Performance/
                // https://www.david-tec.com/2015/02/episerver-debugging-tools/

                // get the Episerver debug view controller
                Type epiDebugController = typeof(EPiServer.Shell.UI.Controllers.EPiDebugController);

                // get the action methods from the type (not perfect where clause but close enough for this demo)
                var actionMethods = epiDebugController.GetMethods().Where(mi => !mi.IsStatic && !mi.IsSpecialName && mi.Name != "Dispose" && mi.DeclaringType == epiDebugController);

                // get Episerver debug controller path
                string controllerUrl = GetDebugControllerPath();

                foreach (var action in actionMethods)
                {
                    viewModel.Links.Add(new DebugViewLink()
                    {
                        LinkText = action.Name,
                        ActionUrl = $"{controllerUrl}{action.Name}" // and we know it is not using attribute routing so we can just use the action method name
                    });
                }

                // sort the actions
                viewModel.Links.Sort((a, b) => a.LinkText.CompareTo(b.LinkText));

                return View(GetViewLocation("Index"), viewModel);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to render view to display EPiDebug view links.", ex);

                throw;
            }
        }

        private static string GetDebugControllerPath()
        {
            string uiPath = UriSupport.InternalUIUrl.Path;

            if (cmsRegex.IsMatch(uiPath) && uiPath.Length > 5) // length > 5 so that there is something left after the replacement other option is to just panic and throw an exception :D
            {
                // currently the episerver uiUrl has to always contain /cms/ at the end and we need to remove that
                uiPath = cmsRegex.Replace(uiPath, "/");
            }

            if (uiPath.StartsWith("~"))
            {
                uiPath = uiPath.Substring(1);
            }

            // we just know the url to the debug controller will be like this, mkay? :D
            return UriSupport.Combine(uiPath, "/Shell/Debug/");
        }

        private static string GetViewLocation(string viewName)
        {
            // Episerver has registered a razor view engine for our module with custom view locations like these:
            // ~/Modules/_Protected/EPiDebugViewLinks/Views/DebugViewLinks/[ACTION-NAME].cshtml
            // ~/Modules/_Protected/EPiDebugViewLinks/Views/Shared/[ACTION-NAME].cshtml

            // BUT THOSE DON'T SEEM TO WORK, I BELIEVE THOSE URLS WORKED OCCASIONALLY DURING TESTING BUT MOST OF THE TIME NOT
            // SO THAT IS THE REASON WHY THIS CUSTOM METHOD TO GET VIEW LOCATIONS

            // ProtectedRootPath == from web.config episever.shell -> <protectedModules rootPath="~/YOUR-VALUE-HERE/">
            // but the views can be found also using the: ProtectedRootPath + our module name + the normal path inside our module (remember we will zip the module)
            // so if you config value is EPiServer you get: ~/EPiServer/EPiDebugViewLinks/Views/DebugViewLinks/[viewName].cshtml

            return $"{EPiServer.Shell.Paths.ProtectedRootPath}EPiDebugViewLinks/Views/DebugViewLinks/{viewName}.cshtml";
        }

        public ActionResult Info()
        {
            ModuleVersion viewModel = new ModuleVersion();

            try
            {
                viewModel.Version = typeof(SwapDebugViewLinksController).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }
            catch{}


            string viewLocation = GetViewLocation("Info");

            // giving the view location
            return View(viewLocation, viewModel);
        }

        public ActionResult Extra()
        {
            // this view file is in the modules folder so it will work ok without specifying view location
            return View();
        }

        public ActionResult ExtraTwo()
        {
            // this view file is in the modules folder so it will work ok without specifying view location
            return View("Extra");
        }
    }
}