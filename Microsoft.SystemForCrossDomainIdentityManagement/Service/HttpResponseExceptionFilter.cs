// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
namespace Microsoft.SCIM
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Net;

    public sealed class HttpResponseExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpResponseException httpEx)
            {
                context.Result = new StatusCodeResult((int)httpEx.StatusCode);
                context.ExceptionHandled = true;
            }
        }
    }
}
