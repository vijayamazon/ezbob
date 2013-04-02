﻿using System;
using System.Linq;
using System.Xml.Linq;

namespace Deveel.Web.Zoho {
	public class ZohoResponse {
		private XElement resultNode;

		internal ZohoResponse(string module, string method, string responseContent) {
			if (module == null)
				throw new ArgumentNullException("module");
			if (method == null)
				throw new ArgumentNullException("method");

			Method = method;
			Module = module;
			ResponseContent = responseContent;

			var doc = XDocument.Parse(responseContent);
			LoadFromXml(doc.Root);
		}

		public string Module { get; private set; }

		public string Method { get; private set; }

		public bool IsError { get; protected set; }

		public string Message { get; private set; }

		public string Code { get; private set; }

		public bool NoData { get; private set; }

		internal string ResponseContent { get; private set; }

		public ZohoResponseException Error {
			get { return !IsError ? null : new ZohoResponseException(this); }
		}

		public void ThrowIfError() {
			if (IsError)
				throw Error;
		}

		internal virtual void LoadFromXml(XElement parent) {
			if (parent.Name != "response")
				throw new FormatException();

			var firstChild = parent.Elements().First();
			if (firstChild.Name == "error") {
				IsError = true;
				var code = firstChild.Descendants("code").First();
				var message = firstChild.Descendants("message").First();
				if (code != null)
					Code = code.Value;
				if (message != null)
					Message = message.Value;			
			} else if (firstChild.Name == "result") {
				LoadResultFromXml(firstChild);
			} else if (firstChild.Name == "nodata") {
				var code = firstChild.Descendants("code").First();
				var message = firstChild.Descendants("message").First();
				if (code != null)
					Code = code.Value;
				if (message != null)
					Message = message.Value;

				NoData = true;
			}
		}

		internal virtual void LoadResultFromXml(XElement resultElement) {
			if (resultElement.Descendants("code").Any()) {
				var code = resultElement.Descendants("code").First();
				var message = resultElement.Descendants("message").First();
				if (code != null)
					Code = code.Value;
				if (message != null)
					Message = message.Value;
			} else {
				resultNode = resultElement;
			}
		}

		internal ZohoEntityCollection<T> LoadCollectionFromResul<T>() where T :ZohoEntity {
			var collection = new ZohoEntityCollection<T>(true);
			if (!NoData) {
				var firstNode = resultNode.Elements().First();
				collection.LoadFromXml(firstNode);
			}
			return collection;
		}
	}

    public class ZohoConvertLeadResponse : ZohoResponse
    {
        public ZohoConvertLeadResponse(string module, string method, string responseContent)
            : base(module, method, responseContent)
        {
        }

        internal override void LoadFromXml(XElement parent)
        {
            if (parent.Name == "success")
            {
                var e = parent.Elements().First();
                Id = e.Value;
            }
            else
            {
                IsError = true;
                base.LoadFromXml(parent);
            }
        }

        public string Id { get; set; }
    }
}