namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using NUnit.Framework;

	[TestFixture]
	class AddressTest : ABaseTest  {
		[Test]
		public void TestAddresses() {
			var os = new List<string>();

			os.Add("Line1,Line2,Line3,House name,House number,Flat/Apt,Address1,Address2");

			foreach (var tpl in testData) {
				var cam = new CustomerAddressModel {
					Line1 = tpl.Item1,
					Line2 = tpl.Item2,
					Line3 = tpl.Item3,
				};

				cam.FillDetails();

				os.Add(string.Join(",", new [] {
					cam.Line1,
					cam.Line2,
					cam.Line3,
					cam.HouseName,
					cam.HouseNumber,
					cam.FlatOrApartmentNumber,
					cam.Address1,
					cam.Address2
				}));
			} // for each

			Log.Msg("\n\n{0}\n\n", string.Join("\n", os));
		} // TestAddresses

		private readonly List<Tuple<string, string, string>> testData = new List<Tuple<string, string, string>> {
			new Tuple<string, string, string>("103 Holland Gardens", "", "" ),
			new Tuple<string, string, string>("11 Keld Close", "", "" ),
			new Tuple<string, string, string>("115 Godinton Road", "", "" ),
			new Tuple<string, string, string>("12 The Coppice", "Easington Colliery", "" ),
			new Tuple<string, string, string>("124 Hollywood Avenue", "Gosforth", "" ),
			new Tuple<string, string, string>("129 Broadmead Avenue", "", "" ),
			new Tuple<string, string, string>("13 Station Road", "Long Eaton", "" ),
			new Tuple<string, string, string>("14 Yalland Close", "", "" ),
			new Tuple<string, string, string>("15-17 The Walk", "", "" ),
			new Tuple<string, string, string>("153 Kenry Street", "", "" ),
			new Tuple<string, string, string>("16 Mount Pleasant", "", "" ),
			new Tuple<string, string, string>("16 Suckling Avenue", "", "" ),
			new Tuple<string, string, string>("16 Wear Road", "", "" ),
			new Tuple<string, string, string>("169 Plashet Road", "", "" ),
			new Tuple<string, string, string>("18 Tyldesley Way", "", "" ),
			new Tuple<string, string, string>("185 Weedon Road", "", "" ),
			new Tuple<string, string, string>("19 Stuart Road", "", "" ),
			new Tuple<string, string, string>("19 Swallow Court", "", "" ),
			new Tuple<string, string, string>("196 High Road", "", "" ),
			new Tuple<string, string, string>("2 Redvale Drive", "", "" ),
			new Tuple<string, string, string>("2 Rose Cottage", "Dale Road", "Coalbrookdale" ),
			new Tuple<string, string, string>("2 St. Ann`s Green", "", "" ),
			new Tuple<string, string, string>("20 Davies Row", "Hirwaun", "" ),
			new Tuple<string, string, string>("20 Orchard Lane", "Moorends", "" ),
			new Tuple<string, string, string>("21 Eagles Chase", "Wick", "" ),
			new Tuple<string, string, string>("22 Maesycoed", "", "" ),
			new Tuple<string, string, string>("23 Priestfields", "", "" ),
			new Tuple<string, string, string>("23 Ryefield Avenue", "", "" ),
			new Tuple<string, string, string>("23 St. Nicholas Avenue", "", "" ),
			new Tuple<string, string, string>("230 Eastcourt Green", "", "" ),
			new Tuple<string, string, string>("23A Birchfield Avenue", "", "" ),
			new Tuple<string, string, string>("24-26 Upperkirkgate", "", "" ),
			new Tuple<string, string, string>("247 Osmaston Park Road", "", "" ),
			new Tuple<string, string, string>("27 Buchan Street", "", "" ),
			new Tuple<string, string, string>("28 Victoria Street", "Sacriston", "" ),
			new Tuple<string, string, string>("29 Hilltown Road", "Rathfriland", "" ),
			new Tuple<string, string, string>("3 Linford Close", "", "" ),
			new Tuple<string, string, string>("30A Belgrave Road", "", "" ),
			new Tuple<string, string, string>("33 North Court Close", "Rustington", "" ),
			new Tuple<string, string, string>("33 Victoria Road", "", "" ),
			new Tuple<string, string, string>("407 Yardley Green Road", "Bordesley Green", "" ),
			new Tuple<string, string, string>("42 New Road", "", "" ),
			new Tuple<string, string, string>("43 Ridings Road", "Coalpit Heath", "" ),
			new Tuple<string, string, string>("45 Leys Road", "", "" ),
			new Tuple<string, string, string>("47 Bramble Bank", "", "" ),
			new Tuple<string, string, string>("47 Coalford", "Jackfield", "" ),
			new Tuple<string, string, string>("4A Rowallen Parade", "Green Lane", "" ),
			new Tuple<string, string, string>("5 Watling Street", "Elstree", "" ),
			new Tuple<string, string, string>("50 Parkland Crescent", "", "" ),
			new Tuple<string, string, string>("51 Thornville Street", "", "" ),
			new Tuple<string, string, string>("51 Weirside Gardens", "", "" ),
			new Tuple<string, string, string>("54 Pengegon Parc", "", "" ),
			new Tuple<string, string, string>("55 Asquith Road", "", "" ),
			new Tuple<string, string, string>("55 The Precinct", "Romiley", "" ),
			new Tuple<string, string, string>("57 Tideslea Tower", "Erebus Drive", "" ),
			new Tuple<string, string, string>("58 Tresham Green", "", "" ),
			new Tuple<string, string, string>("6 Mullins Road", "", "" ),
			new Tuple<string, string, string>("66 Cheston Avenue", "", "" ),
			new Tuple<string, string, string>("66 Highfields", "Great Yeldham", "" ),
			new Tuple<string, string, string>("8 Chesterfield Road", "St. Andrews", "" ),
			new Tuple<string, string, string>("8 Rowood Avenue", "", "" ),
			new Tuple<string, string, string>("80 Denville Crescent", "", "" ),
			new Tuple<string, string, string>("82 Cornwall Drive", "", "" ),
			new Tuple<string, string, string>("9 Rothay Close", "Whitefield", "" ),
			new Tuple<string, string, string>("Apartment 31", "Mcclintock House", "The Boulevard" ),
			new Tuple<string, string, string>("Ellen House", "Marsh End", "Lords Meadow Industrial Estate" ),
			new Tuple<string, string, string>("Enterprise Way", "Thornton Road Industrial Estate", "" ),
			new Tuple<string, string, string>("Flat 16", "Cloisters Court", "77 Cromwell Avenue" ),
			new Tuple<string, string, string>("Flat 27", "Compass Quay", "Haven Road" ),
			new Tuple<string, string, string>("Flat 68", "Linden House", "Common Road" ),
			new Tuple<string, string, string>("Home Farm", "Anchor Road", "Terrington St. Clement" ),
			new Tuple<string, string, string>("Homestead", "Tamworth Road", "Corley" ),
			new Tuple<string, string, string>("Keepers Cottage", "Dunwich Lane", "Peasenhall" ),
			new Tuple<string, string, string>("Lyn-Mar House", "Lock Up Lane", "Abersychan" ),
			new Tuple<string, string, string>("Marischal College", "Broad Street", "" ),
			new Tuple<string, string, string>("Millbrooke", "Common Road", "West Bilney" ),
			new Tuple<string, string, string>("P R House", "Hortonwood 30", "" ),
			new Tuple<string, string, string>("Park House", "16 Finsbury Circus", "" ),
			new Tuple<string, string, string>("Po Box 342", "", "" ),
			new Tuple<string, string, string>("S M C House", "Tanhouse Lane", "Yate" ),
			new Tuple<string, string, string>("St. Nicholas Chare", "", "" ),
			new Tuple<string, string, string>("The Pinery", "Highmoor Bungalows", "" ),
			new Tuple<string, string, string>("The Triangle", "", "" ),
			new Tuple<string, string, string>("Unit 11-12", "Riverside", "" ),
		};
	} // class AddressTest
} // namespace
