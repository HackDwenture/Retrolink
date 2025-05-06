using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Support_pg
{
    public partial class TicketsListPage : Page
    {
        private readonly Employees _currentEmployee;
        private readonly TicketStatus _ticketStatus;
        private List<SupportTickets> _allTickets;
        private ICollectionView _ticketsView;

        public event Action<SupportTickets> ShowDetailsRequested;
        public event Action<SupportTickets> AcceptTicketRequested;
        public event Action<SupportTickets> CompleteTicketRequested;

        public TicketsListPage(Employees employee, TicketStatus ticketStatus)
        {
            InitializeComponent();
            _currentEmployee = employee;
            _ticketStatus = ticketStatus;
            InitializeUI();
            LoadData();
            InitializeView();
        }

        private void InitializeUI()
        {
            switch (_ticketStatus)
            {
                case TicketStatus.New:
                    HeaderText.Text = "Новые заявки";
                    ActionButton.Content = "Принять";
                    break;
                case TicketStatus.InProgress:
                    HeaderText.Text = "Мои заявки";
                    ActionButton.Content = "Завершить";
                    break;
                case TicketStatus.Completed:
                    HeaderText.Text = "Архив заявок";
                    ActionButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    IQueryable<SupportTickets> query = db.SupportTickets
                        .Include(t => t.Customers)
                        .Include(t => t.Employees);

                    switch (_ticketStatus)
                    {
                        case TicketStatus.New:
                            query = query.Where(t => t.Status == "Новая");
                            break;
                        case TicketStatus.InProgress:
                            query = query.Where(t => t.Status == "В обработке" && t.EmployeeID == _currentEmployee.EmployeeID);
                            break;
                        case TicketStatus.Completed:
                            query = query.Where(t => t.Status == "Выполнена");
                            break;
                    }

                    _allTickets = query.OrderByDescending(t => t.TicketDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeView()
        {
            _ticketsView = CollectionViewSource.GetDefaultView(_allTickets);
            _ticketsView.Filter = TicketFilter;
            TicketsListView.ItemsSource = _ticketsView;
        }

        private bool TicketFilter(object item)
        {
            var ticket = item as SupportTickets;
            if (ticket == null) return false;

            string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            bool matchesSearch = string.IsNullOrEmpty(searchText);

            if (!matchesSearch)
            {
                matchesSearch =
                    (ticket.TicketID.ToString().Contains(searchText)) ||
                    (ticket.Customers.LastName?.ToLower().Contains(searchText) ?? false) ||
                    (ticket.Customers.FirstName?.ToLower().Contains(searchText) ?? false) ||
                    (ticket.Description?.ToLower().Contains(searchText) ?? false) ||
                    (ticket.Status?.ToLower().Contains(searchText) ?? false) ||
                    (ticket.Employees?.LastName?.ToLower().Contains(searchText) ?? false);
            }

            bool matchesDate = true;
            if (DateFromPicker.SelectedDate.HasValue)
            {
                matchesDate = ticket.TicketDate >= DateFromPicker.SelectedDate.Value;
            }
            if (DateToPicker.SelectedDate.HasValue && matchesDate)
            {
                matchesDate = ticket.TicketDate <= DateToPicker.SelectedDate.Value.AddDays(1);
            }

            return matchesSearch && matchesDate;
        }

        private void ApplyFilters()
        {
            if (_ticketsView != null)
            {
                _ticketsView.Filter = TicketFilter;
            }
        }

        private SupportTickets GetSelectedTicket()
        {
            return TicketsListView.SelectedItem as SupportTickets;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            ApplyFilters();
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            var ticket = GetSelectedTicket();
            if (ticket != null)
            {
                ShowDetailsRequested?.Invoke(ticket);
            }
            else
            {
                MessageBox.Show("Выберите заявку для просмотра", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var ticket = GetSelectedTicket();
            if (ticket != null)
            {
                if (_ticketStatus == TicketStatus.New)
                {
                    AcceptTicketRequested?.Invoke(ticket);
                }
                else if (_ticketStatus == TicketStatus.InProgress)
                {
                    CompleteTicketRequested?.Invoke(ticket);
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TicketsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ticket = GetSelectedTicket();
            if (ticket != null)
            {
                ShowDetailsRequested?.Invoke(ticket);
            }
        }
    }
}