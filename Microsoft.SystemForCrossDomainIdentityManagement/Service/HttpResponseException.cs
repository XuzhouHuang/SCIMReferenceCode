// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
namespace Microsoft.SCIM
{
    using System;
    using System.Net;

    // Lightweight replacement for System.Web.Http.HttpResponseException used by legacy code paths.
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpResponseException(HttpStatusCode statusCode)
            : base($"HTTP {(int)statusCode} {statusCode}")
        {
            this.StatusCode = statusCode;
        }
    }
}
