using System;
using System.Threading.Tasks;
using ElasticUp.Tests.Infrastructure;
using NUnit.Framework;

namespace ElasticUp.Tests
{
    [SetUpFixture]
    public class ElasticUpTestConfig
    {
        private ElasticSearchContainer _elasticSearchContainer;

        [OneTimeSetUp]
        public async Task SetupElasticSearchInstance()
        {
            _elasticSearchContainer = await StartAndWaitForElasticSearchService();
        }

        [OneTimeTearDown]
        public void TeardownElasticSearchInstance()
        {
            //_elasticSearchContainer.Dispose();
        }
        
        private static async Task<ElasticSearchContainer> StartAndWaitForElasticSearchService()
        {
            var elasticSearchContainer = ElasticSearchContainer.Start();
            await elasticSearchContainer.WaitUntilElasticOperational();
            return elasticSearchContainer;
        }
    }
    
}
