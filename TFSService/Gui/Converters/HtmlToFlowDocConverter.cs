using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Gui.Converters
{
    public class HtmlToFlowDocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var html = value?.ToString();

            if (!string.IsNullOrEmpty(html))
            {
                html = html
                    .Replace("<p>", "<Paragraph>")
                    .Replace("</p>", "</Paragraph>")
                    
                    .Replace("<ul>", "<List>")
                    .Replace("</ul>", "</List>")
                    
                    .Replace("<li>", "<ListItem><Paragraph>")
                    .Replace(@"</li>", "</Paragraph></ListItem>")
                    
                    .Replace("<a", "<Hyperlink")
                    .Replace("</a>", "</Hyperlink>")
                    
                    .Replace("href", "NavigateUri");
            }

            return XamlReader.Parse($"<FlowDocument>{html}</FlowDocument");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}