namespace EzbobAPI {
	using System.IO;
	using System.ServiceModel;
	using System.ServiceModel.Web;

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICustomer" in both code and config file together.
	[ServiceContract]
	public interface IImage {
		[WebInvoke(UriTemplate = "/img/{width}/{height}")]
		Stream GetImage(int width, int height);
	}
}
