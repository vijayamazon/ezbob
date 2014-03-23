using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Repository
{
	[System.Serializable]
	public class DatasourceNotFoundException : System.Exception
	{
		public DatasourceNotFoundException()
		{
		}
		public DatasourceNotFoundException(string message) : base(message)
		{
		}
		public DatasourceNotFoundException(string message, System.Exception inner) : base(message, inner)
		{
		}
		protected DatasourceNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
