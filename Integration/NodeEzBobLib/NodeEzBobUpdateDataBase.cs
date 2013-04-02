using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Exceptions;
using EzBob;
using EzBob.RequestsQueueCore.RequestStates;
using PacketDataFramework;
using WorkflowLibrary;

namespace WorkflowObjects.Ezbob.NodeEzBobLib
{
	public abstract class NodeEzBobUpdateDataBase : Node
	{
		private string description = "NodeEzBobUpdateBaseDescription";

		public static readonly string VarNameErrorMessages = "ErrorString";
		public static readonly string VarNameErrorCodes = "ErrorCode";
		public static readonly string VarNameErrorUmies = "ErrorUmi";
		public static readonly string VarNameErrorMarketplaceTypeId = "ErrorMarketplaceTypeId";		
		public static readonly string VarNameRequestIdVariable = "@InternalRequestId";

		private static readonly string[] _outletNames = {
                                                            NodeOutlet.GetOutletName(NodeOutlet.OutletType.Next),
                                                            NodeOutlet.GetOutletName(NodeOutlet.OutletType.No),
                                                            NodeOutlet.GetOutletName(NodeOutlet.OutletType.Exception)
                                                        };

		protected NodeEzBobUpdateDataBase( string initialValue )
		{
			
		}

		public override string[] OutletNames
		{
			get { return _outletNames; }
		}

		protected abstract int ExecuteRequest( int value );

		public abstract string VariableNameTarget { get; }

		// TODO: DEL ME!!!
		//private int _RequestIdTemp = 0;

		public override string Execute(IWorkflow workflow)
		{
			var rez = string.Empty;
			int requestId = GetValueInt( workflow, VarNameRequestIdVariable );

			
			/*if (_RequestIdTemp != 0)
			{
				requestId = _RequestIdTemp;
			}*/
			try
			{
				if ( requestId == 0)				
				{
					var val = GetValueInt(workflow, VariableNameTarget);
					requestId = ExecuteRequest(val);
					
					//_RequestIdTemp = requestId;
					SetValue( workflow, VarNameRequestIdVariable, requestId );
					Debug.Assert(requestId > 0);

					if (requestId <= 0)
					{
						throw new InvalidOperationException();
					}
					rez = Execute(workflow);
				}
				else
				{
					var state = RetrieveDataHelper.GetRequestState(requestId);
					if (state.InProgress())
					{
						rez = _outletNames[1];
					}
					else if (state.IsSuccess())
					{
						rez = _outletNames[0];
					}
					else if (state.IsNotFound())
					{
						SetValue( workflow, VarNameRequestIdVariable, 0 );
						rez = Execute(workflow);
					}
					else if (state.HasError())
					{
						SetErrorValues(workflow, state);
						rez = _outletNames[2];
					}
					else
					{
						throw new NotImplementedException();
					}
				}

			}
			catch ( Exception ex )
			{
				var errorMessage = ex.Message;
				SetValue( workflow, VarNameErrorMessages, errorMessage );
				Exception = ex;
				Log.Error( errorMessage );
				Log.Debug( errorMessage );

				rez = _outletNames[2];
			}	

			SetValue( workflow, Node.nodeOutletName, rez );
			return rez;
		}

		private void SetErrorValues(IWorkflow workflow, IRequestState state)
		{
			var requestException = state.ErorrInfo.CompositeException;
			
			Exception = requestException;			
			Log.Error(state.ErorrInfo.Message);

			var errors = requestException.ToArray();
			
			var variables = 
				  (
				   from vc in workflow.VariableConnectionDescriptors
				   where vc.TargetVariableOwnerName == workflow.CurrentNodeName 
				   select vc).
				ToDictionary(k => k.SourceVariableName, k => k.TargetVariableName);

			var columnTarget = new List<string>
				                   {
					                   variables.First(v => v.Value == VarNameErrorUmies).Key,
					                   variables.First(v => v.Value == VarNameErrorCodes).Key,
					                   variables.First(v => v.Value == VarNameErrorMessages).Key,
									   variables.First(v => v.Value == VarNameErrorMarketplaceTypeId).Key
				                   };

			var targetType = new List<Type> 
									{
										typeof (float), 
										typeof (string), 
										typeof (string), 
										typeof (string)
									};


			var dataLayerValue = GetValue(workflow, Node.OutputDataLayerNameVariableName) as string;

			using ( IPacketDataWriter writer = VariableKeeper.PrepareWriter( columnTarget, targetType, workflow.CurrentNodeName, description, dataLayerValue, workflow ) )
			{
				var countErrors = errors.Length;
				for (int index = 0; index < countErrors; index++)
				{
					var exception = errors[index];

					if ( exception is InvalidCustomerException )
					{
						writer.WriteValues( new object[] { index + 1, -1, string.Empty, exception.Message, string.Empty } );
					}
					else if ( exception is InvalidCustomerMarketPlaceException )
					{
						var ex = exception as InvalidCustomerMarketPlaceException;
						writer.WriteValues( new object[] { index + 1, ex.CustomerMarketPlaceId, string.Empty, ex.Message, string.Empty } );
					}
					else if ( exception is MarketplaceException )
					{
						var mEx = exception as MarketplaceException;
						var umi = mEx.MarketplaceId.ToString();
						// marketplace Type (GUID)
						var mpType = mEx.DatabaseCustomerMarketPlace.Marketplace.InternalId.ToString();
						var innerException = exception.InnerException;

						if ( innerException is IServiceRequestException )
						{
							if ( innerException is ServiceRequestException )
							{
								var srEx = innerException as ServiceRequestException;
								writer.WriteValues( new object[] { index + 1, umi, srEx.Fault.ErrorCode, srEx.Fault.DetailedMessage, mpType } );
							}
							else if ( innerException is ServiceRequestListException )
							{
								var listSrEx = innerException as ServiceRequestListException;

								foreach ( var ex in listSrEx )
								{
									writer.WriteValues( new object[] { index + 1, umi, ex.ErrorCode, ex.DetailedMessage, mpType } );
								}
							}
							else
							{
								writer.WriteValues(new object[] { index + 1, umi, string.Empty, innerException.Message, mpType });
							}
						}
						else
						{
							writer.WriteValues( new object[] { index + 1, umi, string.Empty, mEx.Message, string.Empty } );
						}
					}
					else
					{
						writer.WriteValues( new object[] { index + 1, -1, string.Empty, exception.Message, string.Empty } );
					}
				}

				writer.Commit();
			}
		}

		private int GetValueInt( IWorkflow workflow, string targetVariableName )
		{
			var value = GetValue( workflow, targetVariableName );
			int val = 0;
			int.TryParse( value.ToString(), out val );

			return val;
		}

		private object GetValue( IWorkflow workflow, string targetVariableName )
		{
			foreach ( var variableConnectionDescriptor in workflow.VariableConnectionDescriptors )
			{
				if (variableConnectionDescriptor.TargetVariableOwnerName.Equals(workflow.CurrentNodeName) && 
					variableConnectionDescriptor.TargetVariableName.Equals(targetVariableName))
				{
					return workflow[variableConnectionDescriptor.SourceVariableName];
				}
			}

			return null;
		}

		private void SetValue( IWorkflow workflow, string targetVariableName, object value )
		{
			foreach ( var variableConnectionDescriptor in workflow.VariableConnectionDescriptors )
			{
				if ( !variableConnectionDescriptor.TargetVariableOwnerName.Equals( workflow.CurrentNodeName ) || !variableConnectionDescriptor.TargetVariableName.Equals( targetVariableName ) )
				{
					continue;
				}

				workflow[variableConnectionDescriptor.SourceVariableName] = value;
				break;

			}
		}
	}
}

