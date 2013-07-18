// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Linq;

namespace MailApi.Model
{
    public class RenderTemplateModel
    {
        public string key { get; set; }
        public string template_name { get; set; }
        public IEnumerable<template_content> template_content { get; set; }
        public IEnumerable<merge_var> merge_vars { get; set; }
    }

    public static class Utils
    {
        public static List<merge_var> AddMergeVars(Dictionary<string, string> parameters)
        {
            var retVal = new List<merge_var>();
            retVal.AddRange(parameters.Select(param => new merge_var
                {
                    name = param.Key,
                    content = param.Value
                }));
            return retVal;
        }
    }
}

// ReSharper restore InconsistentNaming