IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCustAllData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCustAllData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptCustAllData
	@DateStart    DATETIME,
	@DateEnd      DATETIME
	
AS
BEGIN 

SET NOCOUNT ON;
 
DECLARE @tmp_sp1 TABLE
     					(CustomerId INT,
     					WizardStep INT,
                        NumOfStores INT,
                        HasPaypal VARCHAR(1),
                        AnualSales FLOAT,
                        OfferAmount FLOAT);
    
DECLARE @tmp_sp2 TABLE
                        (Id INT,
                        eMail NVARCHAR (128),
                        DateRegister DATETIME,
                        FirstName NVARCHAR (250),
                        SurName NVARCHAR (250),
                        DaytimePhone NVARCHAR (50),
                        MobilePhone NVARCHAR (50),
                        Shops INT,
                        MaxApproved DECIMAL (18, 0),
                        SumOfLoans NUMERIC (38, 0),
                        AnualTurnover INT,
                        ExpirianRating INT,
                        MedalType NVARCHAR (50),
                        PhoneOffer NUMERIC (13, 2));
INSERT INTO @tmp_sp1
EXECUTE RptCustAnualSales @DateStart,@DateEnd;
INSERT INTO @tmp_sp2
EXECUTE RptNewClientsFullEx @DateStart,@DateEnd;
               
SELECT 							T2.Id,
                                T2.eMail,
                                T2.DateRegister,
                                T2.FirstName,
                                T2.SurName,
                                T2.DaytimePhone,
                                T2.MobilePhone,
                                T1.NumOfStores,
                                T1.HasPaypal,
                                T2.MaxApproved,
                                T2.SumOfLoans,
                                T1.AnualSales,
                                T2.ExpirianRating,
                                T1.OfferAmount,
                                T1.WizardStep
                               
FROM  @tmp_sp1 T1
LEFT JOIN            @tmp_sp2 T2 ON T1.CustomerId = T2.Id
 
GROUP BY           T2.Id,T2.eMail,T2.DateRegister,T2.FirstName,T2.SurName,T2.DaytimePhone,T2.MobilePhone,
                   T1.NumOfStores,T1.HasPaypal,T2.MaxApproved,T2.SumOfLoans,T1.AnualSales,T2.ExpirianRating,
                   T1.OfferAmount,T1.WizardStep
                               
SET NOCOUNT OFF
                              
END;
GO
