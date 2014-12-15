SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AmazonAggregation') IS NULL
BEGIN
	CREATE TABLE AmazonAggregation (
		AmazonAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AverageItemsPerOrderDenominator INT NOT NULL,
		AverageItemsPerOrderNumerator INT NOT NULL,
		AverageSumOfOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderNumerator NUMERIC(18, 2) NOT NULL,
		CancelledOrdersCount INT NOT NULL,
		InventoryTotalItems INT NOT NULL,
		InventoryTotalValue NUMERIC(18, 2) NOT NULL,
		NumOfOrders INT NOT NULL,
		OrdersCancellationRate NUMERIC(18, 2) NOT NULL,
		TotalItemsOrdered INT NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_AmazonAggregation PRIMARY KEY (AmazonAggregationID),
		CONSTRAINT FK_AmazonAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('ChannelGrabberAggregation') IS NULL
BEGIN
	CREATE TABLE ChannelGrabberAggregation (
		ChannelGrabberAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AverageSumOfExpensesDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfExpensesNumerator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderNumerator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrdersDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrdersNumerator NUMERIC(18, 2) NOT NULL,
		NumOfExpenses INT NOT NULL,
		NumOfOrders INT NOT NULL,
		TotalSumOfExpenses NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ChannelGrabberAggregation PRIMARY KEY (ChannelGrabberAggregationID),
		CONSTRAINT FK_ChannelGrabberAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('EbayAggregation') IS NULL
BEGIN
	CREATE TABLE EbayAggregation (
		EbayAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AverageItemsPerOrderDenominator INT NOT NULL,
		AverageItemsPerOrderNumerator INT NOT NULL,
		AverageSumOfOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderNumerator NUMERIC(18, 2) NOT NULL,
		CancelledOrdersCount INT NOT NULL,
		InventoryTotalItems INT NOT NULL,
		InventoryTotalValue NUMERIC(18, 2) NOT NULL,
		NumOfOrders INT NOT NULL,
		OrdersCancellationRate NUMERIC(18, 2) NOT NULL,
		TopCategories NVARCHAR(MAX) NOT NULL,
		TotalItemsOrdered INT NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EbayAggregation PRIMARY KEY (EbayAggregationID),
		CONSTRAINT FK_EbayAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('EkmAggregation') IS NULL
BEGIN
	CREATE TABLE EkmAggregation (
		EkmAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AverageSumOfCancelledOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfCancelledOrderNumerator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrderNumerator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOtherOrderDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOtherOrderNumerator NUMERIC(18, 2) NOT NULL,
		CancellationRate NUMERIC(18, 2) NOT NULL,
		NumOfCancelledOrders INT NOT NULL,
		NumOfOrders INT NOT NULL,
		NumOfOtherOrders INT NOT NULL,
		TotalSumOfCancelledOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TotalSumOfOtherOrders NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EkmAggregation PRIMARY KEY (EkmAggregationID),
		CONSTRAINT FK_EkmAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('FreeAgentAggregation') IS NULL
BEGIN
	CREATE TABLE FreeAgentAggregation (
		FreeAgentAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		NumOfExpenses INT NOT NULL,
		NumOfOrders INT NOT NULL,
		SumOfAdminExpensesCatery NUMERIC(18, 2) NOT NULL,
		SumOfCostOfSalesExpensesCatery NUMERIC(18, 2) NOT NULL,
		SumOfDraftInvoices NUMERIC(18, 2) NOT NULL,
		SumOfGeneralExpensesCatery NUMERIC(18, 2) NOT NULL,
		SumOfOpenInvoices NUMERIC(18, 2) NOT NULL,
		SumOfOverdueInvoices NUMERIC(18, 2) NOT NULL,
		SumOfPaidInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfExpenses NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_FreeAgentAggregation PRIMARY KEY (FreeAgentAggregationID),
		CONSTRAINT FK_FreeAgentAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('HmrcAggregation') IS NULL
BEGIN
	CREATE TABLE HmrcAggregation (
		HmrcAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AverageSumOfExpensesDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfExpensesNumerator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrdersDenominator NUMERIC(18, 2) NOT NULL,
		AverageSumOfOrdersNumerator NUMERIC(18, 2) NOT NULL,
		NumOfExpenses INT NOT NULL,
		NumOfOrders INT NOT NULL,
		TotalSumOfExpenses NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_HmrcAggregation PRIMARY KEY (HmrcAggregationID),
		CONSTRAINT FK_HmrcAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('PayPalAggregation') IS NULL
BEGIN
	CREATE TABLE PayPalAggregation (
		PayPalAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AmountPerTransferIn NUMERIC(18, 2) NOT NULL,
		AmountPerTransferOut NUMERIC(18, 2) NOT NULL,
		GrossIncome NUMERIC(18, 2) NOT NULL,
		GrossProfitMargin NUMERIC(18, 2) NOT NULL,
		NetNumOfRefundsAndReturns INT NOT NULL,
		NetSumOfRefundsAndReturns NUMERIC(18, 2) NOT NULL,
		NetTransfersAmount NUMERIC(18, 2) NOT NULL,
		NumOfTotalTransactions INT NOT NULL,
		NumTransfersIn INT NOT NULL,
		NumTransfersOut INT NOT NULL,
		OutstandingBalance NUMERIC(18, 2) NOT NULL,
		RatioNetSumOfRefundsAndReturnsToNetRevenues NUMERIC(18, 2) NOT NULL,
		RevenuePerTrasnaction NUMERIC(18, 2) NOT NULL,
		RevenuesForTransactions NUMERIC(18, 2) NOT NULL,
		TotalNetExpenses NUMERIC(18, 2) NOT NULL,
		TotalNetInPayments NUMERIC(18, 2) NOT NULL,
		TotalNetInPaymentsAnnualized NUMERIC(18, 2) NOT NULL,
		TotalNetOutPayments NUMERIC(18, 2) NOT NULL,
		TotalNetRevenues NUMERIC(18, 2) NOT NULL,
		TransactionsNumber INT NOT NULL,
		TransferAndWireIn NUMERIC(18, 2) NOT NULL,
		TransferAndWireOut NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_PayPalAggregation PRIMARY KEY (PayPalAggregationID),
		CONSTRAINT FK_PayPalAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('PayPointAggregation') IS NULL
BEGIN
	CREATE TABLE PayPointAggregation (
		PayPointAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		CancellationRate NUMERIC(18, 2) NOT NULL,
		CancellationValue NUMERIC(18, 2) NOT NULL,
		NumOfFailures INT NOT NULL,
		NumOfOrders INT NOT NULL,
		OrdersAverageDenominator NUMERIC(18, 2) NOT NULL,
		OrdersAverageNumerator NUMERIC(18, 2) NOT NULL,
		SumOfAuthorisedOrders NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_PayPointAggregation PRIMARY KEY (PayPointAggregationID),
		CONSTRAINT FK_PayPointAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('SageAggregation') IS NULL
BEGIN
	CREATE TABLE SageAggregation (
		SageAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		NumOfExpenditures INT NOT NULL,
		NumOfIncomes INT NOT NULL,
		NumOfOrders INT NOT NULL,
		NumOfPurchaseInvoices INT NOT NULL,
		TotalSumOfExpenditures NUMERIC(18, 2) NOT NULL,
		TotalSumOfIncomes NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrders NUMERIC(18, 2) NOT NULL,
		TotalSumOfOrdersAnnualized NUMERIC(18, 2) NOT NULL,
		TotalSumOfPaidPurchaseInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfPaidSalesInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfPartiallyPaidPurchaseInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfPartiallyPaidSalesInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfPurchaseInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfUnpaidPurchaseInvoices NUMERIC(18, 2) NOT NULL,
		TotalSumOfUnpaidSalesInvoices NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_SageAggregation PRIMARY KEY (SageAggregationID),
		CONSTRAINT FK_SageAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

IF OBJECT_ID('YodleeAggregation') IS NULL
BEGIN
	CREATE TABLE YodleeAggregation (
		YodleeAggregationID BIGINT IDENTITY NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		Turnover NUMERIC(18, 2) NOT NULL,
		AvailableBalance NUMERIC(18, 2) NOT NULL,
		CurrentBalance NUMERIC(18, 2) NOT NULL,
		NumberOfTransactions INT NOT NULL,
		TotalExpense NUMERIC(18, 2) NOT NULL,
		TotalIncomeAnnualized NUMERIC(18, 2) NOT NULL,
		TotlaIncome NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_YodleeAggregation PRIMARY KEY (YodleeAggregationID),
		CONSTRAINT FK_YodleeAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO
