using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for AircraftEditWindow.xaml
    /// </summary>
    public partial class AircraftEditWindow : Window
    {
        private readonly AircraftService _aircraftService;
        private Aircraft _aircraft;
        private bool _isNew;
        public AircraftEditWindow()
        {
            InitializeComponent();
            _aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();
            _isNew = true;
            Title = "Добавление нового самолета";
        }
        public AircraftEditWindow(Aircraft aircraft)
        {
            InitializeComponent();
            _aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();
            _aircraft = aircraft;
            _isNew = false;
            Title = "Редактирование самолета";

            LoadAircraftData();
        }

        private void LoadAircraftData()
        {
            if (_aircraft != null)
            {
                TxtCode.Text = _aircraft.AircraftCode;
                TxtCode.IsEnabled = false; // Нельзя менять код

                // Парсим JSON модели
                try
                {
                    if (!string.IsNullOrWhiteSpace(_aircraft.Model))
                    {
                        using var doc = JsonDocument.Parse(_aircraft.Model);
                        if (doc.RootElement.TryGetProperty("en", out var enProp))
                            TxtModelEn.Text = enProp.GetString();
                        if (doc.RootElement.TryGetProperty("ru", out var ruProp))
                            TxtModelRu.Text = ruProp.GetString();
                    }
                }
                catch { }

                TxtRange.Text = _aircraft.Range.ToString();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(TxtCode.Text) || TxtCode.Text.Length != 3)
                {
                    TxtError.Text = "Код самолета должен состоять из 3 символов";
                    return;
                }

                if (!int.TryParse(TxtRange.Text, out int range) || range <= 0)
                {
                    TxtError.Text = "Дальность должна быть положительным числом";
                    return;
                }

                // Создаем JSON модель
                var modelJson = JsonSerializer.Serialize(new
                {
                    en = TxtModelEn.Text?.Trim() ?? "Unknown",
                    ru = TxtModelRu.Text?.Trim() ?? "Неизвестно"
                });

                var aircraft = new Aircraft
                {
                    AircraftCode = TxtCode.Text.ToUpper(),
                    Model = modelJson,
                    Range = range
                };

                if (_isNew)
                {
                    await _aircraftService.CreateAsync(aircraft);
                    MessageBox.Show("Самолет успешно добавлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    aircraft.AircraftCode = _aircraft.AircraftCode; // Сохраняем старый код
                    await _aircraftService.UpdateAsync(aircraft);
                    MessageBox.Show("Самолет успешно обновлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
       
        private void ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TxtCode.Text) || TxtCode.Text.Length != 3)
            {
                TxtError.Text = "Код самолета должен состоять из 3 символов";
                BtnSave.IsEnabled = false;
                return;
            }

            if (!int.TryParse(TxtRange.Text, out int range) || range <= 0)
            {
                TxtError.Text = "Дальность должна быть положительным числом";
                BtnSave.IsEnabled = false;
                return;
            }

            TxtError.Text = string.Empty;
            BtnSave.IsEnabled = true;
        }

        // Вызывайте ValidateInputs() в обработчиках текстовых полей:
        private void TxtCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInputs();
        }

        private void TxtRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInputs();
        }
    }
}
