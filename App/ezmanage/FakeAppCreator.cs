﻿using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.ApplicationCreator;

namespace ezmanage
{
    public class FakeAppCreator : IAppCreator
    {
        public void AfterSignup(User user, string address)
        {
            
        }

        public void CashTransfered(User user, string firstName, decimal? cashAmount, decimal setUpFee)
        {
            
        }

        public void ThreeInvalidAttempts(User user, string firstName, string password)
        {
            
        }

        public void PasswordChanged(User user, string firstName, string password)
        {
            
        }

        public void PasswordRestored(User user, string emailTo, string firstName, string password)
        {
            
        }

        public void EbayAdded(Customer user, int umi)
        {
            
        }

        public void PayPalAdded(Customer user, int umi)
        {
            
        }

        public void Evaluate(User user, bool isUnderwriterForced = false)
        {
            
        }

        public void EvaluateWithIdHubCustomAddress(User user, int checkType, string houseNumber, string houseName, string street,
                                                   string district, string town, string county, string postcode, string bankAccount,
                                                   string sortCode)
        {
            
        }

        public void AmazonAdded(Customer user, int umi)
        {
            
        }

        public void GetCashFailed(User user, string firstName)
        {
            
        }

        public void PayEarly(User user, DateTime date, decimal? amount, string firstName)
        {
            
        }

        public void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer)
        {
            
        }

        public void ApprovedUser(User user, Customer customer, decimal? loanAmount)
        {
            
        }

        public void RejectUser(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreAMLInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreAMLandBWAInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreBWAInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void SendEmailVerification(User user, Customer customer, string address)
        {
            
        }

        public void PayPointAddedByUnderwriter(User user, Customer customer, string cardno)
        {
            throw new NotImplementedException();
        }

        public void UpdateAllMarketplaces(Customer customer)
        {
            throw new NotImplementedException();
        }

        public void FeeAdded(Customer customer, decimal? feeAmount)
        {
            throw new NotImplementedException();
        }

        public void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate)
        {
            
        }

        public void RenewEbayToken(Customer customer, string marketplaceName, string url)
        {
            throw new NotImplementedException();
        }

        public void Escalated(Customer customer)
        {
            throw new NotImplementedException();
        }

        public void CAISGenerate(User customer, string caisFilesLocationPath, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}