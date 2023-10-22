

## [3.3.3]

### Added 

- Added [ExcludeFromCodeCoverage] to included generated PipelineType entities in src as it messes up code coverage metrics 
- **[Possibly breaking change]** : Exception raised if both EntityTypeCode and EntityLogicalName are set when registering a plugin step. EntityTypeCode is deprecated, please move all plugin registrations to use EntityLogicalName instead.
- Introduced new user-defined exceptions to verify plugin step registrations when EntityTypeCode is used
- Adding support for secure and unsecure configurations in pipeline simulation - https://github.com/DynamicsValue/fake-xrm-easy/issues/103

### Changed

- Resolves an issue when using early bound types without pipeline types being generated caused plugin step registration to fail - https://github.com/DynamicsValue/fake-xrm-easy/issues/85
- Resolves an issue where the exception raised by a plugin was hidden because of reflection. 
- Resolves an issue with PostImages: where attributes defined in a PluginStepDefintion of a PostImage event where not returned  - https://github.com/DynamicsValue/fake-xrm-easy/issues/102
- Update legacy CRM SDK 2011 dependency to use official MS package - https://github.com/DynamicsValue/fake-xrm-easy/issues/105

## [3.3.1]

### Changed

- Bump DataverseClient dependency to target net6 - https://github.com/DynamicsValue/fake-xrm-easy/issues/90

## [3.3.0]

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

## [3.2.0]

### Added 

 - OutputParameters is now populated in PipelineSimulation: DynamicsValue/fake-xrm-easy#39 
 - Plugin Step registration improvements: new general purpose method to register plugin steps: DynamicsValue/fake-xrm-easy#53
 - Added possibility to register plugin steps with logical name, and therefore support for late bound registration DynamicsValue/fake-xrm-easy#38
 
### Changed

- Fix Sonar Quality Gate settings: DynamicsValue/fake-xrm-easy#28

## [3.1.2]

### Changed

- Bump dataverse dependency to 1.0.1

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
