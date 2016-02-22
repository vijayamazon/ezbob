using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesTests
{
    using System.Diagnostics;
    using eBay.Service.Call;
    using eBay.Service.Core.Soap;
    using EzBob3dParties.EBay;
    using EzBob3dPartiesTests.Properties;
    using EzBobCommon;
    using EzBobCommon.Configuration;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class EbayTests : TestBase {
        private string token = "AgAAAA**AQAAAA**aAAAAA**nshJVg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFloajCJODqQWdj6x9nY+seQ**kXgBAA**AAMAAA**DkwwnP/XqTyiAuVbk5cahe2VO+cthn6HTASwCzvCtlsBtK/ZOEAohDpYwJtxWnUAYD1wq9HvzXCy3C57V8n0h4P4hZ26o4ccIJLmP5JDQDsHZr3hqcWjOo0zmE3BMmEcpkdXF9f6q57BK/TMFAr3vvmatwdwBhYJ5v2/tR2/lE3CLGUsBPU+8+PqoR2jNQLe/yrqBrCn8mAXVzWuqEOJZeX+Ej2aVc/NxCKXb3BiMD9LmtAOFmwayOA680cvXcnxCuWk9g27DVTZhRYYFnqCE8ha6poLBB40G9JmrjVtXXi4X31BK06kt3aYVvE5wItLIF+inWdXOk570g2WDnywaEVSXJ2ocq5Dp/dpR15D5e3Bo5qxCZBkVJGyQkx56X7QNioTsO2qD3MPUEAfCnZf2Coy/C7pmLrctd5/1QaoFwt1AUpXTcAjpYv6oDLYnxn4wjy5mozWd/BygrJTsgMiFkAU88VWm5+hcTHxm1OG4pVQOie8TbrWqKLrTSi7j4ZAy4TPqpG2A8UFscjJnAweX3/XR6JFIQdX1X46B3h9pKFMYLBQ+UVlm+Qff1DNWxjK8kUR559VlYpho2Y3bVn+tLtlL5YwVDvjNmZ+v/bkypGhPm/Xp2qbZBPdB7i63Yzmc51DULNftfYOwIFXpOetPd5gbsIWdSN1ZscMizNLQnGDRbJ05p8pFY6YDbOTpQ2eWoTtQFlZ/tukyF2kGQ1zOR7Bdb8ofkJEqYNymMbFJr8hahtNupQgakRYC61EQENP";
//        private string token = "AgAAAA**AQAAAA**aAAAAA**oF7wVQ**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYakCZGAogqdj6x9nY+seQ**kXgBAA**AAMAAA**NSKJd0ZWRUcknmJiTxXhgqtuUQH1e0vwRdfrsgV2tLY663pqQhig6MBR0nBchxd+ZiNHGvHS3Wlf/PBEDod/QZsy4qxBaXXnkmDgnCVmCgjz91Dt70G9915l8Dh3+V7xlGm4mrCCOftxSRB78sfr9Re62IQU9914JHrrm5lHdJoI6dvjpIoyGQhpnDKG1qo694dkPGzb+p9T+2sYSW/8bRHJpvoEowZGnfjdefLNUKGXpFjIF9WeiYLM0V1vqTdoj3DzcYMo83jOareHuc9yQmBf1dPRBrwkippHoZP3QQUrjtWDUgWxgNFnIdKGMNlGTaFiSfb43xR19DopUBOGd5K1VVHZli02BFUjU0Uj2/6h95X5vzM1QQtnqXuijSPdQkm16D61w55UKfwpr8DLQYXrajdm6U29bc7vVfcqKLo1hNEsmKcnhkXorPFKwgF638Yp2TXjTCStU/zfeU+GTX9qeRdaLC79ysjaMVNM+Bb3efdDpESETZXy1DJ6WGXHUMnmLynWG6zz5Hhp9BOQ2+pXWW+qlwPg7J119jUeLtuRohqGnHczIPTq+gkdRf1rVR6aRCoMLx7ZxRhC6O9W/HR7brlIX2N4k1kvlgYHKN7Hpkyf/3890xp+1lCiCZfjAdMaoaIT0OLJsed/qHhHmFHb3l+06UD5hQ7IOXzl5WKgrcedIDNzet6T64Eogxkog0WXrYcrpfaOpCqD8nXRpS3UjYzVrFOHp7m04xTG39RjgV535hFFu1zKqwCo2y9/";
        [Test]
        public async void Test() {
         
            IContainer container = InitContainer(typeof(EBayService));
            var configManager = container.GetInstance<ConfigManager>();
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            var ebayService = container.GetInstance<EBayService>();

//            var account = await ebayService.GetAccount(this.token);
//            var userData = await ebayService.GetUserData(this.token);
            Stopwatch watch = new Stopwatch();
            watch.Start();
//            var from = new DateTime(2015, 1, 1, 0,0,0, DateTimeKind.Utc);
//            var to = from.AddDays(30);
//            var orders = await ebayService.GetOrders(this.token, from, to);
            var orders = await ebayService.GetOrders(this.token, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);

            var transactions = orders.AsParallel()
                .SelectMany(o => o.ApiResponse.OrderArray.Cast<OrderType>()
                    .SelectMany(t => t.TransactionArray.Cast<TransactionType>())).ToArray();
            
            var min = transactions.Min(o => o.CreatedDate.Ticks);

            DateTime date = new DateTime(min);

            
            watch.Stop();
            string time = watch.Elapsed.ToString();
            watch.Restart();



            var total = orders.AsParallel()
                .SelectMany(o => o.ApiResponse.OrderArray.Cast<OrderType>())
                .ToArray();

            int totalCount = total.Length;

            var distinct = total.Distinct(new EqComparer<OrderType>((o1, o2) => o1.OrderID.Equals(o2.OrderID), od => od.OrderID.GetHashCode()))
                .ToArray();
            int distinctCount = distinct.Length;

            watch.Stop();
            var time2 = watch.Elapsed.ToString();
            int kk = 0;
        }
    }
}
