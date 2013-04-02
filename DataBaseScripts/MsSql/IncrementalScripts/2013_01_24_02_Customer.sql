ALTER TABLE [Customer] ADD [WizardStep] [int] NULL;
go

UPDATE [dbo].[Customer]
   SET [WizardStep] = case when [IsSuccessfullyRegistered] = 1 then 3 end    
go

UPDATE [dbo].[Customer]
   SET [WizardStep] = case when (SELECT count(*) FROM [dbo].[MP_CustomerMarketPlace] where customerid =[dbo].[Customer].id and marketPlaceId in (1, 2))>0 then 1
					       when (SELECT count(*) FROM [dbo].[MP_CustomerMarketPlace] where customerid =[dbo].[Customer].id and marketPlaceId = 3 )>0  then 2 end
where [IsSuccessfullyRegistered] = 0 
go


UPDATE [dbo].[Customer]
   SET [WizardStep] = 0
where  [WizardStep] is null 
go
   