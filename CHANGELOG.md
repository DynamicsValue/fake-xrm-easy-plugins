## [3.1.1]

### Changed

- Limit FakeItEasy package dependency to v6.x versions - DynamicsValue/fake-xrm-easy#37
- Updated build script to also include the major version in the Title property of the generated .nuspec file - DynamicsValue/fake-xrm-easy#41

## [3.1.0]

### Added

- New UsePluginStepRegistrationValidation that can be used to validate if a plugin step registration is valid - DynamicsValue/fake-xrm-easy#19 and DynamicsValue/fake-xrm-easy#33
- Improved performance of retrieval of Plugin steps, added benchmark tests DynamicsValue/fake-xrm-easy#14
- PluginStepAudit and new UsePluginStepAudit in PipelineOptions - DynamicsValue/fake-xrm-easy#19
- PreValidation execution in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
- PreImages and Postimages execution in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
- Triggering plugins in PipelineSimulation based on filtering attributes - DynamicsValue/fake-xrm-easy#19
- Support for ILogger and PluginTelemetry - DynamicsValue/fake-xrm-easy#24

### Changed

- Marked several execute plugin methods as obsolete
- Populate OwningExtension in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#17
- Populate PrimaryEntityId in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#8
- Fixed issue in push.ps1 script where it was just pushing the first package, using now a glob pattern

## [3.0.2]

### Changed 

- Bump Dataverse dependency to 0.6.1 from 0.5.10 to solve DynamicsValue/fake-xrm-easy#20
- Also replaced Microsoft.Dynamics.Sdk.Messages dependency, as it has also been deprecated by MSFT, to Microsoft.PowerPlatform.Dataverse.Client.Dynamics 0.6.1 DynamicsValue/fake-xrm-easy#20

## [3.0.1-rc1] - Initial release
