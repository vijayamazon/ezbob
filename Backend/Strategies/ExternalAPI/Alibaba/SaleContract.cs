namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Ezbob.Backend.ModelsWithDB.Alibaba;
    using Ezbob.Database;
    using Ezbob.Backend.Models.ExternalAPI;
    using MailApi;

    public class SaleContract : AStrategy
    {
        public SaleContract(AlibabaContractDto dto)
        {
            this.contract = dto;
            this.Result = new AlibabaSaleContractResult();
        }

        public AlibabaSaleContractResult Result;
        private AlibabaContractDto contract;

        public override string Name
        {
            get { return "AlibabaSaleContract"; }
        }

        public override void Execute()
        {
            Save(Parse(contract));
            SendMail();
        }

        private void SendMail()
        {
            if (Result.aId == null || Result.aliMemberId == null)
                return;
            var message = string.Format(@"<style type=""text/css"">
                                        .tg  {{border-collapse:collapse;border-spacing:0;}}
                                        .tg td{{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}}
                                        .tg th{{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}}
                                        </style>
                                        <table class=""tg"">
                                          <tr><th class=""tg-031e"" colspan=""2"">Seller Details</th></tr>
                                          <tr><td class=""tg-031e"">Business Name</td><td class=""tg-031e"">{0}</td></tr>
                                          <tr><td class=""tg-031e"">Street1</td><td class=""tg-031e"">{1}</td></tr>
                                          <tr><td class=""tg-031e"">Street2</td><td class=""tg-031e"">{2}</td></tr>
                                          <tr><td class=""tg-031e"">City</td><td class=""tg-031e"">{3}</td></tr>
                                          <tr><td class=""tg-031e"">State</td><td class=""tg-031e"">{4}</td></tr>
                                          <tr><td class=""tg-031e"">Country</td><td class=""tg-031e"">{5}</td></tr>
                                          <tr><td class=""tg-031e"">Postal Code</td><td class=""tg-031e"">{6}</td></tr>
                                          <tr><td class=""tg-031e"">Representitive First Name</td><td class=""tg-031e"">{7}</td></tr>
                                          <tr><td class=""tg-031e"">Representitive Last Name</td><td class=""tg-031e"">{8}</td></tr>
                                          <tr><td class=""tg-031e"">Seller Phone</td><td class=""tg-031e"">{9}</td></tr>
                                        </table>
                                        <table class=""tg"">
                                          <tr><th class=""tg-031e"" colspan=""2"">Seller Bank Details</th></tr>
                                          <tr><td class=""tg-031e"">Beneficiary Bank</td><td class=""tg-031e"">{10}</td></tr>
                                          <tr><td class=""tg-031e"">Street1</td><td class=""tg-031e"">{11}</td></tr>
                                          <tr><td class=""tg-031e"">Street2</td><td class=""tg-031e"">{12}</td></tr>
                                          <tr><td class=""tg-031e"">City</td><td class=""tg-031e"">{13}</td></tr>
                                          <tr><td class=""tg-031e"">State</td><td class=""tg-031e"">{14}</td></tr>
                                          <tr><td class=""tg-031e"">Country</td><td class=""tg-031e"">{15}</td></tr>
                                          <tr><td class=""tg-031e"">Postal Code</td><td class=""tg-031e"">{16}</td></tr>
                                          <tr><td class=""tg-031e"">SwiftCode</td><td class=""tg-031e"">{17}</td></tr>
                                          <tr><td class=""tg-031e"">Account Number</td><td class=""tg-031e"">{18}</td></tr>
                                          <tr><td class=""tg-031e"">Wire Instructions</td><td class=""tg-031e"">{19}</td></tr>
                                        </table>
                                        <table class=""tg"">
                                          <tr><th class=""tg-031e"" colspan=""2"">Buyer Details</th></tr>
                                          <tr><td class=""tg-031e"">Customer Id</td><td class=""tg-031e"">{20}</td></tr>
                                          <tr><td class=""tg-031e"">Business Name</td><td class=""tg-031e"">{21}</td></tr>
                                          <tr><td class=""tg-031e"">Street1</td><td class=""tg-031e"">{22}</td></tr>
                                          <tr><td class=""tg-031e"">Street2</td><td class=""tg-031e"">{23}</td></tr>
                                          <tr><td class=""tg-031e"">City</td><td class=""tg-031e"">{24}</td></tr>
                                          <tr><td class=""tg-031e"">State</td><td class=""tg-031e"">{25}</td></tr>
                                          <tr><td class=""tg-031e"">Country</td><td class=""tg-031e"">{26}</td></tr>
                                          <tr><td class=""tg-031e"">Postal Code</td><td class=""tg-031e"">{27}</td></tr>
                                          <tr><td class=""tg-031e"">Representitive First Name</td><td class=""tg-031e"">{28}</td></tr>
                                          <tr><td class=""tg-031e"">Representitive Last Name</td><td class=""tg-031e"">{29}</td></tr>
                                          <tr><td class=""tg-031e"">Phone</td><td class=""tg-031e"">{30}</td></tr>
                                          <tr><td class=""tg-031e"">Email</td><td class=""tg-031e"">{31}</td></tr>
                                        </table>
                                        <table class=""tg"">
                                          <tr><th class=""tg-031e"" colspan=""2"">Loan Details</th></tr>
                                          <tr><td class=""tg-031e"">Deposit</td><td class=""tg-031e"">{32}</td></tr>
                                          <tr><td class=""tg-031e"">Balance</td><td class=""tg-031e"">{33}</td></tr>
                                          <tr><td class=""tg-031e"">Currency</td><td class=""tg-031e"">{34}</td></tr>
                                          <tr><td class=""tg-031e"">Financing Type</td><td class=""tg-031e"">{35}</td></tr>
                                          <tr><td class=""tg-031e"">Confirm Release of Funds</td><td class=""tg-031e"">{36}</td></tr>
                                        </table>", HttpUtility.HtmlEncode(contract.sellerBusinessName), HttpUtility.HtmlEncode(contract.sellerStreet1), HttpUtility.HtmlEncode(contract.sellerStreet2), HttpUtility.HtmlEncode(contract.sellerCity), HttpUtility.HtmlEncode(contract.sellerState), HttpUtility.HtmlEncode(contract.sellerCountry), HttpUtility.HtmlEncode(contract.sellerPostalCode), HttpUtility.HtmlEncode(contract.sellerAuthRepFname), HttpUtility.HtmlEncode(contract.sellerAuthRepLname), HttpUtility.HtmlEncode(contract.sellerPhone), HttpUtility.HtmlEncode(contract.sellerEmail),
                                                 HttpUtility.HtmlEncode(contract.beneficiaryBank), HttpUtility.HtmlEncode(contract.bankStreetAddr1), HttpUtility.HtmlEncode(contract.bankStreetAddr2), HttpUtility.HtmlEncode(contract.bankCity), HttpUtility.HtmlEncode(contract.bankState), HttpUtility.HtmlEncode(contract.bankCountry), HttpUtility.HtmlEncode(contract.bankPostalCode), HttpUtility.HtmlEncode(contract.bankAccountNumber.ToString()), HttpUtility.HtmlEncode(contract.otherWireInstructions),
                                                 HttpUtility.HtmlEncode(this.Result.aId.ToString()), HttpUtility.HtmlEncode(contract.buyerBusinessName), HttpUtility.HtmlEncode(contract.buyerStreet1), HttpUtility.HtmlEncode(contract.buyerStreet2), HttpUtility.HtmlEncode(contract.buyerCity), HttpUtility.HtmlEncode(contract.buyerState), HttpUtility.HtmlEncode(contract.buyerZip), HttpUtility.HtmlEncode(contract.buyerCountry), HttpUtility.HtmlEncode(contract.buyerAuthRepFname), HttpUtility.HtmlEncode(contract.buyerAuthRepLname), HttpUtility.HtmlEncode(contract.buyerPhone), HttpUtility.HtmlEncode(contract.buyerEmail),
                                                 HttpUtility.HtmlEncode(contract.orderDeposit.ToString()), HttpUtility.HtmlEncode(contract.orderBalance.ToString()), HttpUtility.HtmlEncode(contract.orderCurrency), HttpUtility.HtmlEncode(contract.financingType), HttpUtility.HtmlEncode(contract.buyerConfirmReleaseFunds.ToString())
                                        );

            new Mail().Send(
                "dev@ezbob.com;vitasd@ezbob.com",
                null,
                message,
                ConfigManager.CurrentValues.Instance.MailSenderEmail,
                ConfigManager.CurrentValues.Instance.MailSenderName,
                "Payment Request: Alibaba Transaction for customer: " + Result.aId+" AliMemberId: "+Result.aliMemberId
            );
        }

        private AlibabaContract Parse(AlibabaContractDto Data)
        {
            int? customerID = DB.ExecuteScalar<int?>(
                 "CustomerIdByRefNum",
                 CommandSpecies.StoredProcedure,
                 new QueryParameter("CustomerRefNum", Data.aId)
                 );

            if (customerID == null)
            {
                Log.Alert("Customer with refnumber {0} not found", Data.aId);
                this.Result.aId = null;
                return null;
            }

            DB.FillFirst(
                this.Result,
                "LoadSaleContractResult",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", customerID),
                new QueryParameter("AliMemberId", Data.aliMemberId)
            );

            if (this.Result.aliMemberId == null)
            {
                Log.Alert("aliMemberId {0} for customer refnum {1} not found", Data.aliMemberId, Data.aId);
                return null;
            }

            var Res = new AlibabaContract();
            Res.Buyer.CustomerId = (int)customerID;

            // contract
            TryRead(() => Res.RequestId = Data.requestId);
            TryRead(() => Res.LoanId = Data.loanId);
            TryRead(() => Res.OrderNumber = Data.orderNumber);
            TryRead(() => Res.ShippingMark = Data.shippingMark);
            TryRead(() => Res.TotalOrderAmount = Data.totalOrderAmount);
            TryRead(() => Res.DeviationQuantityAllowed = Data.deviationQuantityAllowed);
            TryRead(() => Res.OrderAddtlDetails = Data.orderAddtlDetails);
            TryRead(() => Res.ShippingTerms = Data.shippingTerms);
            Res.ShippingDate = TryReadDate(() => Data.shippingDate);
            TryRead(() => Res.LoadingPort = Data.loadingPort);
            TryRead(() => Res.DestinationPort = Data.destinationPort);
            TryRead(() => Res.TACoveredAmount = Data.taCoveredAmount);
            TryRead(() => Res.OrderDeposit = Data.orderDeposit);
            TryRead(() => Res.OrderBalance = Data.orderBalance);
            TryRead(() => Res.OrderCurrency = Data.orderCurrency);
            TryRead(() => Res.CommercialInvoice = Data.attachmentCommercialInvoice);
            TryRead(() => Res.BillOfLading = Data.attachmentBillOfLading);
            TryRead(() => Res.PackingList = Data.attachmentPackingList);
            TryRead(() => Res.Other = Data.attachmentOther);

            TryRead(() => Res.Seller.BusinessName = Data.sellerBusinessName);
            TryRead(() => Res.Seller.AliMemberId = Data.sellerAliMemberId);
            TryRead(() => Res.Seller.Street1 = Data.sellerStreet1);
            TryRead(() => Res.Seller.Street2 = Data.sellerStreet2);
            TryRead(() => Res.Seller.City = Data.sellerCity);
            TryRead(() => Res.Seller.State = Data.sellerState);
            TryRead(() => Res.Seller.Country = Data.sellerCountry);
            TryRead(() => Res.Seller.PostalCode = Data.sellerPostalCode);
            TryRead(() => Res.Seller.AuthRepFname = Data.sellerAuthRepFname);
            TryRead(() => Res.Seller.AuthRepLname = Data.sellerAuthRepLname);
            TryRead(() => Res.Seller.Phone = Data.sellerPhone);
            TryRead(() => Res.Seller.Fax = Data.sellerFax);
            TryRead(() => Res.Seller.Email = Data.sellerEmail);
            TryRead(() => Res.Seller.GoldSupplierFlag = Data.goldSupplierFlag);
            TryRead(() => Res.Seller.TenureWithAlibaba = Data.supplierTenureWithAlibaba);
            Res.Seller.BusinessStartDate = TryReadDate(() => Data.supplierBusinessStartDate);
            TryRead(() => Res.Seller.Size = Data.supplierSize);
            TryRead(() => Res.Seller.suspiciousReportCountCounterfeitProduct = Data.suspiciousReportCountCounterfeitProduct);
            TryRead(() => Res.Seller.suspiciousReportCountRestrictedProhibitedProduct = Data.suspiciousReportCountRestrictedProhibitedProduct);
            TryRead(() => Res.Seller.suspiciousReportCountSuspiciousMember = Data.suspiciousReportCountSuspiciousMember);
            TryRead(() => Res.Seller.ResponseRate = Data.supplierResponseRate);
            Res.Seller.GoldMemberStartDate = TryReadDate(() => Data.supplierGoldMemberStartDate);
            TryRead(() => Res.Seller.QuotationPerformance = Data.quotationPerformance);

            TryRead(() => Res.Seller.Bank.BeneficiaryBank = Data.beneficiaryBank);
            TryRead(() => Res.Seller.Bank.StreetAddr1 = Data.bankStreetAddr1);
            TryRead(() => Res.Seller.Bank.StreetAddr2 = Data.bankStreetAddr2);
            TryRead(() => Res.Seller.Bank.City = Data.bankCity);
            TryRead(() => Res.Seller.Bank.State = Data.bankState);
            TryRead(() => Res.Seller.Bank.Country = Data.bankCountry);
            TryRead(() => Res.Seller.Bank.PostalCode = Data.bankPostalCode);
            TryRead(() => Res.Seller.Bank.SwiftCode = Data.swiftcode);
            TryRead(() => Res.Seller.Bank.AccountNumber = Data.bankAccountNumber);
            TryRead(() => Res.Seller.Bank.WireInstructions = Data.otherWireInstructions);

            if (Data.orderItems != null)
            {
                for (int i = 0; i < Data.orderItems.Length; i++)
                {
                    var item = new AlibabaContractItem();
                    TryRead(() => item.OrderProdNumber = Data.orderItems[i].orderProdNumber);
                    TryRead(() => item.ProductName = Data.orderItems[i].productName);
                    TryRead(() => item.ProductSpecs = Data.orderItems[i].productSpecs);
                    TryRead(() => item.ProductQuantity = Data.orderItems[i].productQuantity);
                    TryRead(() => item.ProductUnit = Data.orderItems[i].productUnit);
                    TryRead(() => item.ProductUnitPrice = Data.orderItems[i].productUnitPrice);
                    TryRead(() => item.ProductTotalAmount = Data.orderItems[i].productTotalAmount);

                    Res.Items.Add(item);
                }
            }


            TryRead(() => Res.Buyer.AliId = Data.aliMemberId);
            TryRead(() => Res.Buyer.BussinessName = Data.buyerBusinessName);
            TryRead(() => Res.Buyer.street1 = Data.buyerStreet1);
            TryRead(() => Res.Buyer.street2 = Data.buyerStreet2);
            TryRead(() => Res.Buyer.City = Data.buyerCity);
            TryRead(() => Res.Buyer.State = Data.buyerState);
            TryRead(() => Res.Buyer.Zip = Data.buyerZip);
            TryRead(() => Res.Buyer.Country = Data.buyerCountry);
            TryRead(() => Res.Buyer.AuthRepFname = Data.buyerAuthRepFname);
            TryRead(() => Res.Buyer.AuthRepLname = Data.buyerAuthRepLname);
            TryRead(() => Res.Buyer.Phone = Data.buyerPhone);
            TryRead(() => Res.Buyer.Fax = Data.buyerFax);
            TryRead(() => Res.Buyer.Email = Data.buyerEmail);
            TryRead(() => Res.Buyer.OrderRequestCountLastYear = Data.buyerOrderRequestCountLastYear);
            TryRead(() => Res.Buyer.ConfirmShippingDocAndAmount = Data.buyerConfirmShippingDocAndAmount);
            TryRead(() => Res.Buyer.FinancingType = Data.financingType);
            TryRead(() => Res.Buyer.ConfirmReleaseFunds = Data.buyerConfirmReleaseFunds);

            return Res;
        }


        public void Save(AlibabaContract data)
        {
            if (data == null)
                return;

            Log.Info("Saving Alibaba consumer data into DB...");

            var con = DB.GetPersistent();
            con.BeginTransaction();

            try
            {
                int ContractID = DB.ExecuteScalar<int>(con,
                    "SaveAlibabaContract",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaContract>("Tbl", new List<AlibabaContract> { data })
                    );

                if (data.Items.Any())
                {

                    foreach (var item in data.Items)
                    {
                        item.ContractId = ContractID;
                    }

                    DB.ExecuteNonQuery(con,
                        "SaveAlibabaContractItem",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<AlibabaContractItem>("Tbl", data.Items));
                }

                data.Seller.ContractID = ContractID;
                int SellerID = DB.ExecuteScalar<int>(con,
                    "SaveAlibabaSeller",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaSeller>("Tbl", new List<AlibabaSeller> { data.Seller })
                    );

                data.Seller.Bank.SellerID = SellerID;
                DB.ExecuteNonQuery(con,
                    "SaveAlibabaSellerBank",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaSellerBank>("Tbl", data.Seller.Bank)
                    );

                data.Buyer.ContractId = ContractID;
                DB.ExecuteNonQuery(con, "UpdateAlibabaBuyer", CommandSpecies.StoredProcedure,
                            new QueryParameter("CustomerId", data.Buyer.CustomerId),
                            new QueryParameter("ContractId", data.Buyer.ContractId),
                            new QueryParameter("AuthRepFname", data.Buyer.AuthRepFname),
                            new QueryParameter("AuthRepLname", data.Buyer.AuthRepLname),
                            new QueryParameter("BussinessName", data.Buyer.BussinessName),
                            new QueryParameter("City", data.Buyer.City),
                            new QueryParameter("ConfirmReleaseFunds", data.Buyer.ConfirmReleaseFunds),
                            new QueryParameter("ConfirmShippingDocAndAmount", data.Buyer.ConfirmShippingDocAndAmount),
                            new QueryParameter("Country", data.Buyer.Country),
                            new QueryParameter("Email", data.Buyer.Email),
                            new QueryParameter("Fax", data.Buyer.Fax),
                            new QueryParameter("FinancingType", data.Buyer.FinancingType),
                            new QueryParameter("OrderRequestCountLastYear", data.Buyer.OrderRequestCountLastYear),
                            new QueryParameter("Phone", data.Buyer.Phone),
                            new QueryParameter("State", data.Buyer.State),
                            new QueryParameter("Zip", data.Buyer.Zip),
                            new QueryParameter("street1", data.Buyer.street1),
                            new QueryParameter("street2", data.Buyer.street2)
                            );
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to save/update Alibaba data");
                con.Rollback();
                return;
            }

            con.Commit();

            Log.Info("Saving/updating Alibaba data into DB completed successfully");
        }

        private void TryRead(Action a)
        {
            try
            {
                a();
            }
            catch
            {
                return;
            }//Try
        }//TryRead

        private DateTime? TryReadDate(Func<DateTime> f)
        {
            try
            {
                DateTime d = f();
                return (d.Year < 1900) ? (DateTime?)null : d;
            }
            catch
            {
                return null;
            } // try
        } // TryReadDate

        //private byte[] TryReadFile(Func<Stream> f)
        //{
        //    try {
        //        Stream st = f();
        //        using (MemoryStream ms = new MemoryStream()) {
        //            st.CopyTo(ms);
        //            return ms.ToArray();
        //        }
        //    }
        //    catch{
        //        return null;
        //    } // try
        //} // TryReadFile
    }
}
