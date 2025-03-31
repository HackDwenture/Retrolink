using Retrolink.Manager_pg;
using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Retrolink
{
    public partial class ManagerWindow : Window
    {
        private Employees _employee;

        public ManagerWindow(Employees employee)
        {
            InitializeComponent();
            _employee = employee;
            Title = $"Окно менеджера - {_employee.FirstName} {_employee.LastName}";
        }

        private void ContractsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Contracts contracts = new Contracts();
            ManagerFrame.Navigate(new Manager_pg.ContractsView(contracts));
        }

        private void CustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void PaymentsMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void TariffsMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}