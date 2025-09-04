// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Net;
    using System.Net.Http;

    internal static class HttpStringResponseExceptionFactory
    {
        public static HttpResponseMessage CreateMessage(HttpStatusCode statusCode, string content) =>
            new HttpResponseMessage(statusCode){ Content = new StringContent(content) };
    }
}
