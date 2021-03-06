SET QUOTED_IDENTIFIER ON
GO

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

	UPDATE EsignTemplates SET
		Template = CONVERT(VARBINARY(MAX), '<!DOCTYPE html>
<html>
<head>
	<meta charset="utf-8">
	<title>Personal guarantee</title>
</head>

<body style="width: 800px;">
<h2 style="text-align: center;">GUARANTEE AND INDEMNITY</h2>

<p>THIS GUARANTEE is made and entered into on __CURRENT_DATE__.</p>

<p>In consideration of Orange Money Ltd. of 145&ndash;157, St John Street, London, EC1V 4PY,
COMPANY NUMBER 7852687, (&ldquo;the Creditor&rdquo;) entering into the Loan Agreement, and __GUARANTOR_DETAILS__
(&ldquo;the Guarantor&rdquo;) hereby unconditionally and irrevocably and
where there is more than one Guarantor, jointly and severally, undertakes and guarantees to the
Creditor the due and punctual performance and observance by the Principal Debtor of all its
obligations, commitments, undertakings, warranties, indemnities and covenants under or pursuant
to the Loan Agreement and the due and punctual payment of all sums now or subsequently payable
by the Principal Debtor to the Creditor under the Loan Agreement, when the same shall become
due and agrees to indemnify the Creditor against all losses, damages, costs and expenses (including
legal costs and expenses) which the Creditor may incur or suffer through or arising from any breach
by the Principal Debtor of such obligations, commitments, warranties, undertakings, indemnities or
covenants. If the Principal Debtor shall default in the payment of any sum under the Loan
Agreement, the Guarantor will forthwith on demand by the Creditor pay such sum to the Creditor.
Such guarantee and undertaking is a continuing guarantee and undertaking and shall remain in
force until all the obligations of the Principal Debtor under the Loan Agreement have been fully
performed and all sums payable to the Creditor have been fully paid. The Guarantor agrees and
accepts all obligations set out above and detailed below.</p>

<p>1 INTERPRETATION</p>

<p>1.1 Unless the context otherwise requires or unless otherwise defined in this Guarantee, words and
expressions shall have the same respective meanings that are ascribed to them in the Loan Agreement.</p>

<p>1.2 In this Guarantee:</p>

<dl>
	<dt>&ldquo;Loan Agreement&rdquo;
	<dd>means that certain Loan Agreement between __COMPANY_NAME__ as borrower,
	and Creditor, as lender, dated as of the date of this Guarantee, with respect
	to a loan in the original principal amount of __LOAN_AMOUNT__ (&ldquo;Loan&rdquo;).

	<dt>&ldquo;Principal Debtor&rdquo;
	<dd>means __COMPANY_NAME__

	<dt>&ldquo;Principal Debtor&rsquo;s Obligations&rdquo;
	<dd>means all monies and liabilities which are now or at any
	time after the date of this Guarantee advanced to, becoming due from, or owing or incurred
	by, the Principal Debtor to Creditor under or in connection with the Loan Agreement.
</dl>

<p>1.3 In this Guarantee:</p>

<p>1.3.1 references to this Guarantee are to include the indemnity in clause 3.3.</p>

<p>1.3.2 references to this Guarantee or any provisions of this Guarantee or to the Loan Agreement
or any other document or agreement are to be construed as references to this Guarantee, that
Loan Agreement or those provisions or that document or agreement as is in force for the
time being and as amended, varied, supplemented, substituted or novated from time to time.</p>

<p>1.3.3 references to any person are to be construed to include that person&rsquo;s assigns or transferees
or successors in title, whether direct or indirect.</p>

<p>2 REPRESENTATIONS AND WARRANTIES</p>

<p>The Guarantor warrants, represents and undertakes to the Creditor (such warranties, representations
and undertakings to continue so long as this Guarantee remains subsisting) that:</p>

<p>2.1 It is a director, shareholder, officer, member and/or otherwise has a beneficial interest (directly
or indirectly) in Principal Debtor and the making of the Loan by Creditor and the acceptance of the
Loan by Principal Debtor will confer a benefit (direct or indirect) on Guarantor; and</p>

<p>2.2 It has had an opportunity to review the Loan Agreement and understands the obligations of
Principal Debtor thereunder to repay the Loan, along with interest thereon and any other fees,
charges or amounts in accordance with the terms of the Loan Agreement.</p>

<p>3 GUARANTEE AND INDEMNITY</p>

<p>3.1 The Guarantor irrevocably and unconditionally guarantees to the Creditor the payment on
demand, of the Principal Debtor&rsquo;s Obligations.</p>

<p>3.2 If the Principal Debtor&rsquo;s Obligations are not recoverable from the Principal Debtor by reason
of illegality, incapacity, the lack or exceeding of powers, ineffectiveness of execution,
defectiveness or omission of any required internal procedure or any other reason, the Guarantor
shall notwithstanding any of the foregoing be liable under this Guarantee for the Principal Debtor&rsquo;s
Obligations as if it were a principal debtor.</p>

<p>3.3 The Guarantor, as principal obligor and as a separate and independent obligation and liability
from its obligations and liabilities under clause 3.1, irrevocably and unconditionally agrees to
indemnify the Creditor in full on demand against all losses, costs and expenses suffered or incurred
by the Creditor arising from or in connection with from the failure by the Principal Debtor to make
due and punctual payment of the Principal Debtor''s Obligations (or any part thereof) or resulting
from any of the Principal Debtor''s Obligations being or becoming void, voidable, unenforceable or
ineffective against the Principal Debtor.</p>

<p>4 CREDITOR PROTECTIONS</p>

<p>4.1 Guarantor acknowledges and agrees that this Guarantee is and at all times shall be a continuing
security and shall extend to cover the ultimate balance due at any time from the Principal Debtor to
the Creditor under or in connection with the Principal Debtor&rsquo;s Obligations.</p>

<p>4.2 The Guarantor acknowledges and agrees that none of its liabilities under this Guarantee shall
be reduced, discharged or otherwise adversely affected by:</p>

<p>4.2.1 any variation, extension, discharge, compromise, dealing with, exchange or renewal of any
right or remedy which the Creditor may now or after the date of this Guarantee have from
or against the Principal Debtor or any other person in respect of the Principal Debtor&rsquo;s Obligations;</p>

<p>4.2.2 any act or omission by the Creditor or any other person in taking up, perfecting or enforcing
any security or guarantee from or against the Principal Debtor or any other person;</p>

<p>4.2.3 any administration, insolvency, bankruptcy, liquidation, winding-up, incapacity, limitation,
disability, the discharge by operation of law and any change in the constitution, name and
style of the Principal Debtor or any other person;</p>

<p>4.2.4 any termination, amendment, variation, novation or supplement of or to any of the Principal
Debtor&rsquo;s Obligations;</p>

<p>4.2.5 any grant of time, indulgence, waiver or concession to the Principal Debtor or any other person;</p>

<p>4.2.6 any act or omission which would not have discharged or affected the liability of the
Guarantor had it been a principal debtor instead of guarantor or indemnitor or by anything
done or omitted by any person which but for this provision might operate to exonerate or
discharge the Guarantor or otherwise reduce or extinguish its liability under this Guarantee;</p>

<p>4.2.7 any invalidity, illegality, unenforceability, irregularity of, or any defect in, any provision of
the Loan Agreement or this Guaranty, including without limitation any claim by Guarantor,
Principle Obligor or any other person that the Loan Agreement and Principle Obligor&rsquo;s
obligations thereunder were agreed to and/or executed in the absence or proper corporate
power or authority</p>

<p>4.3 The Creditor shall not be obliged before taking steps to enforce any of its rights and remedies
under this Guarantee to take any action or obtain judgment against the Principal Debtor or any
other person.</p>

<p>4.4 The Guarantor warrants to the Creditor that it has not taken or received, and shall not take,
exercise or receive, the benefit of any security or other right or benefit from or exercise any right
against the Principal Debtor, its liquidator, an administrator, a co-guarantor or any other person in
connection with any liability of, or payment by, the Guarantor under this Guarantee.</p>

<p>4.5 If the Guarantor is in breach of clause 4.4 any security or other right or benefit obtained from
the Principal Debtor or any other person shall be held upon trust to transfer or pay them to the
Creditor to the extent necessary to satisfy any liability of the Guarantor under this Guarantee.</p>

<p>5 SUSPENSE ACCOUNT</p>

<p>The Creditor may place to the credit of a suspense account any monies received under or in
connection with this Guarantee and may, at any time, apply any of those monies in or towards
satisfaction of any of the Guarantor&rsquo;s liabilities under clause 3 as the Creditor, in its absolute
discretion, may from time to time determine.</p>

<p>6 APPROPRIATION</p>

<p>The Guarantor shall not direct the application by the Creditor of any sums received by the Creditor
from the Guarantor under this Guarantee.</p>

<p>7 DISCHARGE TO BE CONDITIONAL</p>

<p>7.1 Any release, discharge or settlement between the Guarantor and the Creditor in relation to this
Guarantee shall be conditional upon no right, security, disposition or payment to the Creditor by the
Guarantor, the Principal Debtor or any other person being avoided, set aside or ordered to be
refunded pursuant to any enactment or law relating to breach of duty by any person, bankruptcy,
liquidation, administration, the protection of creditors or insolvency or for any other reason.</p>

<p>7.2 If any such right, security, disposition or payment is avoided, set aside or ordered to be
refunded, the Creditor shall be entitled subsequently to enforce this Guarantee against the Guarantor
as if such release, discharge or settlement had not occurred and any such security,
disposition or payment had not been made.</p>

<p>8 PAYMENTS AND TAXES</p>

<p>8.1 All sums payable by the Guarantor under this Guarantee shall be paid to the Creditor in full:</p>

<p>8.1.1 without any set-off, condition or counterclaim whatsoever; and</p>

<p>8.1.2 free and clear of any deduction or withholding whatsoever except only as may be required
by law or regulation which in either case is binding on it.</p>

<p>8.2 If any deduction or withholding is required by any law or regulation (whether or not that
regulation has the force of law) in respect of any payment due from the Guarantor under this
Guarantee, the sum payable by the Guarantor shall be increased so that, after making the minimum
deduction or withholding so required, the Guarantor shall pay to the Creditor, and the Creditor shall
receive and be entitled to retain on the due date for payment, a net sum at least equal to the sum
which it would have received had no such deduction or withholding been required to be made.</p>

<p>9 DEMANDS AND NOTIFICATION BINDING</p>

<p>Any demand, notification or certificate given by the Creditor specifying amounts due and payable
under or in connection with any of the provisions of this Guarantee shall, in the absence of manifest
error, be conclusive and binding on the Guarantor.</p>

<p>10 COSTS</p>

<p>The Guarantor shall, on demand by the Creditor and on a full indemnity basis, pay to the Creditor
the amount of all reasonable costs and expenses substantiated by invoices which the Creditor incurs
under or in connection with any enforcement of this Guarantee.</p>

<p>11 SET-OFF</p>

<p>The Creditor may, without notice to the Guarantor, apply any credit balance which is at any time
held by any office or branch of the Creditor for the account of the Guarantor in or towards
satisfaction of any sum then due and payable from the Guarantor under this Guarantee.</p>

<p>12 ELECTRONIC TRANSMISSION OF DOCUMENTS, ELECTRONIC SIGNATURE &amp; COMMUNICATIONS</p>

<p>12.1 You agree that we may deliver documents to you via electronic means and that the email
addresses you provide to facilitate this is correct. You may email documents to us at
<a href="mailto:support@ezbob.com">support@ezbob.com</a>. Facsimile, or pdf signed copies of this Guarantee or those sent by other
electronic means shall be deemed originals.</p>

<p>12.2 Any demand or notice under this Guarantee shall be in writing signed by an officer or agent
of the Creditor and (without prejudice to any other effective means of serving it) may be served on
the Guarantor personally or by post or by electronic means and either by delivering it to any officer
of the Guarantor at any place or by despatching it addressed to the Guarantor at the Guarantor&rsquo;s
registered or principal office for the time being or a place of business of the Guarantor, or at the last
email address last known to the Creditor.</p>

<p>12.3 Any such demand or notice sent by post shall be deemed to have been received at the opening
of business in the intended place of receipt on the second business day following the day on which
it was posted, even if returned undelivered. Any such demand or notice delivered by email or fax
shall be deemed to have been delivered at the time the email leaves the server of the sender, if
during normal business hours in the intended place of receipt on a business day in that place and
otherwise at the opening of business in that place on the next succeeding business day.</p>

<p>13 LAW AND JURISDICTION</p>

<p>This Guarantee is governed by and shall be construed in accordance with the law of England and
Wales And the Creditor and the Guarantor(s) submit to the non-exclusive jurisdiction of the
English courts, unless you live in Scotland, Northern Ireland, the Channel Islands or the Isle of
Man, in which case you will be entitled to commence legal proceedings in your local courts.</p>

<p>Signed:</p>

<p>GUARANTOR:</p>

<p>{{_es_signer_signatureblock}}</p>
</body>
</html>')
	WHERE EsignTemplateID = 2
GO
