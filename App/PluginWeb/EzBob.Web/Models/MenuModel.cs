namespace EzBob.Web.Models
{
    public class MenuModel
    {
        public MenuModel(string title, string action)
        {
            Action = action;
            Title = title;
        }

        public string Action { get; set; }
        public string Title { get; set; }
    }
}