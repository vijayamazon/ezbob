using System.Xml.Serialization;

namespace EZBob.DatabaseLib
{
	[XmlRoot( "FaultDetail" )]
	public class ServiceFaultDetail
	{
		public string ErrorCode { get; set; }

		public string Severity { get; set; }

		public string DetailedMessage { get; set; }
	}
}