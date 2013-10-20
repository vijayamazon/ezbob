using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace MailApi.Model
{
    public class merge_var
    {
        public string name;
        public string content;
    }

    public class template_content
    {
        public string name;
        public string content;
    }

    public class attachment
    {
        public string type;
        public string name;
        public string content;
    }

    public class image
    {
        public string type;
        public string name;
        public string content;
    }

    public class EmailModel
    {
        public EmailModel()
        {
            template_content = new[]{new template_content{name = ""}};
        }

        public string key { get; set; }
        public string template_name { get; set; }
        public IEnumerable<template_content> template_content { get; set; }
        public EmailMessageModel message { get; set; }
		
        public void AddGlobalVariable(string name, string content)
        {
            if (message.global_merge_vars == null)
            {
                message.global_merge_vars = new List<merge_var>();
            }

            var mv = new merge_var
                {
                    name = name,
                    content = content
                };
            message.global_merge_vars.Add(mv);
        }

		public void AddAttachment(string mimeType, string fileName, string content)
		{
			if (message.attachments == null)
			{
				message.attachments = new List<attachment>();
			}

			message.attachments.Add(new attachment
				{
					type = mimeType,
					name = fileName,
					content = content
				});
		}
    }
}

// ReSharper restore InconsistentNaming