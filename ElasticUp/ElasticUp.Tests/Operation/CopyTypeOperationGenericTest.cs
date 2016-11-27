﻿using ElasticUp.Operation;
using ElasticUp.Tests.Sample;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticUp.Tests.Operation
{
    [TestFixture]
    [Parallelizable]
    public class CopyTypeOperationGenericTest
    {
        [Test]
        public void Generic_UsesNameOfTypeForTypeName()
        {
            // WHEN
            var operation = new ReindexTypeOperation<SampleDocument>(0);
            operation.TypeName.Should().Be("sampledocument");
        }
    }
}