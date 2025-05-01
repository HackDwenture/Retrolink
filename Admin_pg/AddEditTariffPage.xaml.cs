using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Admin_pg
{
    public partial class AddEditTariffPage : Page
    {
        private Tariffs _tariff;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditTariffPage(Tariffs tariff)
        {
            InitializeComponent();

            if (tariff != null)
            {
                _tariff = tariff;
                _isEditMode = true;
                DataContext = new AddEditTariffViewModel(tariff);
            }
            else
            {
                _tariff = new Tariffs();
                _isEditMode = false;
                DataContext = new AddEditTariffViewModel(null);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditTariffViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(viewModel.TariffName) ||
                string.IsNullOrWhiteSpace(viewModel.Speed) || // Speed теперь string
                !decimal.TryParse(viewModel.Price, out decimal price))
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        var tariffToUpdate = db.Tariffs.FirstOrDefault(t => t.TariffID == _tariff.TariffID);
                        if (tariffToUpdate != null)
                        {
                            tariffToUpdate.TariffName = viewModel.TariffName;
                            tariffToUpdate.Speed = viewModel.Speed; // Сохраняем как строку
                            tariffToUpdate.Price = price;
                            tariffToUpdate.Description = viewModel.Description;
                        }
                    }
                    else
                    {
                        _tariff.TariffName = viewModel.TariffName;
                        _tariff.Speed = viewModel.Speed; // Сохраняем как строку
                        _tariff.Price = price;
                        _tariff.Description = viewModel.Description;

                        db.Tariffs.Add(_tariff);
                    }

                    db.SaveChanges();
                    SaveCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke();
        }
    }

    public class AddEditTariffViewModel
    {
        public string WindowTitle { get; }
        public string TariffName { get; set; }
        public string Speed { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }

        public AddEditTariffViewModel(Tariffs tariff)
        {
            if (tariff != null)
            {
                WindowTitle = "Редактирование тарифа";
                TariffName = tariff.TariffName;
                Speed = tariff.Speed.ToString();
                Price = tariff.Price.ToString("0.00");
                Description = tariff.Description;
            }
            else
            {
                WindowTitle = "Добавление тарифа";
                Speed = "100"; // Значение по умолчанию
                Price = "500.00"; // Значение по умолчанию
            }
        }
    }
}