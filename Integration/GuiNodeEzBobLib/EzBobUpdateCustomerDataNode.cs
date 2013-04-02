using GuiNodeEzBobLib.Properties;
using WorkflowObjects.Ezbob.NodeEzBobLib;

namespace GuiNodeEzBobLib
{
	public class EzBobUpdateCustomerDataNode : EzBobUpdateDataNodeBase<NodeEzBobUpdateCustomerData>
	{
		public const string NodeName = "EzBobUpdateCustomerDataNode";

		public EzBobUpdateCustomerDataNode()
			: base( NodeName )
		{
			DisplayName = Resources.EzBobUpdateCustomerDataNode;
		}

		protected override EzBobUpdateDataNodeBase Create()
		{
			return new EzBobUpdateCustomerDataNode();
		}
	}
}
