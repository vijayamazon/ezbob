using EzBob;

namespace WorkflowObjects.Ezbob.NodeEzBobLib
{
	public class NodeEzBobUpdateCustomerData : NodeEzBobUpdateDataBase
	{
		private readonly string _VariableName = "CustomerId";

		public NodeEzBobUpdateCustomerData(string initialValue) 
			: base(initialValue)
		{
		}

		public NodeEzBobUpdateCustomerData() 
			: this(string.Empty)
		{
			
		}

		protected override int ExecuteRequest(int value)
		{
			return RetrieveDataHelper.UpdateCustomerData( value );
		}

		public override string VariableNameTarget
		{
			get { return _VariableName; }
		}

		protected override string LogName
		{
			get { return "EzBob.Node.UpdateCustomerData"; }
		}
	}
}