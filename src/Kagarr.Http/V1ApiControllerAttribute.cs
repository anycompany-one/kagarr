using System;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Kagarr.Http
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class V1ApiControllerAttribute : Attribute, IRouteTemplateProvider
    {
        public string Template
        {
            get
            {
                return "api/v1/[controller]";
            }
        }

        public int? Order => 2;
        public string Name { get; set; }
    }
}
