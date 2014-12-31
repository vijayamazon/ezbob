--all missing back fill (have source ref or google cookie
SELECT c.Id --, c.ReferenceSource, c.GoogleCookie
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.IsTest=0 AND c.ReferenceSource IS NULL AND c.GoogleCookie IS NULL
AND (c.ReferenceSource IS NOT NULL OR c.GoogleCookie IS NOT NULL) --comment this line to see all that missing row in campaignsourceref

-- all multiple
SELECT c.Id, c.ReferenceSource, c.GoogleCookie
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource LIKE '%[%]3B%' AND c.IsTest=0

--all only google cookie
SELECT c.Id, c.ReferenceSource, c.GoogleCookie
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource IS NULL AND c.GoogleCookie NOT LIKE '%ezbob.com%'


INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Bing', 'Organic', c.GoogleCookie,'none', 'none',c.GreetingMailSentDate,NULL, 'Bing', 'Organic', c.GoogleCookie,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource IS NULL AND c.GoogleCookie LIKE '%utmcsr=bing%'

SELECT * FROM CampaignSourceRef WHERE CustomerId IN (
SELECT h.UserID
FROM Customer c INNER JOIN SourceRefHistory h ON c.Id=h.UserID
GROUP BY h.UserID 
HAVING count(*) > 1
)
SELECT COUNT(*) FROM CampaignSourceRef

SELECT TOP 10 * FROM CampaignSourceRef

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'bros_businessloans%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'comp-%'


INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'wiki', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'wiki', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'wiki%'


INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource IN (
'business_loans_broad',
'business_loans_phrase',
'sme_lending',
'short_term_business_loans',
'business_loans_for_women',
'small_business_loans',
'business_loans_exact',
'comp_funding_circle',
'short-term-business-loans',
'comp_everline',
'comp_iwoca',
'comp_iwoca_broad',
'comp_boostcapital',
'comp_kabbage',
'business_funding_broad',
'alternative_funding_sources',
'ezbob_exact',
'ezbob_broad',
'unsecured_business_loans_broad',
'unsecured_business_loans',
'business_finance_broad',
'business_finance_targeted',
'sources_of_finance',
'bros-business-funding',
'bros-business-funding-broad',
'bros-business-finance-broad',
'business_finance_exact',
'business_finance_sulotions',
'bros-businessloans-broad',
'bros-businessloans-broad',
'bros-businessloans-phrase-inserted-ads',
'bros-businessloans-phrase-inserted-ads',
'bros-comp-fundingcircle',
'bros-comp-fundingcircle',
'bros-comp-fundingcircle',
'bros-comp-kabbage',
'bros-ezbob-exact-lt',
'bros-ezbob-exact-lt',
'bros-sitelinks',
'bros-sitelinks',
'bros-sitelinks-how',
'business_funding_exact',
'remarketing_xmas',
'remarketing_text_ads',
'remarketing_TM_remarketing',
'remarketing_all_visitors_brand',
'remarketing_visited_broker_page',
'business_loans_medium_business_lending',
'business_loans_medium_where_to',
'online_sellers_amazon',
'online_sellers_ebayer',
'online_sellers_paypal',
'online_sellers_selling_on_ebay',
'online_sellers_ebay',
'SCM_text',
'SCM_SCM',
'SCM_business_finance',
'SCM_business_funding',
'brokers_display_banners',
'brokers_display_text',
'SCM_brokers_brokers',
'verticals_vehicle_loans',
'verticals_retail_business_loans',
'verticals_construction_finance',
'verticals_wholesale_loans',
'verticals_restaurant_loans',
'verticals_pub_finance',
'verticals_beauty_salon_loans',
'verticals_service_business',
'business_cash_advance_business_cash_advance',
'business_cash_advance_merchant_cash_advance',
'working_capital_working_capital_broad',
'working_capital_working_capital_exact',
'second_comp_liberis',
'brand_really_broad_easy',
'GoogleadwordsIwoca',
'Googleadwordsezbob',
'GoogleadwordsWongaforbusiness',
'GoogleadwordsCapexpand',
'GoogleadwordsKabbageUK',
'bros-sitelinks',
'bros-businessloans-broad',
'bros-ezbob-exact-lt',
'bros-businessloans-exact-only',
'bros-unsecured-businessloans-broad',
'bros-comp-iwoca',
'BROS-Remarketing-calc',
'bros-businessloans-phrase-inserted-ads',
'bros-business-funding-exact',
'bros-comp-everline',
'BROS-remarketing',
'BROS-Remarketing-calc',
'bros-business-funding',
'BROS-business-cash-advance',
'Bbros-HS-comp-kabbage',
'bros-ezbob-broad',
'bros-comp-kabbage',
'bros-comp-fundingcircle',
'bros-businessloans-broad-small-ad',
'BROS_TM_Remarketing_300X250',
'bros-businessloans-exact-inserted-ads',
'BROS_TM_Remarketing_728X90',
'adwords_xmas-remarketing-728x90',
'comp-everline-mobile',
'businessloans-mobile-broad',
'businessloans-mobile-exact',
'BROS_TM_Remarketing_160X600',
'comp-iwoca-i-wo-who-ad',
'businessloans-mobile-broad',
'comp-iwoca-mobile',
'bros-comp-everline',
'bros-comp-iwoca',
'bros-amazon-br-lt',
'comp-fundingcircle-mobile',
'bros-comp-kabbage',
'bros-ezbob-exact-lt',
'competitive',
'new_remarketing-300x250',
'adwords_Wonga-Business-WU2',
'bros-ebay-lt',
'adwords_xmas-remarketing-160x600',
'adwords_iwoca-NU2',
'adwords_ezbob-U',
'adwords_loans-with-paypal-U',
'adwords-nimrod',
'adwords_amazon-loans-Q',
'adwords_xmas-remarketing-250x250',
'adwords_AmazonBusinessLoan-DM',
'adwords_xmas-remarketing-300x250',
'adwords_WongaBusiness-N',
'adwords_Business-Loans-Q',
'adwords_Fast-Small-Business-Loans-Q',
'adwords_ezbob-U',
'adwords_xmas-remarketing-468x60',
'adwords_EcommerceLoan-DM',
'adwords_eBayLoan-DM',
'adwords_EZBOB-DM',
'adwords_AmazonBusinessLoan-DMN',
'adwords_PayPalLoans-DM',
'adwords_AnimatedMessageAdsRemarketing',
'm_co_uk_banner_google',
'adwords_EZBOB-DMW',
'adwords_iwoca',
'Googleadword_Iwoca',
'Googleadwords_ezbob',
'GoogleadwordsIwocca',
'Googleadwords_wonga_business_loans',
'Googleadwordsebay_funding',
'GoogleadwordsAmazonloans',
'GoogleadwordsKabbageUK',
'Googleadwordsezbob',
'GoogleadwordsCapexpand',
'GoogleadwordsWongaforbusiness'
)

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'MSM', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'MSM', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource IN ('msmLT','MSM','msm')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Money.co.uk', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Money.co.uk', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'm_co_uk%' -- IN ('Moneycouk','money_co_uk','m_co_uk','m.co.uk','Money.co.uk')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Ebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Ebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'ebay%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Gmail', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Gmail', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'bros_gmail%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Bing', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Bing', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE 'bingads%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Bing', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Bing', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource IN ('BING_TM_NTNHNF',
'BING_TM_NTNHNF',
'BING_TM_NTNHNF',
'BING_TM_10minutes',
'bros-bingads-business-loans-broad')


INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Tamebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Tamebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource LIKE '%tamebay%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Tamebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Tamebay', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource IN ('Tambey728X90Needfundstogrowyourbusiness?',
'Tambey300X250Jointhousandsofsellersthatappliedforfundstogrowtheirbusiness',
'Tambey468X60Needfundstogrowyourbusiness',
'Tambey728X90Jointhousandsofsellersthatappliedforfundstogrowtheirbusiness'
)

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Which4u', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Which4u', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource like ('which4u')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'finimpact', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'finimpact', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource like ('finimpact')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Prestashop', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Prestashop', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource like ('Prestashop%')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Facebook', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Facebook', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource like ('face%')

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Business.co.uk', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Business.co.uk', 'Referral', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.ReferenceSource NOT LIKE '%[%]3B%' AND c.ReferenceSource like 'business_co_uk'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Google', 'cpc', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.IsTest=0 AND c.ReferenceSource LIKE 'new-remarketing-%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Direct', '', c.GoogleCookie,'none', 'none',c.GreetingMailSentDate,NULL, 'Direct', '', c.GoogleCookie,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.IsTest=0 AND c.ReferenceSource IS NULL AND c.GoogleCookie LIKE '%utmcsr=ezbob.com%'

INSERT INTO dbo.CampaignSourceRef(CustomerId, FUrl, FSource, FMedium, FTerm, FContent, FName, FDate, RUrl, RSource, RMedium, RTerm, RContent, RName, RDate)
SELECT c.Id,NULL, 'Broker', '', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate,NULL, 'Broker', '', c.ReferenceSource,'none', 'none',c.GreetingMailSentDate
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND c.BrokerID IS NOT NULL



--all by ref
SELECT count(*), c.ReferenceSource
FROM Customer c LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
WHERE s.CustomerId IS NULL AND (c.ReferenceSource <> 'Broker' OR (c.ReferenceSource IS NULL AND c.GoogleCookie IS NOT NULL)) AND c.IsTest=0
GROUP BY c.ReferenceSource
ORDER BY count(*) DESC 
