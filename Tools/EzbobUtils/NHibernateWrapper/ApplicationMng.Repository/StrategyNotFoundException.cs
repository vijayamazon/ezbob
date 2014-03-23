using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Repository
{
	[System.Serializable]
	public class StrategyNotFoundException : System.Exception
	{
		public StrategyNotFoundException()
		{
		}
		public StrategyNotFoundException(string message) : base(message)
		{
		}
		public StrategyNotFoundException(string message, System.Exception inner) : base(message, inner)
		{
		}
		protected StrategyNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
