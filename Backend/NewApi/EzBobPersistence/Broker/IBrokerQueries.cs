namespace EzBobPersistence.Broker
{
    public interface IBrokerQueries
    {
        /// <summary>
        /// Determines whether there is a broker with specified emailAddress
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        bool IsExistsBroker(string emailAddress);
    }
}
