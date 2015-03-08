namespace EzbobAPI {
	using System.ServiceModel.Channels;

	public class JsonContentTypeMapper : WebContentTypeMapper {
		public override WebContentFormat GetMessageFormatForContentType(string contentType) {
			if (contentType.Contains("application/json"))
				return WebContentFormat.Json;
			if (contentType == "text/javascript")
				return WebContentFormat.Json;
			return WebContentFormat.Default;
		}
	}
}
