DECLARE @PreContractTemplateContent NVARCHAR(MAX)
SET @PreContractTemplateContent = 
'<center>
    <h3>
        PRE-CONTRACT CREDIT INFORMATION
    </h3>
    <h3>
        (Standard European Consumer Credit Information)
    </h3>
</center>
<p class="head">
    1. Contract Details</p>
<table  border="1"   class="table-contract-agreement">
    <tr>
        <td width="50%">
            Creditor:
        </td>
        <td width="50%">
            Orange Money Ltd 145-157 St John Street London EC1V 4PW
        </td>
    </tr>
    <tr>
        <td>
            Date:
        </td>
        <td>
            {{CurentDate}}
        </td>
    </tr>
</table>
<p class="head" style="margin-top:15px;">
    2. Key features of the credit product
</p>
<table  border="1"  class="table-contract-agreement">
    <tr>
        <td width="50%">
            The type of credit
        </td>
        <td width="50%">
            Fixed Sum Loan 
        </td>
    </tr>
    <tr>
        <td>
            The total amount of credit.
        </td>
        <td>
            Loan Amount (including set-up fee) {{TotalPrincipal}}
        </td>
    </tr>
    <tr>
        <td>
            Set up fee
        </td>
        <td>
            {{SetupFee}}
        </td>
    </tr>
    <tr>
        <td>
            This means the amount of credit to be provided under the proposed credit agreement or the credit limit.
        </td>
        <td>
            Total amount of credit {{TotalPrincipal}}
        </td>
    </tr>
    <tr>
        <td>
            How and when credit would be provided
        </td>
        <td>
            {{TotalPrincipalWithSetupFee}} will be deposited into the bank account you registered with us
            (your Primary Payment Account), or other account we have allowed you to nominate
            (your Alternative Payment Account), in most cases on the day you enter into the
            agreement with us.
        </td>
    </tr>
    <tr>
        <td>
            The duration of the credit agreement.
        </td>
        <td>
            {{Term}} months
        </td>
    </tr>
    <tr>
        <td>
            Repayments. Your repayments will pay off what you owe in the following order.
            
                <p style="text-align: justify">The amount to be repaid in each of the first {{TermOnlyInterestWords}} ({{TermOnlyInterest}}) scheduled payments is comprised of interest only and does not include a repayment of a portion of the credit advanced.  The following {{TermInterestAndPrincipalWords}} ({{TermInterestAndPrincipal}}) repayments include both interest and repayment of the credit advanced to you.</p>
            {{#isHalwayLoan}}
                <p style="text-align: justify">The amount to be repaid in each of the first {{TermOnlyInterestWords}} ({{TermOnlyInterest}}) scheduled payments is comprised of interest only and does not include a repayment of a portion of the credit advanced.  The following {{TermInterestAndPrincipalWords}} ({{TermInterestAndPrincipal}}) repayments include both interest and repayment of the credit advanced to you.</p>
            {{/isHalwayLoan}}
        </td>
        <td>
            You will pay to us {{TotalAmount}} by way of {{Term}} monthly(s) repayments and in accordance with the repayment schedule set out below: 
            {{#FormattedSchedules}}
                <p  style="text-align: justify" >{{StringNumber}} Payment {{AmountDue}},   {{Date}}</p>
            {{/FormattedSchedules}}

          
        </td>
    </tr>
    <tr>
        <td>
            The total amount you will have to pay. This means the amount you have borrowed plus
            interest and other costs.
        </td>
        <td>
            {{TotalAmount}}
        </td>
    </tr>
    <tr>
        <td>
            Security required. This is a description of the security to be provided by you in
            relation to the credit agreement.
        </td>
        <td>
            You must give us direct debit card details for us to collect your repayment and
            continuous authority to do so. This means that if we cannot collect the money you
            owe us on the scheduled repayment date, we can make further attempts to debit your
            Primary Payment Account and/or your Alternative Payment Account in order to collect
            the money, either in one payment or in several amounts, on the scheduled repayments
            dates or subsequent dates, on a continuous basis until the amount you owe is repaid.
        </td>
    </tr>
</table>
<br/>
<p class="head"  style="margin-top:15px;">
    3. Costs of the credit</p>
<table  border="1"  class="table-contract-agreement">
    <tr>
        <td width="50%">
            The rates of interest which apply to the credit agreement
        </td>
        <td width="50%">
            {{#FormattedSchedules}}
            <p  style="text-align: justify" > MONTH {{Iterration}}: Rate of interest {{InterestRate}}% per month. Fixed </p>
            {{/FormattedSchedules}}

        </td>
    </tr>
    <tr>
        <td>
            Annual Percentage Rate of Charge (APR). This is the total cost expressed as an annual
            percentage of the total amount of credit. The APR is there to help you compare different
            offers.
        </td>
        <td>
            <p  style="text-align: justify" > Total Loan APR: {{APR}}% </p>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            Related costs
        </td>
      
    </tr>
    <tr>
        <td>
            Other costs deriving from the credit agreement
        </td>
        <td>
            We charge a set-up fee equal to
            {{#IsManualSetupFee}} {{ManualSetupFee}} {{/IsManualSetupFee}}
            {{^IsManualSetupFee}}
                {{#IsBrokerFee}} {{SetupFee}} {{/IsBrokerFee}}
                {{^IsBrokerFee}} {{SetupFeePercent}}% of the loan amount (but in no case less than {{SetupFeeAmount}}){{/IsBrokerFee}}
            {{/IsManualSetupFee}}
             and this is deducted from the loan amount actually deposited in your bank account;
             we charge you interest on the entire loan amount (including the set-up fee).
        </td>
    </tr>
    <tr>
        <td>
            Costs in the case of late payments
        </td>
        <td>
            <p>
                If you fail to repay us in full on the scheduled repayment date you will be charged
                a fee of £40 (“missed payment fee”). This fee will be charged on each occasion that
                you fail to make the required payment on your scheduled repayment date.
            </p>
            <p>
                In the event you fail to make two or more of the scheduled repayments, and you owe
                an amount greater than the sum of two scheduled repayments, you will, in addition
                to the missed payment fee, be charged a fee of £50.00 (“default fee”).
            </p>
            <p>
                Also if you do not repay your loan in full and on time we will continue to charge
                interest on the outstanding credit balance. The rate of interest will be the same
                as shown above for the final month. Interest after default is calculated daily and applied to the outstanding
                balance every calendar month (or when the outstanding balance is repaid in full
                if earlier).
            </p>
        </td>
    </tr>
    <tr>
        <td>
            Consequences of missing payments
        </td>
        <td>
            If we cannot collect the money you owe us on the scheduled repayment date, you authorise
            us to can make further attempts to debit your Primary Payment Account and/or your
            Alternative Payment Account in order to collect the money, either in one payment
            or in several amounts, on a continuous basis until the amount you owe is repaid.
            Missing payments could have further severe consequences, including legal proceedings,
            and could make obtaining credit in the future more difficult.
        </td>
    </tr>
</table>
<br/>
<p class="head" style="margin-top:15px;">
    4. Other important legal aspects</p>
<table  border="1"  class="tabbable table-contract-agreement">
    <tr>
        <td width="50%">
            Right of withdrawal.
        </td>
        <td width="50%">
            You have the right to withdraw from the credit agreement before the end of the period
            of 14 days beginning from the day after you receive a copy of the agreement.
        </td>
    </tr>
    <tr>
        <td>
            Early repayment.
        </td>
        <td>
            You have the right to repay the credit early at any time in full or partially and
            there will be no additional charge or penalty in the event you do repay early.
        </td>
    </tr>
    <tr>
        <td>
            Consultation with a Credit Reference Agency(s).
        </td>
        <td>
            If we decide not to proceed with a prospective regulated consumer credit agreement
            on the basis of information from a credit reference agency we must, when informing
            you of the decision, inform you that it has been reached on the basis of information
            from a credit reference agency and of the particulars of that agency.
        </td>
    </tr>
    <tr>
        <td>
            Right to a draft credit agreement.
        </td>
        <td>
            You have the right, upon request, to obtain a copy of the draft credit agreement
            free of charge, unless at the time of your request we are unwilling to proceed to
            the conclusion of the credit agreement.
        </td>
    </tr>
    <tr>
        <td>
            The period of time during which the creditor is bound by the pre-contractual information.
        </td>
        <td>
            This information is valid from the date of receipt of this document until 24 hours
            after.
        </td>
    </tr>
</table>
<br/>
<p class="head" style="margin-top:15px;">
    5. Additional information in the case of distance marketing of financial services</p>
<table  border="1"  class="table-contract-agreement">
    <tr>
        <td colspan="2" width="50%">
            (a) concerning the creditor
        </td>
    </tr>
    <tr>
        <td>
            Registration number.
        </td>
        <td>
            We are authorised and regulated by the Financial Conduct Authority. Our Interim Permissions Reference Number is 647816. Company registration number 7852687.
        </td>
    </tr>
    <tr>
        <td>
            The supervisory authority.
        </td>
        <td>
            Financial Conduct Authority, 25 The North Colonnade, Canary Wharf, London E14 5HS.
        </td>
    </tr>
    <tr>
        <td colspan="2">
            (b) concerning the credit agreement
        </td>
    </tr>
    <tr>
        <td style ="border-bottom: 0px none"><p>The law taken by the creditor as a basis for the establishment of relations with you before the conclusion of the credit agreement.</p></td>
        <td style ="border-bottom: 0px none"><p>The laws of the United Kingdom (and English law in particular) are taken by the
            creditor as the basis for the establishment of relations with you prior to the conclusion
            of the distance credit agreement and is the law applicable to the credit agreement.</p></td>
    </tr>
    
    <tr>
        <td style ="border-top:0px none;  border-bottom: 0px none" ><p>The law applicable to the credit agreement and/or the competent court.</p></td>
        <td style ="border-top:0px none;  border-bottom: 0px none"><p>We and you submit to the non-exclusive jurisdiction of the English courts, unless
                you live in Scotland, Northern Ireland, the Channel Islands or the Isle of Man,
                in which case you will be entitled to commence legal proceedings in your local courts.</p></td>
    </tr>
    
    <tr >
        <td style ="border-top:0px none; "><p>If applicable Language to be used in connection with the credit agreement.</p></td>
        <td style ="border-top:0px none; "><p>All information and contractual terms will be in the English language.</p></td>
    </tr>
    
    <tr>
        <td colspan="2">
            (c) concerning redress
        </td>
    </tr>
    <tr>
        <td>
            Access to out-of-court complaint and redress mechanism.
        </td>
        <td>
            Complaints can be made directly to us via email to <a href="mailto:support@ezbob.com"
                                                                  target="_blank">support@ezbob.com</a> or to the Consumer Credit Trade Association
            (“CCTA”), you may phone them on 01274 714959 via their web-site <a href="http://www.ccta.co.uk"
                                                                               target="_blank">www.ccta.co.uk</a> or to the Financial Services Ombudsman via
            email to <a href="mailto:complaint.info@financial-ombudsman.org.uk" target="_blank">
                         complaint.info@financial-ombudsman.org.uk</a>
        </td>
    </tr>
</table>'

IF NOT EXISTS (SELECT 1 FROM LoanAgreementTemplate WHERE Template = @PreContractTemplateContent AND TemplateType = 2)
BEGIN
	INSERT INTO LoanAgreementTemplate
	(
		Template, 
		TemplateType
	)
	VALUES
	(
		@PreContractTemplateContent, 
		2
	)
END
GO





DECLARE @CreditActTemplateContent NVARCHAR(MAX)
SET @CreditActTemplateContent = 
'<h4>
    Fixed Sum Loan Agreement Regulated by the Consumer Credit Act 1974</h4>
<p>
    This agreement is between:</p>
<p>
    Orange Money Ltd (“We”, “Us”, “Our”, “the Creditor”) of 145-157, St John Street,
    London, EC1V 4PW, and</p>
<p >
    {{FullName}} (“You, “Your”, “the Borrower”)
    </p>
<p>
    {{PersonAddress}}, {{CustomerEmail}}
</p>
<p  style="text-align: justify" >Loan Amount (including set-up fee) {{TotalPrincipal}}</p>
<p  style="text-align: justify" >Set-up fee {{SetupFee}}</p>
<p  style="text-align: justify" >Total amount of credit {{TotalPrincipal}}</p>
<p  style="text-align: justify" >
    The loan amount, less the set-up fee, will be deposited into the bank account you registered with us (your
    Primary Payment Account), or other account we have allowed you to nominate (your
    Alternative Payment Account), in most cases on the day you enter into the agreement
</p>
<p  style="text-align: justify" >This Agreement will be of {{Term}} months duration.</p>

<p  style="text-align: justify" >Repayment Schedule</p>

{{#FormattedSchedules}}
    <p  style="text-align: justify" >{{StringNumber}} Payment {{AmountDue}},   {{Date}}</p>
{{/FormattedSchedules}}

{{#isHalwayLoan}}
    <p style="text-align: justify">The amount to be repaid in each of the first {{TermOnlyInterestWords}} ({{TermOnlyInterest}}) scheduled payments is comprised of interest only and does not include a repayment of a portion of the credit advanced.  The following {{TermInterestAndPrincipalWords}} ({{TermInterestAndPrincipal}}) repayments include both interest and repayment of the credit advanced to you.</p>
{{/isHalwayLoan}}

<p  style="text-align: justify" >Total Amount to be repaid  {{TotalAmount}}</p>

<p  style="text-align: justify;" >Total Loan APR: {{APR}}%</p>

<p  style="text-align: justify" >Interest Rate</p>
            {{#FormattedSchedules}}
            <p  style="text-align: justify; margin-left: 30px;" > MONTH {{Iterration}}: Rate of interest {{InterestRate}}% per month. Fixed </p>
            {{/FormattedSchedules}}


<p  style="text-align: justify" >
    Repayments under this Agreement will be collected on the scheduled repayment dates
    set out above, by debiting one of the debit cards you have registered with us. If
    payment is not available for collection by us in full on the scheduled repayment
    date a fee of £40.00 will be charged on each scheduled repayment date where the
    entire scheduled payment is not received by us (“missed payment fee”) and will be
    added you your outstanding balance, and interest (at the applicable rate) will continue to be applied to
    the total outstanding credit balance. See Clause 12 for more information.
</p>
<p  style="text-align: justify" >
    You must give us direct debit card details for us to collect your repayment and
    continuous authority to do so. See Clause 10 for more information.
</p>
<p  style="text-align: justify" >
    You have the right to receive, on request, and free of charge, at any time throughout
    the duration of the agreement pursuant to section 77B of the Consumer Credit Act
    1974 (“the 1974 Act”), a statement in the form of a table showing-
</p>
<dl>
    <dd>
        <p  style="text-align: justify; margin-left: 30px;" >a) The details of each repayment owing under the agreement;</p>
    </dd>
    <dd>
        <p  style="text-align: justify; margin-left: 30px;" >b) The date on which each repayment is due, the amount and any conditions relatingto the repayment;</p>
    </dd>
    <dd>
        <p  style="text-align: justify; margin-left: 30px;" >c) A breakdown of each repayment showing how much comprises –</p>
        <dl>
            <dd>
                <p  style="text-align: justify; margin-left: 30px;" >i) capital repayment,</p>
            </dd>
            <dd>
                <p  style="text-align: justify; margin-left: 30px;" >ii) interest payment, and</p>
            </dd>
            <dd>
                <p  style="text-align: justify; margin-left: 30px;" >iii) if applicable, any other charges.</p>
            </dd>
        </dl>
    </dd>
</dl>
<p  style="text-align: justify" >
    The statement will be available at all times on your account dashboard on Our website
    ( <a href="http://www.ezbob.com" target="_blank">http://www.ezbob.com</a> or <a href="http://www.ezbob.co.uk" target="_blank">http://www.ezbob.co.uk</a>)</p>
<p  style="text-align: justify" >
    Also if you do not repay your loan in full and on time we will continue to charge
    interest on the outstanding credit balance. The rate of interest will be the same
    as shown above (for the final month). Interest after default is calculated daily and applied to the outstanding
    balance every 30 days (or when the outstanding balance is repaid in full if earlier).
</p>
<p  style="text-align: justify" >
    In addition to the missed payment fee, in the event you fail to make two or more
    of the scheduled repayments, and you owe an amount greater than the sum of two scheduled
    repayments, you will, in addition to the missed payment fee, be charged a fee of
    £50.00 (“default fee”).
</p>
<p  style="text-align: justify" >
    We may also charge all costs and expenses (including enquiry agents, debt collections
    agents and legal costs) reasonably incurred by us in enforcing this Agreement.</p>
<dl>
    <dt>Each repayment will be used: </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        a) first to pay the interest that has accrued on the outstanding balance (i.e. the
        outstanding loan amount and interest due on the outstanding balance);
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        b) then towards repaying any charges resulting from the loan being in arrears (i.e.
        the £40 missed payment fee, the £50 default fee and any charges due to any collection
        agency or legal expenses;</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        c) then towards paying off the credit provided under the loan agreement.
    </p></dd>
</dl>
<p  style="text-align: justify" >
    Missing payments could have severe consequences, including legal proceedings and
    could make obtaining credit in the future more difficult.
</p>
<p  style="text-align: justify" >
    You have a right under section 66A of the1974 Act to withdraw from this Agreement
    without giving any reason. To do so you must give oral or written notice of the
    withdrawal before the end of the period of 14 days beginning with the day after
    we inform you that the agreement has been executed.
</p>
<p  style="text-align: justify" >
    You must give notice of your intention to withdrawal orally over the telephone to
     +44 800 011 4787 or in writing to <a href="mailto:support@ezbob.com" target="_blank">support@ezbob.com</a></p>
<p  style="text-align: justify" >
    In the event that you withdraw from this Agreement you are required to repay the
    total amount of credit provided under the loan agreement (i.e. the loan amount (including the set-up fee)), without delay and no later than 30 calendar days after giving
    notice of withdrawal, together with the interest accrued from the date the credit
    was provided until the day you repay. You can repay by crediting our bank account 23080153
</p>
<p  style="text-align: justify" >
    The amount of interest payable per day will be {{InterestRatePerDayFormatted}}%</p>
<p  style="text-align: justify" >
    You have a right to make early repayment under the 1974 Act. You can exercise this
    right by giving us notice and making payment.
</p>
<p  style="text-align: justify;" >
    <b>Ombudsman Scheme</b></p>
<p  style="text-align: justify" >
    You are entitled to complain to the Financial Ombudsman Service.
</p>
<p  style="text-align: justify" >
    <b>Terms & Conditions</b>
</p>
<p  style="text-align: justify" >
    Other contractual terms and conditions apply and are attached to this Agreement
    and are incorporated herein by reference.
</p>
<p  style="text-align: justify" >
    <b>Supervisory Authority</b></p>
<p  style="text-align: justify" >
    The Financial Conduct Authority is the supervisory authority under the Act. Their address
    is 25 The North Colonnade, Canary Wharf, London E14 5HS.
</p>
<p  style="text-align: justify" >
    By Clicking “I accept” you electronically sign this Agreement and agree to be legally
    bound by its terms
</p>
<p  style="text-align: justify" >Date {{CurentDate}}</p>
<p  style="text-align: justify" >
    Signed for and on behalf of Orange Money Ltd.</p>
<p  style="text-align: justify" >
    <br />
    <br />
    <br />
    <br />
</p>
<p  style="text-align: justify" >Date {{CurentDate}}</p>

<center><h4>TERMS AND CONDITIONS</h4></center>
<dl>
    <dt><b>Application</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        1. These Terms and Conditions will apply to the Fixed Sum Loan Agreement entered
        into by you with Orange Money Ltd trading as EZbob, EZbob.com and orangemoney.co.uk,
        whose registered office address 145-157 St John Street, London EC1V 4PW, company
        number 7852687.
    </p></dd>
</dl>
<dl>
    <dt><b>Electronic Transmission of documents and Electronic Signature</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        2. You agree that we may deliver documents to you via electronic means and that
        the email addresses you provide to facilitate this is correct. You may email documents
        to us at <a href="mailto:support@ezbob.com" target="_blank">support@ezbob.com</a></p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        3. For the purpose of Clause 2 above a document transmitted by email or fax will
        be deemed as having been delivered on the working day immediately following the
        day on which it is transmitted.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        4. By Clicking “I accept” you electronically sign this Agreement and agree to be
        legally bound by its terms.</p></dd>
</dl>
<dl>
    <dt><b>Loan Application</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        5. You agree that at the date of applying for and accepting this loan, you are not:
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i. in a Debt Management Plan (or similar scheme) or considering entering a Debt
                Management Plan (or similar scheme);</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                ii. in an I.V.A or are considering entering into an I.V.A.; or</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iii. bankrupt or considering filing for bankruptcy.</p></dd>
        </dl>
    </p></dd>
</dl>
<dl>
    <dt><b>Loan Approval</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        6. If and when we provisionally approve your loan application, actual payment of
        the loan amount is subject to:
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i. us ensuring that the information you gave us was not inaccurate or deficient
                in any respect (if we discover that it was, then the Agreement will be void and
                where appropriate may be reported to credit reference and fraud agencies);
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                ii. us checking your credit reference file and the information you provide to assess
                the affordability of the loan.</p></dd>
        </dl>
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        7. You must accept the loan and sign the Agreement confirming acceptance of the
        Agreement and its associated Terms and Conditions before the acceptance deadline,
        which will be communicated to you at the time of your loan application.
    </p></dd>
</dl>
<dl>
    <dt><b>Payment</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        8. Upon execution of the Agreement we will advance the Loan Amount into your nominated
        bank account referred to herein as the Primary Payment Account. Any other account
        nominated and/or maintained by you shall be herein referred to as the Alternative
        Payment Account.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        9. You agree to repay us the total amount payable on the scheduled repayment dates
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        10. You irrevocably grant us the right and authorise us to automatically debit,
        on a continuing basis, the Primary Payment Account and/or the Alternative Payment
        Account, using the debit card(s) nominated by you and associated with the Primary
        and/or Alternative Payment Accounts, for all or part of any sums outstanding on
        or after the date upon which sums become due.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        11. You must therefore ensure there are sufficient funds in the account to cover
        payment on the scheduled repayment date. If we are unsuccessful in collecting full
        payment on a scheduled repayment date we will charge you a £40.00 missed payment
        fee on each scheduled repayment date where payment the entire scheduled payment
        is not received by us. We may try to debit your Primary Payment Account at a later
        stage and on more than one occasion on the scheduled repayment date, or subsequent
        dates, for all or any part of the repayment amount due. If you have provided us
        with details of an Alternative Payment Account and we are unsuccessful in collecting
        payment from the Primary Payment Account, we may seek payment from the Alternative
        Payment Account and on multiple occasions, for all or any part of the amount due,
        as we determine necessary until full repayment is made.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        12. Prompt payment is essential. If you are late in paying, you will pay us interest
        on the amount due from when it is due until it is actually received by us (credited
        to our account) at the applicable rate of interest shown above. Please also see Clauses 20
        and 21 below concerning late payments charges. Therefore, in the event your payment
        is not immediate (i.e. real time) and consequently payment does not reach our bank
        for a further few days, interest will continue to accrue at the above rate until
        payment does in fact reach us.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        13. Your obligation to make repayments to us will be satisfied by making payments
        to us by debit card or as otherwise agreed in writing by us.
    </p></dd>
</dl>
<dl>
    <dt><b>Ending the Agreement and Early Repayment</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        14. You may end the Agreement at any time by repaying the full amount you owe under
        the Agreement at that time, without penalty or premium.
    </p></dd>
    <p  style="text-align: justify; margin-left: 30px;" >
        You may also make an early repayment of a portion of the credit at any time (without
        penalty or premium) and any such early repayment will be applied to the amount due
        on the next rescheduled payment date (and any excess to the amounts due on the following
        repayment date(s)). Upon any such partial early repayment the amounts payable on
        the remaining repayment dates shall be recalculated by applying the applicable interest
        rate applied to the outstanding balance of the credit. Any such changes will be
        reflected on the dashboard and in the statement of account you have the right to
        obtain from us in accordance with the 1974 Act.
    </p>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        15. You agree that we may demand repayment of the full amount owed by you under
        the Agreement if you:
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i. pay late;
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                ii. have been dishonest or given misinformation in a way which affects our decision
                to lend money to you;
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iii. have broken the terms of the Agreement; or
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iv. die, become bankrupt or make a voluntary arrangement with people you owe money
                to.</p></dd>
        </dl>
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        16. Before making any demand we will take all the steps we are required to take
        by law for your protection.
    </p></dd>
</dl>
<dl>
    <dt><b>Default and Late Payment</b> </dt>
    <dd>
        <p  style="text-align: justify; margin-left: 30px;" >
              17. If you miss a loan repayment, or find yourself in financial difficulties and
              you believe you cannot afford to repay the amount due under your Agreement, you
              will contact us as soon as possible by email to <a href="mailto:support@ezbob.com"
                                                                 target="_blank">support@ezbob.com</a> so we can discuss a repayment plan and/or
              other appropriate steps (which may include passing your account to a Collections
              Agency).
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        18. You shall tell us or a Collections Agency, as the case may be, when your circumstances
        change in a way that may adversely impact your ability to repay the amount due under
        your Agreement.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        19. If your relationship with us or the Collections Agency has broken down, this
        will be included in the information supplied to the credit reference agencies. You
        should be aware that this may have serious consequences for your ability to get
        credit in the future.</p></dd>
</dl>
<dl>
    <dt><b>Default and Late Payment Charges</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        20. You will be liable for any fees costs and expenses, including legal fees, incurred
        by us in enforcing this Agreement or collecting or recovering any amounts owing
        by you under the Agreement. Such amounts shall be added to your outstanding balance.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        21. The fees we may charge include:-
        <p  style="text-align: justify; margin-left: 60px;" >
            Missed Payment Fee, £40.00;
        </p>
        <p  style="text-align: justify; margin-left: 60px;" >
            Default Fee, £50.00;
        </p>
        <p  style="text-align: justify; margin-left: 60px;" >
            Referral by us of the debt to a third party collections agency £50.00.</p>
    </p></dd>
</dl>
<dl>
    <dt><b>Use of Personal Information</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        22. We are the data controller and may use data that identifies you (“your personal
        data”) for: statistical analysis; to develop and improve our products; to update
        your records; to identify which of our, or others'' products might interest you;
        to assess lending and insurance risks; to arrange, underwrite and administer insurance
        and handle claims; to identify, prevent, detect or tackle fraud, money laundering
        and other crime; to carry out regulatory checks; keeping you informed about your
        loan, for market research; and in the products and services which we offer to third
        parties.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        23. We will keep your personal data confidential and only give it to others for
        the purposes explained above and in our privacy policy at <a href="http://www.ezbob.com"
            target="_blank">www.ezbob.com</a> including:
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i. to our agents and subcontractors, acting for us, to use for the purpose of operating
                our lending business and obtaining payment;
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >34
                ii. to share information via an organisation which provides a centralised application
                matching service which it collects from and about mortgage and/or credit applications,
                for the purpose of preventing and detecting fraud;
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iii. to licensed credit brokers in the event that you apply to borrow money at <a href="http://www.ezbob.com"
            target="_blank">www.ezbob.com</a> or <a href="http://www.ezbob.co.uk"
            target="_blank">www.ezbob.co.uk</a>
                , your application is declined or the loan monies are otherwise
                unavailable and we reasonably believe that a credit broker may be able to help you
                obtain a loan.
            </p></dd>
        </dl>
    </p></dd>
    <dd><p  style="text-align: justify" >
        We reserve the right to amend our privacy policy from time to time and such amendments
        will be posted on our website.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        24. When you apply to us for a loan, we will check the following records about you
        (and others where applicable)
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i. our own;</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                ii. those at credit reference agencies (CRAs);
            </p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iii. those at fraud prevention agencies (FPAs);</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                iv. the “Accounts (defined below) to which you have granted us read-only access
                when you registered with us or applied for a loan.</p></dd>
        </dl>
    </p></dd>
</dl>
<dl>
    <dt><b>CRAs</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        25. When CRAs receive a search from us they will place a search footprint on your
        credit file that may be seen by other lenders. They supply to us both public (including
        the electoral register) and shared credit and fraud prevention information.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        26. We will make checks such as; assessing this application for credit and verifying
        identities to prevent and detect crime and money laundering. We may also make periodic
        searches at CRAs and FPAs to manage your account with us.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        27. Information on applications will be sent to CRAs and will be recorded by them.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        28. Where you borrow from us, we will give details of your loan(s)/account(s) and
        how you manage it/them to CRAs. If you borrow and do not repay in full and on time,
        CRAs will record the outstanding debt and, in some cases, the length of time that
        the debt remains outstanding. This information may be supplied to other organisations
        by CRAs and FPAs to perform similar checks and to trace your whereabouts and recover
        debts that you owe. Records remain on file for 6 years after they are closed, whether
        settled by you or defaulted.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        29. We will give you prior notice of a default being registered on your credit reference
        file. However, we may not always give you notice if we plan to take court action.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        30. If you give us false or inaccurate information and we have reasonable grounds
        to suspect fraud or we identify fraud we may record this and may also pass this
        information to FPAs and other organisations involved in crime and fraud prevention.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        31. If you are making a joint application or tell us that you have a spouse or financial
        associate, we will link your records together so you must be sure that you have
        their agreement to disclose information about them. CRAs also link your records
        together and these links will remain on your and their files until such time as
        you or your partner successfully files for a disassociation with the CRAs to break
        that link.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        32. We and other organisations may access and use from other countries the information
        recorded by fraud prevention agencies. We may transfer your personal data abroad
        to countries whose data protection laws are less strict than in the UK. If so, we
        will ensure the information is held securely to standards as least as good as those
        in the UK and only used for the purposes set out in this clause.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        33. Your data may also be used for other purposes for which you give your permission
        or, in very limited circumstances, when required by law or where permitted under
        the terms of the Data Protection Act 1998.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        34. Under the Data Protection Act 1998, you have a right to access certain personal
        records we, credit reference agencies and fraud prevention agencies hold about you.
        This is called a ‘subject access request’, which you can make by writing to <a href="mailto:support@ezbob.com">support@ezbob.com</a>.
        A fee may be payable, but we will not charge you until we have told you how much
        the fee is and what it is for, and you have told us you still want to proceed.
    </p></dd>
</dl>
<p  style="text-align: justify" >
    This is a condensed version and if you would like to read the full details of how
    your data may be used please visit our website at <a href="http://www.ezbob.com/AboutUs/Privacy"
        target="_blank">www.ezbob.com</a>.
</p>
<dl>
    <dt><b>Complaints and Dispute Resolution</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        35. If you want to make a complaint about Orange Money Ltd you can email <a href="mailto:support@ezbob.com">support@ezbob.com</a>,
        with brief details of your complaint and your account number. Our Customer Service
        staff will acknowledge your complaint by email within 5 working days. We will try
        to sort out your complaint quickly once you have contacted us. If we cannot do this,
        we will tell you about the procedures we have for sorting out complaints. These
        procedures are set out below. If we need to investigate your complaint further to
        respond fully, we will tell you and keep you regularly updated. If a further investigation
        is required one of our staff will investigate and send you an initial response,
        having had access to an officer with the authority to settle the complaint (including,
        where appropriate, an offer of redress). Where appropriate, the member of staff
        investigating the complaint will not be any staff member who was directly involved
        in the subject matter of the complaint.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        36. If you want to make a complaint to the Consumer Credit Trade Association (“CCTA”),
        you may phone them on 01274 714959 or get the relevant information from <a href="http://www.ccta.co.uk"
            target="_blank">www.ccta.co.uk</a></p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        37. If you need an independent third party to mediate in resolving the complaint
        before the involvement of the Financial Ombudsman, CCTA provides its members with
        a Code of Practice Conciliation Service.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        38. The service is directly associated to member’s compliance with the provisions
        of the CCTA Code of Practice. Complaints against a member who has not adhered to
        the Code of Practice, cannot be dealt with under the service.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        39. Once a request to conciliate is made the CCTA will attempt to liaise between
        us and you customer in order to resolve the issue to the satisfaction of all concerned,
        as quickly as possible.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        40. In the unlikely event that conciliation fails to resolve the complaint you may,
        after a period of eight weeks have elapsed from the date of receipt of the initial
        complaint, refer the complaint to the Financial Ombudsman (FOS).
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        41. You cannot use the CCTA conciliation scheme if your complaint has been decided
        by the Financial Ombudsman Service or a court.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        42. Final response letter and the Financial Ombudsman Service We will send you a
        letter by the end of eight weeks after we received your complaint (either directly
        or from the CCTA). This will either:
        <dl>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                i) Give our final view on the issues raised in your complaint, and say whether we:</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                a. Accept the complaint and where appropriate are offering redress; or</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                b. Are offering redress without accepting the complaint; or</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                c. Reject the complaint (giving reasons why we are doing this); or</p></dd>
            <dd><p  style="text-align: justify; margin-left: 60px;" >
                ii) Explain that we are not able to provide you with a final response within that
                eight week period, give the reason for the delay and tell you when we expect to
                give you a final response.</p></dd>
        </dl>
    </p></dd>
</dl>
<p  style="text-align: justify" >
    In each case, we will tell you that if you are still not satisfied with our response
    or the delay, you may refer the complaint to the Financial Ombudsman Service. We
    will give you their details, and a copy of their explanatory leaflet, in the final
    response letter.
</p>
<dl>
    <dt><b>Monitoring and Compliance</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        43. Our internal procedures ensure that we comply with the applicable provisions
        of the CCTA Lending Code. We have to fill in an “annual statement of compliance”
        as a condition of our membership of the CCTA.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        44. The code is monitored by the CCTA which is made up of representatives from the
        finance houses and independent consumers. It also has an independent chairman. The
        Group produces an annual report, which we can send to you, if you ask.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        45. You can contact the CCTA if you have a complaint about the general running of
        the code by writing to: The Compliance Manager,
    </p></dd>
</dl>
<dl>
    <dt><b>Assignment</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        46. We may assign our right, title and interest in the Agreement to any third party.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        47. Any rights and obligations under the Agreement that are assigned shall not be
        adversely affected in any way whatsoever.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        48. You are not entitled to assign the Agreement.
    </p></dd>
</dl>
<dl>
    <dt><b>Notices</b></dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        49. You agree that any notices given to you in relation to any Agreement can be
        delivered to the email address supplied by you during your application. Any notices
        may also be put on your online account. In such cases, you will also receive by
        email to your email address a prompt to refer to your online account. At our discretion,
        we may also serve any notice or demand on you personally, or leave it or send it
        by prepaid envelope addressed to you at your last known address. You may serve any
        written notices on us by first class post or by email to <a href="mailto:support@ezbob.com"
            target="_blank">support@ezbob.com</a>
    </p></dd>
</dl>
<dl>
    <dt><b>General</b> </dt>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        50. You agree to possess and at all times to remain in compliance with all permits,
        licenses, approvals, consents, registrations and other authorisations necessary
        to own, operate and to conduct the business in which you are presently engaged.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        51. You agree to comply, at all times, with all applicable laws relating to the
        operating of its business and with all rules and regulations governing its use of
        on-line sales channels and web merchant accounts (“Accounts”).</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        52. You agree not to pledge, assign, close or otherwise dispose of any of your Accounts
        without our prior written consent.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
         53.	For so long as the Loan remains outstanding you shall continuously grant us “read-only” access to your Accounts.
         In the event that you change your user name, password or other account access
        information with respect to any of the on-line sales channel or web merchant accounts
        you maintains in the operation of our business, you agree to provide us with prompt
        notice. In the event that you create or use on-line sales channel or web merchant
        accounts other than those indicated to your initial loan application to us, you
        agree to promptly advise us of the same and continuously allow us “read-only” access to the same.</p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        54. You agree that we may, from time to time, request copies of your business’ reviewed
        and/or audited financial statements, sales reports and such other information related
        to your business and financial condition.</p></dd>
    
    <dd><p  style="text-align: justify; margin-left: 30px;" >
            55. These Loan Conditions and the Agreement and any dealings with you prior to any
            agreement being made are governed by and construed in accordance with English law,
            and we and you submit to the non-exclusive jurisdiction of the English courts, unless
            you live in Scotland, Northern Ireland, the Channel Islands or the Isle of Man,
            in which case you will be entitled to commence legal proceedings in your local courts.
        </p></dd>

    <dd><p  style="text-align: justify; margin-left: 30px;" >
            56. This Agreement is intended to create a permitted investment transaction under Jewish law (to the extent required under Jewish law), based on the form used by Mizrahi Tefahot Bank Ltd (IL), dated 1.9.1981.  This clause will not operate to the detriment of the Borrower in any way, will not derogate from any of the rights of the Borrower, and will not impose any additional obligations on the Borrower.
        </p></dd>

            <dd>
                <p  style="text-align: justify; margin-left: 30px;" >
        57. If any part of the Loan Conditions that is not fundamental is found to be illegal
        or unenforceable, such finding will not affect the validity or enforceability of
        the remainder of the Loan Conditions or the Agreement, as the case may be.
    </p>
            </dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        58. Any waiver by either party of a breach of any provision of these Loan Conditions
        and/or the Agreement shall not be considered to be a waiver of any subsequent breach
        of the same, or any other, provision.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        59. The records kept by us shall be conclusive of the facts and matters they purport
        to record except where there is an obvious mistake.
    </p></dd>
    <dd><p  style="text-align: justify; margin-left: 30px;" >
        60. We are authorised and regulated by the Financial Conduct Authority. Our Interim Permissions Reference Number is 647816.
        See www.oft.gov.uk for details. The information provided is correct at time of printing
        and is valid until varied in accordance with these Terms and Conditions. We are
        an online lending service providing short term loans. The basis upon which the Loan
        is supplied to you is set out in this Agreement.</p></dd>
    <dd><p style="text-align: justify; margin-left: 30px;" >
        61. All communications with you will be in English.</p></dd>
    <dd><p style="text-align: justify; margin-left: 30px;" >
        62. Some of our loans benefit from a guarantee issued under the ''European Progress
        Microfinance Facility'' established by the European Union. If you were informed during
        your loan application process that this loan benefits from that guarantee program, 
        the following provisions will apply: The counter party acknowledges that the European
        Investment Fund ("EIF"), the agents of EIF, the European Court of Auditors (the "ECA"),
        the Commission and the agents of the Commission including OLAF (the "Agents") shall 
        have the right to carry out controls and to request information in respect of this agreement
        and its execution. The counter party shall permit inspections by EIF, the agents of EIF, the ECA,
        the Commission and the Agents of its business operations, books and records. As these controls 
        may include on the spot controls of the counter party, the counter party shall permit access to 
        its premises to EIF, the agents of EIF, the ECA, the Commission and the Agents during normal 
        business hours. If any deficiencies in the maintenance of records are identified by EIF, agents of EIF,
        the ECA, the Commission and/or the Agents, EIF, agents of EIF, the Borrower undertakes to promptly, 
        and in any event no later than three (3) months after being informed of such deficiencies, comply with 
        the instructions given by EIF, agents of EIF, the ECA, the Commission and/or the Agents and provide any 
        additional information reasonably requested by EIF, agents of EIF, the ECA, the Commission and/or the Agents.
        For a period of five years following the expiry or termination of this Agreement, the Borrower hereby 
        undertakes to keep all documents relating to this Agreement, and shall make these documents available 
        for inspection by EIF, the ECA, the Commission and/or the Agents.</p></dd>
</dl>'

IF NOT EXISTS (SELECT 1 FROM LoanAgreementTemplate WHERE Template = @CreditActTemplateContent AND TemplateType = 3)
BEGIN
	INSERT INTO LoanAgreementTemplate
	(
		Template, 
		TemplateType
	)
	VALUES
	(
		@CreditActTemplateContent, 
		3
	)
END
GO






