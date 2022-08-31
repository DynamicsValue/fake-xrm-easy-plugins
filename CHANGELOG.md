## [2.2.0]

### Added 

 - Plugin Step registration improvements: new general purpose method to register plugin steps: #53
 - Added possibility to register plugin steps with logical name, and therefore support for late bound registration #38
 
### Changed

- Fix Sonar Quality Gate settings: DynamicsValue/fake-xrm-easy#28

## [2.1.1]

### Changed

- Limit FakeItEasy package dependency to v6.x versions - DynamicsValue/fake-xrm-easy#37
- Made CRM SDK v8.2 dependencies less specific - DynamicsValue/fake-xrm-easy#21
- Updated build script to also include the major version in the Title property of the generated .nuspec file - DynamicsValue/fake-xrm-easy#41

## [2.1.0]

### Added

- New UsePluginStepRegistrationValidation that can be used to validate if a plugin step registration is valid - DynamicsValue/fake-xrm-easy#19 and DynamicsValue/fake-xrm-easy#33
- Improved performance of retrieval of Plugin steps, added benchmark tests DynamicsValue/fake-xrm-easy#14
- PluginStepAudit and new UsePluginStepAudit in PipelineOptions - DynamicsValue/fake-xrm-easy#19
- PreValidation execution in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
- PreImages and Postimages execution in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
- Triggering plugins in PipelineSimulation based on filtering attributes - DynamicsValue/fake-xrm-easy#19
- Support for ILogger and PluginTelemetry - DynamicsValue/fake-xrm-easy#24

### Changed

- Upgraded Microsoft.CrmSdk.Coreassemblies dependency to 9.0.2.27 to support PluginTelemetry - DynamicsValue/fake-xrm-easy#24
- Marked several execute plugin methods as obsolete
- Populate OwningExtension in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#17
- Populate PrimaryEntityId in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#8

## [2.0.1-rc1] - Initial release