UPDATE MP_TeraPeakOrderItem
SET RangeMarker = 1
WHERE Id in (
   select mtpoi1.id
   from MP_TeraPeakOrderItem mtpoi1
   LEFT JOIN MP_TeraPeakOrder mtpo ON mtpo.Id = mtpoi1.TeraPeakOrderId   
   where mtpoi1.EndDate = (select MAX(mtpoi2.EndDate)
      from MP_TeraPeakOrderItem mtpoi2
      LEFT JOIN MP_TeraPeakOrder mtpo2 ON mtpo2.Id = mtpoi2.TeraPeakOrderId
      where mtpo.CustomerMarketPlaceId = mtpo2.CustomerMarketPlaceId))
