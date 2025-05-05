using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Engineer_pg
{
    public partial class ServiceDetailsPage : Page
    {
        private ProvidedServices _service;
        public event Action BackRequested;

        public ServiceDetailsPage(ProvidedServices service)
        {
            InitializeComponent();
            _service = service;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    // Загружаем полные данные о заявке
                    var fullService = db.ProvidedServices
                        .Include("Services")
                        .Include("Contracts")
                        .Include("Contracts.Customers")
                        .FirstOrDefault(ps => ps.ProvidedServiceID == _service.ProvidedServiceID);

                    if (fullService != null)
                    {
                        DataContext = new ServiceDetailsViewModel(fullService);
                        ServicesDataGrid.ItemsSource = new[] { fullService };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }
    }

    public class ServiceDetailsViewModel
    {
        public string CustomerFullName { get; }
        public string CustomerPhone { get; }
        public string CustomerEmail { get; }
        public string CustomerAddress { get; }
        public DateTime ServiceDate { get; }
        public int ContractID { get; }
        public string Status { get; } = "Выполнено";

        public ServiceDetailsViewModel(ProvidedServices service)
        {
            CustomerFullName = $"{service.Contracts.Customers.LastName} {service.Contracts.Customers.FirstName} {service.Contracts.Customers.Patronymic}";
            CustomerPhone = service.Contracts.Customers.PhoneNumber;
            CustomerEmail = service.Contracts.Customers.Email;
            CustomerAddress = service.Contracts.Customers.Address;
            ServiceDate = (DateTime)service.ProvideDate;
            ContractID = service.ContractID;
        }
    }
}