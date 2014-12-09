using System;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.Collections;

namespace com.yodlee.sampleapps
{
	using System.Collections.Generic;

	/// <summary>
	/// Search
	/// </summary>
    public class Search : ApplicationSuper
    {
        SearchService searchService = null;

        public Search()
        {

            searchService = new SearchService();
            searchService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "SearchService";
        }

		public Dictionary<long, string> viewUkBank(string keywords)
		{
			object[] searchResults = null;
			try
			{
				searchResults = searchService.searchContentServicesByContainerType(getCobrandContext(), new string[] { "bank" }, keywords);
			}
			catch (Exception ex)
			{

				System.Console.WriteLine("Exception: " + ex.StackTrace);
			}
			var services = new Dictionary<long, string>();
			if (searchResults != null && searchResults.Length > 0)
			{
				XmlNode[] csiNodes = null;
				for (int i = 0; i < searchResults.Length; i++)
				{
					csiNodes = (XmlNode[])searchResults[i];
					int found = 0;
					long csid = 0;
					string csname = string.Empty;
					for (int j = 0; j < csiNodes.Length; j++)
					{
						XmlNode csiNode = csiNodes[j];
						switch (csiNode.Name.ToLower())
						{
							case "contentserviceid":
								csid = Convert.ToInt64(csiNode.InnerText);
								found++;
								break;
							case "sitedisplayname":
								csname = csiNode.InnerText;
								found++;
								break;
						}
						if (found == 2) break;
						//cs.contentServiceId

					}
					services.Add(csid, csname);
					System.Console.WriteLine(i + ".) " + csname + " csId=" + csid);
				}
			}
			else
			{
				System.Console.WriteLine("No search results returned");
			}
			System.Console.WriteLine("\n");
			return services;
		}
        public void searchByKeywords(String keywords)
        {
            object[] searchResults = null; 

            try
            {
                String str = andKeywords(keywords);
                System.Console.WriteLine("Search String = "+str);
                searchResults = searchService.searchContentServices(getCobrandContext(),str );
            }
            catch (SoapException soapEx)
            {
                System.Console.WriteLine("Exception: " + soapEx.StackTrace);
                /*CoreException coreEx = ExceptionHandler.processException(soapEx);
                if (coreEx != null)
                {
                    if (coreEx is InvalidCobrandContextException)
                    {
                        System.Console.WriteLine("InvalidCobrandContextException while searching for keywords [" + keywords + "]");
                    }
                    else if (coreEx is ImproperKeywordsException)
                    {
                        System.Console.WriteLine("ImproperKeywordsException while searching for keywords [" + keywords + "]");
                    }
                    else
                    {
                        System.Console.WriteLine("Other Exception: " + coreEx.message);
                    }
                    //System.Environment.Exit(-1);
                }*/
            }

            if (searchResults != null && searchResults.Length > 0)
            {
                XmlNode[] csiNodes = null;
                for (int i = 0; i < searchResults.Length; i++)
                {
                    csiNodes = (XmlNode[])searchResults[i];
                    int found = 0;
                    long csid = 0;
                    string csname = string.Empty;
                    for (int j = 0; j < csiNodes.Length; j++)
                    {
                        XmlNode csiNode = csiNodes[j];
                        switch (csiNode.Name.ToLower())
                        {
                            case "contentserviceid":
                                csid = Convert.ToInt64(csiNode.InnerText);
                                found++;
                                break;
                            case "sitedisplayname":
                                csname = csiNode.InnerText;
                                found++;
                                break;
                        }
                        if (found == 2) break;
                        //cs.contentServiceId

                    }
                    System.Console.WriteLine(i + ".) " + csname + " csId=" + csid);                  
                }
            }
            else
            {
                System.Console.WriteLine("No search results returned");
            }
            System.Console.WriteLine("\n");
        }

        private static String formatKeyword(String keyword)
        {
            //System.Console.WriteLine("formatKeyword(" + keyword + ")");
            String ret = "";
            if (keyword != null)
            {
                keyword = keyword.Trim();
                if (keyword.Length > 0)
                {
                    ret += keyword;
                    if (!keyword.EndsWith("*"))
                    {
                        ret += "*";
                    }
                }
            }
            return ret.ToLower();
        }

        private static String andKeywords(String keywords)
        {
            //System.Console.WriteLine("andKeywords(" + keywords + ")");
            String sb = "";
            if (keywords != null)
            {
                StringTokenizer st = new StringTokenizer(keywords, " ");
                String token;
                while (st.HasMoreTokens())
                {
                    token = st.NextToken();
                    sb += formatKeyword(token);
                    if (st.HasMoreTokens())
                    {
                        sb += " OR ";
                    }
                }
            }
            //System.Console.WriteLine("search key=" + sb);
            return sb;
        }

    }
}
