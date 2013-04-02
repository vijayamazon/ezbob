using System;
using StrategyBuilder.UI.Nodes;

namespace GuiNodeEzBobLib
{
	public partial class EzBobUpdateCustomerDataNodePropertiesForm : CommonNodePropertiesForm
	{
		private readonly EzBobUpdateDataNodeBase _Node;

		public EzBobUpdateCustomerDataNodePropertiesForm(EzBobUpdateDataNodeBase node)
			:base(node)
		{
			_Node = node;
			InitializeComponent();

			StrategyAndNodeVariableMapping.Init(
				_Node.ParentStrategy.VariableConnections,
				_Node.Variables,
				_Node.ParentStrategy.Variables,
				_Node.Key,
				_Node.ParentStrategy.Name,
				_Node.ParentStrategy.VariableConstraints
				);
			StrategyAndNodeVariableMapping.ParametersChanged += StrategyAndNodeVariableMapping_ParametersChanged;
		}

		protected override void ApplyToData()
		{
			base.ApplyToData();
			if ( StrategyAndNodeVariableMapping.CanApply() )
			{
				StrategyAndNodeVariableMapping.Apply();
			}
		}

		protected override void InitByData()
		{
			base.InitByData();
			StrategyAndNodeVariableMapping.Reset();			
		}

		private void StrategyAndNodeVariableMapping_ParametersChanged(object sender, EventArgs e)
		{
			ParamsIsDirty = true;
		}
	}
}
