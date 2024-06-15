using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps.Extensions;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// The plugin step validator is in charge of validating if a given plugin step definition is valid (that it can be registered in plugin registration tool)
    /// </summary>
    internal interface IPluginStepValidator
    {
        /// <summary>
        /// Returns true if the given combination is valid
        /// </summary>
        /// <param name="stepDefinition">The plugin step definition to validate</param>
        /// <returns>True if the combination can be registered, false otherwise</returns>
        bool IsValid(IPluginStepDefinition stepDefinition);
    }

    /// <summary>
    /// Default implementation of IPluginStepValidator
    /// </summary>
    internal class PluginStepValidator: IPluginStepValidator
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>> _combinations;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginStepValidator()
        {
            _combinations = new Dictionary<string, Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>>()
            {
                { "upsert", GetUpsertValidPluginStepRules() },
                { "upsertmultiple", GetUpsertValidPluginStepRules() },
                { "create", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                },
                { "update", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                },
                { "retrieve", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                },
                { "retrievemultiple", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                },
                { "delete", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                },
                { "*", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()  //default entry
                    {
                        { "*", GetDefaultValidPluginStepAndModeRules() }
                    }
                }
            };

  
        }

        private Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>> GetUpsertValidPluginStepRules()
        {
            var validEntityNames = new string[]
            {
                EntityLogicalNameConstants.AppNotification,
                EntityLogicalNameConstants.BackgroundOperation,
                EntityLogicalNameConstants.CardStateItem,
                EntityLogicalNameConstants.ComponentVersionNrdDataSource,
                EntityLogicalNameConstants.ElasticFileAttachment,
                EntityLogicalNameConstants.EventExpanderBreadcrumb,
                EntityLogicalNameConstants.FlowLog,
                EntityLogicalNameConstants.FlowRun,
                EntityLogicalNameConstants.MsDynTimelinePin,
                EntityLogicalNameConstants.NlsqRegistration,
                EntityLogicalNameConstants.None,
                EntityLogicalNameConstants.PowerPagesLog,
                EntityLogicalNameConstants.RecentlyUsed,
                EntityLogicalNameConstants.SearchTelemetry,
                EntityLogicalNameConstants.SearchResultsCache,
                EntityLogicalNameConstants.SharedWorkspaceAccessToken,
                EntityLogicalNameConstants.SharedWorkspaceNr,
            };
            
            var validUpsertRules =
                new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>();

            foreach (var entityName in validEntityNames)
            {
                validUpsertRules.Add(entityName, GetDefaultValidPluginStepAndModeRules());
            }

            return validUpsertRules;
        }

        private Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>
            GetDefaultValidPluginStepAndModeRules()
        {
            return new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
            {
                {
                    ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                    {
                        { ProcessingStepMode.Synchronous, 0 }
                    }
                },
                {
                    ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                    {
                        { ProcessingStepMode.Synchronous, 0 }
                    }
                },
                {
                    ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                    {
                        { ProcessingStepMode.Synchronous, 0 },
                        { ProcessingStepMode.Asynchronous, 0 }
                    }
                }
            };
        }
        
        public bool IsValid(IPluginStepDefinition stepDefinition)
        {
            var messageNameToCheck = stepDefinition.MessageName.ToLower().Trim();
            if (_combinations.ContainsKey("*") && !_combinations.ContainsKey(messageNameToCheck))
            {
                messageNameToCheck = "*";
            }

            if (!_combinations.ContainsKey(messageNameToCheck))
                return false;

            string entityLogicalNameToCheck = stepDefinition.EntityLogicalName;

#pragma warning disable CS0618
            if (stepDefinition.EntityTypeCode == null && string.IsNullOrWhiteSpace(stepDefinition.EntityLogicalName))
            {
                entityLogicalNameToCheck = "*";
            }
#pragma warning restore CS0618
            
            if (_combinations[messageNameToCheck].ContainsKey("*") && !_combinations[messageNameToCheck].ContainsKey(entityLogicalNameToCheck))
            {
                entityLogicalNameToCheck = "*";
            }


            if(!_combinations[messageNameToCheck].ContainsKey(entityLogicalNameToCheck))
            {
                throw new InvalidPrimaryEntityNameException(stepDefinition.EntityLogicalName);
            }

            if (!_combinations[messageNameToCheck][entityLogicalNameToCheck].ContainsKey(stepDefinition.Stage))
            {
                return false;
            }

            if (!_combinations[messageNameToCheck][entityLogicalNameToCheck][stepDefinition.Stage].ContainsKey(stepDefinition.Mode))
            {
                return false;
            }

            return AreImagesValid(stepDefinition);
        }

        private bool AreImagesValid(IPluginStepDefinition stepDefinition)
        {
            if (stepDefinition.ImagesDefinitions == null)
                return true;

            var images = stepDefinition.ImagesDefinitions.ToList();
            var preImages = images.Where(image => image.ImageType == ProcessingStepImageType.PreImage || image.ImageType == ProcessingStepImageType.Both);
            var postImages = images.Where(image => image.ImageType == ProcessingStepImageType.PostImage || image.ImageType == ProcessingStepImageType.Both);

            foreach(var preImage in preImages)
            {
                if (!PreImage.IsAvailableFor(stepDefinition.MessageName.ToOrganizationCrudRequestType()))
                {
                    throw new PreImageNotAvailableException(preImage.Name);
                }
            }

            foreach (var postImage in postImages)
            {
                if (!PostImage.IsAvailableFor(stepDefinition.MessageName.ToOrganizationCrudRequestType(), stepDefinition.Stage))
                {
                    throw new PostImageNotAvailableException(postImage.Name);
                }
            }

            return true;
        }
    }
}
