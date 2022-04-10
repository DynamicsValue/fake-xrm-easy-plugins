using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// The plugin step validator is in charge of validating that a given combination of message, entity, stage, and mode is valid (that it can be registered in plugin registration tool)
    /// </summary>
    internal interface IPluginStepValidator
    {
        /// <summary>
        /// Returns true if the given combination is valid
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="stage"></param>
        /// <param name="mode"></param>
        /// <returns>True if the combination can be registered, false otherwise</returns>
        bool IsValid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode);
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
                { "upsert", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { EntityLogicalNameContants.AppNotification, new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        },
                        { EntityLogicalNameContants.SearchTelemetry, new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "create", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "update", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "retrieve", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "retrievemultiple", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "delete", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                },
                { "*", new Dictionary<string, Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>>()  //default entry
                    {
                        { "*", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Prevalidation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 }
                                    }
                                },
                                { ProcessingStepStage.Postoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
                                    }
                                }
                            }
                        }
                    }
                }
            };

  
        }

        public bool IsValid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            var messageNameToCheck = messageName.ToLower().Trim();
            if (_combinations.ContainsKey("*") && !_combinations.ContainsKey(messageNameToCheck))
            {
                messageNameToCheck = "*";
            }

            if (!_combinations.ContainsKey(messageNameToCheck))
                return false;

            string entityLogicalNameToCheck = entityLogicalName;
            if (_combinations[messageNameToCheck].ContainsKey("*") && !_combinations[messageNameToCheck].ContainsKey(entityLogicalName))
            {
                entityLogicalNameToCheck = "*";
            }


            if(!_combinations[messageNameToCheck].ContainsKey(entityLogicalNameToCheck))
            {
                throw new InvalidPrimaryEntityNameException(entityLogicalName);
            }

            if (!_combinations[messageNameToCheck][entityLogicalNameToCheck].ContainsKey(stage))
            {
                return false;
            }

            if (!_combinations[messageNameToCheck][entityLogicalNameToCheck][stage].ContainsKey(mode))
            {
                return false;
            }

            return true;
        }
    }
}
