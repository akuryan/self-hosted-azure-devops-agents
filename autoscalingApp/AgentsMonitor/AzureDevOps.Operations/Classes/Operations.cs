﻿using AzureDevOps.Operations.Helpers;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AzureDevOps.Operations.Models;
using Microsoft.Azure.Management.Compute.Fluent;

namespace AzureDevOps.Operations.Classes
{
    public static class Operations
    {
        /// <summary>
        /// Here we will proceed working with VMSS ((de)provision additional agents, keep current agents count)
        /// </summary>
        /// <param name="onlineAgents"></param>
        /// <param name="maxAgentsInPool"></param>
        /// <param name="dataRetriever">Used to get data from Azure DevOps</param>
        /// <param name="agentsPoolId"></param>
        public static void WorkWithVmss(int onlineAgents, int maxAgentsInPool, Retrieve dataRetriever, int agentsPoolId)
        {
            var credentials = AzureCreds();

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(ConfigurationManager.AppSettings[Constants.AzureSubscriptionIdSettingName]);

            //working with VMSS
            var resourceGroupName = ConfigurationManager.AppSettings[Constants.AzureVmssResourceGroupSettingName];
            var vmssName = ConfigurationManager.AppSettings[Constants.AzureVmssNameSettingName];
            var vmss = azure.VirtualMachineScaleSets.GetByResourceGroup(resourceGroupName, vmssName);
            var virtualMachines = vmss.VirtualMachines.List()
                .Select(vmssVm => new ScaleSetVirtualMachineStripped
                {
                    VmInstanceId = vmssVm.InstanceId, 
                    VmName = vmssVm.ComputerName,
                    VmInstanceState = vmssVm.PowerState
                }).ToList();
            //get jobs again to check, if we could deallocate a VM in VMSS (if it is running a job - it is not wise to deallocate it)
            var currentJobs = dataRetriever.GetRuningJobs(agentsPoolId);
            var addMoreAgents = Decisions.AddMoreAgents(currentJobs.Length, onlineAgents);
            var amountOfAgents = Decisions.HowMuchAgents(currentJobs.Length, onlineAgents, maxAgentsInPool);

            if (amountOfAgents == 0)
            {
                //nevertheless - should we (de)provision agents: we are at boundaries
                Console.WriteLine("Could not add/remove more agents, exiting...");
                Environment.Exit(Constants.SuccessExitCode);
            }

            if (!addMoreAgents)
            {
                //TODO: Record deallocation
                Console.WriteLine("Deallocating VMs");
                //we need to downscale
                var instanceIdCollection = Decisions.CollectInstanceIdsToDeallocate(virtualMachines, currentJobs);

                foreach (var instanceId in instanceIdCollection)
                {
                    Console.WriteLine($"Deallocating VM with instance ID {instanceId}");
                    vmss.VirtualMachines.Inner.BeginDeallocateWithHttpMessagesAsync(resourceGroupName, vmssName,
                        instanceId);
                }
            }
            else
            {
                var vmsCounter = 0;
                Console.WriteLine("Starting more VMs");
                foreach (var scaleSetVirtualMachineStripped in virtualMachines.Where(vm => vm.VmInstanceState.Equals(PowerState.Deallocated)))
                {
                    if (vmsCounter >= amountOfAgents)
                    {
                        break;
                    }
                    //TODO: Record starting VM
                    Console.WriteLine($"Starting VM {scaleSetVirtualMachineStripped.VmName} with id {scaleSetVirtualMachineStripped.VmInstanceId}");
                    vmss.VirtualMachines.Inner.BeginStartWithHttpMessagesAsync(resourceGroupName, vmssName,
                        scaleSetVirtualMachineStripped.VmInstanceId);
                    vmsCounter++;
                }
            }
            Console.WriteLine("Finished execution");
        }

        private static AzureCredentials AzureCreds()
        {
            var clientId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientIdSettingName];
            var clientSecret = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientSecretSettingName];
            var tenantId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleTenantIdSettingName];
            //maybe in future I'll need to extend this one to allow other then Global Azure environment
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }
    }
}