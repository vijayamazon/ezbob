IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetPayPalActivity]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetPayPalActivity]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetPayPalActivity] 
	(@iUserId int)
AS
BEGIN
	Select   
-- Number of orders
cmp.Id as umi,
 aot.TransactionsNumber1M,
 aot.TransactionsNumber3M,
 aot.TransactionsNumber6M,
 aot.TransactionsNumber1Y,

 aot.TotalPayments1M,
 aot.TotalPayments3M,
 aot.TotalPayments3M,
 aot.TotalPayments1Y,
 
 aot.TotalTransactions1M,
 aot.TotalTransactions3M,
 aot.TotalTransactions6M,
 aot.TotalTransactions1Y



FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c ON cmp.CustomerId = c.Id


left join
(
 select PayPalOrderTable.CustomerMarketPlaceId,
  COUNT(distinct ppti1.Id) as TransactionsNumber1M,
  COUNT(distinct ppti2.Id)as TransactionsNumber3M,
  COUNT(distinct ppti3.Id)as TransactionsNumber6M,
  COUNT(distinct ppti4.Id)as TransactionsNumber1Y,
  SUM(ppti1.NetAmountAmount) as TotalPayments1M,
  SUM(ppti2.NetAmountAmount) as TotalPayments3M,
  SUM(ppti3.NetAmountAmount) as TotalPayments6M,
  SUM(ppti4.NetAmountAmount) as TotalPayments1Y,
  
  SUM(ppti5.NetAmountAmount) as TotalTransactions1M,
  SUM(ppti6.NetAmountAmount) as TotalTransactions3M,
  SUM(ppti7.NetAmountAmount) as TotalTransactions6M,
  SUM(ppti8.NetAmountAmount) as TotalTransactions1Y

 from
 (
  select ppt.CustomerMarketPlaceId,
   ppti.Id,
   ppti.NetAmountAmount,
   ppti.Created,
   ppti.Type,
      (select TableRate.Rate from dbo.MP_GetCurrencyRate1(ppti.NetAmountCurrency, ppti.Created) TableRate) as Rate
  from  MP_PayPalTransaction ppt
  left join MP_PayPalTransactionItem ppti on ppti.TransactionId = ppt.Id 
     
 ) as PayPalOrderTable
 left join MP_PayPalTransactionItem ppti1 on  ppti1.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 1, PayPalOrderTable.Created) >= GETDATE() and ppti1.Status = 'Completed'  and ppti1.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti2 on  ppti2.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 3, PayPalOrderTable.Created ) >= GETDATE() and ppti2.Status = 'Completed'  and ppti2.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti3 on  ppti3.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 6, PayPalOrderTable.Created ) >= GETDATE() and ppti3.Status = 'Completed'  and ppti3.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti4 on  ppti4.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 12, PayPalOrderTable.Created ) >= GETDATE() and ppti4.Status = 'Completed'  and ppti4.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti5 on  ppti5.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 1, PayPalOrderTable.Created) >= GETDATE() and ppti5.Status = 'Completed'  and ppti5.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti6 on  ppti6.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 3, PayPalOrderTable.Created ) >= GETDATE() and ppti6.Status = 'Completed'  and ppti6.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti7 on  ppti7.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 6, PayPalOrderTable.Created ) >= GETDATE() and ppti7.Status = 'Completed'  and ppti7.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti8 on  ppti8.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 12, PayPalOrderTable.Created ) >= GETDATE() and ppti8.Status = 'Completed'  and ppti8.Type = 'Transfer'
 group by CustomerMarketPlaceId
) as aot on aot.CustomerMarketPlaceId = cmp.id


WHERE c.Id = @iUserId
END
GO
