namespace EzBob.Web.Infrastructure.Html
{
    public static class EzHtml
    {
        public static InputTextBuilder InputText()
        {
            var builder = new InputTextBuilder();
            return builder;
        }

        public static RawCellBuilder RawCell(string title, string value)
        {
            var builder = new RawCellBuilder();
            builder.Title(title).Value(value);
            return builder;
        }
    }
}