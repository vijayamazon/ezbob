INSERT INTO MP_PayPalTransactionItem2 (
	 TransactionId      
	, Created            
	, CurrencyId         
	, FeeAmount
	, GrossAmount
	, NetAmount
	, TimeZone           
	, Type               
	, Status             
	, PayPalTransactionId )
SELECT TransactionId      
	, i.Created            
	, c.Id         
	, i.FeeAmountAmount    
	, i.GrossAmountAmount  
	, i.NetAmountAmount    
	, i.TimeZone           
	, i.Type               
	, i.Status             
	, i.PayPalTransactionId
FROM MP_PayPalTransactionItem i
LEFT OUTER JOIN MP_Currency c
ON i.FeeAmountCurrency = c.Name


