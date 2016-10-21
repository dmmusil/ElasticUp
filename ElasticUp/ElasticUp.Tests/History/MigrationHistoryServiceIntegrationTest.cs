﻿using System;
using System.Collections.Generic;
using System.Linq;
using ElasticUp.History;
using ElasticUp.Migration;
using ElasticUp.Operation;
using ElasticUp.Tests.Sample;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace ElasticUp.Tests.History
{
    [TestFixture]
    public class MigrationHistoryServiceIntegrationTest : AbstractIntegrationTest
    {
        [Test]
        public void CopyMigrationHistory_CopiesExistingMigrationHistoryFromOneIndexToAnother()
        {
            // GIVEN
            const string fromIndex = "from";
            const string toIndex = "to";

            var migrationHistory = Enumerable.Range(1, 10)
                .Select(n => new MigrationHistory
                {
                    Id = $"SampleMigration-{n}",
                    Applied = DateTime.UtcNow
                }).ToList();

            ElasticClient.IndexMany(migrationHistory, fromIndex);
            ElasticClient.Refresh(Indices.All);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            migrationHistoryService.CopyMigrationHistory(fromIndex, toIndex);

            // VERIFY
            ElasticClient.Refresh(Indices.All);
            var actualMigrationHistory = ElasticClient.Search<MigrationHistory>(descriptor => descriptor.Index(toIndex));
            actualMigrationHistory.Documents.ShouldBeEquivalentTo(migrationHistory);
        }

        [Test]
        public void CopyMigrationHistory_DoesNotThrowWhenNoMigrationHistoryInFromIndex()
        {
            // GIVEN
            const string fromIndex = "from";
            const string toIndex = "to";
            
            ElasticClient.CreateIndex(fromIndex);
            ElasticClient.Refresh(Indices.All);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            migrationHistoryService.CopyMigrationHistory(fromIndex, toIndex);

            // VERIFY
            ElasticClient.Refresh(Indices.All);
            var actualMigrationHistory = ElasticClient.Count<MigrationHistory>(descriptor => descriptor.Index(toIndex));
            actualMigrationHistory.Count.Should().Be(0);
        }

        [Test]
        public void AddMigrationHistory_AddsMigrationHistoryForExecutedOperation()
        {
            // GIVEN
            const string toIndex = "to";

            var migration = new SampleEmptyMigration(0);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            migrationHistoryService.AddMigrationToHistory(migration, toIndex);

            // VERIFY
            ElasticClient.Refresh(Indices.All);
            var actualMigrationResponse = ElasticClient.Get<MigrationHistory>(migration.ToString(), descriptor => descriptor.Index(toIndex));
            actualMigrationResponse.Found.Should().BeTrue();
        }

        [Test]
        public void AddMigrationHistory_WithException_AddsMigrationHistoryWithExceptionForExecutedOperation()
        {
            // GIVEN
            const string toIndex = "to";

            var migration = new SampleEmptyMigration(0);
            var exception = new Exception("Sample");

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            migrationHistoryService.AddMigrationToHistory(migration, toIndex, exception);

            // VERIFY
            ElasticClient.Refresh(Indices.All);
            var actualMigration = ElasticClient.Get<MigrationHistory>(migration.ToString(), descriptor => descriptor.Index(toIndex));
            actualMigration.Found.Should().BeTrue();
            actualMigration.Source.Exception.Message.Should().Be(exception.Message);
        }

        [Test]
        public void HasMigrationAlreadyBeenApplied_ReturnsTrueIfMigrationAlreadyApplied()
        {
            // GIVEN
            const string toIndex = "to";

            var migration = new SampleEmptyMigration(0);
            var migrationHistory = new MigrationHistory(migration);

            ElasticClient.Index(migrationHistory, descriptor => descriptor.Index(toIndex));
            ElasticClient.Refresh(Indices.All);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            var hasMigrationAlreadyBeenApplied = migrationHistoryService.HasMigrationAlreadyBeenApplied(migration, toIndex);

            // VERIFY
            hasMigrationAlreadyBeenApplied.Should().BeTrue();
        }

        [Test]
        public void HasMigrationAlreadyBeenApplied_ReturnsFalseIfMigrationNotApplied()
        {
            // GIVEN
            const string toIndex = "to";

            var migration = new SampleEmptyMigration(0);

            ElasticClient.CreateIndex(toIndex);
            ElasticClient.Refresh(Indices.All);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            var hasMigrationAlreadyBeenApplied = migrationHistoryService.HasMigrationAlreadyBeenApplied(migration, toIndex);

            // VERIFY
            hasMigrationAlreadyBeenApplied.Should().BeFalse();
        }

        [Test]
        public void HasMigrationAlreadyBeenApplied_ReturnsFalseIfMigrationNotSucessfullyApplied()
        {
            // GIVEN
            const string toIndex = "to";

            var migration = new SampleEmptyMigration(0);
            var migrationHistory = new MigrationHistory(migration, new Exception());

            ElasticClient.Index(migrationHistory, descriptor => descriptor.Index(toIndex));
            ElasticClient.Refresh(Indices.All);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            var hasMigrationAlreadyBeenApplied = migrationHistoryService.HasMigrationAlreadyBeenApplied(migration, toIndex);

            // VERIFY
            hasMigrationAlreadyBeenApplied.Should().BeFalse();
        }

        [Test]
        public void HasMigrationAlreadyBeenApplied_ThrowsIfIndexDoesNotExist()
        {
            // GIVEN
            var migration = new SampleEmptyMigration(0);

            // TEST
            var migrationHistoryService = new MigrationHistoryService(ElasticClient);
            Assert.Throws<Exception>(() => migrationHistoryService.HasMigrationAlreadyBeenApplied(migration, "unknown-index"));
        }
    }
}