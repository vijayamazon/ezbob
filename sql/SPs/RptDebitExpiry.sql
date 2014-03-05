IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDebitExpiry]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDebitExpiry]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptDebitExpiry] 
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT C.Id,C.Fullname,C.name,C.MobilePhone,C.DaytimePhone,P.CardNo,P.ExpireDateString,p.CardHolder
	FROM PayPointCard P, Customer C 
	WHERE C.Id = P.CustomerId 
	AND ExpireDate >= getdate() 
	AND ExpireDate < dateadd(m,1,getdate()) 
	AND C.Id IN (SELECT CustomerId 
				 FROM Loan 
				 WHERE Loan.Status <> 'PaidOff')
END
GO
