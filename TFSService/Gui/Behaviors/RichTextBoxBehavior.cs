using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Markup;

namespace Gui.Behaviors
{
    public class RichTextBoxBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
            "Html", typeof(string), typeof(RichTextBoxBehavior), 
            new PropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string html 
                && d is RichTextBoxBehavior behavior 
                && behavior.AssociatedObject?.IsLoaded == true)
            {
                html = html
                        // Для ссылок нужен параграф
                    .Replace("<a", "<p><a")
                    .Replace("</a>", "</a></p>")
                    
                    .Replace("<p>", "<Paragraph>")
                    .Replace("</p>", "</Paragraph>\n")
                    
                    .Replace("<ul>", "<List>\n")
                    .Replace("</ul>", "</List>\n")
                    
                    .Replace("<li>", "<ListItem>\n")
                    .Replace(@"</li>", "</ListItem>")
                    
                    .Replace("<a", "<Hyperlink")
                    .Replace("</a>", "</Hyperlink>\n")
                    
                    .Replace("href", "NavigateUri");

                var flowDoc = XamlReader.Parse($"<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{html}</FlowDocument>") as FlowDocument;
                behavior.AssociatedObject.Document = flowDoc;
            }
        }

        public string Html
        {
            get => (string)GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }
    }
}