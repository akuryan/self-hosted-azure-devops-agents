# How to

Shall be executed at Microsoft hosted agents, at least `Hosted VS2017`. Also, it is tied up with autoscaler job as well.

I create it as release pipeline to ease deploynig previous releases, in case current one goes rogue. It have 2 artifacts to be used:

1. Agent image build (build, [described here](.\image-Refresh-Build.md)) - image name retrieved from it.

1. This repository, as it holds `Release.ps1` used for release, `RemoveAgents.ps1` used to remove agents from pool and `Manage.ps1` used to stop VMs in VMSS. Maybe it is wiser to publish it as build artifact as well...

## Job configuration

This release have one job, running at `Hosted VS2017` pool with following steps:

1. `Azure PowerShell` to stop autoscaler web job, which executes following inline script: `Invoke-AzureRmResourceAction -ResourceGroupName $(webjob.rg.name) -ResourceType Microsoft.Web/sites/continuouswebjobs -ResourceName $(webjob.webapp.name)/$(webJob.autoscaler.name) -Action Stop -Force -ApiVersion 2018-02-01`

1. `Azure PowerShell` to stop VMs in VMSS, which executes following script: `$(System.DefaultWorkingDirectory)/repositoryArtifactNameGoesHere/Manage.ps1` with arguments `-resourcesBaseName $(resourcesBaseName) -Action Stop`

1. `Powershell` to cleanup agent pool at Azure DevOps, which executes `$(System.DefaultWorkingDirectory)/repositoryArtifactNameGoesHere/RemoveAgents.ps1` with arguments `-VSTSToken $(VSTSToken) -VSTSUrl $(VSTSUrl) -agentPoolPattern $(VMName)`

1. `Azure PowerShell` to actually create new VMSS from image built previously - execute script `$(System.DefaultWorkingDirectory)/repositoryArtifactNameGoesHere/Release.ps1` with params `-VMUser $(VMUser) -VMUserPassword $(VMUserPassword) -VMName $(VMName) -ManagedImageResourceGroupName $(ManagedImageResourceGroupName) -ManagedImageName $(vmssImageName) -resourcesBaseName $(resourcesBaseName) -VSTSToken $(VSTSToken) -VSTSUrl $(VSTSUrl) -pipRg $(pipRg) -vstsPoolName $(vstsPoolName) -vmssCapacity $(vmssCapacity) -vmssSkuName $(vmssSkuName) -vstsAgentPackageUri $(vstsAgentLink) -vmssDiskStorageAccount $(azureDiskType) -attachDataDisk $(attachDataDiskParam) -allowedIps "$(allowedIps)" -deployToExistingVnet $(deployToExistingVnet) -subnetName "$(subnetName)" -vnetName "$(vnetName)" -vnetResourceGroupName "$(vnetResourceGroupName)"`

1. `Azure PowerShell` to start autoscaler webjob, which launched inline script: `Invoke-AzureRmResourceAction -ResourceGroupName $(webjob.rg.name) -ResourceType Microsoft.Web/sites/continuouswebjobs -ResourceName $(webjob.webapp.name)/$(webJob.autoscaler.name) -Action Start -Force -ApiVersion 2018-02-01`

## Variables configuration

I add following variables at this release (see [..\README.md] for description):

 - `allowedIps`, `attachDataDiskParam`, `azureDiskType`, `vmssImageName` (I set it equal to `$(ManagedImageName)-$(Release.Artifacts.agentImageBuild.BuildNumber)`, where `agentImageBuild` is name of my artifact from agent image build), `vstsAgentLink` 

 I add shared variables `Agent Image data` from [.\image-Refresh-Build.md] and `Azure Web jobs` from [.\autoscaler-app-release.md]

That what's needed to be done to deploy new agents.



