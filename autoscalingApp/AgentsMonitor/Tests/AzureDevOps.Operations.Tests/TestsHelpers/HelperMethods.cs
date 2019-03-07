using System.Collections.Generic;
using AzureDevOps.Operations.Models;
using AzureDevOps.Operations.Tests.Classes;
using AzureDevOps.Operations.Tests.Data;
using Microsoft.Azure.Management.Compute.Fluent;

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

        /// <summary>
        /// Generates stripped VMSS list to work with; allows to generate to amount which is needed and add custom data to the collection
        /// </summary>
        /// <param name="testListSize"></param>
        /// <param name="addedData"></param>
        /// <returns></returns>
        internal static List<ScaleSetVirtualMachineStripped> GetTestData(int testListSize, ScaleSetVirtualMachineStripped[] addedData = null)
        {
            var vmScaleSetData = new List<ScaleSetVirtualMachineStripped>();
            if (addedData != null)
            {
                vmScaleSetData.AddRange(addedData);
            }

            for (var counter = 0; counter < testListSize; counter++)
            {
                vmScaleSetData.Add(new ScaleSetVirtualMachineStripped
                {
                    VmName = $"vm{counter}",
                    VmInstanceId = $"{counter}",
                    VmInstanceState = PowerState.Running
                });
            }

            return vmScaleSetData;
        }
    }
}