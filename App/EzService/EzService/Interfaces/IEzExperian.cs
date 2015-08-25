using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;

    public interface IEzExperian {
        [OperationContract]
        ActionMetaData BackfillCustomerAnalyticsCompany();

        [OperationContract]
        ActionMetaData BackFillExperianNonLtdScoreText();

        [OperationContract]
        ActionMetaData BackfillExperianConsumer();

        [OperationContract]
        ActionMetaData BackfillExperianDirectors(int? customerID);

        [OperationContract]
        ActionMetaData BackfillExperianLtd();

        [OperationContract]
        ActionMetaData BackfillExperianLtdScoreText();

        [OperationContract]
        ActionMetaData ExperianCompanyCheck(int userId, int customerID, bool forceCheck);

        [OperationContract]
        ActionMetaData ExperianConsumerCheck(int userId, int nCustomerID, int? nDirectorID, bool forceCheck);

        [OperationContract]
        IntActionResult GetExperianAccountsCurrentBalance(int customerId, int underwriterId);

        [OperationContract]
        DateTimeActionResult GetExperianCompanyCacheDate(int userId, string refNumber);

        [OperationContract]
        IntActionResult GetExperianConsumerScore(int customerId);

        [OperationContract]
        ActionMetaData UpdateExperianDirectorDetails(int? customerID, int? underwriterID, Esigner details);

        [OperationContract]
        ActionMetaData DeleteExperianDirector(int directorID, int underwriterID);

        [OperationContract]
        ExperianConsumerActionResult ParseExperianConsumer(long serviceLogId);

        [OperationContract]
        ExperianLtdActionResult ParseExperianLtd(long serviceLogID);

        [OperationContract]
        ExperianConsumerActionResult LoadExperianConsumer(int userId, int customerId, int? directorId, long? serviceLogId);

        [OperationContract]
        ExperianConsumerMortgageActionResult LoadExperianConsumerMortgageData(int userId, int customerId);

        [OperationContract]
        ExperianLtdActionResult LoadExperianLtd(long serviceLogID);

        [OperationContract]
        ExperianLtdActionResult CheckLtdCompanyCache(int userId, string companyRefNum);

        [OperationContract]
        CompanyDataForCompanyScoreActionResult GetCompanyDataForCompanyScore(int underwriterId, string refNumber);

        [OperationContract]
        CompanyDataForCreditBureauActionResult GetCompanyDataForCreditBureau(int underwriterId, string refNumber);

        [OperationContract]
        CompanyCaisDataActionResult GetCompanyCaisDataForAlerts(int underwriterId, int customerId);

        [OperationContract]
        ActionMetaData UpdateCustomerAnalyticsOnCompanyChange(int customerID);

        [OperationContract]
        ExperianTargetingActionResult ExperianTarget(int customerID, int userID, ExperianTargetingRequest request);

        [OperationContract]
        ActionMetaData WriteToServiceLog(int customerID, int userID, WriteToLogPackage.InputData packageInputData);
    }
}
