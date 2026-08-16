using Microsoft.AspNetCore.Mvc.Razor;

namespace wwfpp.Helpers
{
    public class SubfolderViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context) { }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Add your custom subfolders here
            var extraLocations = new[]
            {
            "/Views/Shared/{1}/Assister/{0}.cshtml",
            "/Views/Shared/{1}/Reports/{0}.cshtml",
            "/Views/Shared/{1}/Dashboards/{0}.cshtml",
            // add as many category folders as you use
        };

            // {0} = view name, {1} = controller name
            return extraLocations.Concat(viewLocations);
        }
    }
}
