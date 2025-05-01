using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Retrolink.Manager_pg
{
    public partial class CustomersListPage : Page
    {
        public event Action<Customers> EditCustomerRequested;
        public event Action<Customers> DeleteCustomerRequested;
        public event Action<Customers> ViewCustomerDetailsRequested;

        public CustomersListPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            using (var db = new Entities())
            {
                CustomersDataGrid.ItemsSource = db.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadCustomers();
                return;
            }

            using (var db = new Entities())
            {
                var filterType = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                IQueryable<Customers> query = db.Customers;

                switch (filterType)
                {
                    case "Фамилия":
                        query = query.Where(c => c.LastName.ToLower().Contains(searchText));
                        break;
                    case "Телефон":
                        query = query.Where(c => c.PhoneNumber.Contains(searchText));
                        break;
                    case "Email":
                        query = query.Where(c => c.Email.ToLower().Contains(searchText));
                        break;
                    default:
                        query = query.Where(c =>
                            c.LastName.ToLower().Contains(searchText) ||
                            c.FirstName.ToLower().Contains(searchText) ||
                            c.PhoneNumber.Contains(searchText) ||
                            c.Email.ToLower().Contains(searchText));
                        break;
                }

                CustomersDataGrid.ItemsSource = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchTextBox != null && !string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox_TextChanged(sender, null);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var customer = (sender as FrameworkElement)?.DataContext as Customers;
            if (customer != null)
            {
                EditCustomerRequested?.Invoke(customer);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var customer = (sender as FrameworkElement)?.DataContext as Customers;
            if (customer != null)
            {
                DeleteCustomerRequested?.Invoke(customer);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var customer = (sender as FrameworkElement)?.DataContext as Customers;
            if (customer != null)
            {
                ViewCustomerDetailsRequested?.Invoke(customer);
            }
        }
    }
}