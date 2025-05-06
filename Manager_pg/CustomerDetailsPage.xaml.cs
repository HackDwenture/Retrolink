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
        public event Action AddContractRequested;
        public event Action<Contracts> EditContractRequested;
        public event Action<Contracts> DeleteContractRequested;
        public event Action<ProvidedServices> AddProvidedServiceRequested;
        public event Action<ProvidedServices> EditProvidedServiceRequested;
        public event Action<ProvidedServices> DeleteProvidedServiceRequested;

        public Customers CurrentCustomer => _customer;

        public CustomerDetailsPage(Customers customer)
        {
            InitializeComponent();
            _customer = customer;
            DataContext = new CustomerDetailsViewModel(customer);
            LoadContracts();
            LoadProvidedServices();
            LoadPayments();
        }

        private void LoadContracts()
        {
            try
            {
                using (var db = new Entities())
                {
                    ContractsDataGrid.ItemsSource = db.Contracts
                        .Include(c => c.Tariffs)
                        .Where(c => c.CustomerID == _customer.CustomerID)
                        .OrderByDescending(c => c.ContractDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке контрактов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProvidedServices()
        {
            try
            {
                using (var db = new Entities())
                {
                    ProvidedServicesDataGrid.ItemsSource = db.ProvidedServices
                        .Include(ps => ps.Contracts)
                        .Include(ps => ps.Services)
                        .Include(ps => ps.Employees)
                        .Where(ps => ps.Contracts.CustomerID == _customer.CustomerID)
                        .OrderByDescending(ps => ps.ProvideDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPayments()
        {
            try
            {
                using (var db = new Entities())
                {
                    PaymentsDataGrid.ItemsSource = db.Payments
                        .Where(p => p.CustomerID == _customer.CustomerID)
                        .OrderByDescending(p => p.PaymentDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке платежей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            AddContractRequested?.Invoke();
        }

        private void EditContract_Click(object sender, RoutedEventArgs e)
        {
            var selectedContract = ContractsDataGrid.SelectedItem as Contracts;
            if (selectedContract != null)
            {
                EditContractRequested?.Invoke(selectedContract);
            }
            else
            {
                MessageBox.Show("Выберите контракт для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            var selectedContract = ContractsDataGrid.SelectedItem as Contracts;
            if (selectedContract != null)
            {
                DeleteContractRequested?.Invoke(selectedContract);
            }
            else
            {
                MessageBox.Show("Выберите контракт для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddProvidedService_Click(object sender, RoutedEventArgs e)
        {
            AddProvidedServiceRequested?.Invoke(new ProvidedServices { ProvideDate = DateTime.Now });
        }

        private void EditProvidedService_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ProvidedServicesDataGrid.SelectedItem as ProvidedServices;
            if (selectedService != null)
            {
                EditProvidedServiceRequested?.Invoke(selectedService);
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteProvidedService_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ProvidedServicesDataGrid.SelectedItem as ProvidedServices;
            if (selectedService != null)
            {
                DeleteProvidedServiceRequested?.Invoke(selectedService);
            }
            else
            {
                MessageBox.Show("Выберите услугу для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FilterPayments_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите период для фильтрации", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var startDate = StartDatePicker.SelectedDate.Value.Date;
                var endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1);

                using (var db = new Entities())
                {
                    var filteredPayments = db.Payments
                        .Where(p => p.CustomerID == _customer.CustomerID &&
                                   p.PaymentDate >= startDate &&
                                   p.PaymentDate < endDate)
                        .OrderByDescending(p => p.PaymentDate)
                        .ToList();

                    PaymentsDataGrid.ItemsSource = filteredPayments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации платежей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
    }

    public class CustomerDetailsViewModel
    {
        public string CustomerName { get; }
        public string PhoneNumber { get; }
        public string Email { get; }
        public string Address { get; }
        public string PassportData { get; }
        public DateTime RegistrationDate { get; }

        public CustomerDetailsViewModel(Customers customer)
        {
            CustomerName = $"{customer.LastName} {customer.FirstName} {customer.Patronymic}";
            PhoneNumber = customer.PhoneNumber;
            Email = customer.Email;
            Address = customer.Address;
            PassportData = customer.PassportData;
            RegistrationDate = customer.RegistrationDate;
        }
    }
}