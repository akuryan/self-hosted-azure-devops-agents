using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Tests.Data;
using AzureDevOps.Operations.Tests.TestsHelpers;
using NUnit.Framework;

namespace AzureDevOps.Operations.Tests.Helpers
{
    public static class DataPreparationTests
    {
        /// <summary>
        /// Test <see cref="AzureDevOps.Operations.Helpers.DataPreparation.CollectDemandedAgentNames" to get data from scheduled jobs collection/>
        /// </summary>
        /// <param name="agentPoolId"></param>
        /// <param name="jsonData"></param>
        /// <param name="expectedCountOfDemandCollection"></param>
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json1JobIsRunning, 0)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json3JobIsRunning, 0)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_3_jobs_2_demands, 2)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_0_jobs_1_demands, 1)]
        public static void TestRetrievalOfDemandedAgentsNames(int agentPoolId, string jsonData, int expectedCountOfDemandCollection)
        {
            var scheduledJobs = HelperMethods.GetSimulatedJobRequests(agentPoolId, jsonData);
            Assert.AreEqual(expectedCountOfDemandCollection, DataPreparation.CollectDemandedAgentNames(scheduledJobs).Length);
        }
    }
}