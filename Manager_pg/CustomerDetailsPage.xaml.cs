using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Manager_pg
{
    public partial class CustomerDetailsPage : Page
    {
        private Customers _customer;

        public event Action BackRequested;
        public event Action<Contracts> EditContractRequested;
        public event Action AddContractRequested;
        public event Action<Contracts> DeleteContractRequested;

        public CustomerDetailsPage(Customers customer)
        {
            InitializeComponent();
            _customer = customer;
            DataContext = new CustomerDetailsViewModel(customer);
            LoadContracts();
        }

        private void LoadContracts()
        {
            using (var db = new Entities())
            {
                ContractsDataGrid.ItemsSource = db.Contracts
                    .Where(c => c.CustomerID == _customer.CustomerID)
                    .Include("Tariffs")
                    .OrderByDescending(c => c.ContractDate)
                    .ToList();
            }
        }

        private void LoadPayments()
        {
            using (var db = new Entities())
            {
                var payments = db.Payments
                    .Where(p => p.CustomerID == _customer.CustomerID)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();

                PaymentsDataGrid.ItemsSource = payments;
            }
        }

        private void FilterPayments_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты для фильтрации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new Entities())
            {
                var filteredPayments = db.Payments
                    .Where(p => p.CustomerID == _customer.CustomerID &&
                                p.PaymentDate >= StartDatePicker.SelectedDate &&
                                p.PaymentDate <= EndDatePicker.SelectedDate)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();

                PaymentsDataGrid.ItemsSource = filteredPayments;
            }
        }

        private void ResetPaymentsFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            LoadPayments();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            AddContractRequested?.Invoke();
        }

        private void EditContract_Click(object sender, RoutedEventArgs e)
        {
            var contract = (sender as FrameworkElement)?.DataContext as Contracts;
            if (contract != null)
            {
                EditContractRequested?.Invoke(contract);
            }
        }

        private void DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            var contract = (sender as FrameworkElement)?.DataContext as Contracts;
            if (contract != null)
            {
                DeleteContractRequested?.Invoke(contract);
            }
        }
    }

    public class CustomerDetailsViewModel
    {
        public string CustomerName { get; }
        public string PhoneNumber { get; }
        public string Email { get; }
        public string Address { get; }
        public DateTime RegistrationDate { get; }

        public CustomerDetailsViewModel(Customers customer)
        {
            CustomerName = $"{customer.LastName} {customer.FirstName} {customer.Patronymic}";
            PhoneNumber = customer.PhoneNumber;
            Email = customer.Email;
            Address = customer.Address;
            RegistrationDate = customer.RegistrationDate;
        }
    }
}