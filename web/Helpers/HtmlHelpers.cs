using System;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;

namespace web.Helpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString IconActionLink(this AjaxHelper helper, string icon, string text, string actionName, string controllerName, object routeValues = null, AjaxOptions ajaxOptions = null, object htmlAttributes = null)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.ActionLink("[replaceme] " + text, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes).ToHtmlString();
            return new MvcHtmlString(link.Replace("[replaceme]", builder.ToString()));
        }

        public static MvcHtmlString IconActionLink(this HtmlHelper helper, string icon, string text, string actionName, string controllerName, object routeValues = null, object htmlAttributes = null)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.ActionLink("[replaceme] " + text, actionName, controllerName, routeValues, htmlAttributes).ToHtmlString();
            return new MvcHtmlString(link.Replace("[replaceme]", builder.ToString()));
        }
    }
}