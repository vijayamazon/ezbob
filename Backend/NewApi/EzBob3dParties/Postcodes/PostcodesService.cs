using System.Threading.Tasks;

namespace EzBob3dParties.Postcodes {
    using EzBobCommon;
    using EzBobCommon.Web;
    using EzBobModels.Postcode;

    public class PostcodesService {
        [Injected]
        public IEzBobWebBrowser Browser { get; set; }

        [PostInject]
        private void Init() {
            Browser.SetBaseAddress("https://api.postcodes.io/postcodes/");
        }

        public Task<PostcodesAddress> GetBritishAddressByPostCode(string postCode) {
            return Browser.GetAsyncJsonResponseAs<PostcodesAddress>(postCode);
        }
    }
}