//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Net; // For WebUtility

    public sealed class Query : IQuery
    {
        private const string AttributeNameSeparator = ",";

        public IReadOnlyCollection<IFilter> AlternateFilters
        {
            get;
            set;
        }

        public IReadOnlyCollection<string> ExcludedAttributePaths
        {
            get;
            set;
        }

        public IPaginationParameters PaginationParameters
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public IReadOnlyCollection<string> RequestedAttributePaths
        {
            get;
            set;
        }

        public string Compose()
        {
            string result = this.ToString();
            return result;
        }

        private static Filter Clone(IFilter filter, Dictionary<string, string> placeHolders)
        {
            string placeHolder = Guid.NewGuid().ToString();
            placeHolders.Add(placeHolder, filter.ComparisonValueEncoded);
            Filter result = new Filter(filter.AttributePath, filter.FilterOperator, placeHolder);
            if (filter.AdditionalFilter != null)
            {
                result.AdditionalFilter = Query.Clone(filter.AdditionalFilter, placeHolders);
            }
            return result;
        }

        public override string ToString()
        {
            // Build query parameters manually to avoid System.Web dependency
            var parameters = new List<(string Key, string Value)>();

            if (true == this.RequestedAttributePaths?.Any())
            {
                IReadOnlyCollection<string> encodedPaths = this.RequestedAttributePaths.Encode();
                string requestedAttributes =
                    string.Join(Query.AttributeNameSeparator, encodedPaths);
                parameters.Add((QueryKeys.Attributes, requestedAttributes));
            }

            if (true == this.ExcludedAttributePaths?.Any())
            {
                IReadOnlyCollection<string> encodedPaths = this.ExcludedAttributePaths.Encode();
                string excludedAttributes =
                    string.Join(Query.AttributeNameSeparator, encodedPaths);
                parameters.Add((QueryKeys.ExcludedAttributes, excludedAttributes));
            }

            Dictionary<string, string> placeHolders;
            if (true == this.AlternateFilters?.Any())
            {
                placeHolders = new Dictionary<string, string>(this.AlternateFilters.Count);
                IReadOnlyCollection<IFilter> clones =
                    this.AlternateFilters
                    .Select(
                        (IFilter item) =>
                            Query.Clone(item, placeHolders))
                    .ToArray();
                string filters = Filter.ToString(clones); // already returns 'filter=...'
                // Parse the returned query style string minimally (key=value&...)
                foreach (string pair in filters.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    int idx = pair.IndexOf('=');
                    if (idx > 0)
                    {
                        string k = pair.Substring(0, idx);
                        string v = pair.Substring(idx + 1);
                        parameters.Add((k, v));
                    }
                }
            }
            else
            {
                placeHolders = new Dictionary<string, string>();
            }

            if (this.PaginationParameters != null)
            {
                if (this.PaginationParameters.StartIndex.HasValue)
                {
                    string startIndex =
                        this
                        .PaginationParameters
                        .StartIndex
                        .Value
                        .ToString(CultureInfo.InvariantCulture);
                    parameters.Add((QueryKeys.StartIndex, startIndex));
                }

                if (this.PaginationParameters.Count.HasValue)
                {
                    string count =
                        this
                        .PaginationParameters
                        .Count
                        .Value
                        .ToString(CultureInfo.InvariantCulture);
                    parameters.Add((QueryKeys.Count, count));
                }
            }

            // Serialize parameters manually
            var sb = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0) sb.Append('&');
                var (k, v) = parameters[i];
                sb.Append(Uri.EscapeDataString(k));
                sb.Append('=');
                sb.Append(v); // values assumed already encoded where needed earlier
            }
            string result = sb.ToString();
            foreach (KeyValuePair<string, string> placeholder in placeHolders)
            {
                result = result.Replace(placeholder.Key, placeholder.Value, StringComparison.InvariantCulture);
            }
            return result;
        }
    }
}