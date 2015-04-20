namespace CallCreditLib {
	using System;
	using System.Collections.Generic;
	using Callcredit.CRBSB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;

	public partial class CallCreditModelBuilder {
		public List<CallCreditDataAccs> GetAccs(CT_outputapplicant applicant, int oiaid) {

			var Accounts = new List<CallCreditDataAccs>();
			TryRead(() => {
				foreach (var Ac in applicant.accs) {
					var account = new CallCreditDataAccs {
						AccNocs = new List<CallCreditDataAccsNocs>(),
						AccHistory = new List<CallCreditDataAccsHistory>(),
						OiaID = oiaid
					};

					var acc = Ac;
					//account holder details
					TryRead(() => account.AccHolderName = acc.accholderdetails.name, "Account holder’s name details");
					account.Dob = TryReadDate(() => acc.accholderdetails.dob, "Account holder’s date of birth", false);
					TryRead(() => account.StatusCode = acc.accholderdetails.statuscode, "Account holder status code");
					account.StartDate = TryReadDate(() => acc.accholderdetails.startdate, "account holder start day", false);
					account.EndDate = TryReadDate(() => acc.accholderdetails.enddate, "account holder end day", false);
					//account holder address
					TryRead(() => account.CurrentAddress = Convert.ToBoolean(acc.accholderdetails.address.current), "account holder current address check", false);
					TryRead(() => account.UnDeclaredAddressType = (int)acc.accholderdetails.address.undeclaredaddresstype, "type of undeclared address for account holder", false);
					TryRead(() => account.AddressValue = acc.accholderdetails.address.Value, "account holder address value");
					//account default related details
					account.DefDate = TryReadDate(() => acc.@default.defdate, "Default date", false);
					TryRead(() => account.OrigDefBal = acc.@default.origdefbal, "Original default balance", false);
					TryRead(() => account.TermBal = acc.@default.termbal, "Termination balance", false);
					account.DefSatDate = TryReadDate(() => acc.@default.defsatdate, "Default satisfaction date", false);
					account.RepoDate = TryReadDate(() => acc.@default.repodate, "Repossession date", false);
					//account delinquency related details
					account.DelinqDate = TryReadDate(() => acc.delinquent.delinqdate, "Delinquency date", false);
					TryRead(() => account.DelinqBal = acc.delinquent.delinqbal, "Delinquency balance", false);
					//general account details
					TryRead(() => account.AccNo = acc.accdetails.accno, "Account number (own data only)");
					TryRead(() => account.AccSuffix = (int)acc.accdetails.accsuffix, "Account suffix code (own data only)", false);
					TryRead(() => account.Joint = acc.accdetails.joint, "Joint account indicator", false);
					TryRead(() => account.Status = acc.accdetails.status, "Status of account");
					account.DateUpdated = TryReadDate(() => acc.accdetails.dateupdated, "Date that account was last updated", false);
					TryRead(() => account.AccTypeCode = acc.accdetails.acctypecode, "Account type code", false);
					TryRead(() => account.AccGroupId = (int)acc.accdetails.accgroupid, "Account Type Group Identifier", false);
					TryRead(() => account.CurrencyCode = acc.accdetails.currencycode, "Currency code");
					TryRead(() => account.Balance = acc.accdetails.balance, "Current balance on account", false);
					TryRead(() => account.CurCreditLimit = acc.accdetails.limit, "Current credit limit on account", false);
					TryRead(() => account.OpenBalance = acc.accdetails.openbalance, "Account opening balance", false);
					account.ArrStartDate = TryReadDate(() => acc.accdetails.arrstartdate, "Arrangement start date", false);
					account.ArrEndDate = TryReadDate(() => acc.accdetails.arrenddate, "Arrangement end date", false);
					account.PayStartDate = TryReadDate(() => acc.accdetails.paystartdate, "Payment start date", false);
					account.accStartDate = TryReadDate(() => acc.accdetails.accstartdate, "Account start date", false);
					account.AccEndDate = TryReadDate(() => acc.accdetails.accenddate, "Account end date", false);
					TryRead(() => account.RegPayment = acc.accdetails.regpayment, "Regular payment value", false);
					TryRead(() => account.ExpectedPayment = acc.accdetails.expectedpayment, "Expected payment value", false);
					TryRead(() => account.ActualPayment = acc.accdetails.actualpayment, "Actual payment value", false);
					TryRead(() => account.RepayPeriod = (int)acc.accdetails.repayperiod, "Repayment period", false);
					TryRead(() => account.RepayFreqCode = acc.accdetails.repayfreqcode, "Repayment frequency code", false);
					TryRead(() => account.LumpPayment = acc.accdetails.lumppayment, "Lump or balloon payment", false);
					TryRead(() => account.PenIntAmt = acc.accdetails.penintamt, "Penalty interest amount", false);
					TryRead(() => account.PromotionalRate = Convert.ToBoolean(acc.accdetails.promotionalrate), "Promotional rate check", false);
					TryRead(() => account.MinimumPayment = Convert.ToBoolean(acc.accdetails.minimumpayment), "Check for minimum payment has been made within accepted tolerances", false);
					TryRead(() => account.StatementBalance = acc.accdetails.statementbalance, "The balance at the statement date", false);
					//account supplier details
					TryRead(() => account.SupplierName = acc.supplierdetails.suppliername, "Supplier name", false);
					TryRead(() => account.SupplierTypeCode = acc.supplierdetails.suppliertypecode, "Supplier type code", false);
					TryRead(() => account.Apacs = Convert.ToBoolean(acc.supplierdetails.apacs), " Check for data supplier has supplied Behavioral Data", false);

					TryRead(() => {
						foreach (var AcHis in acc.acchistory) {
							var AccHist = new CallCreditDataAccsHistory();

							var acchistory = AcHis;
							AccHist.M = TryReadDate(() => new DateTime(Convert.ToInt32(acchistory.m.Substring(0, 4)), Convert.ToInt32(acchistory.m.Substring(5)), 1, 0, 0, 0, DateTimeKind.Utc), "Year and month that history record applies to");
							TryRead(() => AccHist.Bal = acchistory.bal, "Balance amount for account history record");
							TryRead(() => AccHist.CreditLimit = acchistory.limit, "Credit limit for account history record", false);
							TryRead(() => AccHist.Acc = acchistory.acc, "Account status code for account history record", false);
							TryRead(() => AccHist.Pay = acchistory.pay, "Payment status code for account history record", false);
							TryRead(() => AccHist.StmtBal = acchistory.stmtbal, "Statement balance for account history record ", false);
							TryRead(() => AccHist.PayAmt = acchistory.payamt, "Payment amount for account history record", false);
							TryRead(() => AccHist.CashAdvCount = acchistory.cashadvcount, "Number of cash advances for account history record", false);
							TryRead(() => AccHist.CashAdvTotal = (int)acchistory.cashadvtotal, "Total value of cash advances for account history record ", false);
							account.AccHistory.Add(AccHist);
						}
					},"Account history");

					TryRead(() => {
						foreach (var AcN in acc.notice) {
							var AccNoc = new CallCreditDataAccsNocs();
							var accnotice = AcN;
							TryRead(() => AccNoc.NoticeType = accnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => AccNoc.RefNum = accnotice.refnum, "Notice Type (Notice Reference Number)", false);
							AccNoc.DateRaised = TryReadDate(() => accnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => AccNoc.Text = accnotice.text, "Text for Notice of Correction", false);
							TryRead(() => AccNoc.NameDetails = accnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => AccNoc.CurrentAddress = Convert.ToBoolean(accnotice.address.current), "current address check", false);
							TryRead(() => AccNoc.UnDeclaredAddressType = (int)accnotice.address.undeclaredaddresstype, "type of undeclared address", false);
							TryRead(() => AccNoc.AddressValue = accnotice.address.Value, "Address value related to notice against account", false);
							account.AccNocs.Add(AccNoc);
						}
					},"Account notices");
					Accounts.Add(account);
				}
			}, "Accs");

			return Accounts;
		}

		public List<CallCreditDataAddresses> GetSummaryAddresses(CT_outputsummaryblock summary) {
			var Addresses = new List<CallCreditDataAddresses>();

			TryRead(() => {
				foreach (var Ad in summary.address) {

					var address = new CallCreditDataAddresses();

					var add = Ad;

					TryRead(() => address.CurrentAddress = Convert.ToBoolean(add.current), "applicant’s/associate's current address check");
					TryRead(() => address.AddressId = (int)add.id, "Address Identifier", false);
					TryRead(() => address.Messagecode = (int)add.messagecode, "Message code indicating the level of confirmation", false);
					TryRead(() => address.UnDeclaredAddressType = (int)add.undeclaredaddresstype, "type of undeclared address", false);
					TryRead(() => address.AddressValue = add.Value, "Address value related to summary addresses item");
					Addresses.Add(address);
				}
			},"Summary Addresses");
			return Addresses;
		}

		public List<CallCreditDataAddressConfs> GetAddressConfs(CT_outputapplicant applicant) {

			var AddressConfs = new List<CallCreditDataAddressConfs>();
			TryRead(() => {
				foreach (var AdCnf in applicant.addressconfs) {
					var conf = new CallCreditDataAddressConfs {
						Residents = new List<CallCreditDataAddressConfsResidents>()
					};

					var addconfs = AdCnf;
					TryRead(() => conf.PafValid = Convert.ToBoolean(addconfs.pafvalid), "Postcode Address File validation check", false);
					TryRead(() => conf.OtherResidents = Convert.ToBoolean(addconfs.otherresidents), "Other residents flag (provided on LSAR only)", false);
					TryRead(() => conf.CurrentAddress = Convert.ToBoolean(addconfs.address.current), "applicant’s/associate's current address check");
					TryRead(() => conf.UnDeclaredAddressType = (int)addconfs.address.undeclaredaddresstype, "type of undeclared address", false);
					TryRead(() => conf.AddressValue = addconfs.address.Value, "Address value to be confirmed");
					TryRead(() => {
						foreach (var Rsd in addconfs.resident) {
							var resident = new CallCreditDataAddressConfsResidents {
								ErHistory = new List<CallCreditDataAddressConfsResidentsErHistory>(),
								ResidentNocs = new List<CallCreditDataAddressConfsResidentsNocs>()
							};
							var resids = Rsd;

							TryRead(() => resident.MatchType = resids.matchtype, "resident is individual match indicator", false);
							TryRead(() => resident.CurrentName = Convert.ToBoolean(resids.currentname), "resident's current name check", false);
							TryRead(() => resident.DeclaredAlias = Convert.ToBoolean(resids.declaredalias), "resident's declared alias check", false);
							TryRead(() => resident.NameDetails = resids.name, "Confirmation of individual's details", false);
							TryRead(() => resident.Duration = resids.duration, "Effective duration of residency", false);
							resident.StartDate = TryReadDate(() => resids.startdate, "Effective start date of residency", false);
							resident.EndDate = TryReadDate(() => resids.enddate, "Effective end date of residency", false);
							TryRead(() => resident.ErValid = (int)resids.ervalid, "Electoral Roll validation identifier", false);
							TryRead(() => {
								foreach (var ErHis in resids.erhistory) {
									var erhistory = new CallCreditDataAddressConfsResidentsErHistory {
										ErHistoryNocs = new List<CallCreditDataAddressConfsResidentsErHistoryNocs>()
									};

									var erhist = ErHis;
									erhistory.StartDate = TryReadDate(() => erhist.startdate, "", false);
									erhistory.EndDate = TryReadDate(() => erhist.enddate, "", false);
									TryRead(() => erhistory.Optout = Convert.ToBoolean(erhist.optout), "", false);
									TryRead(() => erhistory.RollingRoll = Convert.ToBoolean(erhist.rollingroll), "", false);
									TryRead(() => {
										foreach (var ErHisNoc in erhist.notice) {
											var erhnoc = new CallCreditDataAddressConfsResidentsErHistoryNocs();

											var erhisnotice = ErHisNoc;
											TryRead(() => erhnoc.NoticeType = erhisnotice.type, "Notice Type (Correction or Dispute)", false);
											TryRead(() => erhnoc.RefNum = erhisnotice.refnum, "Notice Type (Notice Reference Number)", false);
											erhnoc.DateRaised = TryReadDate(() => erhisnotice.dateraised, "Date that the Notice was raised)", false);
											TryRead(() => erhnoc.Text = erhisnotice.text, "Text for Notice of Correction", false);
											TryRead(() => erhnoc.NameDetails = erhisnotice.name, "Name details as provided on the Notice of Correction)", false);
											TryRead(() => erhnoc.CurrentAddress = Convert.ToBoolean(erhisnotice.address.current), "current address check", false);
											TryRead(() => erhnoc.UnDeclaredAddressType = (int)erhisnotice.address.undeclaredaddresstype, "type of undeclared address", false);
											TryRead(() => erhnoc.AddressValue = erhisnotice.address.Value, "Address value related to notice against a period of Electoral Roll history", false);
											erhistory.ErHistoryNocs.Add(erhnoc);
										}
									}, "Erhistory Notices");

									resident.ErHistory.Add(erhistory);
								}
							}, "Erhistory");

							TryRead(() => {
								foreach (var ResNoc in resids.notice) {
									var resnoc = new CallCreditDataAddressConfsResidentsNocs();

									var resnotice = ResNoc;
									TryRead(() => resnoc.NoticeType = resnotice.type, "Notice Type (Correction or Dispute)", false);
									TryRead(() => resnoc.RefNum = resnotice.refnum, "Notice Type (Notice Reference Number)", false);
									resnoc.DateRaised = TryReadDate(() => resnotice.dateraised, "Date that the Notice was raised)", false);
									TryRead(() => resnoc.Text = resnotice.text, "Text for Notice of Correction", false);
									TryRead(() => resnoc.NameDetails = resnotice.name, "Name details as provided on the Notice of Correction)", false);
									TryRead(() => resnoc.CurrentAddress = Convert.ToBoolean(resnotice.address.current), "current address check", false);
									TryRead(() => resnoc.UnDeclaredAddressType = (int)resnotice.address.undeclaredaddresstype, "type of undeclared address", false);
									TryRead(() => resnoc.AddressValue = resnotice.address.Value, "Address value related to notice against a redident", false);
									resident.ResidentNocs.Add(resnoc);
								}
							}, "Resident Notices");

							conf.Residents.Add(resident);
						}
					}, "Residents");

					AddressConfs.Add(conf);
				}
			}, "AddressConfs");

			return AddressConfs;
		}

		public List<CallCreditDataAddressLinks> GetAddressLinks(CT_outputapplicantAddresslinks addresslink) {
			var AddressLinks = new List<CallCreditDataAddressLinks>();
			TryRead(() => {
				foreach (var AdL in addresslink.links) {
					var addlink = new CallCreditDataAddressLinks {
						AddressLinkNocs = new List<CallCreditDataAddressLinksNocs>()
					};

					var addlinks = AdL;
					addlink.CreationDate = TryReadDate(() => addlinks.creationdate, "Address Link creation date", false);
					addlink.LastConfDate = TryReadDate(() => addlinks.lastconfdate, "Address Link most recent confirmation date", false);
					TryRead(() => addlink.From = (int)addlinks.from, "Address ID for the from address", false);
					TryRead(() => addlink.To = (int)addlinks.to, "Address ID for the to address", false);
					TryRead(() => addlink.SupplierName = addlinks.supplierdetails.suppliername, "Address Link Supplier name", false);
					TryRead(() => addlink.SupplierTypeCode = addlinks.supplierdetails.suppliertypecode, "Address Link Supplier type code", false);
					TryRead(() => {
						foreach (var AddLinkNoc in addlinks.notice) {
							var addlinknoc = new CallCreditDataAddressLinksNocs();

							var addlinknotice = AddLinkNoc;
							TryRead(() => addlinknoc.NoticeType = addlinknotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => addlinknoc.RefNum = addlinknotice.refnum, "Notice Type (Notice Reference Number)", false);
							addlinknoc.DateRaised = TryReadDate(() => addlinknotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => addlinknoc.Text = addlinknotice.text, "Text for Notice of Correction", false);
							TryRead(() => addlinknoc.NameDetails = addlinknotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => addlinknoc.CurrentAddress = Convert.ToBoolean(addlinknotice.address.current), "current address check", false);
							TryRead(() => addlinknoc.UnDeclaredAddressType = (int)addlinknotice.address.undeclaredaddresstype, "type of undeclared address", false);
							TryRead(() => addlinknoc.AddressValue = addlinknotice.address.Value, "Address value related to notice against address link", false);
							addlink.AddressLinkNocs.Add(addlinknoc);
						}
					}, "Address link Notices");

					AddressLinks.Add(addlink);
				}
			}, "Address links");

			return AddressLinks;
		}

		public List<CallCreditDataLinkAddresses> GetLinkAddresses(CT_outputapplicantAddresslinks addresslink) {
			var LinkAddresses = new List<CallCreditDataLinkAddresses>();
			TryRead(() => {
				foreach (var LinkAd in addresslink.addresses) {
					var linkaddresses = new CallCreditDataLinkAddresses ();

					var linkadd = LinkAd;
					TryRead(() => linkaddresses.AddressID = (int)linkadd.addressid, "Address id within an Address Link", false);
					TryRead(() => linkaddresses.Declared = Convert.ToBoolean(linkadd.declared), "Address declared on the application check", false);
					TryRead(() => linkaddresses.NavLinkID = linkadd.navlinkid, "Identifier for Address Link navigation", false);
					TryRead(() => linkaddresses.CurrentAddress = Convert.ToBoolean(linkadd.current), "Applicant's current address check", false);
					TryRead(() => linkaddresses.UnDeclaredAddressType = (int)linkadd.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => linkaddresses.AddressValue = linkadd.Value, "Address value held against an address link", false);
					LinkAddresses.Add(linkaddresses);
				}
			}, "Link Addresses");

			return LinkAddresses;
		}


		public List<CallCreditDataAliasLinks> GetAliasLinks(CT_outputapplicant applicant) {
			var AliasLinks = new List<CallCreditDataAliasLinks>();
			TryRead(() => {
				foreach (var AlL in applicant.aliaslinks) {
					var aliaslink = new CallCreditDataAliasLinks {
						AliasLinkNocs = new List<CallCreditDataAliasLinksNocs>()
					};

					var alslinks = AlL;
					TryRead(() => aliaslink.Declared = Convert.ToBoolean(alslinks.declared), "alias was declared on input check", false);
					TryRead(() => aliaslink.NameBefore = alslinks.name, "Name before Alias Link", false);
					TryRead(() => aliaslink.Alias = alslinks.alias, "Name after Alias Link", false);
					aliaslink.CreationDate = TryReadDate(() => alslinks.creationdate, "Date the Alias Link was created", false);
					aliaslink.LastConfDate = TryReadDate(() => alslinks.lastconfdate, "Most recent date that the Alias Link was confirmed", false);
					TryRead(() => aliaslink.SupplierName = alslinks.supplierdetails.suppliername, "Alias Link Supplier name", false);
					TryRead(() => aliaslink.SupplierTypeCode = alslinks.supplierdetails.suppliertypecode, "Alias Link Supplier type code", false);
					TryRead(() => {
						foreach (var AlsLinkNoc in alslinks.notice) {
							var aliaslinknoc = new CallCreditDataAliasLinksNocs();

							var alslinknotice = AlsLinkNoc;
							TryRead(() => aliaslinknoc.NoticeType = alslinknotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => aliaslinknoc.RefNum = alslinknotice.refnum, "Notice Type (Notice Reference Number)", false);
							aliaslinknoc.DateRaised = TryReadDate(() => alslinknotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => aliaslinknoc.Text = alslinknotice.text, "Text for Notice of Correction", false);
							TryRead(() => aliaslinknoc.NameDetails = alslinknotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => aliaslinknoc.CurrentAddress = Convert.ToBoolean(alslinknotice.address.current), "Current address check", false);
							TryRead(() => aliaslinknoc.UnDeclaredAddressType = (int)alslinknotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => aliaslinknoc.AddressValue = alslinknotice.address.Value, "Address value related to notice against alias link", false);
							aliaslink.AliasLinkNocs.Add(aliaslinknoc);
						}
					}, "Alias link Notices");

					AliasLinks.Add(aliaslink);

				}
			}, "Alias links");

			return AliasLinks;
		}

		public List<CallCreditDataAssociateLinks> GetAssociateLinks(CT_outputapplicant applicant) {
			var AssociateLinks = new List<CallCreditDataAssociateLinks>();
			TryRead(() => {
				foreach (var AsL in applicant.associatelinks) {
					var associatelink = new CallCreditDataAssociateLinks {
						AssociateLinkNocs = new List<CallCreditDataAssociateLinksNocs>()
					};

					var asslinks = AsL;
					TryRead(() => associatelink.DeclaredAddress = Convert.ToBoolean(asslinks.declaredaddress), "Declared address associate check", false);
					TryRead(() => associatelink.OiaID = (int)asslinks.oiaid, "pointer between an applicant's Associate Link and the corresponding opt-in associate report", false);
					TryRead(() => associatelink.NavLinkID = asslinks.navlinkid, "Identifier for Associate Link Navigation", false);
					TryRead(() => associatelink.AssociateName = asslinks.name, "Name of the associate", false);
					associatelink.CreationDate = TryReadDate(() => asslinks.creationdate, "Date the Associate Link was created", false);
					associatelink.LastConfDate = TryReadDate(() => asslinks.lastconfdate, "Most recent date the Associate Link was confirmed", false);
					TryRead(() => associatelink.SupplierName = asslinks.supplierdetails.suppliername, "Associate Link Supplier name", false);
					TryRead(() => associatelink.SupplierTypeCode = asslinks.supplierdetails.suppliertypecode, "Associate Link Supplier type code", false);
					TryRead(() => {
						foreach (var AssLinkNoc in asslinks.notice) {
							var associatelinknoc = new CallCreditDataAssociateLinksNocs();

							var asslinknotice = AssLinkNoc;
							TryRead(() => associatelinknoc.NoticeType = asslinknotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => associatelinknoc.RefNum = asslinknotice.refnum, "Notice Type (Notice Reference Number)", false);
							associatelinknoc.DateRaised = TryReadDate(() => asslinknotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => associatelinknoc.Text = asslinknotice.text, "Text for Notice of Correction", false);
							TryRead(() => associatelinknoc.NameDetails = asslinknotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => associatelinknoc.CurrentAddress = Convert.ToBoolean(asslinknotice.address.current), "Current address check", false);
							TryRead(() => associatelinknoc.UnDeclaredAddressType = (int)asslinknotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => associatelinknoc.AddressValue = asslinknotice.address.Value, "Address value related to notice against associate link", false);
							associatelink.AssociateLinkNocs.Add(associatelinknoc);
						}
					}, "Associate link Notices");

					AssociateLinks.Add(associatelink);

				}
			}, "Associate links");

			return AssociateLinks;
		}

		public List<CallCreditDataCifasFiling> GetCifasFiling(CT_outputapplicantCifas applicantcifas) {
			var CifasFiling = new List<CallCreditDataCifasFiling>();
			TryRead(() => {
				foreach (var CfsFl in applicantcifas.filing) {
					var cifasfiling = new CallCreditDataCifasFiling {
						CifasFilingNocs = new List<CallCreditDataCifasFilingNocs>()
					};

					var cifasfil = CfsFl;
					TryRead(() => cifasfiling.PersonName = cifasfil.person.name, "Person`s name details", false);
					cifasfiling.Dob = TryReadDate(() => cifasfil.person.dob, "Person`s date of birth", false);
					TryRead(() => cifasfiling.CurrentAddressP = Convert.ToBoolean(cifasfil.person.address.current), "Current address check", false);
					TryRead(() => cifasfiling.UnDeclaredAddressTypeP = (int)cifasfil.person.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => cifasfiling.AddressValueP = cifasfil.person.address.Value, "Address value related to person against CIFAS filing", false);
					TryRead(() => cifasfiling.CompanyNumber = cifasfil.company.number, "Registered Company number", false);
					TryRead(() => cifasfiling.CompanyName = cifasfil.company.name, "Company name", false);
					TryRead(() => cifasfiling.CurrentAddressC = Convert.ToBoolean(cifasfil.person.address.current), "Current address check", false);
					TryRead(() => cifasfiling.UnDeclaredAddressTypeC = (int)cifasfil.person.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => cifasfiling.AddressValueC = cifasfil.person.address.Value, "Address value related to company against CIFAS filing", false);
					TryRead(() => cifasfiling.MemberNumber = (int)cifasfil.details.membernumber, "Owning member reference number", false);
					TryRead(() => cifasfiling.CaseReferenceNo = cifasfil.details.casereferenceno, "Case reference number", false);
					TryRead(() => cifasfiling.MemberName = cifasfil.details.name, "Member name", false);
					TryRead(() => cifasfiling.ProductCode = cifasfil.details.productcode, "Product code", false);
					TryRead(() => cifasfiling.FraudCategory = cifasfil.details.fraudcategory, "Fraud category", false);
					TryRead(() => cifasfiling.ProductDesc = cifasfil.details.productdesc, "Description of product code", false);
					TryRead(() => cifasfiling.FraudDesc = cifasfil.details.frauddesc, "Description of fraud category", false);
					cifasfiling.InputDate = TryReadDate(() => cifasfil.details.inputdate, "The exact date upon which a confirmed fraud originally became live on the CIFAS database", false);
					cifasfiling.ExpiryDate = TryReadDate(() => cifasfil.details.expirydate, "The date upon which a confirmed fraud becomes expired on the CIFAS database", false);
					TryRead(() => cifasfiling.TransactionType = cifasfil.details.transactiontype, "Transaction type", false);
					TryRead(() => {
						foreach (var CifasFilkNoc in cifasfil.notice) {
							var cifasfilingnoc = new CallCreditDataCifasFilingNocs();

							var cifasfilnotice = CifasFilkNoc;
							TryRead(() => cifasfilingnoc.NoticeType = cifasfilnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => cifasfilingnoc.Refnum = cifasfilnotice.refnum, "Notice Type (Notice Reference Number)", false);
							cifasfilingnoc.DateRaised = TryReadDate(() => cifasfilnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => cifasfilingnoc.Text = cifasfilnotice.text, "Text for Notice of Correction", false);
							TryRead(() => cifasfilingnoc.NameDetails = cifasfilnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => cifasfilingnoc.CurrentAddress = Convert.ToBoolean(cifasfilnotice.address.current), "Current address check", false);
							TryRead(() => cifasfilingnoc.UnDeclaredAddressType = (int)cifasfilnotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => cifasfilingnoc.AddressValue = cifasfilnotice.address.Value, "Address value related to Cifas filing notice", false);
							cifasfiling.CifasFilingNocs.Add(cifasfilingnoc);
						}
					}, "Associate link Notices");

					CifasFiling.Add(cifasfiling);

				}
			}, "Associate links");

			return CifasFiling;
		}


		public List<CallCreditDataCifasPlusCases> GetCifasPlusCases(CT_outputapplicantCifas applicantcifas) {
			var CifasPlusCase = new List<CallCreditDataCifasPlusCases>();
			TryRead(() => {
				foreach (var CfsCase in applicantcifas.@case) {
					var cifascases = new CallCreditDataCifasPlusCases {
						Dmrs = new List<CallCreditDataCifasPlusCasesDmrs>(),
						FilingReasons = new List<CallCreditDataCifasPlusCasesFilingReasons>(),
						CifasPlusCaseNocs = new List<CallCreditDataCifasPlusCasesNocs>(),
						Subjects = new List<CallCreditDataCifasPlusCasesSubjects>()
					};

					var cifascase = CfsCase;
					TryRead(() => cifascases.CaseId = Convert.ToInt32(cifascase.caseid), "Case unique reference number within FIND", false);
					TryRead(() => cifascases.OwningMember = (int)cifascase.details.owningmember, "Owning member reference number", false);
					TryRead(() => cifascases.ManagingMember = (int)cifascase.details.managingmember, "Managing member reference number", false);
					TryRead(() => cifascases.CaseType = cifascase.details.casetype, "CIFAS Case Type", false);
					TryRead(() => cifascases.ProductCode = cifascase.details.productcode, "CIFAS Product code", false);
					TryRead(() => cifascases.Facility = cifascase.details.facility, "Financial facility was granted indicator", false);
					cifascases.SupplyDate = TryReadDate(() => cifascase.details.supplydate, "The exact date upon which a confirmed fraud originally became live on the CIFAS database", false);
					cifascases.ExpiryDate = TryReadDate(() => cifascase.details.expirydate, "The date upon which a confirmed fraud becomes expired on the CIFAS database", false);
					cifascases.ApplicationDate = TryReadDate(() => cifascase.details.applicationdate, "The date upon which an application for credit was made", false);

					for (int i = 0; i < cifascase.details.filingreasons.Length; ++i) {
						var cifascasefr = new CallCreditDataCifasPlusCasesFilingReasons();

						TryRead(() => cifascasefr.FilingReason = cifascase.details.filingreasons[i], "CIFAS Case Filing Reason", false);
						cifascases.FilingReasons.Add(cifascasefr);
					}

					for (int k = 0; k < cifascase.dmrs.Length; ++k) {
						var cifascasedmr = new CallCreditDataCifasPlusCasesDmrs();

						TryRead(() => cifascasedmr.dmr = (int)cifascase.dmrs[k], "The data matching routine used to match to the CIFAS case", false);
						cifascases.Dmrs.Add(cifascasedmr);
					}

					TryRead(() => {
						foreach (var CifasCaseSubj in cifascase.details.subjects) {
							var cifascasesubject = new CallCreditDataCifasPlusCasesSubjects();

							var cifascasesubj = CifasCaseSubj;
							TryRead(() => cifascasesubject.PersonName = cifascasesubj.person.name, "Notice Type (Person name for a CIFAS Case)", false);
							cifascasesubject.PersonDob = TryReadDate(() => cifascasesubj.person.dob, "Person date of birth for a CIFAS Case", false);
							TryRead(() => cifascasesubject.CompanyName = cifascasesubj.company.name, "Company name for a CIFAS Case", false);
							TryRead(() => cifascasesubject.CompanyNumber = cifascasesubj.company.number, "Company number for a CIFAS Case", false);
							TryRead(() => cifascasesubject.HomeTelephone = cifascasesubj.hometelephone, "Home telephone number of a Subject", false);
							TryRead(() => cifascasesubject.MobileTelephone = cifascasesubj.mobiletelephone, "Mobile telephone number of a Subject", false);
							TryRead(() => cifascasesubject.Email = cifascasesubj.email, "Email address of a Subject", false);
							TryRead(() => cifascasesubject.SubjectRole = cifascasesubj.subjectrole, "Subject's indication of the type of its involvement with the SIFAS Case ", false);
							TryRead(() => cifascasesubject.SubjectRoleQualifier = cifascasesubj.subjectrolequalifier, "Subject's role qualifier within the SIFAS Case", false);
							TryRead(() => cifascasesubject.AddressType = cifascasesubj.addressdata[0].type, "CIFAS Address Type", false);
							TryRead(() => cifascasesubject.CurrentAddress = Convert.ToBoolean(cifascasesubj.addressdata[0].address.current), "Current address check", false);
							TryRead(() => cifascasesubject.UndeclaredAddressType = (int)cifascasesubj.addressdata[0].address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => cifascasesubject.AddressValue = cifascasesubj.addressdata[0].address.Value, "Address value held against CIFAS Plus Case subject", false);
							cifascases.Subjects.Add(cifascasesubject);
						}
					}, "CIFAS Plus Case Subjects");

					TryRead(() => {
						foreach (var CifasCaseNoc in cifascase.notice) {
							var cifascasenoc = new CallCreditDataCifasPlusCasesNocs();

							var cifascasenotice = CifasCaseNoc;
							TryRead(() => cifascasenoc.NoticeType = cifascasenotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => cifascasenoc.Refnum = cifascasenotice.refnum, "Notice Type (Notice Reference Number)", false);
							cifascasenoc.DateRaised = TryReadDate(() => cifascasenotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => cifascasenoc.Text = cifascasenotice.text, "Text for Notice of Correction", false);
							TryRead(() => cifascasenoc.NameDetails = cifascasenotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => cifascasenoc.CurrentAddress = Convert.ToBoolean(cifascasenotice.address.current), "Current address check", false);
							TryRead(() => cifascasenoc.UnDeclaredAddressType = (int)cifascasenotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => cifascasenoc.AddressValue = cifascasenotice.address.Value, "Address value related to Cifas Plus Case notice", false);
							cifascases.CifasPlusCaseNocs.Add(cifascasenoc);
						}
					}, "CIFAS Plus Case Notices");

					CifasPlusCase.Add(cifascases);

				}
			}, "Associate links");

			return CifasPlusCase;
		}
	}
}
