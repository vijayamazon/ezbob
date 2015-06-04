namespace Ezbob.Backend.Strategies.ExternalAPI
{
    using System;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Database;
    using Ezbob.Utils.Extensions;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using EZBob.DatabaseLib.Model.ExternalAPI;
    using StructureMap;

    public class SaveApiCall : AStrategy
    {
        public override string Name
        {
            get { return "SaveApiCall to system API"; }
        }

        public SaveApiCall(ApiCallData data)
        {
            Data = data;
            this.dataRep = ObjectFactory.GetInstance<ExternalApiLogRepository>();
        }

        public override void Execute() {

			try {
				// save to db
				ExternalApiLog apicall = new ExternalApiLog{
					RequestId = this.Data.RequestId,
					Request = this.Data.Request,
					Response = this.Data.Response,
					Url = this.Data.Url,
					StatusCode = this.Data.StatusCode,
					ErrorCode = this.Data.ErrorCode, 
					ErrorMessage = this.Data.ErrorMessage, 
					CreateDate = DateTime.UtcNow,
					Source = this.Data.Source ?? ExternalAPISource.Other.DescriptionAttr(),
					Comments = this.Data.Comments,
                    Customer=ObjectFactory.GetInstance<ICustomerRepository>().GetCustomerByRefNum(Data.CustomerRefNum)
				};

				Log.Debug(apicall);

				this.dataRep.Save(apicall);

			} catch (Exception ex) {
				Log.Alert(ex, string.Format("Failed to SaveApiCall requestID: {0}", Data.RequestId)); 
			}
			
		}

        public ApiCallData Data { get; private set; }

        private readonly ExternalApiLogRepository dataRep;

    }
}