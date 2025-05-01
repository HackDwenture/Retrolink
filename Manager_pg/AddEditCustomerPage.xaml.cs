using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Manager_pg
{
    public partial class AddEditCustomerPage : Page
    {
        private Customers _customer;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditCustomerPage(Customers customer)
        {
            InitializeComponent();

            if (customer != null)
            {
                _customer = customer;
                _isEditMode = true;
                DataContext = new AddEditCustomerViewModel(customer);
            }
            else
            {
                _customer = new Customers { RegistrationDate = DateTime.Now };
                _isEditMode = false;
                DataContext = new AddEditCustomerViewModel(null);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditCustomerViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(viewModel.LastName) || string.IsNullOrWhiteSpace(viewModel.FirstName) ||
                string.IsNullOrWhiteSpace(viewModel.PhoneNumber) || string.IsNullOrWhiteSpace(viewModel.Email) ||
                string.IsNullOrWhiteSpace(viewModel.Address) || string.IsNullOrWhiteSpace(viewModel.PassportData))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        var customerToUpdate = db.Customers.FirstOrDefault(c => c.CustomerID == _customer.CustomerID);
                        if (customerToUpdate != null)
                        {
                            customerToUpdate.LastName = viewModel.LastName;
                            customerToUpdate.FirstName = viewModel.FirstName;
                            customerToUpdate.Patronymic = viewModel.Patronymic;
                            customerToUpdate.PhoneNumber = viewModel.PhoneNumber;
                            customerToUpdate.Email = viewModel.Email;
                            customerToUpdate.Address = viewModel.Address;
                            customerToUpdate.PassportData = viewModel.PassportData;
                        }
                    }
                    else
                    {
                        _customer.LastName = viewModel.LastName;
                        _customer.FirstName = viewModel.FirstName;
                        _customer.Patronymic = viewModel.Patronymic;
                        _customer.PhoneNumber = viewModel.PhoneNumber;
                        _customer.Email = viewModel.Email;
                        _customer.Address = viewModel.Address;
                        _customer.PassportData = viewModel.PassportData;
                        _customer.RegistrationDate = DateTime.Now;

                        db.Customers.Add(_customer);
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

    public class AddEditCustomerViewModel
    {
        public string WindowTitle { get; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PassportData { get; set; }
        public DateTime RegistrationDate { get; set; }

        public AddEditCustomerViewModel(Customers customer)
        {
            if (customer != null)
            {
                WindowTitle = "Редактирование клиента";
                LastName = customer.LastName;
                FirstName = customer.FirstName;
                Patronymic = customer.Patronymic;
                PhoneNumber = customer.PhoneNumber;
                Email = customer.Email;
                Address = customer.Address;
                PassportData = customer.PassportData;
                RegistrationDate = customer.RegistrationDate;
            }
            else
            {
                WindowTitle = "Добавление клиента";
                RegistrationDate = DateTime.Now;
            }
        }
    }
}