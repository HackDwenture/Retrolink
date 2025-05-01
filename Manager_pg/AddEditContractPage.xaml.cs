using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Manager_pg
{
    public partial class AddEditContractPage : Page
    {
        private Contracts _contract;
        private Customers _customer;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditContractPage(Contracts contract, Customers customer)
        {
            InitializeComponent();
            _customer = customer;

            if (contract != null)
            {
                _contract = contract;
                _isEditMode = true;
                DataContext = new AddEditContractViewModel(contract, customer);
            }
            else
            {
                _contract = new Contracts
                {
                    CustomerID = customer.CustomerID,
                    ContractDate = DateTime.Now,
                    InstallationDate = DateTime.Now.AddDays(3)
                };
                _isEditMode = false;
                DataContext = new AddEditContractViewModel(null, customer);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditContractViewModel)DataContext;

            if (viewModel.ContractDate == null || viewModel.InstallationDate == null || viewModel.SelectedTariffId == 0)
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
                        var contractToUpdate = db.Contracts.FirstOrDefault(c => c.ContractID == _contract.ContractID);
                        if (contractToUpdate != null)
                        {
                            contractToUpdate.ContractDate = viewModel.ContractDate.Value;
                            contractToUpdate.InstallationDate = viewModel.InstallationDate.Value;
                            contractToUpdate.TariffID = viewModel.SelectedTariffId;
                        }
                    }
                    else
                    {
                        _contract.ContractDate = viewModel.ContractDate.Value;
                        _contract.InstallationDate = viewModel.InstallationDate.Value;
                        _contract.TariffID = viewModel.SelectedTariffId;

                        db.Contracts.Add(_contract);
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

    public class AddEditContractViewModel
    {
        public string WindowTitle { get; }
        public string CustomerName { get; }
        public DateTime? ContractDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public System.Collections.Generic.List<Tariffs> Tariffs { get; }
        public int SelectedTariffId { get; set; }

        public AddEditContractViewModel(Contracts contract, Customers customer)
        {
            using (var db = new Entities())
            {
                Tariffs = db.Tariffs.ToList();
            }

            CustomerName = $"{customer.LastName} {customer.FirstName} {customer.Patronymic}";

            if (contract != null)
            {
                WindowTitle = "Редактирование контракта";
                ContractDate = contract.ContractDate;
                InstallationDate = contract.InstallationDate;
                SelectedTariffId = contract.TariffID;
            }
            else
            {
                WindowTitle = "Добавление контракта";
                ContractDate = DateTime.Now;
                InstallationDate = DateTime.Now.AddDays(3);
            }
        }
    }
}