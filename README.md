# Self hosted Azure DevOps agents
Scripts to build and deploy VMs to be used as hosted agents for VSTS, based on great work of Wouter de Kort - see [this repository](https://github.com/WouterDeKort/VSTSHostedAgentPool) and his [blog posts series](https://wouterdekort.com/2018/02/25/build-your-own-hosted-vsts-agent-cloud-part-1-build/).

Since it took big amount of efforts to put the repository to this state - I decided to create own public repository, without maintaining a fork relations with original repository.

## Setting up

1. Create your own packer image description. I am using [own fork](https://github.com/akuryan/vsts-image-generation) of https://github.com/Microsoft/azure-pipelines-image-generation as a submodule, as I do not need all the features Microsoft adds to their's build agents.

1. Build image locally (in future, you will be able to do it via agent, but, if you do not have private agents – msft one’s would not allow you to run 4-7 hours long job, as far as I know). It is done via [Build.ps1](https://github.com/akuryan/self-hosted-azure-devops-agents/blob/master/Build.ps1) with parameters (for automating it as a Build pipeline at Azure DevOps – there is a https://github.com/akuryan/self-hosted-azure-devops-agents/blob/master/builds/build.yaml - in fact, just one step). See further for parameters. After you have an own hosted agent running - you could create a build on it to [refresh an image](./docs/image-Refresh-Build.md). 

1. After image is built – you can deploy your new agents via https://github.com/akuryan/self-hosted-azure-devops-agents/blob/master/Release.ps1 - this you could do already at Microsoft agents. See further for parameters and see [description of release pipeline](./docs/deploy-Agent.md)

When this is done – you can build and deploy autoscaling application https://github.com/akuryan/self-hosted-azure-devops-agents/tree/master/autoscalingApp (there is an arm template and little bit of description at my blog https://dobryak.org/self-hosted-agents-at-azure-devops-a-little-cost-saving-trick/ ).

While you are working on Autoscaling app – you can use https://github.com/akuryan/self-hosted-azure-devops-agents/blob/master/Manage.ps1 to be executed on schedule to save little bit on costs.

### Build.ps1 parameters

```Location``` - in which datacenter image for VMSS shall be built

```PackerFile``` - packer file path to use

```ClientId``` - Client ID for your Azure Service Principle

```ClientSecret``` - Client Secret for your Azure Service Principle

```TenantId``` - Tenant ID for your Azure Service Principle

```SubscriptionId``` - Subscription ID

```ObjectId``` - Object ID for your Azure Service Principle

```ManagedImageResourceGroupName``` - resource group, where image will be stored

```ManagedImageName``` - Image name prefix; it will be postfixed with build number.

```InstallPrerequisites``` - switch, should script install packer and git on environment, where it is executed

```EnforceAzureRm``` - switch, should script install latest AzureRM module

```abortPackerOnError``` - switch, specifies, if packer resources should be kept online if there was an error during packer build.

### Release.ps1 parameters

```VMUser``` - username to access VM via RDP or any other allowed mean of connection; during Azure DevOps build could be specified just a variable.

```VMUserPassword``` - password for ```VMUser```; during Azure DevOps build could be specified just a variable.

```VMName``` - virtual machines prefix name; could not be longer than 9 symbols

```ManagedImageResourceGroupName``` - resource group name, where image will be stored.

```ManagedImageName``` - name for the image

```Location``` - Azure datacenter location for an image, defaults to West Europe

```resourcesBaseName``` - base name for resources; other resource names would be constructed by adding postfixes to this base name

```VSTSToken``` - your Azure DevOps personal access token [see this](https://docs.microsoft.com/en-gb/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops)

```VSTSUrl``` - your Azure DevOps url

```pipRg``` - resource group name for Public IP address. By default, resource group, which hosts VM Scale Set, will be destroyed, during redeployment of VMSS (Virtual Machines Scale Set). So, if you want to keep Public IP address to yourself - put it in separate resource group.

```vmssCapacity``` - amount of Virtual Machines in VMSS

```vmssSkuName``` - VMSS SKU Name; default value set to "Standard_D4s_v3"

```vstsPoolName``` - pool name to add agents too in Azure DevOps; default value set to "Default"

```vstsAgentPackageUri``` - URL to download Azure DevOps agents package for deployment; it is auto-updating, so shall not be the latest one here; have default value specified

```vmssDiskStorageAccount``` - disk accounts to be used by VMs in VMSS; defaults to "StandardSSD_LRS", which means that it is Standard SSD drive (IMHO, good balance between cost and speed)

```attachDataDisk``` - specifies, if we should provision a data disk; defaults to ```false```, as current image will be built with 256 GiB, which is enough.

```vmssDataDiskSize``` - if ```attachDataDisk``` is set to ```true```, then this parameter specifies size in GiB for data disk to be attached (one pays for size); also, on this disk work folder of agent will be installed as well.

```attachNsg``` - specifies, if Network Security Group (NSG) shall be attached to VMSS

```allowedIps``` - Provide an address range using CIDR notation (e.g. 192.168.99.0/24); an IP address (e.g. 192.168.99.0); or a list of address ranges or IP addresses (e.g. 192.168.99.0/24,10.0.0.0/24,44.66.0.0/24)

```allowedPorts``` - Provide a single port, such as 80; a port range, such as 1024-65535; or a comma-separated list of single ports and/or port ranges, such as 80,1024-65535. This specifies on which ports traffic will be allowed or denied by this rule. Provide an asterisk (*) to allow traffic on any port.

```deployToExistingVnet``` - defines, if we shall deploy to existing VNet or to provision new VNet

```subnetName``` - if ```deployToExistingVnet``` is set to ```true```, then here valid and existing subnet name shall be provided

```vnetName``` - if ```deployToExistingVnet``` is set to ```true```, then here valid and existing vnet name shall be provided

```vnetResourceGroupName``` - if ```deployToExistingVnet``` is set to ```true```, then here valid and existing resource group name, which holds vnet shall be provided.
