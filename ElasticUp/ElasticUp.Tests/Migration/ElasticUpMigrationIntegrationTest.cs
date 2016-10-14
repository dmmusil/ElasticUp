﻿using System;
using ElasticUp.Migration;
using ElasticUp.Migration.Meta;
using ElasticUp.Operation;
using ElasticUp.Tests.Operation;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ElasticUp.Tests.Migration
{
    [TestFixture]
    public class ElasticUpMigrationIntegrationTest : AbstractIntegrationTest
    {
        private ElasticUpMigration _elasticUpMigration;

        public ElasticUpMigrationIntegrationTest()
            : base(ElasticServiceStartup.StartupForEach)
        {
            
        }
        
        [SetUp]
        public void Setup()
        {
            _elasticUpMigration = new TestMigration(0);
            _elasticUpMigration.OnIndexAlias("test");
        }

        [Test]
        public void Operation_AddsOperationToOperations()
        {
            // GIVEN
            var operation = new TestOperation(0);

            // WHEN
            _elasticUpMigration.Operation(operation);

            // THEN
            _elasticUpMigration.Operations.Should().HaveCount(1);
        }

        [Test]
        public void Operation_ThrowsWhenAddingOperationWithSameIndex()
        {
            // GIVEN
            var operation1 = new TestOperation(0);
            var operation2 = new TestOperation(0);

            // WHEN / THEN
            _elasticUpMigration.Operation(operation1);
            Assert.Throws<ArgumentException>(() => _elasticUpMigration.Operation(operation2), "Duplicate operation number.");
        }

        [Ignore("No alias available for type 'test'")]
        [Test]
        public void Execute_ExecutesEachOperation()
        {
            // GIVEN
            var operation1 = Substitute.For<ElasticUpOperation>(0);
            var operation2 = Substitute.For<ElasticUpOperation>(1);

            _elasticUpMigration.Operation(operation1);
            _elasticUpMigration.Operation(operation2);

            var index0 = new VersionedIndexName("test", 0);
            var index1 = index0.GetIncrementedVersion();

            // WHEN
            _elasticUpMigration.Execute(ElasticClient, index0, index1);

            // THEN
            operation1.Received().Execute(ElasticClient, index0.Name, index1.Name);
            operation2.Received().Execute(ElasticClient, index0.Name, index1.Name);
        }

    [Test]
        public void ToString_ReturnsMigrationNumberPlusClassName()
        {
            new TestMigration(5).ToString().Should().Be("005_TestMigration");
            new TestMigration(14).ToString().Should().Be("014_TestMigration");
        }
    }
}
