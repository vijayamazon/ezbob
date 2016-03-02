namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class TestHarvester : IHarvester {
		public TestHarvester(ASafeLog log) {
			this.log = log.Safe();
			ReplyMode = ReplyModes.Random;
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration cfg) {
			string reply = GetReply();

			this.log.Debug("Chosen reply in test harvester:\n{0}", reply);

			return new Response<Reply>(Status, reply);
		} // Infer

		public ReplyModes ReplyMode { get; set; }

		public enum ReplyModes {
			Random,
			Success,
			BadInferenceRequest,
			BadEtlRequest,
			EquifaxTimeout,
			LogicalGlueInferenceApiTimeout,
			HardRejection,
			EtlFailBadAddress,
			TimeoutA,
			TimeoutL,
		} // enum ReplyModes

		private HttpStatusCode Status { get; set; }

		private string GetReply() {
			string now = DateTime.UtcNow.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			return ChooseReply().Replace("__NOW__", now);
		} // GetReply

		private string ChooseReply() {
			switch (ReplyMode) {
			case ReplyModes.Success:
				Status = HttpStatusCode.OK;
				return CreateSuccess();

			case ReplyModes.BadInferenceRequest:
				Status = HttpStatusCode.BadRequest;
				return BadInferenceRequest;

			case ReplyModes.BadEtlRequest:
				Status = HttpStatusCode.BadRequest;
				return BadEtlRequest;

			case ReplyModes.EquifaxTimeout:
				Status = HttpStatusCode.GatewayTimeout;
				return EquifaxTimout;

			case ReplyModes.LogicalGlueInferenceApiTimeout:
				Status = HttpStatusCode.GatewayTimeout;
				return LogicalGlueInferenceApiTimout;

			case ReplyModes.HardRejection:
				Status = HttpStatusCode.OK;
				return HardRejection;

			case ReplyModes.EtlFailBadAddress:
				Status = HttpStatusCode.OK;
				return ReadEmbeddedFile("etl_F_bad_address.json");

			case ReplyModes.TimeoutA:
			case ReplyModes.TimeoutL:
				Status = HttpStatusCode.GatewayTimeout;
				return ReadEmbeddedFile(ReplyMode + ".json");
			} // switch

			int rnd = new Random().Next(1, 101);

			if (rnd <= 85) {
				Status = HttpStatusCode.OK;
				return CreateSuccess();
			} // if

			if (rnd <= 88) {
				Status = HttpStatusCode.BadRequest;
				return BadInferenceRequest;
			} // if

			if (rnd <= 91) {
				Status = HttpStatusCode.BadRequest;
				return BadEtlRequest;
			} // if

			if (rnd <= 94) {
				Status = HttpStatusCode.GatewayTimeout;
				return EquifaxTimout;
			} // if

			if (rnd <= 97) {
				Status = HttpStatusCode.GatewayTimeout;
				return LogicalGlueInferenceApiTimout;
			} // if
			
			Status = HttpStatusCode.OK;
			return HardRejection;
		} // ChooseReply

		private static string ReadEmbeddedFile(string fileNameBase) {
			string fileName = "Ezbob.Integration.LogicalGlue.Harvester.Implementation." + fileNameBase;

			Stream stream = typeof(TestHarvester).Assembly.GetManifestResourceStream(fileName);

			if (stream == null)
				throw new Exception("Failed to read embedded file " + fileName);

			var reader = new StreamReader(stream);

			string json = reader.ReadToEnd();

			reader.Close();

			stream.Close();

			return json;
		} // ReadEmbeddedFile

		private static string CreateSuccess() {
			var fl = ModelContent.Replace("__MAP_OUTPUT_RATIOS__", MapOutputRatios);
			var nn = ModelContent.Replace("__MAP_OUTPUT_RATIOS__", string.Empty);

			return SuccessfulRequest
				.Replace("__FL_MODEL__", fl)
				.Replace("__NN_MODEL__", nn);
		} // CreateSuccess

		private const string SuccessfulRequest = @"{
	""status"": 200,
	""etl"": {
		""code"": ""P"",
		""message"": ""Successfully processed '__NOW__'""
	},
	""equifax"": ""<xml>Equifax data for successful request '__NOW__'.</xml>"",
	""logicalGlue"": {
		""FL"": __FL_MODEL__,
		""NN"": __NN_MODEL__,
		""bucket"": 1
	}
}";

		private const string BadInferenceRequest = @"{
	""status"": 400,
	""etl"": {
		""code"": ""P"",
		""message"": ""Successfully processed '__NOW__'""
	},
	""equifax"": ""<xml>Equifax data for bad inference request '__NOW__'.</xml>"",
	""logicalGlue"": {
		""FL"": __FL_MODEL__,
		""NN"": __NN_MODEL__,
		""bucket"": 1
	}
}";

		private const string BadEtlRequest = @"{
	""status"": 400,
	""error"": ""Company Identifier must be a number '__NOW__'""
}";

		private const string EquifaxTimout = @"{
	""status"": 504,
	""timeout"": ""E"",
	""error"": ""Equifax Unavailable '__NOW__'""
}";

		private const string LogicalGlueInferenceApiTimout = @"{
	""status"": 504,
	""timeout"": ""L"",
	""error"": ""Logical Glue Inference Service Unavailable '__NOW__'"",
	""equifax"": ""<xml>Equifax data for LG inference API timeout '__NOW__'.</xml>""
}";

		private const string HardRejection = @"{
	""status"": 200,
	""etl"": {
		""code"": ""R"",
		""message"": ""Hard Rejection Rule Fired: Company is less than 2 years old '__NOW__'""
	},
	""equifax"": ""<xml>Equifax data for hard rejection '__NOW__'.</xml>"",
	""logicalGlue"": {
		""FL"": __FL_MODEL__,
		""NN"": __NN_MODEL__,
		""bucket"": 1
	}
}";

		private const string MapOutputRatios = @"""mapOutputRatios"": {
	""BAD"": 0.5927377710432571,
	""GOOD"": 0.4072622289567429
},";

		private const string ModelContent = @"{
	""score"": 0.5927377710432571,
	""inferenceResultEncoded"": -2147483446,
	""inferenceResultDecoded"": ""BAD"",
	""warnings"": [
		{ ""featureName"": ""feature"", ""maxValue"": ""100"", ""minValue"": ""0"", ""currentValue"": ""ab"" },
		{ ""featureName"": ""FEATURE"", ""maxValue"": ""900"", ""minValue"": ""1"", ""currentValue"": ""-1"" }
	],
	__MAP_OUTPUT_RATIOS__
	""status"": ""SUCCESS"",
	""exception"": ""some exception"",
	""errorCode"": ""no error code __NOW__"",
	""missingColumns"": [
		""missing 0"",
		""missing 1"",
		""missing 2""
	],
	""encodingFailures"": [
		{
			""rowIndex"": 0,
			""columnName"": ""Bad encoded col __NOW__"",
			""unencodedValue"": ""a value"",
			""reason"": ""good reason"",
			""message"": ""it's bad""
		},
		{
			""rowIndex"": 1,
			""columnName"": ""Another bad encoded col __NOW__"",
			""unencodedValue"": ""another value"",
			""reason"": ""bad reason"",
			""message"": ""it's really bad""
		}
	],
	""uuid"": ""2626f582-bd52-4d92-95fc-c2ec2ee1b73c""
}";
		private readonly ASafeLog log;
	} // class TestHarvester
} // namespace
