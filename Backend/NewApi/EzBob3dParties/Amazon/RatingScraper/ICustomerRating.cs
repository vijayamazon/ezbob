namespace EzBob3dParties.Amazon.RatingScraper
{
    using System.Threading.Tasks;

    interface ICustomerRating {
        Task<AmazonRatingInfo> GetRating(string merchantId);
    }
}
