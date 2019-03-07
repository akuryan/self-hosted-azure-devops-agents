using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Models;
using AzureDevOps.Operations.Tests.Data;
using AzureDevOps.Operations.Tests.TestsHelpers;
using Microsoft.Azure.Management.Compute.Fluent;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_0_jobs_1_demands, 0)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_0_jobs_NO_demands, 0)]
        public static void TestRetrievalOfDemandedAgentsNames(int agentPoolId, string jsonData, int expectedCountOfDemandCollection)
        {
            var scheduledJobs = HelperMethods.GetSimulatedJobRequests(agentPoolId, jsonData);
            Assert.AreEqual(expectedCountOfDemandCollection, DataPreparation.CollectDemandedAgentNames(scheduledJobs).Length);
        }

        /// <summary>
        /// Tests Virtual Machines allocation
        /// </summary>
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json1JobIsRunning, 1, 1)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json3JobIsRunning, 3, 3)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_3_jobs_2_demands, 3, 3)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_0_jobs_1_demands, 0, 0)]
        [TestCase(TestsConstants.TestPoolId, TestsConstants.Json_0_jobs_NO_demands, 0, 0)]
        public static void TestVirtualMachinesAllocationMethod(int agentPoolId, string jsonData, int amountOfAgentsToAllocate, int expectedAmountOfAgents)
        {
            var vmScaleSetData = GenerateTestData().ToArray();
            var scheduledJobs = HelperMethods.GetSimulatedJobRequests(agentPoolId, jsonData);
            var vmsToStart =
                DataPreparation.GetVmsForAllocation(scheduledJobs, vmScaleSetData, amountOfAgentsToAllocate);
            Assert.AreEqual(expectedAmountOfAgents, vmsToStart.Count());
        }
        /// <summary>
        /// Tests specific names are present in VMs collection
        /// </summary>
        [Test]
        public static void TestVirtualMachinesSpecificNames()
        {
            var vmScaleSetData = GenerateTestData().ToArray();
            var scheduledJobs = HelperMethods.GetSimulatedJobRequests(TestsConstants.TestPoolId, TestsConstants.Json_3_jobs_2_demands);
            var vmsToStart =
                DataPreparation.GetVmsForAllocation(scheduledJobs, vmScaleSetData, 3).ToArray();
            Assert.AreEqual(3, vmsToStart.Count());
            //ensures that demanded agents are selected for upscaling
            Assert.IsTrue(vmsToStart.Any(vm => vm.VmName.Equals("Agent")));
            Assert.IsTrue(vmsToStart.Any(vm => vm.VmName.Equals("Agent1")));

            //check, that there is different objects in the end 
            Assert.AreEqual(vmsToStart.Count(), vmsToStart.Distinct().Count());
        }

        private static IEnumerable<ScaleSetVirtualMachineStripped> GenerateTestData()
        {
            var testArray = new ScaleSetVirtualMachineStripped[3];
            var testValid = new ScaleSetVirtualMachineStripped
            {
                VmName = "Agent",
                VmInstanceId = "205",
                VmInstanceState = PowerState.Deallocated
            };

            testArray[0] = testValid;
            testValid = new ScaleSetVirtualMachineStripped
            {
                VmName = "Agent1",
                VmInstanceId = "2052",
                VmInstanceState = PowerState.Deallocated
            };

            testArray[1] = testValid;
            testValid = new ScaleSetVirtualMachineStripped
            {
                VmName = "Agent2",
                VmInstanceId = "20522",
                VmInstanceState = PowerState.Deallocated
            };

            testArray[2] = testValid;
            return HelperMethods.GetTestData(10, testArray);
        }
    }
}