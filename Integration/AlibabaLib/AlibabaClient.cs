﻿namespace AlibabaLib {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using RestSharp;

	public class AlibabaClient {

		private readonly string baseUrl;
		private readonly string urlPath;
		private readonly string appSecret;

		public JsonSerializerSettings jf = new JsonSerializerSettings { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
	

		public AlibabaClient(string baseUrl, string path, string secret) {
			this.appSecret = secret; // "IOVVt8lbOfDE";
			this.baseUrl = baseUrl; // "http://119.38.217.38:1680/openapi/";
			this.urlPath = path; // "param2/1/alibaba.open/partner.feedback/89978";
		}

		public IRestResponse SendDecision(JObject obj, AlibabaBusinessType businessType) {

			IRestResponse response;

			StringDictionary parameters = new StringDictionary();
			StringBuilder datas = new StringBuilder(this.urlPath);

			obj.Add(new JProperty("requestID", new Random().Next(Int32.MaxValue).ToString()));
			obj.Add(new JProperty("bankCode", "ezbob"));
			obj.Add(new JProperty("bizType", businessType.DescriptionAttr()));

			if (businessType == AlibabaBusinessType.APPLICATION_REVIEW) {
				obj.Remove("locOfferStatus");
				obj.Remove("locOfferAmount");
				obj.Remove("locOfferDate");
				obj.Remove("locOfferExpireDate");
				obj.Remove("locOfferCurrency");
			} else {
				obj.Remove("locApproveStatus");
				obj.Remove("locApproveAmount");
				obj.Remove("locApproveDate");
				obj.Remove("locExpireDate");
				obj.Remove("locApproveCurrency");
			}

			string json = JsonConvert.SerializeObject(obj);

			parameters.Add("data", json);

			var request = new RestRequest(this.urlPath, Method.POST);
			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			// add json to POST form
			foreach (DictionaryEntry de in parameters) {
				request.AddParameter(de.Key.ToString(), de.Value);
				datas.Append(de.Key.ToString()).Append(de.Value);
			}

			Encoding enc = Encoding.UTF8;
			string signature;
			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(this.appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				request.AddParameter("_aop_signature", signature);
			}

			request.Parameters.ForEach(s => Debug.WriteLine(s.Name + "=" + s.Value));

			var client = new RestClient(this.baseUrl);

			try {
				response = client.Post(request);
				HttpStatusCode status = response.StatusCode;
			} catch (Exception e) {
				// log error TODO
				throw new HttpException(String.Format("Failed to process request to Alibaba for customer {0}, alibabaID {1}, error: {2}", arg0: obj.GetValue("aId"), arg1: obj.GetValue("aliMemberId"), arg2: e));
			}

			return response;
		}
	}
}
