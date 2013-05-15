namespace EKM.API 
{
    public partial class Order
    {
        public override string ToString()
        {
            return string.Format("OrderDateISO: {0}, OrderDate: {1}, TotalCost: {2}, OrderStatusColour: {3}, OrderStatus: {4}, EmailAddress: {5}, CompanyName: {6}, LastName: {7}, FirstName: {8}, CustomerID: {9}, OrderNumber: {10}, OrderID: {11}, ExtensionData: {12}", OrderDateISO, OrderDate, TotalCost, OrderStatusColour, OrderStatus, EmailAddress, CompanyName, LastName, FirstName, CustomerID, OrderNumber, OrderID, ExtensionData);
        }
    }
}