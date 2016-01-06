IF NOT EXISTS (SELECT * FROM BrokerTerms WHERE DateAdded = 'January 10 2016' AND OriginID = 1)
BEGIN
	DECLARE @BrokerTermsID INT = 1 + (SELECT MAX(BrokerTermsID) FROM BrokerTerms)

	INSERT INTO BrokerTerms(BrokerTermsID, DateAdded, TermsTextID, OriginID, BrokerTerms)
		VALUES (@BrokerTermsID, 'January 10 2016', 9, 1,
'<p>Broker and Intermediary Program Terms and Conditions</p>
<p>The following terms and conditions govern Your relationship with EZBob Ltd. (trading as ezbob) and participation in Our broker and intermediary program ("Program"). Our privacy policy, found at: <a href="http://www.ezbob.com/privacy-and-cookies" target="_blank">http://www.ezbob.com/privacy-and-cookies</a>, is incorporated in these terms and conditions and made a part hereof. These terms and conditions are an important legal document and by accepting they will be legally binding upon You. Please read them carefully and print out a copy for Your records before You agree to them.</p>
<p>References in these terms and conditions to "We", "Us" and "Our", means, EZBob Ltd. References to "You" and "Your" means the person identified in the registration process and any person who accesses Your Account (defined below) whether as an individual or on behalf of such entity.</p>
<p>EZBob Ltd. is authorized and regulated by the Financial Control Authority for its consumer credit business.</p>
<p>By registering and participating in the Program You will be able to earn commissions or fees by introducing Us to potential borrowers and/or assisting Your clients in applying for business loans using Our proprietary on-line lending platform.</p>
<p>You agree to only market and promote Us as a provider of loans for business purposes.</p>
<p>Registration.</p>
<p>You warrant to Us that all information provided to Us in the course of Your registration is true and accurate in all respects. You will update Us if any of the information You provide to Us changes.</p>
<p>On registering with Us, You must provide a username and email address and create a password which will allow You to access Your account ("Account"). Your username and password are personal to Your Account and are not transferable. Your username and password allow You to access Your Account and are the methods used by Us to identify You and so You must keep them secure at all times. You are responsible for all information and activity on Your Account by anyone using Your username and password. You must provide Us with immediate notice of any breach of security, loss, theft or unauthorised use of a username or password.</p>
<p>If We suspect that the person logged into Your Account is not You or We suspect illegal or fraudulent activity or unauthorised use, We may cancel or suspend Your Account without advance notice.</p>
<p>As part of the registration process You authorize Us to obtain and review Your credit file and other reports and statements from credit reference agencies (<em>e.g.</em> CallCredit, Experian, Equifax), and fraud prevention agencies and make further searches from time to time in accordance with the next following sentence. </p>
<p>You also authorize Us to undertake, from time to time, periodic investigations and audits and make other inquiries with the relevant trade organizations, credit reference agencies, and government agencies and offices as We deem necessary to ensure You are and remain compliant with the terms and requirements set forth herein and Your continued suitability and fitness to participate in the Program </p>
<p>You shall at all times be an independent contractor, and not a co-venturer, agent, employee, or representative of Us. It will be Your sole obligation to report as self-employment income (or other non-wage income) all compensation received from Us, and You will hold the Company harmless to the extent of any obligations imposed on Us by law to pay any withholding taxes, social insurance payments, unemployment or disability insurance or similar items in connection with any payments made by Us to You hereunder.</p>
<p>If you are working with or representing another broker or intermediary, you must ensure that such other person also registers for the Program and agrees to be bound by these terms and conditions; provided that no registration or undertakings by a third party shall modify these terms or conditions as applicable to You or relive you of any of Your obligations hereunder. </p>
<p>Your access and use of Our website in connection with Your participation in the Program and otherwise is also subject to the terms and conditions of our website found at: <a href="http://www.ezbob.com/terms-and-conditions" target="_blank">http://www.ezbob.com/terms-and-conditions</a>.</p>
<p>Loan Applications. </p>
<p>You warrant that You shall disclose in full to each Applicant the existence of the commission arrangement between us, and promptly upon the request of an Application, the amount of any commission, fee or other payment You may be entitled to receive from Us in connection with said Applicant&rsquo;s application for and/or acceptance of a loan from Us.</p>
<p>If You refer a potential borrower ("Applicant") to Us or assist or make a loan application on behalf of an Applicant, You warrant that You are duly authorised to act on such Applicant. You expressly acknowledge that if the Applicant is an "individual", as such person is defined in the Consumer Credit Act 1974 (including a natural person, a sole trader or a partnership of two or three persons, not all of whom are bodies corporate) You must be authorized and regulated by the Financial Conduct Authority, and upon Our request provide Us with the details of such license and You must comply at all times with the requirements of the FCA Handbook (including the Consumer Credit source book (CONC)).</p>
<p>Without derogating from the generality of the foregoing, You warrant that if You assist or complete a loan application on behalf of an Applicant, You have authority from said Applicant to perform all the acts that You will be performing on such Applicant&rsquo;s behalf, including without limitation:</p>
<p>(a) authority to provide US with all the details of the Applicant as required by the loan application process and consent that such details to be processed and used in accordance with these terms and conditions and Our privacy policy (found at <a href="http://www.ezbob.com/privacy-and-cookies" target="_blank">http://www.ezbob.com/privacy-and-cookies</a>);</p>
<p>(b) authority to set up a loan account for the Applicant on Our website (<a href="http://www.ezbob.com/" target="_blank">www.ezbob.com</a>); </p>
<p>(c) authority to allow Us to obtain, access and review the Applicant&rsquo;s credit file and other reports and statements from credit reference agencies (<em>e.g.</em> CallCredit, Experian, Equifax), and fraud prevention agencies;</p>
<p>(d) authority to share information with You regarding the loan application, and the Applicant&rsquo;s account with Us on an ongoing basis (including with respect to payment history and any defaults under the applicable loan agreements).</p>
<p>In making a loan application on behalf of an Applicant, You warrant that You are not aware of any circumstances that would mean that the borrower will be unable to repay the loan. You warrant and represent that You have disclosed to Us any circumstances that You are aware of which could or might result in a material adverse change in the Applicant&rsquo;s financial condition, business or assets or ability to repay a loan to Us.</p>
<p>Before You make a loan application on behalf of an Applicant, You must apply full customer due diligence measures to the Applicant in accordance with the Money Laundering Regulations 2007 and any other applicable laws, regulations, codes of practice and guidance relating to money laundering, including without limitation guidance of the Joint Money Laundering Steering Group. You must make available to Us, on request, copies of all data, documents and other information used for such verification and retain the customer due diligence records for a minimum period of 5 years from the date on which We rely on the records.</p>
<p>You acknowledge and warrant that You shall advise each Applicant that We use Our own internal guidelines and policies when assessing loan applications and have complete discretion as to whether We offer the Applicant a loan and on what terms (if any).</p>
<p>If We elect to offer a loan to an Applicant, We will generally keep the offer open for a period of three (3) working days; after that time, a new application may be required. Notwithstanding the foregoing, We may withdraw Our offer at any time if We determine, in Our sole discretion, that the risk profile of the Applicant has changed or that any of the information You or the Applicant has provided to Us is incomplete or inaccurate.</p>
<p>Before You provide information about the borrower to Us, You must tell the borrower how their information will be used and provide them with a copy of the relevant section of Our privacy policy (<a href="http://www.ezbob.com/privacy-and-cookies" target="_blank">http://www.ezbob.com/privacy-and-cookies</a>).</p>
<p>You shall comply with the Data Protection Act in all respects with respect to any personal data You obtain and/or provide to Us.</p>
<p>You acknowledge and agree that You are not an agent of EZBob Ltd. and will not hold yourself out to be Our agent. You acknowledge that You may not make any representations or give any warranty on behalf of Us nor have the authority to enter into any agreement on behalf of Us or otherwise bind Us.</p>
<p>You warrant that, in respect of all of Your dealings with Us, You are operating and will operate in full compliance with the Consumer Credit Act 1974, and all related legislation and hold all required licenses and registrations applicable to Your activities hereunder. You agree to indemnify, and keep indemnified, Us and Our affiliates, officers, shareholders, directors and employees from and against all costs, expenses, damages, loss, liabilities, demands, claims, actions or proceedings which We may suffer or incur arising out of Your breach of the obligations out of the warranties and obligations contained in these terms and conditions.</p>
<p>Commission</p>
<p>We will pay You commissions as set forth below. The payment of the commission, if any, shall be the entire compensation to which You are entitled from Us, and is inclusive of all costs and expenses that You may incur in connection with Your participation in the Program.</p>
<p><em>Definitions</em>:</p>
<p>"Represented Borrower" means an Applicant first introduced to Us by You that applies to Us for a loan within thirty (30) days of such introduction. The term "first introduced" means the Applicant (or You, on its behalf) first accessed the Our website (<a href="http://www.ezbob.com/" target="_blank">www.ezbob.com</a> ) via Your Account and applied for a loan from Us or that We can otherwise determine (in our sole discretion) was introduced to Us by You and with and with whom we did not have a prior relationship. </p>
<p>A borrower or applicant shall not be and shall cease to be Your Represented Borrower if it is represented or becomes represented by a different broker or intermediary; we shall make reasonable efforts to confirm with the borrower or Applicant that it has or has engaged a new or different broker or intermediary.</p>
<p>"Commission Fee" means with respect to a Qualifying Loan, an amount equal to the product of the original principal balance of the Qualifying Loan actually advanced to the borrower, multiplied by the applicable Commission Rate therefor.</p>
<p>"Commission Rate" means:</p>
<p>&middot; Up to 5% with respect to the First Qualifying Loan made to a Represented Borrower for the first &pound;50,000 of the original principal amount of such First Qualifying Loan and up to 2.5% for amounts in excess of &pound;50,000 of the original principal amount of such First Qualifying Loan;</p>
<p>&middot; Up to 5% with respect to any Subsequent Qualifying Loan made to a Represented Borrower where You submitted the loan application on behalf of the Represented Borrower for the first &pound;50,000 of the original principal amount of such Subsequent Qualifying Loan and up to 2.5% for amounts in excess of &pound;50,000 of the original principal amount of such Subsequent Qualifying Loan; provided that if the Subsequent Qualifying Loan is made within three months of the immediately prior Qualifying Loan, the Commission Rate for the entire original principal amount of said Qualifying Loan shall be up to 2.5%.</p>
<p>&middot; Up to 2.5% with respect to any Subsequent Qualifying Loan made to Your Represented Borrower where the Represented Borrower independently or directly applied for the loan;</p>
<p>"First Qualifying Loan" means the first Qualifying Loan made to the applicable Represented Borrower. </p>
<p>&ldquo;Qualifying Loan&rdquo; means a loan made by Us to a Represented Borrower.</p>
<p>&ldquo;Subsequent Qualifying Loan&rdquo; means any Qualifying Loan made to the same Represented Borrower after the First Qualifying Loan to said Represented Borrower.</p>
<p>No Commission shall be payable to you with respect to any Qualifying Loan where 36 months or more have elapsed from the last time you submitted a loan application on behalf of the Represented Borrower and we made a loan to said Represented Borrower.</p>
<p><em>Accrual of Commission Fees</em>. With respect to each Qualifying Loan We shall pay to You the applicable Commission Fee. The Commission Fee shall accrue upon the advance of the Qualifying Loan by Us to your Represented Borrower.</p>
<p><em>Payments</em>. Within no more than two business days of our advance of a loan for which a Commission Fee is payable to You, We will pay to You the Commission Fee owing therefor. In connection with the payment, of the Commission Fee our system will automatically generate an invoice and deliver copies to you and to our accounting department. The invoice will contain a unique sequential serial number, date, and your personal and bank account details and will reference, the name of the Represented Borrower, the amount of the Qualifying Loan and corresponding Commission amount paid. You shall be responsible for including this invoice in your accounting books and records and reporting the payment in connection with your own tax and other reporting obligations.</p>
<p>If a Commission Fee has been paid with respect to a Qualifying Loan and the Represented Borrower subsequently defaults on a repayment under such Qualifying Loan before We have received the first three (3) repayments under such Qualifying Loan, or if We have a reasonable belief that you have committed fraud, made a material misrepresentation or acting in a grossly negligent manner in connection with the loan application of the Represented Borrower, the corresponding Commission Fee shall be refunded to Us by You, and We will be entitled to deduct such refund from subsequent payments of the Commission Fee to You. All payments to You shall be made in GBP, to a bank account in the United Kingdom designated in writing by You.</p>
<p><em>Taxes</em>. All payments to You hereunder will be deemed to be inclusive of VAT, if applicable. You shall be responsible for the payments of all taxes and other levies of any kind which result from the payment of the Commission Fee to you. We may deduct from payments to You all amounts required by law.</p>
<p>Use of Our Name and Content</p>
<p>You may not use any graphic, textual or other materials referencing the name EZBob Ltd. or ezbob (or any variation thereof) other than as specifically provided for below in connection with your permitted use of Promotional Materials (defined below).</p>
<p>You may not copy or use any content from Our website.</p>
<p>You may not create or maintain any link, website or otherwise publish any material (including mini-sites or copy-cat sites) which is designed to divert traffic from Our website or suggests or could be interpreted to mean that you are a subsidiary, affiliate or related party of EZBob Ltd.</p>
<p>We may, at our discretion make available to you certain banner advertisements, button links, text links, and/or other graphic or textual material you to use to attract customers (the "Promotional Materials"). You may only use the Promotional Materials strictly in accordance with the following terms and conditions:</p>
<p>&middot; You may not alter, add to, subtract from, or otherwise modify the Promotional Materials as provided to You. </p>
<p>&middot; We reserve the right to modify or alter the Promotional Materials from time to time and you shall use and display only the most recent versions of the Promotional Materials provided by Us.</p>
<p>&middot; Upon the earlier of Our written request and the termination of your participation in the Program You must cease to use the Promotional Materials and remove them from any website, link or other media or location where you have placed them or caused them to appear.</p>
<p>&middot; You may not make any representation, promise, or commitment regarding Us, or our products and services, including without limitation, the terms on which we may offer a loan or an Applicant&rsquo;s likelihood of Us approving a loan, other than by using the Promotional Materials in their unaltered content and form.</p>
<p>&middot; You may not use the Promotional Materials or otherwise reference EZBob Ltd. or ezbob (or any variation thereof) on any website or other publication that contains or has links to or advertisement for any Prohibited Content (as described below).</p>
<p>Prohibited Content shall mean and include any content that:</p>
<p>1. Promotes sexually explicit or pornographic materials. </p>
<p>2. Promotes violence.</p>
<p>3. Promotes discrimination based on race, sex, religion, nationality, disability, sexual orientation, or age.</p>
<p>4. Promotes illegal activities. </p>
<p>5. Incorporates any materials which infringe or assist others to infringe on any copyright, trademark or other intellectual property rights or to violate the law. </p>
<p>6. Is otherwise in any way unlawful, harmful, threatening, defamatory, obscene, harassing, or racially, ethnically or otherwise objectionable to us in our sole discretion. </p>
<p>7. Resembles Our website in a manner which may lead customers to believe You are a subsidiary or affiliate of EZBob Ltd.</p>
<p>You must not engage in the distribution of any unsolicited bulk emails (spam) in any way mentioning or referencing EZBob Ltd. or ezbob (or any variation thereof) or containing any link to Our website.</p>
<p>We retain all right, ownership, and interest in the Promotional Materials, and in any copyright, trademark, or other intellectual property in the Promotional Materials. Nothing in this Agreement shall be construed to grant Affiliate any rights, ownership or interest in the Promotional Materials, or in the underlying intellectual property, other than the limited rights to use the Promotional Materials set forth above.</p>
<p>Termination</p>
<p>Either You or We may terminate Your participation in the Program at any time, with or without cause, upon written advice to that effect to the other. Commissions which you have earned prior to the date of termination will be paid in accordance herewith.</p>
<p>Miscellaneous</p>
<p>No failure or delay in exercising any right, power or remedy by Us shall constitute a waiver by Us of, or impair or preclude any further exercise of, that or any right, power or remedy arising under these terms and conditions or otherwise.</p>
<p>These terms and conditions (including the privacy policy and website terms referenced herein) set out the entire agreement between You and Us with respect to Your participation in the Program and supersede any and all representations, communications and prior agreements (written or oral) made by You or Us.</p>
<p>These terms and conditions are personal to You and You may not assign or transfer them to any other person without Our prior written permission.</p>
<p>No term of these terms and conditions is enforceable pursuant to the Contracts (Rights of Third Parties) Act 1999 by any person who is not a party to it.</p>
<p>We reserve the right to change these terms and conditions from time to time, and the amended terms will be posted on Our website. Any revised terms shall take effect as at the date when the change is made and posted.</p>
<p>Should You have any questions about these terms and conditions, or wish to contact Us for any reason whatsoever, please contact Us via Your Account.</p>
<p>These terms and conditions are governed by English law. In the event of any matter or dispute arising out of or in connection with these terms and conditions, You and We shall submit to the non-exclusive jurisdiction of the English courts.</p>
<p>These terms and conditions were last updated on January 10, 2016.</p>')
END
GO

