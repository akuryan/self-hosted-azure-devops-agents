using AzureDevOps.Operations.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

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
        internal static IEnumerable<ScaleSetVirtualMachineStripped> GetVmsForAllocation(JobRequest[] runningJobs, ScaleSetVirtualMachineStripped[] virtualMachines, int agentsToAllocateCount)
        {
            var vmsToStart = new List<ScaleSetVirtualMachineStripped>();
            const string agentNameMarker = "Agent.Name -equals ";

            foreach (var job in runningJobs)
            {
                //check, if any of our jobs wants specific agent
                if (job.Demands == null)
                {
                    continue;
                }
                var agentNameIndex =
                    Array.FindIndex(job.Demands, x => x.ToLower().StartsWith(agentNameMarker.ToLower()));
                if (agentNameIndex < 0)
                {
                    continue;
                }
                var agentName = job.Demands[agentNameIndex].Replace(agentNameMarker, string.Empty);

                if (string.IsNullOrWhiteSpace(agentName))
                {
                    continue;
                }
                vmsToStart.Add(virtualMachines.SingleOrDefault(vm =>
                    vm.VmName.Equals(agentName, StringComparison.OrdinalIgnoreCase)));
                agentsToAllocateCount = agentsToAllocateCount - 1;
            }
            //we do not need to start extra VMs, if there is some of them starting already
            agentsToAllocateCount = agentsToAllocateCount -
                             virtualMachines
                                 .Count(vm => vm.VmInstanceState.Equals(PowerState.Starting));
            agentsToAllocateCount = agentsToAllocateCount < 0 ? 0 : agentsToAllocateCount;

            //out of deallocated VMs - select needed amount of agents
            vmsToStart.AddRange(virtualMachines
                .Where(vm => vm.VmInstanceState.Equals(PowerState.Deallocated))
                .Take(agentsToAllocateCount).ToList());

            return vmsToStart;
        }
    }
}