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