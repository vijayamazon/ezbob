namespace EzBob3dParties.HMRC
{
    using System.Collections.Generic;
    using Common.Logging;
    using EzBobCommon;
    using HtmlAgilityPack;

    public class DLParser
    {
        [Injected]
        public ILog Log { get; set; }

        public IDictionary<string, string> Parse(HtmlNode dl, bool isFailOnEmptyDT) {
            Log.DebugFormat("DL parser started, fail on empty DT: {0}", isFailOnEmptyDT ? "yes" : "no");

            FsmState curState = FsmState.A;
            IDictionary<string, string> data = new Dictionary<string, string>();
            HtmlNode curChild = null;
            string curKey = null;
            bool isSuccess = false;

            while (curState != FsmState.I)
            {
                    Log.DebugFormat(
                        "State: {0}, Current Key: {1}, Current Child Tag Name: {2}",
                        curState,
                        (curKey ?? "-- null --"),
                        curChild == null ? "-- null --" : curChild.Name
                    );

                switch (curState)
                {
                    case FsmState.A:
                        data.Clear();
                        isSuccess = false;

                        curState = dl.FirstChild == null ? FsmState.I : FsmState.B;

                        break;

                    case FsmState.B:
                        curChild = (curChild == null) ? dl.FirstChild : curChild.NextSibling;
                        curState = FsmState.C;

                        break;

                    case FsmState.C:
                        if (curChild == null)
                        {
                            curState = FsmState.I;
                            isSuccess = true;
                        }
                        else
                        {
                            switch (curChild.Name.ToLower())
                            {
                                case "#text":
                                    curState = FsmState.B;
                                    break;

                                case "dt":
                                    curState = FsmState.D;
                                    break;

                                default:
                                    curState = FsmState.H;
                                    break;
                            } // switch
                        } // if

                        break;

                    case FsmState.D:
                        string sKey = curChild.InnerText;

                        if (string.IsNullOrWhiteSpace(sKey))
                        {
                            if (isFailOnEmptyDT)
                                curState = FsmState.H;
                            else if (curKey == null)
                                curState = FsmState.H;
                            else
                                curState = FsmState.E;
                        }
                        else
                        {
                            curKey = sKey.Trim();
                            curState = FsmState.E;
                        } // if

                        break;

                    case FsmState.E:
                        curChild = curChild.NextSibling;
                        curState = FsmState.F;

                        break;

                    case FsmState.F:
                        if (curChild == null)
                            curState = FsmState.H;
                        else
                        {
                            switch (curChild.Name.ToLower())
                            {
                                case "#text":
                                    curState = FsmState.E;
                                    break;

                                case "dd":
                                    curState = FsmState.G;
                                    break;

                                default:
                                    curState = FsmState.H;
                                    break;
                            } // switch
                        } // if

                        break;

                    case FsmState.G:
                        string sValue = curChild.InnerText.Trim();

                        if (data.ContainsKey(curKey))
                        {
                            data[curKey] += "\n" + sValue;
                            Log.DebugFormat("Appended[{0}] = {1}", curKey, sValue);
                        }
                        else
                        {
                            data[curKey] = sValue;
                            Log.DebugFormat("New[{0}] = {1}", curKey, sValue);
                        } // if

                        curState = FsmState.B;

                        break;

                    case FsmState.H:
                        Log.Warn("Wrong DL structure encountered.");
                        isSuccess = false;
                        curState = FsmState.I;

                        break;
                } // switch current state
            } // while

            if (isSuccess) {
                return data;
            }

            return null;
        }


        private enum FsmState
        {
            A, B, C, D, E, F, G, H, I
        } // FsmState
    }
}
