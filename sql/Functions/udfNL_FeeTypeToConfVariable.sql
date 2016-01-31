SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfNL_FeeTypeToConfVariable') IS NOT NULL
	DROP FUNCTION dbo.udfNL_FeeTypeToConfVariable
GO

CREATE FUNCTION udfNL_FeeTypeToConfVariable(@LoanFeeTypeID int)
RETURNS int
AS
BEGIN
	
	declare @ConVarID int;
	
	if @LoanFeeTypeID=2 	
		set @ConVarID= 555; --	SpreadSetupFeeCharge  555=2
		
	if @LoanFeeTypeID=4 	
		set @ConVarID= 5;	--RolloverCharge 5= 4

	if @LoanFeeTypeID=5 	
		set @ConVarID= 7; --AdministrationCharge 7=5

	if @LoanFeeTypeID=6 	
		set @ConVarID= 4; --LatePaymentCharge 4=6

	if @LoanFeeTypeID=8 	
		set @ConVarID= 23; --	OtherCharge 23=8

	if @LoanFeeTypeID=7 	
		set @ConVarID= 6; --PartialPaymentCharge 6=7
		
	return @ConVarID;
	
	--SpreadSetupFeeCharge  555=2
	--RolloverCharge 5= 4
	--AdministrationCharge 7=5
	--LatePaymentCharge 4=6
	--OtherCharge 23=8
	--PartialPaymentCharge 6=7
END
GO

