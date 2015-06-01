UPDATE EsignTemplates SET
		Template = CONVERT(VARBINARY(MAX), '<!DOCTYPE html>
<html>
<head>
	<meta charset="utf-8">
	<title>Board resolution</title>
</head>

<body style="width: 800px;">
<h2 style="text-align: center;">UNANIMOUS WRITTEN CONSENT OF THE BOARD OF DIRECTORS</h2>

<h4 style="text-align: center;">OF __COMPANY_NAME__ (the &ldquo;Company&rdquo;)</h4>

<p>The following, being all of the directors of the Company, acting by unanimous written consent, and having waived any rights to receive notice of a meeting:</p>

<p>Having discussed and noted that it is convenient and prudent for the business of the Company to, <u>from time to time</u>,
borrow from and enter into one or more loan agreements with Orange Money Ltd. (trading as EZBOB) and to appoint
a person who may act on behalf of the Company regarding said loans and agreements, including authority to complete
online applications do adopt the following written resolutions:</p>

<h3 style="text-align: center;"><u>IT WAS RESOLVED</u></h3>

<dl>
	<dt>FIRST
	<dd>That the Company may, from time to time borrow from and enter into one or more loan agreements with Orange Money Ltd.
	in connection with the Company&rsquo;s business operations and activities;

	<dt><br>SECOND
	<dd>That __CUSTOMER_NAME__ is authorised to, <u>from time to time</u>, sign individually for and on behalf of the Company
	and to transact any and all business with Orange Money Ltd., to borrow money with or without  security; to assign,
	transfer, pledge or otherwise charge property of the Company; including granting fixed and floating charges and charging of receivables.

	<dt><br>THIRD
	<dd>That the authority granted hereunder shall be valid and effective until such time as the Company,
	by written notice, advises Orange Money Ltd. that it has been revoked; provided, however no such
	revocation shall be deemed to affect or revoke any authority or consent to apply for and accept
	loans from Orange Money Ltd or enter into any agreements with respect to such loans,
	prior to the date such written notice is actually received by Orange Money Ltd.
</dl>

<p>Signed:</p>

<p>{{_es_signer_signatureblock}}</p>
</body>
</html>')
	WHERE EsignTemplateID = 1
GO
