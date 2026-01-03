using AviationDB_CSharp.Models;
using System.Text.Json;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class AircraftViewWindow : Window
    {
        private readonly Aircraft _aircraft;

        public AircraftViewWindow(Aircraft aircraft)
        {
            InitializeComponent();
            _aircraft = aircraft;
            DataContext = aircraft;
            LoadModelDetails();
        }

        private void LoadModelDetails()
        {
            if (_aircraft != null && !string.IsNullOrWhiteSpace(_aircraft.Model))
            {
                try
                {
                    using var doc = JsonDocument.Parse(_aircraft.Model);
                    if (doc.RootElement.TryGetProperty("en", out var enProp))
                        ModelEnTextBlock.Text = enProp.GetString() ?? "Unknown";
                    if (doc.RootElement.TryGetProperty("ru", out var ruProp))
                        ModelRuTextBlock.Text = ruProp.GetString() ?? "Неизвестно";
                }
                catch
                {
                    ModelEnTextBlock.Text = "Ошибка парсинга JSON";
                    ModelRuTextBlock.Text = "Ошибка парсинга JSON";
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AircraftEditWindow(_aircraft);
            if (editWindow.ShowDialog() == true)
            {
                DataContext = null;
                DataContext = _aircraft;
                LoadModelDetails();
                DialogResult = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}