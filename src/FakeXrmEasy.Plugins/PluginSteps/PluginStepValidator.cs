using FakeXrmEasy.Abstractions.Plugins.Enums;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// The plugin step validator is in charge of validating that a given combination of message, entity, stage, and mode is valid (that it can be registered in plugin registration tool)
    /// </summary>
    public interface IPluginStepValidator
    {
        /// <summary>
        /// Returns true if the given combination is valid
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="stage"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        bool IsValid(string messageName, string entityLogicalName, ProcessingStepStage stage, ProcessingStepMode mode);
    }

    /// <summary>
    /// Default implementation of IPluginStepValidator
    /// </summary>
    public class PluginStepValidator: IPluginStepValidator
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
                        { "appnotification", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
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
                        { "searchtelemetry", new Dictionary<ProcessingStepStage, Dictionary<ProcessingStepMode, int>>()
                            {
                                { ProcessingStepStage.Preoperation, new Dictionary<ProcessingStepMode, int>()
                                    {
                                        {  ProcessingStepMode.Synchronous, 0 },
                                        {  ProcessingStepMode.Asynchronous, 0 }
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
            if (!_combinations.ContainsKey(messageName.ToLower()))
                return false;

            if (!_combinations[messageName].ContainsKey(entityLogicalName.ToLower()))
                return false;

            if (!_combinations[messageName][entityLogicalName].ContainsKey(stage))
                return false;

            if (!_combinations[messageName][entityLogicalName][stage].ContainsKey(mode))
                return false;

            return true;
        }
    }
}
