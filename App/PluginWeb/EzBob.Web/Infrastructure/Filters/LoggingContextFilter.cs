﻿using System.Web.Mvc;
using Scorto.Web;
using StructureMap;
using log4net;

namespace EzBob.Web.Infrastructure.Filters
{
    public class LoggingContextFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null) return;

            ThreadContext.Properties["Action"] = filterContext.ActionDescriptor.ActionName;
            ThreadContext.Properties["Controller"] = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            ThreadContext.Properties["IP"] = filterContext.RequestContext.HttpContext.Request.UserHostAddress;

            var context = ObjectFactory.GetInstance<IWorkplaceContext>();
            if (context == null) return;
            if (context.User == null) return;
            ThreadContext.Properties["UserId"] = context.User.Id;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}