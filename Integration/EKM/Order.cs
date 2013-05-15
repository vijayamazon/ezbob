using System;
using System.Globalization;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace EKM.API 
{
    public partial class Order
    {
        public override string ToString()
        {
            return string.Format("OrderDateISO: {0}, OrderDate: {1}, TotalCost: {2}, OrderStatusColour: {3}, OrderStatus: {4}, EmailAddress: {5}, CompanyName: {6}, LastName: {7}, FirstName: {8}, CustomerID: {9}, OrderNumber: {10}, OrderID: {11}", OrderDateISO, OrderDate, TotalCost, OrderStatusColour, OrderStatus, EmailAddress, CompanyName, LastName, FirstName, CustomerID, OrderNumber, OrderID);
        }

        public DateTime OrderDateParsed
        {
            get { return DateTime.ParseExact(this.OrderDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture); }
        }

        public DateTime OrderDateISOParsed
        {
            get { return DateTime.Parse(this.OrderDateISO, CultureInfo.InvariantCulture); }
        }

        public EkmOrderItem ToEkmOrderItem()
        {
            return new EkmOrderItem
                       {
                           EkmOrderId = this.OrderID,
                           OrderNumber = this.OrderNumber,
                           CustomerID = this.CustomerID,
                           CompanyName = this.CompanyName,
                           FirstName = this.FirstName,
                           LastName = this.LastName,
                           EmailAddress = this.EmailAddress,
                           TotalCost = this.TotalCost,
                           OrderDate = this.OrderDateParsed,
                           OrderStatus = this.OrderStatus,
                           OrderDateIso = this.OrderDateISOParsed,
                           OrderStatusColour = this.OrderStatusColour,
                       };
        }

    }
}