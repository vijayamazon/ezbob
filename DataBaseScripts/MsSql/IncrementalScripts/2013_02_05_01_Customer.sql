UPDATE [dbo].[Customer]
   SET [WizardStep] = case when [IsSuccessfullyRegistered] = 1 then 4 end
go

UPDATE [dbo].[Customer]
   SET [WizardStep] = case when (SELECT count(*) FROM [dbo].[MP_CustomerMarketPlace] where customerid =[dbo].[Customer].id and marketPlaceId in (1, 2))>0 then 2
					       when (SELECT count(*) FROM [dbo].[MP_CustomerMarketPlace] where customerid =[dbo].[Customer].id and marketPlaceId = 3 )>0  then 3 end
where [IsSuccessfullyRegistered] = 0 
go


UPDATE [dbo].[Customer]
   SET [WizardStep] = 1
where  [WizardStep] is null 
go