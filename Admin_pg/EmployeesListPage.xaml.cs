using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Retrolink.Admin_pg
{
    public partial class EmployeesListPage : Page
    {
        public event Action<Employees> EditEmployeeRequested;
        public event Action<Employees> DeleteEmployeeRequested;

        public EmployeesListPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var db = new Entities())
                {
                    var employees = db.Employees
                        .Include("Roles")
                        .Include("Accounts")
                        .ToList();

                    EmployeesListView.ItemsSource = employees;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Employees GetSelectedEmployee()
        {
            return EmployeesListView.SelectedItem as Employees;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = GetSelectedEmployee();
            if (selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования",
                              "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            EditEmployeeRequested?.Invoke(selectedEmployee);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = GetSelectedEmployee();
            if (selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления",
                              "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DeleteEmployeeRequested?.Invoke(selectedEmployee);
        }

        // Добавляем обработчик двойного клика для редактирования
        private void EmployeesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }
    }
}