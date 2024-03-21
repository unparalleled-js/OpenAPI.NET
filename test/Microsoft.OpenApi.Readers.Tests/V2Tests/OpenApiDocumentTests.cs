﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Json.Schema;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.V2Tests
{
    public class OpenApiDocumentTests
    {
        private const string SampleFolderPath = "V2Tests/Samples/";

        [Fact]
        public void ShouldParseProducesInAnyOrder()
        {
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "twoResponses.json"));
            var reader = new OpenApiStreamReader();
            var doc = reader.Read(stream, out var diagnostic);

            var okSchema = new JsonSchemaBuilder()
                    .Properties(("id", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("Item identifier.")));

            var errorSchema = new JsonSchemaBuilder()
                    .Ref("#/definitions/Error")
                    .Properties(("code", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Format("int32")),
                    ("message", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                    ("fields", new JsonSchemaBuilder().Type(SchemaValueType.String)));

            var okMediaType = new OpenApiMediaType
            {
                Schema = new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Ref("#/components/schemas/okSchema"))
            };

            var errorMediaType = new OpenApiMediaType
            {
                Schema = new JsonSchemaBuilder().Ref("#/components/schemas/errorSchema")
            };

            doc.Should().BeEquivalentTo(new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Two responses",
                    Version = "1.0.0"
                },
                Servers =
                    {
                        new OpenApiServer
                        {
                            Url = "https://"
                        }
                    },
                Paths = new OpenApiPaths
                {
                    ["/items"] = new OpenApiPathItem
                    {
                        Operations =
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    Responses =
                                    {
                                        ["200"] = new OpenApiResponse
                                        {
                                            Description = "An OK response",
                                            Content =
                                            {
                                                ["application/json"] = okMediaType,
                                                ["application/xml"] = okMediaType,
                                            }
                                        },
                                        ["default"] = new OpenApiResponse
                                        {
                                            Description = "An error response",
                                            Content =
                                            {
                                                ["application/json"] = errorMediaType,
                                                ["application/xml"] = errorMediaType
                                            }
                                        }
                                    }
                                },
                                [OperationType.Post] = new OpenApiOperation
                                {
                                    Responses =
                                    {
                                        ["200"] = new OpenApiResponse
                                        {
                                            Description = "An OK response",
                                            Content =
                                            {
                                                ["html/text"] = okMediaType
                                            }
                                        },
                                        ["default"] = new OpenApiResponse
                                        {
                                            Description = "An error response",
                                            Content =
                                            {
                                                ["html/text"] = errorMediaType
                                            }
                                        }
                                    }
                                },
                                [OperationType.Patch] = new OpenApiOperation
                                {
                                    Responses =
                                    {
                                        ["200"] = new OpenApiResponse
                                        {
                                            Description = "An OK response",
                                            Content =
                                            {
                                                ["application/json"] = okMediaType,
                                                ["application/xml"] = okMediaType,
                                            }
                                        },
                                        ["default"] = new OpenApiResponse
                                        {
                                            Description = "An error response",
                                            Content =
                                            {
                                                ["application/json"] = errorMediaType,
                                                ["application/xml"] = errorMediaType
                                            }
                                        }
                                    }
                                }
                            }
                    }
                },
                Components = new OpenApiComponents
                {
                    Schemas =
                        {
                            ["Item"] = okSchema,
                            ["Error"] = errorSchema
                        }
                }
            });
        }


        [Fact]
        public void ShouldAssignSchemaToAllResponses()
        {
            OpenApiDocument document;
            OpenApiDiagnostic diagnostic;
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "multipleProduces.json")))
            {
                document = new OpenApiStreamReader().Read(stream, out diagnostic);
            }

            Assert.Equal(OpenApiSpecVersion.OpenApi2_0, diagnostic.SpecificationVersion);

            var successSchema = new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(new JsonSchemaBuilder()
                    .Ref("#/definitions/Item")
                    .Properties(("id", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("Item identifier."))))
                .Build();

            var errorSchema = new JsonSchemaBuilder()
                    .Ref("#/definitions/Error")
                    .Properties(("code", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Format("int32")),
                        ("message", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("fields", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .Build();

            var responses = document.Paths["/items"].Operations[OperationType.Get].Responses;
            foreach (var response in responses)
            {
                var targetSchema = response.Key == "200" ? successSchema : errorSchema;

                var json = response.Value.Content["application/json"];
                Assert.NotNull(json);
                json.Schema.Should().BeEquivalentTo(targetSchema);

                var xml = response.Value.Content["application/xml"];
                Assert.NotNull(xml);
                xml.Schema.Should().BeEquivalentTo(targetSchema);
            }
        }

        [Fact]
        public void ShouldAllowComponentsThatJustContainAReference()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "ComponentRootReference.json"));
            OpenApiStreamReader reader = new OpenApiStreamReader();

            // Act
            OpenApiDocument doc = reader.Read(stream, out OpenApiDiagnostic diags);
            JsonSchema schema = doc.Components.Schemas["AllPets"];

            schema = doc.ResolveJsonSchemaReference(schema.GetRef()) ?? schema;

            // Assert
            if (schema.Keywords.Count.Equals(1) && schema.GetRef() != null)
            {
                // detected a cycle - this code gets triggered
                Assert.Fail("A cycle should not be detected");
            }
        }

        [Fact]
        public void ParseDocumentWithDefaultContentTypeSettingShouldSucceed()
        {
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "docWithEmptyProduces.yaml"));
            var doc = new OpenApiStreamReader(new() { DefaultContentType =  new() { "application/json" } })
                .Read(stream, out var diags);
            var mediaType = doc.Paths["/example"].Operations[OperationType.Get].Responses["200"].Content;
            Assert.Contains("application/json", mediaType);
        }
    }
}
