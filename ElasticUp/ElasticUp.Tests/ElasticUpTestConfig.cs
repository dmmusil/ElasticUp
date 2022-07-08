using System;
using ElasticUp.Tests.Infrastructure;
using NUnit.Framework;

namespace ElasticUp.Tests
{
    [SetUpFixture]
    public class ElasticUpTestConfig
    {
        private ElasticSearchContainer _elasticSearchContainer;

        [OneTimeSetUp]
        public void SetupElasticSearchInstance()
        {
            _elasticSearchContainer = StartAndWaitForElasticSearchService();
        }

        [OneTimeTearDown]
        public void TeardownElasticSearchInstance()
        {
            _elasticSearchContainer.Dispose();
        }
        
        private static ElasticSearchContainer StartAndWaitForElasticSearchService()
        {
            var elasticSearchContainer = ElasticSearchContainer.Start(new Version(2, 4,1));
            elasticSearchContainer.WaitUntilElasticOperational();
            return elasticSearchContainer;
        }
    }
    
}
