using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace EzbobAPI {
	using System.IO;
	using System.ServiceModel.Web;

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
	[ServiceContract]
	public interface IAuthenticationService {
		[OperationContract]
		[WebGet(UriTemplate = "signin/{username}/{password}", BodyStyle = WebMessageBodyStyle.Bare)]
		Stream Signin(string username, string password);

		[OperationContract]
		[WebGet(UriTemplate = "login/{username}/{password}", BodyStyle = WebMessageBodyStyle.Bare)]
		Stream Login(string username, string password);

		[OperationContract(Name = "GetSampleMethod_With_OAuth")]
		[WebGet(UriTemplate = "auth/{username}")]
		string GetSampleMethod_With_OAuth(string username);

		[OperationContract]
		[WebGet(UriTemplate = "testauth", BodyStyle = WebMessageBodyStyle.Bare)]
		Stream DoWork();
	}
}
