# How to

Shall be executed on your own hosted agent at pool, specified in `vstsPoolName` var, as it is lengthy process.

1. Create New build pipeline based on YAML configuration; point it for yaml at [/builds/build.yaml](../builds/build.yaml)

1. At variables, create following variables to be used by this build ([variables description](../README.md#buildps1-parameters)): `abortPackerOnError`, `EnforceAzureRm`, `InstallPrerequisites` (those variables are exclusive for a build)

1. Create shared variable group (I call it `Agent Image data`) and add there following vars: `AzureConnectionName` (here I store connection name for Azure, which is set up at `Service Connections` of my Azure DevOps project), `ClientId`, `ClientSecret`, `Location`, `ManagedImageName`, `ManagedImageResourceGroupName`, `ObjectId`, `Packerfile`, `SubscriptionId`, `TenantId`, `VMName`, `VMUser`, `VMUserPassword`, `VSTSToken`, `VSTSUrl`, `pipRg` (I put there value `$(ManagedImageResourceGroupName)` to ensure that it is not recreated on each deployment), `resourcesBaseName`, `vmssCapacity`, `vmssSkuName`, `vstsPoolName`, `deployToExistingVnet`, `subnetName`, `vnetName`, `vnetResourceGroupName`. Part of those variable are used by release as well.

## You are good to go :)