using EzBob;

namespace WorkflowObjects.Ezbob.NodeEzBobLib
{
	public class NodeEzBobUpdateCustomerMarketPlaceData : NodeEzBobUpdateDataBase
	{
		private readonly string _VariableName = "CustomerMarketPlaceId";

		public NodeEzBobUpdateCustomerMarketPlaceData(string initialValue) 
			: base(initialValue)
		{
		}

		public NodeEzBobUpdateCustomerMarketPlaceData() 
			: this(string.Empty)
		{
		}

		protected override int ExecuteRequest(int value)
		{
			return RetrieveDataHelper.UpdateCustomerMarketplaceData( value );
		}

		public override string VariableNameTarget
		{
			get { return _VariableName; }
		}

		protected override string LogName
		{
			get { return "EzBob.Node.UpdatecustomerMarketPlaceData"; }
		}
	}
}