namespace EchoSignLib.Rest.Api
{
    using System;

    internal class EchoSignRefreshAccessTokenResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; } //sec
        public DateTime Timestamp { get; set; }

        public bool IsTimeOut() {
            TimeSpan span = DateTime.Now - Timestamp;
            int margin = 15;//sec
            if (span.TotalSeconds > expires_in - margin) {
                return true;
            }

            return false;
        }
    }
}
