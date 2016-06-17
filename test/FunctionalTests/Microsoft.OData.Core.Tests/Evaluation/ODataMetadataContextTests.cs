﻿//---------------------------------------------------------------------
// <copyright file="ODataMetadataContextTests.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using FluentAssertions;
using Microsoft.OData.Evaluation;
using Microsoft.OData.Metadata;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Xunit;

namespace Microsoft.OData.Tests.Evaluation
{
    public class ODataMetadataContextTests
    {
        private IEdmModel edmModel;

        public ODataMetadataContextTests()
        {
            this.edmModel = TestModel.BuildDefaultTestModel();
        }

        [Fact]
        public void GetEntityMetadataBuilderShouldThrowWhenMetadataDocumentUriIsNull()
        {
            ODataMetadataContext context = new ODataMetadataContext(
                true /*readingResponse*/,
                null,
                new EdmTypeReaderResolver(this.edmModel, null),
                this.edmModel,
                null /*metadataDocumentUri*/,
                null /*requestUri*/);
            IEdmEntitySet set = this.edmModel.EntityContainer.FindEntitySet("Products");
            ODataResource entry = TestUtils.CreateODataEntry(set, new EdmStructuredValue(new EdmEntityTypeReference(set.EntityType(), true), new IEdmPropertyValue[0]), set.EntityType());
            Action action = () => context.GetResourceMetadataBuilderForReader(new TestJsonLightReaderEntryState { Resource = entry, SelectedProperties = SelectedPropertiesNode.EntireSubtree }, null);
            action.ShouldThrow<ODataException>().WithMessage(Strings.ODataJsonLightEntryMetadataContext_MetadataAnnotationMustBeInPayload("odata.context"));
        }

        [Fact]
        public void GetEntityMetadataBuilderShouldNotThrowWhenMetadataDocumentUriIsNonNullAndAbsolute()
        {
            ODataMetadataContext context = new ODataMetadataContext(
                true /*readingResponse*/,
                null,
                new EdmTypeReaderResolver(this.edmModel, null),
                this.edmModel,
                new Uri("http://myservice.svc/$metadata", UriKind.Absolute),
                null /*requestUri*/);
            IEdmEntitySet set = this.edmModel.EntityContainer.FindEntitySet("Products");
            ODataResource entry = TestUtils.CreateODataEntry(set, new EdmStructuredValue(new EdmEntityTypeReference(set.EntityType(), true), new IEdmPropertyValue[0]), set.EntityType());
            Action action = () => context.GetResourceMetadataBuilderForReader(new TestJsonLightReaderEntryState { Resource = entry, SelectedProperties = new SelectedPropertiesNode("*") }, null);
            action.ShouldNotThrow();
        }

        [Fact]
        public void CheckForOperationsRequiringContainerQualificationFallBackToWhetherTypeIsOpen()
        {
            var closedType = new EdmEntityType("NS", "Type1");
            var openType = new EdmEntityType("NS", "Type2", null, false, true);

            ODataMetadataContext context = new ODataMetadataContext(
                true /*readingResponse*/,
                null,
                new EdmTypeReaderResolver(this.edmModel, null),
                this.edmModel,
                null /*metadataDocumentUri*/,
                null /*requestUri*/);
            context.OperationsBoundToStructuredTypeMustBeContainerQualified(closedType).Should().BeFalse();
            context.OperationsBoundToStructuredTypeMustBeContainerQualified(openType).Should().BeTrue();
        }
    }
}