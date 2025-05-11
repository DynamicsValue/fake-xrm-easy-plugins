namespace FakeXrmEasy.Plugins.PluginSteps.Extensions
{
    /// <summary>
    /// Converts between OrganizationRequest's names and SdkMessage names
    /// </summary>
    internal static class SdkMessageConverterExtensions
    {
        /// <summary>
        /// Converts an OrganizationRequest name to an equivalent message name (i.e. SendEmailRequest is executed as a 'Send' message)
        /// </summary>
        /// <param name="requestName"></param>
        /// <returns></returns>
        internal static string ToMessageName(this string requestName)
        {
            switch (requestName)
            {
                case OrganizationRequestNameConstants.SEND_EMAIL:
                case OrganizationRequestNameConstants.SEND_FAX:
                case OrganizationRequestNameConstants.SEND_TEMPLATE:
                    return MessageNameConstants.Send;
                default:
                    return requestName;
            }
        }
    }
}