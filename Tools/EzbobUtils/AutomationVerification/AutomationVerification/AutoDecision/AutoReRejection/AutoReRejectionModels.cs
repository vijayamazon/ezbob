
namespace AutomationCalculator.AutoDecision.AutoReRejection
{
	using System;
	using Newtonsoft.Json;
	using ProcessHistory;

	public class ReRejectInputData : ITrailInputData
	{
		public void Init(DateTime dataAsOf) {
			DataAsOf = dataAsOf;
			//todo populate the rest
		}
		public DateTime DataAsOf { get; private set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
