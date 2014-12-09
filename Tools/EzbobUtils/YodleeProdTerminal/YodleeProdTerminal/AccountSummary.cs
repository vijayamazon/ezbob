using System;
using System.Collections;
using com.yodlee.sampleapps.datatypes;
using System.Configuration;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Display User's Account Summary
	/// </summary>
	public class AccountSummary : ApplicationSuper
	{
		DataServiceService dataService;

		public AccountSummary()
		{
			dataService = new DataServiceService();
            dataService.Url = ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		public void displayAccountSummary (UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

            //// Create Credit Card Container Criteria
            //ContainerCriteria cc = new ContainerCriteria();
            //cc.dataExtent = dataExtent;
            //cc.containerType = ContainerTypes.CREDIT_CARD;

            //// Create Investment Container Criteria
            //ContainerCriteria invest = new ContainerCriteria();
            //invest.dataExtent = dataExtent;
            //invest.containerType = ContainerTypes.INVESTMENT;

			// Create Bank Container Criteria
			ContainerCriteria bank = new ContainerCriteria();
			bank.dataExtent = dataExtent;
			bank.containerType = ContainerTypes.BANK;

            //// Create Loan Container Criteria
            //ContainerCriteria loan = new ContainerCriteria();
            //loan.dataExtent = dataExtent;
            //loan.containerType = ContainerTypes.LOAN;

            //// Create Insurance Container Criteria
            //ContainerCriteria insur = new ContainerCriteria();
            //insur.dataExtent = dataExtent;
            //insur.containerType = ContainerTypes.INSURANCE;

            //// Create Bill Container Criteria
            //ContainerCriteria bill = new ContainerCriteria();
            //bill.dataExtent = dataExtent;
            //bill.containerType = ContainerTypes.BILL;

			// Create a list of Container Criteria
            object[] list = { bank/*, cc, invest, loan, insur, bill*/ };

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
                //String containerName =
                //    itemSummary.contentServiceInfo.containerInfo.containerName;

                //if(containerName.Equals(ContainerTypes.CREDIT_CARD))
                //{
                //    DisplayCardData displayCard = new DisplayCardData();
                //    displayCard.DisplayCardDataForItem(itemSummary);
                //}
                //else if(containerName.Equals(ContainerTypes.INSURANCE))
                //{
                //    DisplayInsuranceData displayInsurance = new DisplayInsuranceData();
                //    displayInsurance.displayInsuranceDataForItem(itemSummary);
                //}
                //else if(containerName.Equals(ContainerTypes.INVESTMENT))
                //{
                //    DisplayInvestmentData displayInvestment = new DisplayInvestmentData();
                //    displayInvestment.displayInvestmentDataForItem(itemSummary);
                //}
                //else if(containerName.Equals(ContainerTypes.BANK))
                //{
					DisplayBankData displayBank = new DisplayBankData();
					displayBank.displayBankDataForItem(itemSummary);
                //}
                //else if(containerName.Equals(ContainerTypes.BILL))
                //{
                //    DisplayBillsData displayBills = new DisplayBillsData();
                //    displayBills.displayBillsDataForItem(itemSummary);
                //}
                //else if(containerName.Equals(ContainerTypes.LOAN))
                //{
                //    DisplayLoanData displayLoan = new DisplayLoanData();
                //    displayLoan.displayLoanDataForItem(itemSummary);
                //}
                //else
                //{
                //    System.Console.WriteLine("Unsupported Container: "+ containerName );
                //}
				System.Console.WriteLine("");
			}
		}

	public void displayItemSummary (UserContext userContext, long itemId)
		{
			// Create Data Extent, Use the data extent as required for better performance int.MaxValue used only for the sample application.
            // Refer Javadocs for available Extent values for all containers

			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

            dataExtent.startLevelSpecified = true;
            dataExtent.endLevelSpecified = true;

			DataExtent da = new DataExtent();
			ItemSummary itemSummary = dataService.getItemSummaryForItem1(userContext, itemId, true, dataExtent );
			String containerName = itemSummary.contentServiceInfo.containerInfo.containerName;			
			if(containerName.Equals(ContainerTypes.CREDIT_CARD))
			{
				DisplayCardData displayCard = new DisplayCardData();
				displayCard.DisplayCardDataForItem(itemSummary);
			}
			else if(containerName.Equals(ContainerTypes.INSURANCE))
			{
				DisplayInsuranceData displayInsurance = new DisplayInsuranceData();
				displayInsurance.displayInsuranceDataForItem(itemSummary);
			}
			else if(containerName.Equals(ContainerTypes.INVESTMENT))
			{
				DisplayInvestmentData displayInvestment = new DisplayInvestmentData();
				displayInvestment.displayInvestmentDataForItem(itemSummary);
			}
			else if(containerName.Equals(ContainerTypes.BANK))
			{
				DisplayBankData displayBank = new DisplayBankData();
				displayBank.displayBankDataForItem(itemSummary);
			}
			else if(containerName.Equals(ContainerTypes.BILL))
			{
				DisplayBillsData displayBills = new DisplayBillsData();
				displayBills.displayBillsDataForItem(itemSummary);
			}
			else if(containerName.Equals(ContainerTypes.LOAN))
			{
				DisplayLoanData displayLoan = new DisplayLoanData();
				displayLoan.displayLoanDataForItem(itemSummary);
			}
			else
			{
				System.Console.WriteLine("Unsupported Container: "+ containerName );
			}
			System.Console.WriteLine("");

		}
	}	

}
