// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Microsoft.AspNetCore.WebUtilities;

    public sealed class ResourceQuery : IResourceQuery
    {
        private const char SeperatorAttributes = ',';

        private static readonly Lazy<char[]> SeperatorsAttributes =
            new Lazy<char[]>(
                () =>
                    new char[]
                        {
                            ResourceQuery.SeperatorAttributes
                        });

        public ResourceQuery()
        {
            this.Filters = Array.Empty<Filter>();
            this.Attributes = Array.Empty<string>();
            this.ExcludedAttributes = Array.Empty<string>();
        }

        public ResourceQuery(
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> attributes,
            IReadOnlyCollection<string> excludedAttributes)
        {
            this.Filters = filters ?? throw new ArgumentNullException(nameof(filters));
            this.Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            this.ExcludedAttributes = excludedAttributes ?? throw new ArgumentNullException(nameof(excludedAttributes));
        }

        public ResourceQuery(Uri resource)
        {
            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            string query = resource.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                // query starts with '?'
                var parsed = QueryHelpers.ParseQuery(query);
                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> pair in parsed)
                {
                    string key = pair.Key;
                    string value = pair.Value.FirstOrDefault();
                    if (string.Equals(key, QueryKeys.Attributes, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                    {
                        this.Attributes = ResourceQuery.ParseAttributes(value);
                    }
                    else if (string.Equals(key, QueryKeys.Count, StringComparison.OrdinalIgnoreCase))
                    {
                        Action<IPaginationParameters, int> action = (p, v) => p.Count = v;
                        this.ApplyPaginationParameter(value, action);
                    }
                    else if (string.Equals(key, QueryKeys.ExcludedAttributes, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                    {
                        this.ExcludedAttributes = ResourceQuery.ParseAttributes(value);
                    }
                    else if (string.Equals(key, QueryKeys.Filter, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                    {
                        this.Filters = ResourceQuery.ParseFilters(value);
                    }
                    else if (string.Equals(key, QueryKeys.StartIndex, StringComparison.OrdinalIgnoreCase))
                    {
                        Action<IPaginationParameters, int> action = (p, v) => p.StartIndex = v;
                        this.ApplyPaginationParameter(value, action);
                    }
                }
            }

            if (null == this.Filters)
            {
                this.Filters = Array.Empty<Filter>();
            }

            if (null == this.Attributes)
            {
                this.Attributes = Array.Empty<string>();
            }

            if (null == this.ExcludedAttributes)
            {
                this.ExcludedAttributes = Array.Empty<string>();
            }
        }

        public IReadOnlyCollection<string> Attributes
        {
            get;
            private set;
        }

        public IReadOnlyCollection<string> ExcludedAttributes
        {
            get;
            private set;
        }

        public IReadOnlyCollection<IFilter> Filters
        {
            get;
            private set;
        }

        public IPaginationParameters PaginationParameters
        {
            get;
            set;
        }

        private void ApplyPaginationParameter(
            string value,
            Action<IPaginationParameters, int> action)
        {
            if (null == action)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            int parsedValue = int.Parse(value, CultureInfo.InvariantCulture);
            if (null == this.PaginationParameters)
            {
                this.PaginationParameters = new PaginationParameters();
            }
            action(this.PaginationParameters, parsedValue);
        }

        private static IReadOnlyCollection<string> ParseAttributes(string attributeExpression)
        {
            if (string.IsNullOrWhiteSpace(attributeExpression))
            {
                throw new ArgumentNullException(nameof(attributeExpression));
            }

            IReadOnlyCollection<string> results =
                attributeExpression
                .Split(ResourceQuery.SeperatorsAttributes.Value)
                .Select(
                    (string item) =>
                        item.Trim())
                .ToArray();
            return results;
        }

        private static IReadOnlyCollection<IFilter> ParseFilters(string filterExpression)
        {
            if (string.IsNullOrWhiteSpace(filterExpression))
            {
                throw new ArgumentNullException(nameof(filterExpression));
            }

            if (!Filter.TryParse(filterExpression, out IReadOnlyCollection<IFilter> results))
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }

            return results;
        }
    }
}
