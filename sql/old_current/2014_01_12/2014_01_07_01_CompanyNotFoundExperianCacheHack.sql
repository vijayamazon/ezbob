IF NOT EXISTS (SELECT Id FROM MP_ExperianDataCache WHERE CompanyRefNumber='NotFound')
BEGIN 
INSERT INTO dbo.MP_ExperianDataCache (LastUpdateDate, JsonPacket, ExperianError, CompanyRefNumber)
	VALUES
	(
	 '2000-01-01'
	,'<GEODS>
		<REQUEST type="RETURN" success="N">
		  <ERR1>
		    <MESSAGE>Customer Clicked Company Not Found Button</MESSAGE>
		  </ERR1>
		</REQUEST>
	  </GEODS>'
	, 'Company Not Found'
	, 'NotFound'
	)
END 	
GO