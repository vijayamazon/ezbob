namespace EzBob3dParties.Amazon.RatingScraper
{
    using System.Threading.Tasks;

    public interface IAmazonCustomerRating {
        Task<AmazonCustomerRatingInfo> GetRating(string merchantId);
    }
}
