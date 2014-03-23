using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Repository
{
	[System.Serializable]
	public class NodeNotFoundException : System.Exception
	{
		public NodeNotFoundException()
		{
		}
		public NodeNotFoundException(string message) : base(message)
		{
		}
		public NodeNotFoundException(string message, System.Exception inner) : base(message, inner)
		{
		}
		protected NodeNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
