﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Infra {
    using System.Net.Http;
    using Thinktecture.IdentityModel.Client;

    public class RestClient {
        private OAuth2Client auth2Client = new OAuth2Client(new Uri("http://localhost:12345/token"));
        private TokenResponse tokenResponse;

        public HttpClient GetClient() {
            if (this.tokenResponse == null) {
                try {
                    this.tokenResponse = this.auth2Client.RequestResourceOwnerPasswordAsync("test", "test")
                        .Result;
                } catch (AggregateException ex) {
                    int kk = 0;
                    return null;
                }
            }

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }

    }
}
