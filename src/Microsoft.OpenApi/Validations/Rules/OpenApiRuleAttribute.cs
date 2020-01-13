// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.OpenApi.Validations.Rules
{
    /// <summary>
    /// The Validator attribute.  This attribute is only needed for discovery of rules that are 
    /// required to ensure compliance with the specification.  Other Validation rules can be optionally added to the RuleSet.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OpenApiRuleAttribute : Attribute
    {
    }
}
