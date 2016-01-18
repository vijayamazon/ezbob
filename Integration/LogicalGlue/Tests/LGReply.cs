namespace Ezbob.Integration.LogicalGlue.Tests {
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Newtonsoft.Json;
	using NUnit.Framework;

	[TestFixture]
	class LGReply : ABaseTest {
		[Test]
		public void TestLGReply() {
			Reply reply = JsonConvert.DeserializeObject<Reply>(RawReply);

			Log.Msg("Raw reply: {0}", RawReply);

			Log.Msg("Deserialized: {0}", reply);
		} // TestLGReply

		private const string RawReply = @"{
  ""status"" : ""200"",
  ""logicalglue"" : {
    ""decision"" : {
      ""models"" : {
        ""FL_response"" : ""{\""inferenceResultEncoded\"":-2.147483445E9,\""score\"":0.11924927902253624,\""inferenceResultDecoded\"":\""GOOD\"",\""warnings\"":[],\""status\"":\""SUCCESS\"",\""mapOutputRatios\"":{\""BAD\"":0.11924927902253628,\""GOOD\"":0.8807507209774638}}"",
        ""NN_response"" : ""{\""inferenceResultEncoded\"":0.0,\""score\"":0.1455421191179597,\""inferenceResultDecoded\"":\""GOOD\"",\""warnings\"":[],\""status\"":\""SUCCESS\"",\""mapOutputRatios\"":{}}""
      },
      ""reason"" : ""Bucket A, model accepts, number of debt collector searches last 24 months > 0"",
      ""outcome"" : ""Refer"",
      ""bucket"" : ""A""
    }
  },
  ""etl"" : {
    ""code"" : ""P""
  },
  ""equifax"" : {
    ""equifax_response"" : ""eq-fax""
  }
}";
	} // class LGReply
} // namespace
