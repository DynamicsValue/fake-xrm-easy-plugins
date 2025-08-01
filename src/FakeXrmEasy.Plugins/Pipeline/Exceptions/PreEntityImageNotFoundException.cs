using System;

namespace FakeXrmEasy.Pipeline.Exceptions
{
    /// <summary>
    /// Exception raised when an image has been registered but the associated target entity doesn't exist
    /// </summary>
    public class PreEntityImageNotFoundException: Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logicalName">The pre entity image logical name</param>
        /// <param name="id">The pre entity image Id</param>
        public PreEntityImageNotFoundException(string logicalName, Guid id) : base(
            $"An entity image could not be retrieved for a registered preimage because the associated entity record with LogicalName '{logicalName}' and Id '{id}' does not exist")
        {
            
        }
    }
}