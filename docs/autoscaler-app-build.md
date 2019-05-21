# How to

This build is not using modern yaml pipeline. Eventually, I will code it down to yaml. Also, there is a reason why I do not code it down to yaml - we are using SonarQube to perform static analysis.

So, here is steps:

1. `Nuget` Restore; path to solution is stored at var `$(slnFilePath)`; packages are residing on Nuget.org

1. `Visual Studio Build` to build solution at `$(slnFilePath)` with platform set to `$(BuildPlatform)` and Configuration set to `$(BuildConfiguration)`

1. `Visual Studio Test` with default settings

1. `Copy Files` from `$(base.App.Folder.Name)\WebJob` with content `**` to `$(Build.ArtifactStagingDirectory)\WebJob\$(webJob.autoscaler.path)`

1. `Archive Files` from `$(Build.ArtifactStagingDirectory)\WebJob` to zip `$(Build.ArtifactStagingDirectory)/autoScaler.zip`

1. `Publish Build Artifacts` from `$(Build.ArtifactStagingDirectory)/autoScaler.zip` with name `AutoScalerWebJob`