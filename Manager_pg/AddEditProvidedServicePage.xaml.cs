using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Manager_pg
{
    public partial class AddEditProvidedServicePage : Page
    {
        private ProvidedServices _currentService;
        private Customers _customer;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public new string Title => _currentService.ProvidedServiceID == 0 ? "Добавить услугу" : "Редактировать услугу";
        public ProvidedServices CurrentService => _currentService;

        public AddEditProvidedServicePage(ProvidedServices service, Customers customer)
        {
            InitializeComponent();
            _currentService = service ?? new ProvidedServices();
            _customer = customer;
            DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    var contracts = db.Contracts
                        .Where(c => c.CustomerID == _customer.CustomerID)
                        .OrderByDescending(c => c.ContractDate)
                        .ToList();

                    ContractComboBox.ItemsSource = contracts;
                    ContractComboBox.DisplayMemberPath = "ContractDate";
                    ContractComboBox.SelectedValuePath = "ContractID";

                    var services = db.Services
                        .OrderBy(s => s.ServiceName)
                        .ToList();

                    ServiceComboBox.ItemsSource = services;
                    ServiceComboBox.DisplayMemberPath = "ServiceName";
                    ServiceComboBox.SelectedValuePath = "ServiceID";

                    var engineers = db.Employees
                        .Include("Roles")
                        .Where(e => e.Roles.RoleName == "Инженер")
                        .ToList()
                        .Select(e => new
                        {
                            e.EmployeeID,
                            FullName = $"{e.LastName} {e.FirstName}"
                        })
                        .ToList();

                    EmployeeComboBox.ItemsSource = engineers;
                    EmployeeComboBox.DisplayMemberPath = "FullName";
                    EmployeeComboBox.SelectedValuePath = "EmployeeID";

                    if (_currentService.ProvidedServiceID != 0)
                    {
                        ContractComboBox.SelectedValue = _currentService.ContractID;
                        ServiceComboBox.SelectedValue = _currentService.ServiceID;
                        EmployeeComboBox.SelectedValue = _currentService.EmployeeID;
                        ProvideDatePicker.SelectedDate = _currentService.ProvideDate;
                    }
                    else
                    {
                        _currentService.ProvideDate = DateTime.Now;
                        ProvideDatePicker.SelectedDate = _currentService.ProvideDate;

                        if (ContractComboBox.Items.Count > 0)
                            ContractComboBox.SelectedIndex = 0;
                        if (EmployeeComboBox.Items.Count > 0)
                            EmployeeComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContractComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите контракт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ServiceComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите услугу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EmployeeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите исполнителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentService.ContractID = (int)ContractComboBox.SelectedValue;
                _currentService.ServiceID = (int)ServiceComboBox.SelectedValue;
                _currentService.EmployeeID = (int)EmployeeComboBox.SelectedValue;
                _currentService.ProvideDate = ProvideDatePicker.SelectedDate ?? DateTime.Now;

                using (var db = new Entities())
                {
                    if (_currentService.ProvidedServiceID == 0)
                    {
                        db.ProvidedServices.Add(_currentService);
                    }
                    else
                    {
                        var existing = db.ProvidedServices.Find(_currentService.ProvidedServiceID);
                        if (existing != null)
                        {
                            existing.ContractID = _currentService.ContractID;
                            existing.ServiceID = _currentService.ServiceID;
                            existing.EmployeeID = _currentService.EmployeeID;
                            existing.ProvideDate = _currentService.ProvideDate;
                        }
                    }

                    db.SaveChanges();
                    SaveCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke();
        }
    }
}