namespace TestApplication
{
	using System;
	using System.Data;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using EzBob;
	using EzBob.AmazonLib;
	using EzBob.AmazonServiceLib.UserInfo;
	using EzBob.PayPal;
	using EzBob.RequestsQueueCore.RequestStates;
	using EzBob.eBayLib;
	using CustomSchedulers.Currency;

	public partial class Form1 : Form
	{
		private bool _CanClose = true;

		public Form1()
		{
			InitializeComponent();
			_ProgressBar.Visible = false;
		}

		private void SetAccesibilityOfControls( bool inProgress )
		{
			_CanClose = !inProgress;

			_ButtonRetrieveAmazonDataOrders.Enabled = !inProgress;

			_ProgressBar.Visible = inProgress;
			_ButtonStoreTestDataAmazon.Enabled = !inProgress;
			_ButtonRetrieveAmazonDataInventory.Enabled = !inProgress;
			_ButtonRetrieveAmazonUserFeedbackInfo.Enabled = !inProgress;
			_ButtonStoreTestDataEbay.Enabled = !inProgress;
			_ButtonEbaySroteUserData.Enabled = !inProgress;
			_ButtonEbayStoreUserAccountInfo.Enabled = !inProgress;
			_ButtonEbayFeedbackInfo.Enabled = !inProgress;
			_ButtonEbayInventoryInfo.Enabled = !inProgress;
			_EditCommonCustomer.Enabled = !inProgress;
			_EditCommonMarketPlace.Enabled = !inProgress;
			_ButtonCommonUpdateAllData.Enabled = !inProgress;
			_ButtonCommonHasDataForUser.Enabled = !inProgress;
			_ButtonEbayStoreOrdersData.Enabled = !inProgress;
			_ButtonPayPalStoreAccountInfo.Enabled = !inProgress;
			_ButtonPayPalStoreTestData.Enabled = !inProgress;
			_ButtonPayPalStoreTransactionData.Enabled = !inProgress;
			_ButtonUpdateCustomerData.Enabled = !inProgress;
			_ButtonUpdateCustomerMarketPlaceData.Enabled = !inProgress;
			_EditUpdateDataCustomerId.Enabled = !inProgress;
			_EditUpdateDataCustomerMarketplaceId.Enabled = !inProgress;
			_ButtonUpdateCurrency.Enabled = !inProgress;
		}

		private void Form1_Load( object sender, EventArgs e )
		{
			// TODO: This line of code loads data into the 'sSSEZBobDataSet.MP_CustomerMarketPlace' table. You can move, or remove it, as needed.
			this.mP_CustomerMarketPlaceTableAdapter.Fill( this.sSSEZBobDataSet.MP_CustomerMarketPlace );
			// TODO: This line of code loads data into the 'eZBobDataSet.Customer' table. You can move, or remove it, as needed.
			this.customerTableAdapter.Fill( this.eZBobDataSet.Customer );
			// TODO: This line of code loads data into the 'eZBobDataSet.MP_MarketplaceType' table. You can move, or remove it, as needed.
			this.mP_MarketPlaceTableAdapter.Fill( this.eZBobDataSet.MP_MarketPlace );

		}

		private void ProcessDataEbay( Action<int> action, string actionName )
		{
			var customerId = GetByMPCustomerId();

			ProcessData( () => action( customerId ), actionName );
		}

		private void ProcessDataCommon( Action<int, int> action, string actionName )
		{
			var customer = GetCommonCustomerId();
			var marketPlace = GetCommonMarketPlace();

			if ( customer == null || marketPlace == null )
			{
				ShowError("Please, select correct data!");
				return;
			}

			Action action1 = () => action( customer.Id, marketPlace.Id );
			ProcessData( action1, string.Format( "{0}: {1}", marketPlace.Name, actionName ) );
		}

		private EZBobDataSet.MP_MarketPlaceRow GetCommonMarketPlace()
		{
			var item = _EditCommonMarketPlace.SelectedItem as DataRowView;

			if ( item == null )
			{
				return null;
			}
			return item.Row as EZBobDataSet.MP_MarketPlaceRow;
		}

		private EZBobDataSet.CustomerRow GetCommonCustomerId()
		{
			var item = _EditCommonCustomer.SelectedItem as DataRowView;

			if ( item == null )
			{
				return null;
			}
			return  item.Row as EZBobDataSet.CustomerRow;
		}

		private void ProcessDataAmazon( Action<int> action, string actionName )
		{
			var customerId = GetByMPCustomerId();
			ProcessData( () => action( customerId ), actionName );
		}

		private void ProcessData( Action action, string actionName )
		{
			StartWait( actionName );

			try
			{

				var waitTask = Task.Factory.StartNew( () =>
				{
					try
					{
						var task = Task.Factory.StartNew( action );
						task.Wait();
						EndWait();
					}
					catch ( AggregateException ex )
					{
						EndWithError( ex.InnerException );
					}
				} );
			}
			catch ( Exception ex )
			{
				ShowError( ex );
			}
		}

		private void StartWait(string actionName)
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( () => StartWait( actionName ) ) );
				return;
			}
			_LabelLastAction.Text = actionName;
			_LabelActionResult.Text = string.Empty; 
			SetAccesibilityOfControls(true);
		}

		

		private void EndWithError(Exception ex)
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( () => EndWithError(ex) ) );
				return;
			}

			ShowError(ex);
			EndWait();
		}

		private void EndWait()
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( EndWait ) );
				return;
			}

			SetAccesibilityOfControls( false );
		}

		private void ShowError( string errText )
		{
			_LabelActionResult.Text = string.Format( "- Error: {0}", errText );
		}

		private void ShowError( Exception ex )
		{
			ShowError( ex.Message );
		}

		private void _ButtonStoreTestDataAmazon_Click( object sender, EventArgs e )
		{
			ProcessDataAmazon( AmazonTest.TestStoreCustomerSecurityData, "Amazon: Update Customer's Security Data " );
		}

		private void _ButtonStoreTestDataEbay_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestStoreCustomerSecurityData, "eBay: Update Customer's Security Data" );
		}

		private void _ButtonRetrieveAmazonDataOrders_Click( object sender, EventArgs e )
		{
			ProcessDataAmazon( AmazonTest.TestRetrieveAmazonOrdersData, "Amazon: Update Data of Customer Orders" );
		}

		private void _ButtonRetrieveAmazonDataInventory_Click_1( object sender, EventArgs e )
		{
			ProcessDataAmazon( AmazonTest.TestRetrieveAmazonInventoryData, "Amazon: Update Data of Customer Inventory" );
		}

		private void _ButtonRetrieveAmazonUserFeedbackInfo_Click_1( object sender, EventArgs e )
		{
			ProcessDataAmazon( AmazonTest.TestRetrieveAmazonUserFeedbackInfo, "Amazon: Update Users Feedback Data" );
		}

		private void _ButtonEbaySroteUserData_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateAllUserInfo, "eBay: Update User Info" );
		}

		private void Form1_FormClosing( object sender, FormClosingEventArgs e )
		{
			e.Cancel = !_CanClose;

			if ( _CanClose )
			{
				RetrieveDataHelper.Exit();
			}
		}

		private void _ButtonEbayStoreUserAccountInfo_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateAllAccountInfo, "eBay: Update User Account Info" );
		}

		private void _ButtonEbayFeedbackInfo_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateFeedbackInfo, "eBay: Update User Feedback Info" );
		}

		private void _ButtonEbayUpdateInventoryInfo_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateInventoryInfo, "eBay: Update User Inventory Info" );
		}

		
		private void _ButtonCommonUpdateAllData_Click( object sender, EventArgs e )
		{
			//ProcessDataCommon( RetrieveDataHelper.ForceRetrieveData, "Update All User Info" );
		}

		private void _ButtonCommonHasDataForUser_Click( object sender, EventArgs e )
		{
			/*ProcessDataCommon( (c, m) => 
									{
										bool rez = RetrieveDataHelper.HasStoredData(c, m);
										this.InvokeIfNeeded( () =>
															{
																_LabelActionResult.Text = string.Format( "- {0}", rez ? "YES" : "NO" );
															} );
									}, 
								"Has Stored Data" );
			*/
			
		}

		private void _ButtonEbayStoreOrdersData_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateOrdersInfo, "eBay: Update User Orders Info" );
		}

		private void _ButtonPayPalStoreTestData_Click( object sender, EventArgs e )
		{
			ProcessDataPayPal( PayPalTest.StoreTestData, "PayPal: Update User Account Info" );
		}

		private void _ButtonPayPalStoreAccountInfo_Click( object sender, EventArgs e )
		{
			//ProcessDataPayPal( PayPalTest.UpdateAccountInfo, "PayPal: Update User Account Info" );
		}

		private void ProcessDataPayPal( Action<int> action, string actionName )
		{
			var customerId = GetByMPCustomerId();
			ProcessData( () => action( customerId ), actionName );			
		}

		private int GetByMPCustomerId()
		{
			var item = _EditByMPCustomerId.SelectedItem as DataRowView;

			if ( item == null )
			{
				throw new Exception( "Please, select correct data!" );

			}
			var row = item.Row as EZBobDataSet.CustomerRow;

			return row.Id;
		}

		private void _ButtonPayPalStoreTransactionData_Click( object sender, EventArgs e )
		{
			ProcessDataPayPal( PayPalTest.UpdateTransactionInfo, "PayPal: Update Transaction Info" );
		}

		private void DisplayRequestState( string requestState )
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( () => DisplayRequestState( requestState ) ) );
				return;
			}

			_ViewUpdateDataRequestState.Text = requestState;

		}

		private void DisplayErrorMessage( string message )
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( () => DisplayErrorMessage( message ) ) );
				return;
			}

			_ViewUpdateDataErrorMessage.Text = message;

		}

		private void DisplayRequestId( int id )
		{
			if ( this.InvokeRequired )
			{
				this.Invoke( new MethodInvoker( () => DisplayRequestId( id ) ) );
				return;
			}

			_ViewUpdateDataRequestId.Text = id.ToString();
		}

		private void _ButtonUpdateCustomerData_Click( object sender, EventArgs e )
		{
			var item = _EditUpdateDataCustomerId.SelectedItem as DataRowView;

			if ( item == null )
			{
				return;
			}
			var row = item.Row as EZBobDataSet.CustomerRow;


			if ( row == null )
			{
				ShowError( "Please, select correct customer marketplace!" );
				_EditUpdateDataCustomerMarketplaceId.Focus();
				return;
			}
			var id = row.Id;
			UpdateData( id, 
						RetrieveDataHelper.UpdateCustomerData,
						"Update Customer Data" );
		}

		private void _ButtonUpdateCustomerMarketPlaceData_Click( object sender, EventArgs e )
		{
			var item = _EditUpdateDataCustomerMarketplaceId.SelectedItem as DataRowView;

			if ( item == null )
			{
				return;
			}
			var row = item.Row as SSSEZBobDataSet.MP_CustomerMarketPlaceRow;


			if ( row == null )
			{
				ShowError( "Please, select correct customer marketplace!" );
				_EditUpdateDataCustomerMarketplaceId.Focus();
				return;
			}
			var id = row.Id;
			UpdateData( id, 
						RetrieveDataHelper.UpdateCustomerMarketplaceData, 
						"Update Customer Marketplace Data" );
		}


		private void UpdateData( int id, Func<int, int> action, string actionName )
		{
			Task.Factory.StartNew( () =>
			{
				StartWait( actionName );

				
				var requestId = action( id );

				DisplayRequestId( requestId );

				var isDone = false;
				while ( !isDone )
				{
					isDone = RetrieveDataHelper.IsRequestDone( requestId );
					IRequestState requestState = RetrieveDataHelper.GetRequestState( requestId );
					DisplayRequestState( requestState.ToString() );
				}

				DisplayErrorMessage( RetrieveDataHelper.GetError( requestId ) );

				EndWait();
			} );
		}

		private void _ButtonCheckAmazonUser_Click( object sender, EventArgs e )
		{
			if ( string.IsNullOrWhiteSpace( _EditCheckAmazonUser.Text ) )
			{
				return;
			}

			var userInfo = new AmazonUserInfo
			{
				MerchantId = _EditCheckAmazonUser.Text,
			};
			var rez = RetrieveDataHelper.IsAmazonUserCorrect( userInfo );

			_ViewCheckAmazonUser.Text = rez ? "OK" : "FAIL";
		}

		private void _EditCheckAmazonUser_TextChanged( object sender, EventArgs e )
		{
			_ViewCheckAmazonUser.Text = string.Empty;
		}

		private void _ButtonUpdateCurrency_Click( object sender, EventArgs e )
		{
			ProcessData( CurrencyUpdateController.Run, "Update Currency" );
		}

		private void _ButtonEbayStoreCategoriesInfo_Click( object sender, EventArgs e )
		{
			ProcessDataEbay( eBayTest.TestUpdateCategoriesInfo, "eBay: Update User Categories Info" );
		}

	}


}
