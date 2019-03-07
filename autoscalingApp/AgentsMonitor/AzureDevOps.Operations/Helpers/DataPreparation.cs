using AzureDevOps.Operations.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using AzureDevOps.Operations.Classes;

namespace AzureDevOps.Operations.Helpers
{
    public static class DataPreparation
    {
        /// <summary>
        /// Collects Virtual Machines to be started in Virtual Machines Scale Set
        /// </summary>
        /// <param name="runningJobs">Currently running jobs</param>
        /// <param name="virtualMachines">Stripped data about VM in VMSS</param>
        /// <param name="agentsToAllocateCount">Amount of agents to allocate</param>
        /// <returns></returns>
        public static IEnumerable<ScaleSetVirtualMachineStripped> GetVmsForAllocation(JobRequest[] runningJobs, IEnumerable<ScaleSetVirtualMachineStripped> virtualMachines, int agentsToAllocateCount)
        {
            var virtualMachinesCollectionEnumerated = virtualMachines.ToList();
            var vmsToStart = virtualMachinesCollectionEnumerated
                .Where(vm => CollectDemandedAgentNames(runningJobs).Contains(vm.VmName)).ToList();

            //remove already added values
            foreach (var virtualMachine in vmsToStart)
            {
                virtualMachinesCollectionEnumerated.Remove(virtualMachine);
            }
            
            agentsToAllocateCount = agentsToAllocateCount - vmsToStart.Count;
            //we do not need to start extra VMs, if there is some of them starting already
            agentsToAllocateCount = agentsToAllocateCount -
                             virtualMachinesCollectionEnumerated
                                 .Count(vm => vm.VmInstanceState.Equals(PowerState.Starting));
            agentsToAllocateCount = agentsToAllocateCount < 0 ? 0 : agentsToAllocateCount;

            //out of deallocated VMs - select needed amount of agents
            vmsToStart.AddRange(virtualMachinesCollectionEnumerated
                .Where(vm => vm.VmInstanceState.Equals(PowerState.Deallocated))
                .Take(agentsToAllocateCount).ToList());

            return vmsToStart;
        }

        /// <summary>
        /// Collects all agent names, which are demanded <see cref="JobRequest.Demands"/> by scheduled jobs
        /// </summary>
        /// <param name="scheduledJobs"></param>
        /// <returns></returns>
        public static string[] CollectDemandedAgentNames(JobRequest[] scheduledJobs)
        {
            return (from job in scheduledJobs 
                where job.Demands != null 
                let agentNameIndex = Array.FindIndex(job.Demands, x => x.ToLower().StartsWith(Constants.AgentNameMarker.ToLower())) 
                where agentNameIndex >= 0 
                select job.Demands[agentNameIndex].Replace(Constants.AgentNameMarker, string.Empty) 
                into agentName 
                where !string.IsNullOrWhiteSpace(agentName) 
                select agentName)
                .ToArray();
        }
    }
}