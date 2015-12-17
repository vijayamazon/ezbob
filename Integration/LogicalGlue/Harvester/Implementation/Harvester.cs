namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using Ezbob.Integration.LogicalGlue.Exceptions.Harvester;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Newtonsoft.Json;

	public class Harvester : IHarvester {
		public Harvester(ASafeLog log) {
			this.log = log.Safe();
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration cfg) {
			ValidateInput(inputData, cfg);

			using (var restClient = new HttpClient { BaseAddress = new Uri("https://" + cfg.HostName), }) {
				HttpRequestMessage request = CreateRequest(inputData, cfg, restClient);

				HttpResponseMessage response = restClient.SendAsync(request).Result;

				string responseContent = response.Content.ReadAsStringAsync().Result;

				this.log.Say(
					response.StatusCode == HttpStatusCode.OK ? Severity.Debug : Severity.Warn,
					"Response status code is {0}, content:\n" +
					"=== Start of encrypted and serialized text ===\n" +
					"{1}\n" +
					"=== End of encrypted and serialized text ===",
					response.StatusCode,
					new Encrypted(responseContent)
				);

				return new Response<Reply>(response.StatusCode, responseContent);
			} // using
		} // Infer

		private HttpRequestMessage CreateRequest(
			InferenceInput inputData,
			HarvesterConfiguration cfg,
			HttpClient restClient
		) {
			string path = string.IsNullOrWhiteSpace(inputData.EquifaxData)
				? cfg.NewCustomerRequestPath
				: cfg.OldCustomerRequestPath;

			string requestUri = string.Format(path, inputData.RequestID);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

			request.Headers.Authorization = new AuthenticationHeaderValue(cfg.AuthorizationScheme, cfg.AccessToken);
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("EverlineLogicalGlueConnector", "1.0"));

			string serialized = JsonConvert.SerializeObject(inputData);

			request.Content = new StringContent(serialized, new UTF8Encoding(), JsonContentType);

			string contentToLog = serialized.TrimStart();

			const int maxLen = 256;

			if (contentToLog.Length > maxLen)
				contentToLog = contentToLog.Substring(0, maxLen).TrimEnd() + "...";

			var uri = new Uri(restClient.BaseAddress, request.RequestUri);

			this.log.Debug(
				"Executing {0} request:\nURL: {1}\nHeaders:\n\t{2}\nContent:\n\t{3}",
				request.Method,
				uri,
				string.Join("\n\t", request.Headers.Select(pair => string.Format(
					"{0}: {1}",
					pair.Key,
					string.Join(", ", pair.Value)
				))),
				contentToLog
			);

			return request;
		} // CreateRequest

		private void ValidateInput(InferenceInput inputData, HarvesterConfiguration cfg) {
			if (cfg == null)
				throw new NoConfigurationAlert(this.log);

			List<string> errors = cfg.Validate();

			if (errors != null)
				throw new BadConfigurationAlert(errors, this.log);

			if (inputData == null)
				throw new NoInputDataAlert(this.log);

			errors = inputData.Validate();

			if (errors != null)
				throw new BadInputDataAlert(errors, this.log);
		} // ValidateInput

		private readonly ASafeLog log;
		private const string JsonContentType = "application/json";
	} // class Harvester
} // namespace
