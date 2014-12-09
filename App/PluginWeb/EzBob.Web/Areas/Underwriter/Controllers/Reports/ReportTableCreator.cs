using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using Aspose.Cells;
using EzBob.Web.Areas.Underwriter.Models.Reports;

namespace EzBob.Web.Areas.Underwriter.Controllers.Reports
{
    public static class ReportTableCreator
    {
        public enum DataType { Overall, Time, Amount, IntNumber, Count, TimeInSeconds, Percents, Date }

        private static readonly Dictionary<Color, int> _registeredColors = new Dictionary<Color, int>();

        public static MemoryStream CreateExcelFile<T>(ReportTableModel<T> model, bool withTotal = true)
        {
            var wb = new Workbook();
            wb.Worksheets.Clear();
            var sheet = wb.Worksheets.Add("Report");
            var i = 0;
            var start = 1;
            var headerStyle = sheet.Cells[start,0].GetStyle();
            headerStyle.VerticalAlignment = TextAlignmentType.Center;
            headerStyle.HorizontalAlignment = TextAlignmentType.Center;
            headerStyle.Borders[BorderType.BottomBorder].Color = Color.Black;
            headerStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
            headerStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
            headerStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
            headerStyle.Borders[BorderType.RightBorder].Color = Color.Black;
            headerStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
            headerStyle.Borders[BorderType.TopBorder].Color = Color.Black;
            headerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
            headerStyle.Pattern = BackgroundType.Solid;
            headerStyle.Font.IsBold = true;
            headerStyle.ForegroundColor = GetColor(Color.LightGray, wb);

            foreach (ReportTableColumn reportTableColumn in model.Columns)
            {
                var childs = reportTableColumn.Childs;
                if (childs == null)
                {
                    sheet.Cells.Merge(start, i, 2, 1);
                    var cell = sheet.Cells[start, i];
                    cell.SetStyle(headerStyle);
                    var s = cell.GetStyle();
                    s.IsTextWrapped = true;
                    cell.SetStyle(s);
                    sheet.Cells[start+1, i].SetStyle(headerStyle);
                    cell.PutValue(FormatCaption(reportTableColumn.Caption));
                    ++i;
                }
                else
                {
                    var count = childs.Count;
                    sheet.Cells.Merge(start, i, 1, count);
                    var cell = sheet.Cells[start, i];
                    cell.PutValue(reportTableColumn.Caption);
                    for (int y = 0; y < count; y++)
                        sheet.Cells[start, i + y].SetStyle(headerStyle);
                    foreach (ReportTableColumn tableColumn in childs)
                    {
                        var c = sheet.Cells[start + 1, i];
                        c.PutValue(tableColumn.Caption);
                        c.SetStyle(headerStyle);
                        ++i;
                    }
                }
            }
            var columns = GetValueColumns(model.Columns);
            int row = start + 1;
            foreach (var o in model.Data)
            {
                FormExcelRow(columns, o, sheet.Cells, ++row);
            }
            if (withTotal)
                FormExcelRow(columns, model.Total, sheet.Cells, ++row);
            return wb.SaveToStream();
        }

        private static string FormatCaption(string caption)
        {
            return caption.Replace("</br>", "\r\n");
        }

        private static void FormExcelRow(IEnumerable<ReportTableColumn> columns, object o, Cells cells, int row)
        {
            int i = 0;
            foreach (var reportTableColumn in columns)
            {
                var cell = cells[row, i];
                cell.PutValue(GetExcelValue(GetValue(o, reportTableColumn), reportTableColumn));
                SetAdditonalFormat(cell, reportTableColumn);
                ++i;
            }
        }

        private static void SetAdditonalFormat(Cell cell, ReportTableColumn column)
        {
            var style = cell.GetStyle();
            switch (column.DataType)
            {
                case DataType.Overall:
                case DataType.Amount:
                case DataType.Time:
                case DataType.TimeInSeconds:
                    break;
                case DataType.IntNumber:
                    style.Number = 0;
                    break;
                case DataType.Percents:
                    style.Number = 10;
                    break;
                case DataType.Date:
                    style.Number = 14;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            cell.SetStyle(style);
        }

        private static object GetExcelValue(object value, ReportTableColumn column)
        {
            switch (column.DataType)
            {
                case DataType.Time:
                case DataType.TimeInSeconds:
                    return GetText(value, column);
                case DataType.Overall:
                case DataType.Amount:
                case DataType.Percents:
                case DataType.Date:
                case DataType.IntNumber:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MvcHtmlString CreateTable<T>(ReportTableModel<T> model, bool withTotal = true)
        {
            var table = new TagBuilder("table");
            table.AddCssClass("table");

            table.InnerHtml += FormThead(model);
            IEnumerable<ReportTableColumn> columns = GetValueColumns(model.Columns);
            table.InnerHtml += FormBody(model, columns, withTotal);

            return MvcHtmlString.Create(table.ToString());
        }

        //Временное быстрое решения расчитанное на 2 уровня
        //При большем уровне вложенности необходимо переделать
        private static IEnumerable<ReportTableColumn> GetValueColumns(IEnumerable<ReportTableColumn> inColumns)
        {
            var columns = new List<ReportTableColumn>();
            foreach (var reportTableColumn in inColumns)
            {
                var childs = reportTableColumn.Childs;
                if (childs == null)
                {
                    columns.Add(reportTableColumn);
                }
                else
                {
                    columns.AddRange(childs);
                }
            }
            return columns;
        }

        //Временное быстрое решения расчитанное на 2 уровня
        //При большем уровне вложенности необходимо переделать
        private static TagBuilder FormThead<T>(ReportTableModel<T> model)
        {
            var thead = new TagBuilder("thead");
			thead.AddCssClass("box");
            var trTop = new TagBuilder("tr");
			trTop.AddCssClass("box-title");
            var trBottom = new TagBuilder("tr");
			trBottom.AddCssClass("box-title");
            foreach (var reportTableColumn in model.Columns)
            {
                var th = CreateColumn(reportTableColumn);
                var childs = reportTableColumn.Childs;
                if (childs == null)
                {
                    th.Attributes.Add("rowspan", "2");
                }
                else
                {
                    th.Attributes.Add("colspan", childs.Count.ToString(CultureInfo.InvariantCulture));
                    foreach (var tableColumn in childs)
                    {
                        trBottom.InnerHtml += CreateColumn(tableColumn);
                    }
                }
                trTop.InnerHtml += th;
            }
            thead.InnerHtml += trTop;
            thead.InnerHtml += trBottom;
            return thead;
        }

        private static TagBuilder CreateColumn(ReportTableColumn reportTableColumn)
        {
            var th = new TagBuilder("th");
            //th.AddCssClass("reportTableHeader");
            th.InnerHtml += reportTableColumn.Caption;
            return th;
        }

        private static TagBuilder FormBody<T>(ReportTableModel<T> model, IEnumerable<ReportTableColumn> columns, bool withTotal)
        {
            var tbody = new TagBuilder("tbody");
            foreach (var o in model.Data)
            {
                tbody.InnerHtml += FormRow(columns, o);
            }
            if (withTotal)
                tbody.InnerHtml += FormRow(columns, model.Total);
            return tbody;
        }

        private static TagBuilder FormRow(IEnumerable<ReportTableColumn> columns, object o)
        {
            var trLine = new TagBuilder("tr");
            foreach (var reportTableColumn in columns)
            {
                trLine.InnerHtml += FormTdData(o, reportTableColumn);
            }
            return trLine;
        }

        private static TagBuilder FormTdData(object o, ReportTableColumn reportTableColumn)
        {
            var td = new TagBuilder("td");
            var val = GetValue(o, reportTableColumn);
            var text = GetText(val, reportTableColumn);
            if (reportTableColumn.DataType == DataType.Amount || reportTableColumn.DataType == DataType.Percents || reportTableColumn.DataType == DataType.IntNumber || reportTableColumn.DataType == DataType.Time || reportTableColumn.DataType == DataType.TimeInSeconds)
                td.AddCssClass("numberTd");
            if (reportTableColumn.ImageProvider != null)
            {
                var imgPath = reportTableColumn.ImageProvider.GetImagePath(val);
                if (!string.IsNullOrEmpty(imgPath))
                {
                    var img = new TagBuilder("img");
                    img.Attributes.Add("src", imgPath);
                    img.Attributes.Add("width", "16px");
                    img.Attributes.Add("height", "16px");
                    td.InnerHtml += img;    
                }
            }
            td.InnerHtml += text;
            return td;
        }

        private static object GetText(object val, ReportTableColumn reportTableColumn)
        {
            string text = "";
            if (val != null)
            {
                switch (reportTableColumn.DataType)
                {
                    case DataType.Overall:
                        text = val.ToString();
                        break;
                    case DataType.Time:
                        var dt = new TimeSpan((long)val);
                        text = string.Format("{0}:{1}", dt.TotalHours, dt.Minutes);
                        break;
                    case DataType.TimeInSeconds:
                        var seconds = (long)val;
                        TimeSpan ts = TimeSpan.FromSeconds(seconds);
                        text = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
                        break;
                    case DataType.Amount:
                        var format = "N2";
                        if (!string.IsNullOrEmpty(reportTableColumn.Format))
                            format = reportTableColumn.Format;
                        text = ((double)val).ToString(format, CultureInfo.InvariantCulture);
                        break;
                    case DataType.IntNumber:
                        text = ((int)val).ToString(CultureInfo.InvariantCulture);
                        break;                        
                    case DataType.Count:
                        if (!string.IsNullOrEmpty(reportTableColumn.Format))
                            format = reportTableColumn.Format;
                        text = ((int)val).ToString(CultureInfo.InvariantCulture);
                        break;
                    case DataType.Percents:
                        text = string.Format("{0}%", ((decimal)val * 100).ToString("0.00"));
                        break;
                    case DataType.Date:
                        text = ((DateTime)val).ToShortDateString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return text;
        }

        private static object GetValue(object o, ReportTableColumn reportTableColumn)
        {
            if (reportTableColumn.FieldValue != null)
            {
                return reportTableColumn.FieldValue(o);
            }
            var p = o.GetType().GetProperty(reportTableColumn.FieldName);
            return p.GetValue(o, null);
        }

        private static Color GetColor(Color color, Workbook workBook)
        {
            var firstColorPalleteIndex = workBook.Colors.Length;
            if (!_registeredColors.ContainsKey(color))
            {
                firstColorPalleteIndex = firstColorPalleteIndex - 1;
                _registeredColors.Add(color, firstColorPalleteIndex);
                workBook.ChangePalette(color, firstColorPalleteIndex);
            }
            return workBook.GetMatchingColor(color);
        }
    }
}
