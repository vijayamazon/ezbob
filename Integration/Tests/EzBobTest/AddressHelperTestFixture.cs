namespace EzBobTest 
{
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NUnit.Framework;
	using System.Collections.Generic;
	
	[TestFixture]
	public class AddressHelperTestFixture
	{
		private readonly List<CustomerAddressModel> _list = new List<CustomerAddressModel>();
		private ConsoleLog _log;

		[Test]
		public void TestAddress()
		{
			var helper = new CustomerAddressHelper(0);
			helper.Execute();
			foreach (var addr in helper.OwnedAddresses)
			{
				bool pass = (!string.IsNullOrEmpty(addr.HouseNumber) || !string.IsNullOrEmpty(addr.HouseName));
				_log.Debug(addr.ToString());
				Assert.AreEqual(true, pass);
			}
		}

		[SetUp]
		public void Init()
		{
			_log = new ConsoleLog();

			var env = new Ezbob.Context.Environment();

			Ezbob.Backend.Strategies.Library.Initialize(env, new SqlConnection(env, _log), _log);

			_list.Add(new CustomerAddressModel
			{
				Line1 = "Summer Cottage",
				Line2 = "Orchard Road",
				Line3 = "Pratts Bottom",
				City = "Orpington",
				County = "Kent",
				PostCode = "BR6 7NS"
			});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "29 Hilltown Road",
					Line2 = "Rathfriland",
					Line3 = "",
					City = "Newry",
					County = "County Down",
					PostCode = "BT34 5NA"
				});

			_list.Add(new CustomerAddressModel
				{
					Line1 = "42 Church Street",
					Line2 = "Yaxley",
					Line3 = "",
					City = "Peterborough",
					County = "Cambridgeshire",
					PostCode = "PE7 3LH"
				});

			_list.Add(new CustomerAddressModel
				{
					Line1 = "Holbear House",
					Line2 = "Forton Road",
					Line3 = "",
					City = "Chard",
					County = "Somerset",
					PostCode = "TA20 2HS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "345 Soundwell Road",
					Line2 = "",
					Line3 = "",
					City = "Bristol",
					County = "Avon",
					PostCode = "BS15 1JN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "3 Cypress Walk",
					Line2 = "",
					Line3 = "",
					City = "Barrow-In-Furness",
					County = "Cumbria",
					PostCode = "LA13 0JY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "114 Green Lane",
					Line2 = "Shelfield",
					Line3 = "",
					City = "Walsall",
					County = "West Midlands",
					PostCode = "WS4 1RR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 1",
					Line2 = "12 Upperkirkgate",
					Line3 = "",
					City = "Aberdeen",
					County = "",
					PostCode = "AB10 1BA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "15 Sandy Road",
					Line2 = "Calvert",
					Line3 = "",
					City = "Buckingham",
					County = "Buckinghamshire",
					PostCode = "MK18 2FW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Warren Lingley Way",
					Line2 = "Tiptree",
					Line3 = "",
					City = "Colchester",
					County = "Essex",
					PostCode = "CO5 0FE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "90 Mill Road",
					Line2 = "",
					Line3 = "",
					City = "Walsall",
					County = "Staffordshire",
					PostCode = "WS4 1BU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Gresham Drive",
					Line2 = "",
					Line3 = "",
					City = "Telford",
					County = "Shropshire",
					PostCode = "TF3 5ES"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "42 Merlin Way",
					Line2 = "",
					Line3 = "",
					City = "East Grinstead",
					County = "West Sussex",
					PostCode = "RH19 3XG"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "12 Copper Beech Drive",
					Line2 = "Wombourne",
					Line3 = "",
					City = "Wolverhampton",
					County = "West Midlands",
					PostCode = "WV5 0LH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "305 Middleton Road",
					Line2 = "",
					Line3 = "",
					City = "Manchester",
					County = "Lancashire",
					PostCode = "M8 4LY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "50 Telford Road",
					Line2 = "",
					Line3 = "",
					City = "Walsall",
					County = "West Midlands",
					PostCode = "WS2 7LD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Mill Close",
					Line2 = "Mill Street",
					Line3 = "Kineton",
					City = "Warwick",
					County = "",
					PostCode = "CV35 0LB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "40 Benson Road",
					Line2 = "",
					Line3 = "",
					City = "Grays",
					County = "Essex",
					PostCode = "RM17 6DL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "52 St. Margarets Street",
					Line2 = "",
					Line3 = "",
					City = "Rochester",
					County = "Kent",
					PostCode = "ME1 1TU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "36 Park Street",
					Line2 = "Kirkby-In-Ashfield",
					Line3 = "",
					City = "Nottingham",
					County = "Nottinghamshire",
					PostCode = "NG17 8DY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "9 The Coppice",
					Line2 = "",
					Line3 = "",
					City = "Dawlish",
					County = "Devon",
					PostCode = "EX7 0LN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "47 Beamway",
					Line2 = "",
					Line3 = "",
					City = "Dagenham",
					County = "Essex",
					PostCode = "RM10 8XR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Swift Gardens",
					Line2 = "Heysham",
					Line3 = "",
					City = "Morecambe",
					County = "Lancashire",
					PostCode = "LA3 2WL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "19 St. Georges Road",
					Line2 = "",
					Line3 = "",
					City = "Portland",
					County = "Dorset",
					PostCode = "DT5 2AT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "16A Queens Parade",
					Line2 = "",
					Line3 = "",
					City = "Grimsby",
					County = "South Humberside",
					PostCode = "DN31 2LE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "6 Mill Lane",
					Line2 = "Barrowden",
					Line3 = "",
					City = "Oakham",
					County = "",
					PostCode = "LE15 8EH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Unit 2 Block 1",
					Line2 = "Lakeside Park",
					Line3 = "Mells",
					City = "Frome",
					County = "Somerset",
					PostCode = "BA11 3RH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "80 Folly Lane",
					Line2 = "Swinton",
					Line3 = "",
					City = "Manchester",
					County = "Lancashire",
					PostCode = "M27 0DH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Leet Close",
					Line2 = "Eastchurch",
					Line3 = "",
					City = "Sheerness",
					County = "Kent",
					PostCode = "ME12 4EE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "6 Southend Crescent",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE9 2SB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Wexford Ridge",
					Line2 = "Noctorum Lane",
					Line3 = "",
					City = "Prenton",
					County = "Merseyside",
					PostCode = "CH43 9UE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "11 Bradford Street",
					Line2 = "",
					Line3 = "",
					City = "Llanelli",
					County = "Dyfed",
					PostCode = "SA15 1ET"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 2",
					Line2 = "44 Upperkirkgate",
					Line3 = "",
					City = "Aberdeen",
					County = "",
					PostCode = "AB10 1BA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "145-157 St. John Street",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "EC1V 4PW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "145-157 St. John Street",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "EC1V 4PW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "49 Farfield Avenue",
					Line2 = "Beeston",
					Line3 = "",
					City = "Nottingham",
					County = "Nottinghamshire",
					PostCode = "NG9 2PU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Marmion Crescent",
					Line2 = "",
					Line3 = "",
					City = "North Berwick",
					County = "East Lothian",
					PostCode = "EH39 4PA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "69 Mayhew Road",
					Line2 = "Rendlesham",
					Line3 = "",
					City = "Woodbridge",
					County = "Suffolk",
					PostCode = "IP12 2GZ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "9 Margards Lane",
					Line2 = "",
					Line3 = "",
					City = "Verwood",
					County = "Dorset",
					PostCode = "BH31 6JG"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "17 Scholars Gate",
					Line2 = "Garforth",
					Line3 = "",
					City = "Leeds",
					County = "",
					PostCode = "LS25 1BF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 1",
					Line2 = "44 Upperkirkgate",
					Line3 = "",
					City = "Aberdeen",
					County = "",
					PostCode = "AB10 1BA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "79 Micklethwaite Grove",
					Line2 = "",
					Line3 = "",
					City = "Wetherby",
					County = "West Yorkshire",
					PostCode = "LS22 5LA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "42 Bedhampton Hill",
					Line2 = "",
					Line3 = "",
					City = "Havant",
					County = "Hampshire",
					PostCode = "PO9 3JW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "29-33 Camberwell Church Street",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE5 8TR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "5 Mcdonald Crescent",
					Line2 = "",
					Line3 = "",
					City = "Falkirk",
					County = "",
					PostCode = "FK2 9FN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "10 Blackbrook Park Avenue",
					Line2 = "",
					Line3 = "",
					City = "Fareham",
					County = "Hampshire",
					PostCode = "PO15 5JJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "64 Croftfield Crescent",
					Line2 = "Newton",
					Line3 = "",
					City = "Swansea",
					County = "",
					PostCode = "SA3 4UL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "56-57 High Street Fold",
					Line2 = "Luddenden",
					Line3 = "",
					City = "Halifax",
					County = "West Yorkshire",
					PostCode = "HX2 6PY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "9 Cartmel Close",
					Line2 = "Bletchley",
					Line3 = "",
					City = "Milton Keynes",
					County = "Buckinghamshire",
					PostCode = "MK3 5LT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "10 Brookes Close",
					Line2 = "Tividale",
					Line3 = "",
					City = "Oldbury",
					County = "West Midlands",
					PostCode = "B69 1LB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "14 Millwood Close",
					Line2 = "Withnell",
					Line3 = "",
					City = "Chorley",
					County = "Lancashire",
					PostCode = "PR6 8AR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "33 Vince Dunn Mews",
					Line2 = "",
					Line3 = "",
					City = "Harlow",
					County = "Essex",
					PostCode = "CM17 0FF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Cherry Tree Road",
					Line2 = "",
					Line3 = "",
					City = "Stowmarket",
					County = "Suffolk",
					PostCode = "IP14 1NW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "98 Main Road",
					Line2 = "Cleeve",
					Line3 = "",
					City = "Bristol",
					County = "Avon",
					PostCode = "BS49 4PN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Sauncey Wood",
					Line2 = "",
					Line3 = "",
					City = "Harpenden",
					County = "Hertfordshire",
					PostCode = "AL5 5DP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "34 New North Road",
					Line2 = "",
					Line3 = "",
					City = "Attleborough",
					County = "Norfolk",
					PostCode = "NR17 2BJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 77A",
					Line2 = "Princess Park Manor",
					Line3 = "Royal Drive",
					City = "London",
					County = "",
					PostCode = "N11 3FN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "34 Jarvis Way",
					Line2 = "Stalbridge",
					Line3 = "",
					City = "Sturminster Newton",
					County = "Dorset",
					PostCode = "DT10 2NR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "18 Studley Drive",
					Line2 = "",
					Line3 = "",
					City = "Spennymoor",
					County = "County Durham",
					PostCode = "DL16 7GB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "87 Orton Lane",
					Line2 = "",
					Line3 = "",
					City = "Wolverhampton",
					County = "West Midlands",
					PostCode = "WV4 4XA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "3 Holly Terrace",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "N6 6LX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "11 Highwoods Park",
					Line2 = "Brockhall Village",
					Line3 = "Old Langho",
					City = "Blackburn",
					County = "Lancashire",
					PostCode = "BB6 8HN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Moss Grove",
					Line2 = "",
					Line3 = "",
					City = "Kenilworth",
					County = "Warwickshire",
					PostCode = "CV8 2WB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "14 Borley Road",
					Line2 = "",
					Line3 = "",
					City = "Poole",
					County = "Dorset",
					PostCode = "BH17 7DT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "144 Weston Road",
					Line2 = "Aston Clinton",
					Line3 = "",
					City = "Aylesbury",
					County = "Buckinghamshire",
					PostCode = "HP22 5EP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "27 Titian Road",
					Line2 = "",
					Line3 = "",
					City = "Hove",
					County = "East Sussex",
					PostCode = "BN3 5QR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "221 Forest Road",
					Line2 = "",
					Line3 = "",
					City = "Tunbridge Wells",
					County = "Kent",
					PostCode = "TN2 5HT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Runsel Grange",
					Line2 = "",
					Line3 = "",
					City = "Ross-On-Wye",
					County = "Herefordshire",
					PostCode = "HR9 7TJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "20 Oaklands Court",
					Line2 = "Aldcliffe",
					Line3 = "",
					City = "Lancaster",
					County = "Lancashire",
					PostCode = "LA1 5AT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "17 Breinton Avenue",
					Line2 = "",
					Line3 = "",
					City = "Hereford",
					County = "Hertfordshire",
					PostCode = "HR4 0JZ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Beancroft Road",
					Line2 = "",
					Line3 = "",
					City = "Thatcham",
					County = "Berkshire",
					PostCode = "RG19 3XS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Camellia Close",
					Line2 = "Narborough",
					Line3 = "",
					City = "Leicester",
					County = "Leicestershire",
					PostCode = "LE19 3WL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "11 Banbury Lane",
					Line2 = "Cold Higham",
					Line3 = "",
					City = "Towcester",
					County = "Northamptonshire",
					PostCode = "NN12 8LR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Northolme",
					Line2 = "Wainfleet",
					Line3 = "",
					City = "Skegness",
					County = "Lincolnshire",
					PostCode = "PE24 4EQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "31 Glynswood",
					Line2 = "",
					Line3 = "",
					City = "High Wycombe",
					County = "Buckinghamshire",
					PostCode = "HP13 5QL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "101 Orchard Way",
					Line2 = "",
					Line3 = "",
					City = "Knebworth",
					County = "Hertfordshire",
					PostCode = "SG3 6BT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "38 Marston Road",
					Line2 = "Tockwith",
					Line3 = "",
					City = "York",
					County = "North Yorkshire",
					PostCode = "YO26 7PR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Laburnum Crescent",
					Line2 = "",
					Line3 = "",
					City = "Sunbury-On-Thames",
					County = "Middlesex",
					PostCode = "TW16 5NA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "7 Greenlake Terrace",
					Line2 = "Laleham Road",
					Line3 = "",
					City = "Staines-Upon-Thames",
					County = "Middlesex",
					PostCode = "TW18 2NU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "55 Primrose Copse",
					Line2 = "",
					Line3 = "",
					City = "Horsham",
					County = "West Sussex",
					PostCode = "RH12 5PZ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 2",
					Line2 = "20 Upperkirkgate",
					Line3 = "",
					City = "Aberdeen",
					County = "",
					PostCode = "AB10 1BA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 1",
					Line2 = "6 Upperkirkgate",
					Line3 = "",
					City = "Aberdeen",
					County = "",
					PostCode = "AB10 1BA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flagstones",
					Line2 = "Frith Hill Road",
					Line3 = "",
					City = "Godalming",
					County = "Surrey",
					PostCode = "GU7 2EE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Milverton Road",
					Line2 = "",
					Line3 = "",
					City = "Winchester",
					County = "Hampshire",
					PostCode = "SO22 5AU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "29 Ynysowen Fach",
					Line2 = "Aberfan",
					Line3 = "",
					City = "Merthyr Tydfil",
					County = "Mid Glamorgan",
					PostCode = "CF48 4RL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "54 Hainault Road",
					Line2 = "",
					Line3 = "",
					City = "Chigwell",
					County = "Essex",
					PostCode = "IG7 6QX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "32 The Ridgeway",
					Line2 = "Disley",
					Line3 = "",
					City = "Stockport",
					County = "Cheshire",
					PostCode = "SK12 2JQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "20 Ross Place",
					Line2 = "Newtongrange",
					Line3 = "",
					City = "Dalkeith",
					County = "Midlothian",
					PostCode = "EH22 4JD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "9 Stewart Close",
					Line2 = "",
					Line3 = "",
					City = "Chislehurst",
					County = "Kent",
					PostCode = "BR7 6TE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "18 Carryhugh Road",
					Line2 = "Derrynoose",
					Line3 = "",
					City = "Armagh",
					County = "",
					PostCode = "BT60 3DQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "39 Highside Drive",
					Line2 = "",
					Line3 = "",
					City = "Sunderland",
					County = "Tyne And Wear",
					PostCode = "SR3 1UL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Doncaster Road",
					Line2 = "",
					Line3 = "",
					City = "Selby",
					County = "North Yorkshire",
					PostCode = "YO8 9BY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "12 Dartford Court",
					Line2 = "Glanville Way",
					Line3 = "",
					City = "Epsom",
					County = "Surrey",
					PostCode = "KT19 8HQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Lurkins Rise",
					Line2 = "Goudhurst",
					Line3 = "",
					City = "Cranbrook",
					County = "Kent",
					PostCode = "TN17 1ED"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "2 Main Road",
					Line2 = "Dyffryn Cellwen",
					Line3 = "",
					City = "Neath",
					County = "West Glamorgan",
					PostCode = "SA10 9HR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "142 Hayling Avenue",
					Line2 = "",
					Line3 = "",
					City = "Portsmouth",
					County = "",
					PostCode = "PO3 6ED"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "18 Knutsford Road",
					Line2 = "",
					Line3 = "",
					City = "Wilmslow",
					County = "Cheshire",
					PostCode = "SK9 6JA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "148 Cloverfield",
					Line2 = "West Allotment",
					Line3 = "",
					City = "Newcastle Upon Tyne",
					County = "Tyne And Wear",
					PostCode = "NE27 0BX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "7 Attlee Crescent",
					Line2 = "",
					Line3 = "",
					City = "Bilston",
					County = "West Midlands",
					PostCode = "WV14 8UF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "64 Goldsmith Avenue",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "W3 6HN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "32 Darlands Drive",
					Line2 = "",
					Line3 = "",
					City = "Barnet",
					County = "Hertfordshire",
					PostCode = "EN5 2DF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Tucker Hill",
					Line2 = "",
					Line3 = "",
					City = "Clitheroe",
					County = "Lancashire",
					PostCode = "BB7 2NR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "17 St. Dunstans Hill",
					Line2 = "",
					Line3 = "",
					City = "Sutton",
					County = "Surrey",
					PostCode = "SM1 2JX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Vicarage Lane",
					Line2 = "",
					Line3 = "",
					City = "Kidwelly",
					County = "Dyfed",
					PostCode = "SA17 4SY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "5 Woodpecker Close",
					Line2 = "Allerton",
					Line3 = "",
					City = "Bradford",
					County = "West Yorkshire",
					PostCode = "BD15 7WJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "13 Cross Flatts Grove",
					Line2 = "",
					Line3 = "",
					City = "Leeds",
					County = "West Yorkshire",
					PostCode = "LS11 7JA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "57 Lonsdale Road",
					Line2 = "Formby",
					Line3 = "",
					City = "Liverpool",
					County = "",
					PostCode = "L37 3HD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "40 Perry Wood Road",
					Line2 = "",
					Line3 = "",
					City = "Birmingham",
					County = "West Midlands",
					PostCode = "B42 2BQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "11 Purwell Lane",
					Line2 = "",
					Line3 = "",
					City = "Hitchin",
					County = "Hertfordshire",
					PostCode = "SG4 0NE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "80 Camdale Road",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE18 2DS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Spinney Close",
					Line2 = "Endon",
					Line3 = "",
					City = "Stoke-On-Trent",
					County = "Staffordshire",
					PostCode = "ST9 9BP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "16 Claremont Avenue",
					Line2 = "",
					Line3 = "",
					City = "Stockport",
					County = "Cheshire",
					PostCode = "SK4 4QR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "10 Mayhurst Close",
					Line2 = "",
					Line3 = "",
					City = "Tipton",
					County = "West Midlands",
					PostCode = "DY4 0TS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Bolton Avenue",
					Line2 = "",
					Line3 = "",
					City = "Windsor",
					County = "Berkshire",
					PostCode = "SL4 3JB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "45 Storeton Road",
					Line2 = "",
					Line3 = "",
					City = "Prenton",
					County = "Merseyside",
					PostCode = "CH43 5TW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "36 Bamber Avenue",
					Line2 = "",
					Line3 = "",
					City = "Sale",
					County = "Cheshire",
					PostCode = "M33 2TH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "7 King George Avenue",
					Line2 = "Horsforth",
					Line3 = "",
					City = "Leeds",
					County = "West Yorkshire",
					PostCode = "LS18 5ND"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "7 Thrush Green",
					Line2 = "Nightingale Road",
					Line3 = "",
					City = "Rickmansworth",
					County = "Hertfordshire",
					PostCode = "WD3 7GF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Carver Brow",
					Line2 = "Higher Walton",
					Line3 = "",
					City = "Preston",
					County = "Lancashire",
					PostCode = "PR5 4EL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "131 Uppingham Avenue",
					Line2 = "",
					Line3 = "",
					City = "Stanmore",
					County = "Middlesex",
					PostCode = "HA7 2HW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "106 Lion Road",
					Line2 = "",
					Line3 = "",
					City = "Bexleyheath",
					County = "Kent",
					PostCode = "DA6 8PQ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 York House",
					Line2 = "Baxter Road",
					Line3 = "",
					City = "Sunderland",
					County = "Tyne And Wear",
					PostCode = "SR5 4DR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "17 Hartley Hill",
					Line2 = "",
					Line3 = "",
					City = "Purley",
					County = "Surrey",
					PostCode = "CR8 4EP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Martinet Road",
					Line2 = "Thornaby",
					Line3 = "",
					City = "Stockton-On-Tees",
					County = "Cleveland",
					PostCode = "TS17 0JB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "162 Holmrook Road",
					Line2 = "",
					Line3 = "",
					City = "Carlisle",
					County = "Cumbria",
					PostCode = "CA2 7TJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "37 Grange Road",
					Line2 = "",
					Line3 = "",
					City = "Hayes",
					County = "Middlesex",
					PostCode = "UB3 2RP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "32 Cornubia Close",
					Line2 = "",
					Line3 = "",
					City = "Truro",
					County = "Cornwall",
					PostCode = "TR1 1SA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "16 Michael Pyms Road",
					Line2 = "",
					Line3 = "",
					City = "Malmesbury",
					County = "Wiltshire",
					PostCode = "SN16 9TY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "43 Palmer Road",
					Line2 = "",
					Line3 = "",
					City = "Devizes",
					County = "Wiltshire",
					PostCode = "SN10 2FJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Lawrence Road",
					Line2 = "Eaton Ford",
					Line3 = "",
					City = "St. Neots",
					County = "Cambridgeshire",
					PostCode = "PE19 7RP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Barrie Grove",
					Line2 = "",
					Line3 = "",
					City = "Crewe",
					County = "Cheshire",
					PostCode = "CW1 5HD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "30 Hiskins",
					Line2 = "",
					Line3 = "",
					City = "Wantage",
					County = "Oxfordshire",
					PostCode = "OX12 9HU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Attwyll Avenue",
					Line2 = "",
					Line3 = "",
					City = "Exeter",
					County = "Devon",
					PostCode = "EX2 5HN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "3 Blackbird Close",
					Line2 = "",
					Line3 = "",
					City = "Burgess Hill",
					County = "West Sussex",
					PostCode = "RH15 9XT"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "44 Wingrove Road",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE6 1QE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "57 Abbey Road",
					Line2 = "",
					Line3 = "",
					City = "Aylesbury",
					County = "Buckinghamshire",
					PostCode = "HP19 9NP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "116 Tower Road",
					Line2 = "",
					Line3 = "",
					City = "Boston",
					County = "Lincolnshire",
					PostCode = "PE21 9AU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "216B Dunstans Road",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE22 0ES"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "7 Clarendon Close",
					Line2 = "",
					Line3 = "",
					City = "Redditch",
					County = "Worcestershire",
					PostCode = "B97 6ST"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Ferndale Road",
					Line2 = "Waterloo",
					Line3 = "",
					City = "Liverpool",
					County = "",
					PostCode = "L22 9QN"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 Oakdene",
					Line2 = "Totton",
					Line3 = "",
					City = "Southampton",
					County = "Hampshire",
					PostCode = "SO40 8FW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "19 Oxenden Wood Road",
					Line2 = "",
					Line3 = "",
					City = "Orpington",
					County = "Kent",
					PostCode = "BR6 6HR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "28 Bedwell Gardens",
					Line2 = "",
					Line3 = "",
					City = "Hayes",
					County = "Middlesex",
					PostCode = "UB3 4EF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "5 Alabama Street",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SE18 2SJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "5 Lelant Drive",
					Line2 = "Four Marks",
					Line3 = "",
					City = "Alton",
					County = "Hampshire",
					PostCode = "GU34 5GA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "5 Bewicke View",
					Line2 = "Birtley",
					Line3 = "",
					City = "Chester Le Street",
					County = "County Durham",
					PostCode = "DH3 1RU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "16 Jordan Avenue",
					Line2 = "",
					Line3 = "",
					City = "Bangor",
					County = "County Down",
					PostCode = "BT20 4UP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "6 Jacksons Orchard",
					Line2 = "Long Marston",
					Line3 = "",
					City = "Stratford-Upon-Avon",
					County = "Warwickshire",
					PostCode = "CV37 8RU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 St. Lukes Field",
					Line2 = "Walrow",
					Line3 = "",
					City = "Highbridge",
					County = "Somerset",
					PostCode = "TA9 4RA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "60 West End",
					Line2 = "Walkington",
					Line3 = "",
					City = "Beverley",
					County = "North Humberside",
					PostCode = "HU17 8SX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "76 Boundary Walk",
					Line2 = "Knowle",
					Line3 = "",
					City = "Fareham",
					County = "Hampshire",
					PostCode = "PO17 5GB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "24 Woodlands Meadow",
					Line2 = "",
					Line3 = "",
					City = "Chorley",
					County = "Lancashire",
					PostCode = "PR7 3QH"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Hoveland Lane",
					Line2 = "",
					Line3 = "",
					City = "Taunton",
					County = "Somerset",
					PostCode = "TA1 5DE"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "102A West Drive",
					Line2 = "Highfields Caldecote",
					Line3 = "",
					City = "Cambridge",
					County = "Cambridgeshire",
					PostCode = "CB23 7NY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "10 The Pagoda",
					Line2 = "",
					Line3 = "",
					City = "Maidenhead",
					County = "Berkshire",
					PostCode = "SL6 8EU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Bassett Fields 272A",
					Line2 = "High Road",
					Line3 = "North Weald",
					City = "Epping",
					County = "Essex",
					PostCode = "CM16 6EF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Kitchener Street",
					Line2 = "",
					Line3 = "",
					City = "Selby",
					County = "North Yorkshire",
					PostCode = "YO8 4BU"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "Flat 96",
					Line2 = "Clifton Court",
					Line3 = "Northwick Terrace",
					City = "London",
					County = "",
					PostCode = "NW8 8HX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "193 School Road",
					Line2 = "",
					Line3 = "",
					City = "Blackpool",
					County = "Lancashire",
					PostCode = "FY4 5EL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "4 Vickers Road",
					Line2 = "",
					Line3 = "",
					City = "Southend-On-Sea",
					County = "Essex",
					PostCode = "SS2 6XD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "17 Audley Road",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "W5 3ES"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "82 Caulstran Road",
					Line2 = "",
					Line3 = "",
					City = "Dumfries",
					County = "",
					PostCode = "DG2 9FL"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "6 Blackamoor Lane",
					Line2 = "",
					Line3 = "",
					City = "Maidenhead",
					County = "Berkshire",
					PostCode = "SL6 8RD"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "103 Falmouth Gardens",
					Line2 = "",
					Line3 = "",
					City = "Ilford",
					County = "Essex",
					PostCode = "IG4 5JJ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "97 Thorpe Road",
					Line2 = "Kirby Cross",
					Line3 = "",
					City = "Frinton-On-Sea",
					County = "Essex",
					PostCode = "CO13 0ND"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "22 Wallace Avenue",
					Line2 = "",
					Line3 = "",
					City = "Worthing",
					County = "West Sussex",
					PostCode = "BN11 5QY"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "37 Scott Street",
					Line2 = "Hartford",
					Line3 = "",
					City = "Cramlington",
					County = "Northumberland",
					PostCode = "NE23 3AW"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "15 Gala Way",
					Line2 = "",
					Line3 = "",
					City = "Retford",
					County = "Nottinghamshire",
					PostCode = "DN22 7SX"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "3 Gainsborough Court",
					Line2 = "",
					Line3 = "",
					City = "Skipton",
					County = "North Yorkshire",
					PostCode = "BD23 1QG"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "54 Rectory Lane",
					Line2 = "Worlingham",
					Line3 = "",
					City = "Beccles",
					County = "Suffolk",
					PostCode = "NR34 7RP"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "21 Harrier Way",
					Line2 = "Morley",
					Line3 = "",
					City = "Leeds",
					County = "West Yorkshire",
					PostCode = "LS27 8TG"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "64 Newman Street",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "W1T 3EF"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "85 Cardiff Place",
					Line2 = "Bassingbourn",
					Line3 = "",
					City = "Royston",
					County = "Hertfordshire",
					PostCode = "SG8 5LR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "1 Paxton Terrace",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "SW1V 3DA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "8 The Studios",
					Line2 = "",
					Line3 = "",
					City = "Bushey",
					County = "Hertfordshire",
					PostCode = "WD23 3GZ"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "94 Cuthbury Gardens",
					Line2 = "",
					Line3 = "",
					City = "Wimborne",
					County = "Dorset",
					PostCode = "BH21 1YB"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "75 Mary Rose Avenue",
					Line2 = "Wootton Bridge",
					Line3 = "",
					City = "Ryde",
					County = "Isle Of Wight",
					PostCode = "PO33 4LR"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "21 Eden Road",
					Line2 = "",
					Line3 = "",
					City = "London",
					County = "",
					PostCode = "E17 9JS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "3 Finucane Gardens",
					Line2 = "",
					Line3 = "",
					City = "Rainham",
					County = "Essex",
					PostCode = "RM13 7QA"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "16 Bishops Road",
					Line2 = "Prestwich",
					Line3 = "",
					City = "Manchester",
					County = "Lancashire",
					PostCode = "M25 0HS"
				});
			_list.Add(new CustomerAddressModel
				{
					Line1 = "51 Headcorn Road",
					Line2 = "",
					Line3 = "",
					City = "Bromley",
					County = "Kent",
					PostCode = "BR1 4SQ"
				});
		}
	}
}
