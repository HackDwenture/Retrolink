using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Manager_pg
{
    public partial class CustomersListPage : Page
    {
        private ICollectionView _customersView;

        public event Action<Customers> EditCustomerRequested;
        public event Action<Customers> DeleteCustomerRequested;
        public event Action<Customers> ViewCustomerDetailsRequested;

        public CustomersListPage()
        {
            InitializeComponent();
            LoadCustomers();
            InitializeView();
        }

        private void LoadCustomers()
        {
            try
            {
                using (var db = new Entities())
                {
                    var customers = db.Customers
                        .OrderBy(c => c.LastName)
                        .ThenBy(c => c.FirstName)
                        .ToList();

                    _customersView = CollectionViewSource.GetDefaultView(customers);
                    CustomersListView.ItemsSource = _customersView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeView()
        {
            if (_customersView != null)
            {
                _customersView.Filter = CustomerFilter;
            }
        }

        private bool CustomerFilter(object item)
        {
            var customer = item as Customers;
            if (customer == null) return false;

            string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText)) return true;

            var filterType = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            switch (filterType)
            {
                case "Фамилия":
                    return customer.LastName?.ToLower().Contains(searchText) ?? false;
                case "Телефон":
                    return customer.PhoneNumber?.Contains(searchText) ?? false;
                case "Email":
                    return customer.Email?.ToLower().Contains(searchText) ?? false;
                default:
                    return (customer.LastName?.ToLower().Contains(searchText) ?? false) ||
                           (customer.FirstName?.ToLower().Contains(searchText) ?? false) ||
                           (customer.PhoneNumber?.Contains(searchText) ?? false) ||
                           (customer.Email?.ToLower().Contains(searchText) ?? false);
            }
        }

        private void ApplyFilters()
        {
            if (_customersView != null)
            {
                _customersView.Refresh();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            FilterComboBox.SelectedIndex = 0;
            ApplyFilters();
        }

        private Customers GetSelectedCustomer()
        {
            return CustomersListView.SelectedItem as Customers;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var customer = GetSelectedCustomer();
            if (customer != null)
            {
                EditCustomerRequested?.Invoke(customer);
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var customer = GetSelectedCustomer();
            if (customer != null)
            {
                DeleteCustomerRequested?.Invoke(customer);
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var customer = GetSelectedCustomer();
            if (customer != null)
            {
                ViewCustomerDetailsRequested?.Invoke(customer);
            }
            else
            {
                MessageBox.Show("Выберите клиента для просмотра", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CustomersListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewDetails_Click(sender, e);
        }
    }
}