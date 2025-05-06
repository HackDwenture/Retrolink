using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Retrolink.Support_pg;

namespace Retrolink
{
    public partial class SupportWindow : Window
    {
        private Employees _currentEmployee;

        public SupportWindow(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            Title = $"Поддержка - {employee.FirstName} {employee.LastName}";
            ShowNewTickets();
        }

        private void ShowNewTickets_Click(object sender, RoutedEventArgs e) => ShowNewTickets();
        private void ShowMyTickets_Click(object sender, RoutedEventArgs e) => ShowMyTickets();
        private void ShowArchiveTickets_Click(object sender, RoutedEventArgs e) => ShowArchiveTickets();

        private void ShowNewTickets()
        {
            var page = new Support_pg.TicketsListPage(_currentEmployee, TicketStatus.New);
            page.ShowDetailsRequested += (ticket) => ShowTicketDetails(ticket);
            page.AcceptTicketRequested += (ticket) => AcceptTicket(ticket);
            MainFrame.Navigate(page);
        }

        private void ShowMyTickets()
        {
            var page = new Support_pg.TicketsListPage(_currentEmployee, TicketStatus.InProgress);
            page.ShowDetailsRequested += (ticket) => ShowTicketDetails(ticket);
            page.CompleteTicketRequested += (ticket) => CompleteTicket(ticket);
            MainFrame.Navigate(page);
        }

        private void ShowArchiveTickets()
        {
            var page = new Support_pg.TicketsListPage(_currentEmployee, TicketStatus.Completed);
            page.ShowDetailsRequested += (ticket) => ShowTicketDetails(ticket);
            MainFrame.Navigate(page);
        }

        private void ShowTicketDetails(SupportTickets ticket)
        {
            var page = new Support_pg.TicketDetailsPage(ticket);
            page.BackRequested += () =>
            {
                if (ticket.Status == "Новая") ShowNewTickets();
                else if (ticket.Status == "В обработке") ShowMyTickets();
                else ShowArchiveTickets();
            };
            MainFrame.Navigate(page);
        }

        private void AcceptTicket(SupportTickets ticket)
        {
            try
            {
                using (var db = new Entities())
                {
                    var ticketToUpdate = db.SupportTickets.FirstOrDefault(t => t.TicketID == ticket.TicketID);
                    if (ticketToUpdate != null)
                    {
                        ticketToUpdate.Status = "В обработке";
                        ticketToUpdate.EmployeeID = _currentEmployee.EmployeeID;
                        db.SaveChanges();
                        ShowNewTickets();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при принятии заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteTicket(SupportTickets ticket)
        {
            try
            {
                using (var db = new Entities())
                {
                    var ticketToUpdate = db.SupportTickets.FirstOrDefault(t => t.TicketID == ticket.TicketID);
                    if (ticketToUpdate != null)
                    {
                        ticketToUpdate.Status = "Выполнена";
                        db.SaveChanges();
                        ShowMyTickets();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }

    public enum TicketStatus
    {
        New,
        InProgress,
        Completed
    }
}