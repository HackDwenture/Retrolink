using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Admin_pg
{
    public partial class ServicesListPage : Page
    {
        public event Action<Services> EditServiceRequested;
        public event Action<Services> DeleteServiceRequested;

        public ServicesListPage()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                using (var db = new Entities())
                {
                    ServicesListView.ItemsSource = db.Services.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Services GetSelectedService()
        {
            return ServicesListView.SelectedItem as Services;
        }

        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            var service = GetSelectedService();
            if (service != null)
            {
                EditServiceRequested?.Invoke(service);
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            var service = GetSelectedService();
            if (service != null)
            {
                DeleteServiceRequested?.Invoke(service);
            }
            else
            {
                MessageBox.Show("Выберите услугу для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ServicesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var service = GetSelectedService();
            if (service != null)
            {
                EditServiceRequested?.Invoke(service);
            }
        }
    }
}