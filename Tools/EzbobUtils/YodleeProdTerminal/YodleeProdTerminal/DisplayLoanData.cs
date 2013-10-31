using System;
using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's loan item data in the Yodlee software platform..
	/// </summary>
	public class DisplayLoanData : ApplicationSuper
	{
		DataServiceService dataService;
		
		public DisplayLoanData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of loan items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="isHistoryNeeded"></param>
		public void displayLoanData(UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.LOAN;
			
			// Create a list of Container Criteria
			object[] list = {cc};

			// Create Summary request and add Container Criteria
			SummaryRequest sr = new SummaryRequest();
			sr.containerCriteria = list;
			sr.deletedItemAccountsNeeded = true;
			sr.historyNeeded = isHistoryNeeded;

			// Get ItemSummary
			object[] itemSummaries = dataService.getItemSummaries1(userContext, sr);
			
			// Verify that there is an ItemSummary
			if(itemSummaries == null || itemSummaries.Length == 0) 
			{
				System.Console.WriteLine("No bank data available");
				return;
			}

			for(int i = 0; i < itemSummaries.Length; i++)
			{
				ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
				displayLoanDataForItem(itemSummary);
			}
		}

		/// <summary>
		/// Convert UTC to DateTime
		/// </summary>
		/// <param name="utc">time in utc</param>
		public DateTime UtcToDateTime(long utc)
		{
			//calculate UTC format for Now
			DateTime then = new DateTime(1970,1,1);
			TimeSpan unixnow = DateTime.Now.Subtract(then);
			int seconds = Convert.ToInt32(utc);

			//convert it back to current DateTime
			DateTime dateTime = new DateTime(1970,1,1).AddSeconds(seconds);

			return dateTime;
		}

		/// <summary>
		/// Displays the item information and item data information
		/// for the given loan itemSummary.
		/// </summary>
		/// <param name="itemSummary">an itemSummary whose containerType is 'loan'</param>
		public void displayLoanDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals(ContainerTypes.LOAN)) 
			{
				throw new Exception("DisplayLoanDataForItem called with invalid container type" +
					containerType);
			}

			DisplayItemInfo displayItemInfo = new DisplayItemInfo();
			displayItemInfo.displayItemSummaryInfo(itemSummary);

			// Get ItemData
			ItemData1 itemData = itemSummary.itemData;
			if( itemData == null )
			{
				System.Console.WriteLine("\tItemData is null");
			}
			else
			{
				// LoanLoginAccountData
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{
					System.Console.WriteLine("\n\t\t**LoanLoginAccountData**");
					for(int i = 0; i < accounts.Length; i++)
					{
						LoanLoginAccountData llad = (LoanLoginAccountData) accounts[i];
						System.Console.WriteLine("\t\tLoanLoginAccountData.loanAccountNumber: "+ llad.loanAccountNumber );
						System.Console.WriteLine("\t\tLoanLoginAccountData.loanAccountNumber: " + UtcToDateTime(llad.lastUpdated.Value) );
						
						// Loan
						object[] loans = llad.loans;
						if (loans == null || loans.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo Loans.");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**Loan**");
							for (int j = 0; j < loans.Length; j++)
							{
								Loan loan = (Loan) loans[j];
								System.Console.WriteLine("\t\t\tLoan.accountName: " + loan.accountName );
								System.Console.WriteLine("\t\t\tLoan.accountNumber: " + loan.accountNumber );
								System.Console.WriteLine("\t\t\tLoan.interestRate: " + loan.interestRate );							
								
								// LoanPayOffs
								object[] loanPayOffs = loan.loanPayOffs;
								if (loanPayOffs == null || loanPayOffs.Length == 0) 
								{
									System.Console.WriteLine("\t\t\tNo loanPayOffs");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t\t**LoanPayoff**");
									for (int u = 0; u < loanPayOffs.Length; u++)
									{
										LoanPayoff loanPayOff = (LoanPayoff) loanPayOffs[u];
										System.Console.WriteLine("\t\t\t\tLoanPayoff.payoffAmount: "+ loanPayOff.payoffAmount.amount );
										System.Console.WriteLine("\t\t\t\tLoan Pay By Date: " + loanPayOff.payByDate.date );
									}
								}
								// End LoanPayOffs

								// LoanPayMentDues
								object[] loanPaymentDues = loan.loanPaymentDues;
								if (loanPaymentDues == null || loanPaymentDues.Length == 0) 
								{
									System.Console.WriteLine("\t\t\tNo loanPaymentDues");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t\t**LoanPaymentDue**");
									for (int u = 0; u < loanPaymentDues.Length; u++)
									{
										LoanPaymentDue lpd = (LoanPaymentDue) loanPaymentDues[u];
										System.Console.WriteLine("\t\t\t\tLoanPaymentDue.interestAmount: "+ lpd.interestAmount.amount );
										System.Console.WriteLine("\t\t\t\tLoanPaymentDue.principalAmount: " + lpd.principalAmount.amount );
										
										// Bill
										Bill bill = lpd.bill;
										if( bill == null ){
											System.Console.WriteLine("\t\t\t\t\tNo Bill");
										}else{
											System.Console.WriteLine("\t\t\t\tBill.dueDate: " + bill.dueDate.date );
											System.Console.WriteLine("\t\t\t\tBill.minPayment: "+ bill.minPayment.amount );
										}
										// end Bill
									}
								}
								// End LoanPayMentDues

								// LoanTransaction
								object[] loanTransactions = loan.loanTransactions;
								if (loanTransactions == null || loanTransactions.Length == 0) 
								{
									System.Console.WriteLine("\t\tNo loan tranactions");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t**LoanTransaction**");
									for (int u = 0; u < loanTransactions.Length; u++)
									{
										LoanTransaction trans =
											(LoanTransaction) loanTransactions[u];
										System.Console.WriteLine("\t\t\t\tTranaction.amount: " + trans.amount.amount);
                                        System.Console.WriteLine("\t\t\t\tTranaction.transDate : " + (trans.transactionDate.month + '-' + trans.transactionDate.dayOfMonth + '-' + trans.transactionDate.year));
										System.Console.WriteLine("\t\t\t\tTransaction.description: " + trans.description );
										System.Console.WriteLine("\t\t\t\tTranaction.transactionType : " + trans.transactionType );
									}
								}
								// End LoanTransaction
							}
						}
						// End Loan
					}
				}
				// End LoanLoginAccountData
			}

			/*// Get AccountHistory            
			object[] acctHistories = itemData.accountHistory;
			if(acctHistories == null || acctHistories.Length == 0)
			{
				System.Console.WriteLine("\tNo Account History");
			}
			else
			{
				System.Console.WriteLine("\n\t**Account History**");				
                for(int i = 0; i < acctHistories.Length; i++)
				{
					AccountHistory acctHistory = (AccountHistory)acctHistories[i];

					System.Console.WriteLine("\tAccount ID: {0}", acctHistory.accountId );

					// Get History
					object[] histories = acctHistory.history;
					if(histories == null || histories.Length == 0)
					{
						System.Console.WriteLine("\t\tNo History");
					}
					else
					{
						System.Console.WriteLine("\t\t**History**");
						for(int j = 0; j < histories.Length; j++)
						{	
							LoanLoginAccountData llad = (LoanLoginAccountData) histories[j];
							System.Console.WriteLine("\t\tLoanLoginAccountData.loanAccountNumber: "+ llad.loanAccountNumber );
							System.Console.WriteLine("\t\tLoanLoginAccountData.loanAccountNumber: " + UtcToDateTime(llad.lastUpdated.Value) );
						
							// Loan
							object[] loans = llad.loans;
							if (loans == null || loans.Length == 0) 
							{
								System.Console.WriteLine("\t\tNo Loans.");
							}
							else 
							{
								System.Console.WriteLine("\t\t\t**Loan**");
								for (int u = 0; u < loans.Length; u++)
								{
									Loan loan = (Loan) loans[u];
									System.Console.WriteLine("\t\t\tLoan.accountName: " + loan.accountName );
									System.Console.WriteLine("\t\t\tLoan.accountNumber: " + loan.accountNumber );
									System.Console.WriteLine("\t\t\tLoan.interestRate: " + loan.interestRate );	

								}
							}
						}	
					}
				}
			}
			// end AccountHistory

            */
		}
		
	}
}
