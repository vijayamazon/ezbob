namespace Ezbob.API.AuthenticationAPI.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Ezbob.API.AuthenticationAPI.Models;
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Utils.Extensions;
    using WebApi.OutputCache.V2;
    using ServiceClientProxy;

    [RoutePrefix("api/Contracts")]
    public class ContractsController:ApiController
    {
        [Route("Contract", Name = "GetPaymentRequest")]
        [HttpPost]
        [ResponseType(typeof(AlibabaDto))]
        [ValidateModelState]
        [Authorize(Roles = "PartnerAlibaba")]
        [CacheOutput(ClientTimeSpan = 60, ServerTimeSpan = 60, ExcludeQueryStringFromCacheKey = true)] // client cache length in seconds
        public async Task<IHttpActionResult> GetAlibabaPaymentRequest([FromBody] AlibabaContractDto data) {
            try
            {
                ServiceClient client = new ServiceClient();
                var result = client.Instance.SaleContract(data.aId, data).Result;

                string url = ActionContext.Request.RequestUri.ToString();

                var response = new AlibabaDto()
                {
                    requestId = data.requestId,
                    responseId = data.responseId,
                    aId = data.aId,
                    aliMemberId = data.aliMemberId,
                    url = url
                };

                // customerID and aliMemberID doesn't match each other in in system DB
                if (result.aId == null && result.aliMemberId == null)
                {
                    response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH;
                    response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH.DescriptionAttr();
                    Helper.SaveApiLog<AlibabaContractDto, AlibabaDto>(data, response, data.requestId, data.aId, "400", "", response.errCode.DescriptionAttr(), response.errMsg, response.url, ActionContext.Request.Headers);
                    return BadRequest(response.errMsg);
                }

                // customerID not found in system DB
                if (result.aId == null)
                {
                    response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND;
                    response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND.DescriptionAttr();
                    Helper.SaveApiLog<AlibabaContractDto, AlibabaDto>(data, response, data.requestId, data.aId, "400", "", response.errCode.DescriptionAttr(), response.errMsg, response.url, ActionContext.Request.Headers);
                    return BadRequest(response.errMsg);
                }

                // ali memberID not found in system DB
                if (result.aliMemberId == null)
                {
                    response.errCode = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND;
                    response.errMsg = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND.DescriptionAttr();
                    Helper.SaveApiLog<AlibabaContractDto, AlibabaDto>(data, response, data.requestId, data.aId, "400", "", response.errCode.DescriptionAttr(), response.errMsg, response.url, ActionContext.Request.Headers);
                    return BadRequest(response.errMsg);
                }
                Helper.SaveApiLog<AlibabaContractDto, AlibabaDto>(data, response, data.requestId, data.aId, "200", "", response.errCode.DescriptionAttr(), response.errMsg, response.url, this.ActionContext.Request.Headers);
                
                // set cache for 1 minute 
                //cache.Add(cachKey, response, DateTime.Now.AddSeconds(60), null);

                return Ok(response);

            }
            catch (Exception e)
            {
                Trace.TraceError(DateTime.UtcNow + ": " + e);
                return InternalServerError();
            }
        }

    }
}