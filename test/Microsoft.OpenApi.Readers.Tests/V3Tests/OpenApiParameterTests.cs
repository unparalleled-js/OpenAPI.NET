﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System;
using System.IO;
using FluentAssertions;
using Json.Schema;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.V3Tests
{
    [Collection("DefaultSettings")]
    public class OpenApiParameterTests
    {
        private const string SampleFolderPath = "V3Tests/Samples/OpenApiParameter/";

        public OpenApiParameterTests() 
        {
            OpenApiReaderRegistry.RegisterReader("yaml", new OpenApiYamlReader());
        }

        [Fact]
        public void ParsePathParameterShouldSucceed()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "pathParameter.yaml"));

            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(stream, OpenApiSpecVersion.OpenApi3_0, "yaml", out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = ParameterLocation.Path,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Schema = new JsonSchemaBuilder().Type(SchemaValueType.String)
                });
        }

        [Fact]
        public void ParseQueryParameterShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "queryParameter.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = "id",
                    Description = "ID of the object to fetch",
                    Required = false,
                    Schema = new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String)),
                    Style = ParameterStyle.Form,
                    Explode = true
                });
        }

        [Fact]
        public void ParseQueryParameterWithObjectTypeShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "queryParameterWithObjectType.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = "freeForm",
                    Schema = new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                    Style = ParameterStyle.Form
                });
        }

        [Fact]
        public void ParseQueryParameterWithObjectTypeAndContentShouldSucceed()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "queryParameterWithObjectTypeAndContent.yaml"));

            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(stream, OpenApiSpecVersion.OpenApi3_0, "yaml", out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = "coordinates",
                    Content =
                    {
                        ["application/json"] = new()
                        {
                            Schema = new JsonSchemaBuilder()
                                .Type(SchemaValueType.Object)
                                .Required("lat", "long")
                                .Properties(
                                    ("lat", new JsonSchemaBuilder()
                                        .Type(SchemaValueType.Number)
                                    ),
                                    ("long", new JsonSchemaBuilder()
                                        .Type(SchemaValueType.Number)
                                    )
                                )
                        }
                    }
                });
        }

        [Fact]
        public void ParseHeaderParameterShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "headerParameter.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = ParameterLocation.Header,
                    Name = "token",
                    Description = "token to be passed as a header",
                    Required = true,
                    Style = ParameterStyle.Simple,

                    Schema = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Array)
                        .Items(new JsonSchemaBuilder()
                            .Type(SchemaValueType.Integer)
                            .Format("int64"))
                });
        }

        [Fact]
        public void ParseParameterWithNullLocationShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "parameterWithNullLocation.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = null,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Schema = new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                });
        }

        [Fact]
        public void ParseParameterWithNoLocationShouldSucceed()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "parameterWithNoLocation.yaml"));

            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(stream, OpenApiSpecVersion.OpenApi3_0, "yaml", out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = null,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Schema = new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                });
        }

        [Fact]
        public void ParseParameterWithUnknownLocationShouldSucceed()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "parameterWithUnknownLocation.yaml"));

            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(stream, OpenApiSpecVersion.OpenApi3_0, "yaml", out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = null,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Schema = new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                });
        }

        [Fact]
        public void ParseParameterWithExampleShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "parameterWithExample.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = null,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Example = new OpenApiAny((float)5.0),
                    Schema = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Number)
                        .Format("float")
                }, options => options.IgnoringCyclicReferences().Excluding(p => p.Example.Node.Parent));
        }

        [Fact]
        public void ParseParameterWithExamplesShouldSucceed()
        {
            // Act
            var parameter = OpenApiModelFactory.Load<OpenApiParameter>(Path.Combine(SampleFolderPath, "parameterWithExamples.yaml"), OpenApiSpecVersion.OpenApi3_0, out _);

            // Assert
            parameter.Should().BeEquivalentTo(
                new OpenApiParameter
                {
                    In = null,
                    Name = "username",
                    Description = "username to fetch",
                    Required = true,
                    Examples =
                    {
                        ["example1"] = new()
                        {
                            Value = new OpenApiAny(5.0)
                        },
                        ["example2"] = new()
                        {
                            Value = new OpenApiAny((float)7.5)
                        }
                    },
                    Schema = new JsonSchemaBuilder()
                                .Type(SchemaValueType.Number)
                                .Format("float")
                }, options => options.IgnoringCyclicReferences()
                .Excluding(p => p.Examples["example1"].Value.Node.Parent)
                .Excluding(p => p.Examples["example2"].Value.Node.Parent));
        }

        [Fact]
        public void ParseParameterWithReferenceWorks()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Swagger Petstore (Simple)"
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer
                    {
                        Url = "http://petstore.swagger.io/api"
                    }
                },
                Paths = new OpenApiPaths
                {
                    ["/pets"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Description = "Returns all pets from the system that the user has access to",
                                OperationId = "findPets",
                                Parameters = new List<OpenApiParameter>
                                {
                                    new() {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Parameter,
                                            Id = "tagsParameter"
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                Components = new OpenApiComponents
                {
                    Parameters = new Dictionary<string, OpenApiParameter>()
                    {
                        ["tagsParameter"] = new OpenApiParameter
                        {
                            Name = "tags",
                            In = ParameterLocation.Query,
                            Description = "tags to filter by",
                            Required = false,
                            Schema = new JsonSchemaBuilder()
                                        .Type(SchemaValueType.Array)
                                        .Items(new JsonSchemaBuilder().Type(SchemaValueType.String)).Build(),
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Parameter,
                                Id = "tagsParameter"
                            }
                        }
                    }
                }
            };

            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "parameterWithRef.yaml"));
            var node = TestHelper.CreateYamlMapNode(stream);

            var expected = document.Components.Parameters["tagsParameter"];

            // Act
            var param = OpenApiV3Deserializer.LoadParameter(node, document);

            // Assert
            param.Should().BeEquivalentTo(expected, options => options.Excluding(p => p.Reference.HostDocument));
        }
    }
}
