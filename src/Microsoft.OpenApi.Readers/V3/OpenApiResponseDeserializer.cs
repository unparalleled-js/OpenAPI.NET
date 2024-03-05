﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Readers.ParseNodes;

namespace Microsoft.OpenApi.Readers.V3
{
    /// <summary>
    /// Class containing logic to deserialize Open API V3 document into
    /// runtime Open API object model.
    /// </summary>
    internal static partial class OpenApiV3Deserializer
    {
        private static readonly FixedFieldMap<OpenApiResponse> _responseFixedFields = new()
        {
            {
                "description",
                (o, n) => o.Description = n.GetScalarValue()
            },
            {
                "headers",
                (o, n) => o.Headers = n.CreateMap(LoadHeader)
            },
            {
                "content",
                 (o, n) => o.Content = n.CreateMap(LoadMediaType)
            },
            {
                "links",
                (o, n) => o.Links = n.CreateMap(LoadLink)
            }
        };

        private static readonly PatternFieldMap<OpenApiResponse> _responsePatternFields =
            new()
            {
                {s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p,n))}
            };

        public static OpenApiResponse LoadResponse(ParseNode node)
        {
            var mapNode = node.CheckMapNode("response");

            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                var refSegments = pointer.Split('/');
                var refId = refSegments.Last();
                var isExternalResource = !refSegments.First().StartsWith("#");

                string externalResource = isExternalResource ? $"{refSegments.First()}/{refSegments[1].TrimEnd('#')}" : null;
                return new OpenApiResponseReference(refId, _openApiDocument, externalResource);
            }

            var response = new OpenApiResponse();
            ParseMap(mapNode, response, _responseFixedFields, _responsePatternFields);

            return response;
        }
    }
}
