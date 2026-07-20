using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Prg_UI.HelperWins
{
    public partial class TextInputDialog : Window
    {
        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(TextInputDialog),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(3, 169, 244))));

        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public string SearchText => SearchTextBox.Text.Trim();

        public TextInputDialog(string initialText, Brush accentBrush)
        {
            InitializeComponent();
            SearchTextBox.Text = initialText ?? string.Empty;
            if (accentBrush != null) AccentBrush = accentBrush;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchTextBox.Focus();
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                Apply_Click(sender, e);
                e.Handled = true;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
    }
}
