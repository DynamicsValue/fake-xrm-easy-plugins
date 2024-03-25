## [2.5.0]

### Changed

- Upgraded dependency "Microsoft.CrmSdk.CoreAssemblies" to 9.0.2.52
- Upgraded dependency "Microsoft.CrmSdk.XrmTooling.CoreAssembly" to 9.1.1.45

## [2.4.1]

### Changed

- Fix for GetServiceProvider should return null for unsupported types - https://github.com/DynamicsValue/fake-xrm-easy/issues/127
 
## [2.4.0]

### Added

- **Alpha**: Introduced subscription usage monitoring based on customer feedback

### Changed

- Set default build configuration in solution file to FAKE_XRM_EASY_9
- build.ps1 improvements: do not build project twice (added --no-build) when running dotnet test, do not build again either when packing assemblies either: https://github.com/DynamicsValue/fake-xrm-easy/issues/119
- Remove release notes from package description: https://github.com/DynamicsValue/fake-xrm-easy/issues/115
- Update build scripts to use 'all' target frameworks by default - https://github.com/DynamicsValue/fake-xrm-easy/issues/126
- Update github actions to use new Sonar environment variables - https://github.com/DynamicsValue/fake-xrm-easy/issues/120

## [2.3.3]

### Added 

- Added [ExcludeFromCodeCoverage] to included generated PipelineType entities in src as it messes up code coverage metrics 
- **[Possibly breaking change]** : Exception raised if both EntityTypeCode and EntityLogicalName are set when registering a plugin step. EntityTypeCode is deprecated, please move all plugin registrations to use EntityLogicalName instead.
- Introduced new user-defined exceptions to verify plugin step registrations when EntityTypeCode is used
- Adding support for secure and unsecure configurations in pipeline simulation - https://github.com/DynamicsValue/fake-xrm-easy/issues/103

### Changed

- Update legacy CRM SDK 2011 dependency to use official MS package - https://github.com/DynamicsValue/fake-xrm-easy/issues/105

## [2.3.2]
 
### Changed

 - Resolves an issue when using early bound types without pipeline types being generated caused plugin step registration to fail - https://github.com/DynamicsValue/fake-xrm-easy/issues/85
 - Resolves an issue where the exception raised by a plugin was hidden because of reflection. 
 - Resolves an issue with PostImages: where attributes defined in a PluginStepDefintion of a PostImage event where not returned  - https://github.com/DynamicsValue/fake-xrm-easy/issues/102

## [2.3.0]

### Changed

- Implement execution of custom apis, as well as registration of extra plugin steps against these apis - https://github.com/DynamicsValue/fake-xrm-easy/issues/50
- Allow execution of PreImages in prevalidation : https://github.com/DynamicsValue/fake-xrm-easy/issues/71
 
### Added

- Added execution of fake custom api's
- Added validation of preimages and postimages during plugin step registration - https://github.com/DynamicsValue/fake-xrm-easy/issues/33
- Added custom function to support registration of plugin steps via a custom reflection function (CustomPluginStepDiscoveryFunction) - https://github.com/DynamicsValue/fake-xrm-easy/issues/11
- First implementation (preview) of automatic registration of plugin steps via reflection - https://github.com/DynamicsValue/fake-xrm-easy/issues/11


### Changed

- IXrmFakedContextPluginExtensions is now IXrmBaseContextPluginExtensions. All extensions methods were changed to use the IXrmBaseContext interface as opposed to the IXrmFakedContext interface only so they could be used by both XrmFakedContext and XrmRealContext classes. - https://github.com/DynamicsValue/fake-xrm-easy/issues/35

## [2.2.0]

### Added 

 - OutputParameters is now populated in PipelineSimulation: DynamicsValue/fake-xrm-easy#39 
 - Plugin Step registration improvements: new general purpose method to register plugin steps: DynamicsValue/fake-xrm-easy#53
 - Added possibility to register plugin steps with logical name, and therefore support for late bound registration DynamicsValue/fake-xrm-easy#38
 
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