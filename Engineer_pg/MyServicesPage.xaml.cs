using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Engineer_pg
{
    public partial class MyServicesPage : Page
    {
        private int _employeeId;
        public event Action<ProvidedServices> ViewDetailsRequested;

        public MyServicesPage(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadData();
        }

        private void LoadData(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                using (var db = new Entities())
                {
                    var query = db.ProvidedServices
                        .Include("Services")
                        .Include("Contracts")
                        .Include("Contracts.Customers")
                        .Where(ps => ps.EmployeeID == _employeeId);

                    if (dateFrom.HasValue)
                        query = query.Where(ps => ps.ProvideDate >= dateFrom.Value.Date);

                    if (dateTo.HasValue)
                        query = query.Where(ps => ps.ProvideDate <= dateTo.Value.Date.AddDays(1));

                    ServicesDataGrid.ItemsSource = query
                        .OrderByDescending(ps => ps.ProvideDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterServices_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dateFrom = DateFromPicker.SelectedDate;
            DateTime? dateTo = DateToPicker.SelectedDate;

            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            {
                MessageBox.Show("Дата 'с' не может быть позже даты 'по'", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadData(dateFrom, dateTo?.Date.AddDays(1)); // Добавляем 1 день для включения всей конечной даты
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            LoadData();
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var service = (sender as FrameworkElement)?.DataContext as ProvidedServices;
            if (service != null)
            {
                ViewDetailsRequested?.Invoke(service);
            }
        }
    }
}