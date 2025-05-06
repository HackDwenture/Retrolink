using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Support_pg
{
    public partial class TicketDetailsPage : Page
    {
        private readonly SupportTickets _ticket;

        public event Action BackRequested;

        public TicketDetailsPage(SupportTickets ticket)
        {
            InitializeComponent();
            _ticket = ticket;
            DataContext = _ticket;
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                using (var db = new Entities())
                {
                    ContractsDataGrid.ItemsSource = db.Contracts
                        .Include(c => c.Tariffs)
                        .Where(c => c.CustomerID == _ticket.CustomerID)
                        .OrderByDescending(c => c.ContractDate)
                        .ToList();

                    EquipmentDataGrid.ItemsSource = db.InstalledEquipment
                        .Include(ie => ie.Equipment)
                        .Where(ie => ie.Contracts.CustomerID == _ticket.CustomerID)
                        .ToList();

                    ServicesDataGrid.ItemsSource = db.ProvidedServices
                        .Include(ps => ps.Services)
                        .Where(ps => ps.Contracts.CustomerID == _ticket.CustomerID)
                        .OrderByDescending(ps => ps.ProvideDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных клиента: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }
    }
}