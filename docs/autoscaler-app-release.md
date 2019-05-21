# How to

This release gets it's artifacts from [autoscaler app build](./autoscaler-app-build.md) and contains 1 job with 3 steps:

1. `Azure PowerShell` which invokes inline script to stop web job: `Invoke-AzureRmResourceAction -ResourceGroupName $(webjob.rg.name) -ResourceType Microsoft.Web/sites/continuouswebjobs -ResourceName $(webjob.webapp.name)/$(webJob.autoscaler.name) -Action Stop -Force -ApiVersion 2018-02-01`

1. `Azure App Service Deploy` which deploys artifact `$(System.DefaultWorkingDirectory)/_Azure DevOps Webjobs/AutoScalerWebJob/autoScaler.zip` to web app

1. `Azure PowerShell` which invokes inline script to start web job: `Invoke-AzureRmResourceAction -ResourceGroupName $(webjob.rg.name) -ResourceType Microsoft.Web/sites/continuouswebjobs -ResourceName $(webjob.webapp.name)/$(webJob.autoscaler.name) -Action Start -Force -ApiVersion 2018-02-01`

This release have only shared variables defined under name `Azure Web jobs` (they are reused at [agent release](./deploy-Agent.md)) and contains following vars:

 - `webJob.autoscaler.name` - name of web job

 - `webjob.rg.name` - resource group, where web app, hosting web job resides

 - `webjob.webapp.name` - web app name, where web job should be deployed