using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace EzbobAPI {
	using System.IO;
	using System.Web.Script.Services;

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICustomer" in both code and config file together.
	[ServiceContract]
	public interface IImage{

		
		[WebInvoke(UriTemplate = "/img/{width}/{height}")]
		Stream GetImage(int width, int height);


	}

}