## [Unreleased]

### Added

Support for PreValidation execution in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
Support for PreImages and Postimages in Pipeline Simulation - DynamicsValue/fake-xrm-easy#19
Support for triggering plugins in PipelineSimulation based on filtering attributes - DynamicsValue/fake-xrm-easy#19
Support for ILogger and PluginTelemetry - DynamicsValue/fake-xrm-easy#24

### Changed

Upgraded Microsoft.CrmSdk.Coreassemblies dependency to 9.0.2.27 to support PluginTelemetry - DynamicsValue/fake-xrm-easy#24
Marked several execute plugin methods as obsolete
Populate OwningExtension in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#17
Populate PrimaryEntityId in plugin execution context from default fake plugin context DynamicsValue/fake-xrm-easy#8

## [2.0.1-rc1] - Initial release