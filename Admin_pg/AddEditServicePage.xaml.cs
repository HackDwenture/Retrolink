using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Admin_pg
{
    public partial class AddEditServicePage : Page
    {
        private Services _service;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditServicePage(Services service)
        {
            InitializeComponent();

            if (service != null)
            {
                _service = service;
                _isEditMode = true;
                DataContext = new AddEditServiceViewModel(service);
            }
            else
            {
                _service = new Services();
                _isEditMode = false;
                DataContext = new AddEditServiceViewModel(null);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditServiceViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(viewModel.ServiceName) ||
                viewModel.Price <= 0)
            {
                MessageBox.Show("Заполните обязательные поля: название и цена (должна быть больше 0)",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        var serviceToUpdate = db.Services.FirstOrDefault(s => s.ServiceID == _service.ServiceID);
                        if (serviceToUpdate != null)
                        {
                            serviceToUpdate.ServiceName = viewModel.ServiceName;
                            serviceToUpdate.Description = viewModel.Description;
                            serviceToUpdate.Price = viewModel.Price;
                        }
                    }
                    else
                    {
                        _service.ServiceName = viewModel.ServiceName;
                        _service.Description = viewModel.Description;
                        _service.Price = viewModel.Price;
                        db.Services.Add(_service);
                    }

                    db.SaveChanges();
                    SaveCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke();
        }
    }

    public class AddEditServiceViewModel
    {
        public string WindowTitle { get; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public AddEditServiceViewModel(Services service)
        {
            if (service != null)
            {
                WindowTitle = "Редактирование услуги";
                ServiceName = service.ServiceName;
                Description = service.Description;
                Price = service.Price;
            }
            else
            {
                WindowTitle = "Добавление услуги";
                Price = 0;
                Description = "";
            }
        }
    }
}