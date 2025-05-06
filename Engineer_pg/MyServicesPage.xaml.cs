using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Engineer_pg
{
    public partial class MyServicesPage : Page
    {
        private int _employeeId;
        private List<ProvidedServices> _allServices;
        private ICollectionView _servicesView;

        public event Action<ProvidedServices> ViewDetailsRequested;

        public MyServicesPage(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadData();
            InitializeView();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    _allServices = db.ProvidedServices
                        .Include("Services")
                        .Include("Contracts")
                        .Include("Contracts.Customers")
                        .Where(ps => ps.EmployeeID == _employeeId)
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

        private void InitializeView()
        {
            _servicesView = CollectionViewSource.GetDefaultView(_allServices);
            _servicesView.Filter = ServicesFilter;
            ServicesListView.ItemsSource = _servicesView;
        }

        private bool ServicesFilter(object item)
        {
            var service = item as ProvidedServices;
            if (service == null) return false;

            DateTime? dateFrom = DateFromPicker.SelectedDate;
            DateTime? dateTo = DateToPicker.SelectedDate;

            if (dateFrom.HasValue && service.ProvideDate < dateFrom.Value.Date)
                return false;

            if (dateTo.HasValue && service.ProvideDate > dateTo.Value.Date.AddDays(1))
                return false;

            return true;
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

            _servicesView.Refresh();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            _servicesView.Refresh();
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ServicesListView.SelectedItem as ProvidedServices;
            if (selectedService != null)
            {
                ViewDetailsRequested?.Invoke(selectedService);
            }
            else
            {
                MessageBox.Show("Выберите заявку для просмотра подробностей", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ServicesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedService = ServicesListView.SelectedItem as ProvidedServices;
            if (selectedService != null)
            {
                ViewDetailsRequested?.Invoke(selectedService);
            }
        }
    }
}