using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Model
{
	[System.Serializable]
	public class ExportTemplateNotFoundException : System.Exception
	{
		public ExportTemplateNotFoundException()
		{
		}
		public ExportTemplateNotFoundException(string message) : base(message)
		{
		}
		public ExportTemplateNotFoundException(string message, System.Exception inner) : base(message, inner)
		{
		}
		protected ExportTemplateNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
