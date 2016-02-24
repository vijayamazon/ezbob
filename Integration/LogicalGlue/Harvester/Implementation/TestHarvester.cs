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

			return new Response<Reply>(HttpStatusCode.OK, reply);
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
		} // enum ReplyModes

		private string GetReply() {
			string now = DateTime.UtcNow.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			return ChooseReply().Replace("__NOW__", now);
		} // GetReply

		private string ChooseReply() {
			switch (ReplyMode) {
			case ReplyModes.Success:
				return CreateSuccess();

			case ReplyModes.BadInferenceRequest:
				return BadInferenceRequest;

			case ReplyModes.BadEtlRequest:
				return BadEtlRequest;

			case ReplyModes.EquifaxTimeout:
				return EquifaxTimout;

			case ReplyModes.LogicalGlueInferenceApiTimeout:
				return LogicalGlueInferenceApiTimout;

			case ReplyModes.HardRejection:
				return HardRejection;

			case ReplyModes.EtlFailBadAddress:
				return ReadEtlFailBadAddress();
			} // switch

			int rnd = new Random().Next(1, 101);

			if (rnd <= 85)
				return CreateSuccess();

			if (rnd <= 88)
				return BadInferenceRequest;

			if (rnd <= 91)
				return BadEtlRequest;

			if (rnd <= 94)
				return EquifaxTimout;

			if (rnd <= 97)
				return LogicalGlueInferenceApiTimout;
			
			return HardRejection;
		} // ChooseReply

		private static string ReadEtlFailBadAddress() {
			const string fileName = "Ezbob.Integration.LogicalGlue.Harvester.Implementation.etl_F_bad_address.json";

			Stream stream = typeof(TestHarvester).Assembly.GetManifestResourceStream(fileName);

			if (stream == null)
				throw new Exception("Failed to read embedded file " + fileName);

			var reader = new StreamReader(stream);

			string json = reader.ReadToEnd();

			reader.Close();

			stream.Close();

			return json;
		} // ReadEtlFailBadAddress

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
