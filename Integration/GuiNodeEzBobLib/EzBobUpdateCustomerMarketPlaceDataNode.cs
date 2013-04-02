using GuiNodeEzBobLib.Properties;
using WorkflowObjects.Ezbob.NodeEzBobLib;

namespace GuiNodeEzBobLib
{
	public class EzBobUpdateCustomerMarketPlaceDataNode : EzBobUpdateDataNodeBase<NodeEzBobUpdateCustomerMarketPlaceData>
	{
		public const string NodeName = "EzBobUpdateCustomerMarketPlaceDataNode";

		public EzBobUpdateCustomerMarketPlaceDataNode()
			: base( NodeName )
		{
			DisplayName = Resources.EzBobUpdateCustomerMarketPlaceDataNode;
		}

		protected override EzBobUpdateDataNodeBase Create()
		{
			return new EzBobUpdateCustomerMarketPlaceDataNode();
		}
	}
}