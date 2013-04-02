using System;
using System.Collections.Generic;
using StrategyBuilder.Logic;
using StrategyBuilder.Logic.Exceptions;
using StrategyBuilder.Logic.GUINodes;
using StrategyBuilder.UI.Nodes;
using StrategyBuilder.UI.VariableEditor;
using WorkflowLibrary;
using WorkflowObjects;
using WorkflowObjects.Ezbob.NodeEzBobLib;
using StrategyBuilder.Logic.GUINodes.BehavioralNodes;

namespace GuiNodeEzBobLib
{
	public abstract class EzBobUpdateDataNodeBase<T> : EzBobUpdateDataNodeBase
		where T : NodeEzBobUpdateDataBase, new()
	{
		protected EzBobUpdateDataNodeBase(string name) : base(name)
		{

		}

		public override Type AssotiatedNodeType
		{
			get { return typeof( T ); }
		}

		protected override string VariableName
		{
			get { return new T().VariableNameTarget; }
		}

		protected override NodeEzBobUpdateDataBase CreateNode()
		{
			return new T();
		}
	}

	public abstract class EzBobUpdateDataNodeBase : AbstractNode, IVariableModifier, IDataSourceColumnNode
	{
		protected EzBobUpdateDataNodeBase(string name) : base(name)
		{
			Init();			
		}

		sealed protected override void Init()
		{
			base.Init();
			AddConditionalDockHosts();
		}

		public override CommonNodePropertiesForm GetNodePropertiesForm()
		{
			return new EzBobUpdateCustomerDataNodePropertiesForm( this );
		}
		
		protected abstract EzBobUpdateDataNodeBase Create();
		protected abstract string VariableName { get; }
		protected abstract NodeEzBobUpdateDataBase CreateNode();


		public override string Category
		{
			get
			{
				return "integration";
			}
		}

		public override object Clone()
		{
			return Create().AddVariable();
		}


		public override void Validate()
		{
			base.Validate();
			IsValid = true;			
		}

		public override bool CloneVariables( Strategy activeStrategy, AbstractNode destinationNode )
		{
			// For Copy/Paste
			return true;
		}

		private EzBobUpdateDataNodeBase AddVariable()
		{
			m_Variables.Clear();
			var variable = new VariableDescriptor( Key, 
													VariableName,
													VariableDescriptorWrapperConverter.ConvertTypeName( UserFriendlyType.Numeric ),
													string.Empty
													);
			variable.IsOutput = false;
			m_Variables.Add( variable );

			var varRequestId = new VariableDescriptor( Key,
													NodeEzBobUpdateDataBase.VarNameRequestIdVariable,
													VariableDescriptorWrapperConverter.ConvertTypeName( UserFriendlyType.Numeric ),
													"0",
													"0",
													string.Empty,
													true,
													true,
													false
													);			
			m_Variables.Add( varRequestId );

			var outletname = new VariableDescriptor( Key, 
													Node.nodeOutletName, 
													VariableDescriptorWrapperConverter.ConvertTypeName( UserFriendlyType.String ),
			                                        string.Empty
													);
			outletname.IsInput = false;
			m_Variables.Add( outletname );

			var dataLayerName = new VariableDescriptor( Key,
										Node.OutputDataLayerNameVariableName,
										VariableDescriptorWrapperConverter.ConvertTypeName( UserFriendlyType.String ),
										string.Empty
										);
			dataLayerName.IsInput = true;
			dataLayerName.IsOutput = true;

			m_Variables.Add( dataLayerName );


			var errorVar = new VariableDescriptor( Key, 
													NodeEzBobUpdateDataBase.VarNameErrorMessages,
													VariableColumnReferenceString.VariableType,
													string.Empty
													);
			errorVar.IsInput = false;
			m_Variables.Add( errorVar );
			var errorCode = new VariableDescriptor( Key,
													NodeEzBobUpdateDataBase.VarNameErrorCodes,
													VariableColumnReferenceString.VariableType,
													string.Empty
													);
			errorCode.IsInput = false;
			m_Variables.Add( errorCode );

			var errorUmi = new VariableDescriptor( Key,
													NodeEzBobUpdateDataBase.VarNameErrorUmies,
													VariableColumnReferenceNumeric.VariableType,
													string.Empty
													);
			errorUmi.IsInput = false;
			m_Variables.Add( errorUmi );

			var errorMarketplaceTypeId = new VariableDescriptor( Key,
													NodeEzBobUpdateDataBase.VarNameErrorMarketplaceTypeId,
													VariableColumnReferenceString.VariableType,
													string.Empty
													);
			errorMarketplaceTypeId.IsInput = false;
			m_Variables.Add( errorMarketplaceTypeId );

			return this;
		}

		private void AddConditionalDockHosts()
		{
			var tmpNode = CreateNode();
			m_SourceDockHosts.Clear();
			foreach ( string item in tmpNode.OutletNames )
			{
				ConditionDockHost host;
				if ( NodeOutlet.IsEqual( NodeOutlet.OutletType.Exception, item ) )
				{
					host = new ConditionDockHost( item,
					                              item,
					                              ConditionDockHostType.Exception,
					                              Key );
				}
				else
				{
					host = new ConditionDockHost( string.Empty,
					                              item,
					                              ConditionDockHostType.NormalFlowResult,
					                              Key );
					
				}
				m_SourceDockHosts.Add( host );
			}
		}

		#region Implementation of IVariableModifier

		//-----------------------------------------------------------------------------
		public string[] ModifiableVariables()
		{
			var result = new List<string>();
			if ( m_ParentStrategy != null )
			{
				foreach ( VariableConnectionDescriptor connection in m_ParentStrategy.VariableConnections )
				{
					if ( Name == connection.TargetVariableOwnerName )
					{
						result.Add( connection.SourceVariableName );
					}
				}
			}

			return result.ToArray();
		}

		#endregion

	}
}