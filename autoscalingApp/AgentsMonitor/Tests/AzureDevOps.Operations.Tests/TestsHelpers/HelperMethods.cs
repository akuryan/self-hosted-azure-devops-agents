using AzureDevOps.Operations.Models;
using AzureDevOps.Operations.Tests.Classes;
using AzureDevOps.Operations.Tests.Data;

namespace AzureDevOps.Operations.Tests.TestsHelpers
{
    /// <summary>
    /// This class contains different helper methods for tests
    /// </summary>
    public static class HelperMethods
    {
        public static JobRequest[] GetSimulatedJobRequests(int poolId = TestsConstants.TestPoolId,
            string jsonData = TestsConstants.Json1JobIsRunning)
        {
            var dataRetriever = RetrieveTests.CreateRetriever(jsonData);
            return dataRetriever.GetRunningJobs(poolId);
        }
    }
}