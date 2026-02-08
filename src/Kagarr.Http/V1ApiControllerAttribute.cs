using System;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Kagarr.Http
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class V1ApiControllerAttribute : Attribute, IRouteTemplateProvider
    {
        private const string ControllerResource = "[controller]";

        public V1ApiControllerAttribute(string resource = ControllerResource)
        {
            Resource = resource;
            Template = $"api/v1/{resource}";
        }

        public string Resource { get; }
        public string Template { get; }

        public int? Order => 2;
        public string Name { get; set; }
    }
}
