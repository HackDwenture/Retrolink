using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Admin_pg
{
    public partial class EmployeesListPage : Page
    {
        private List<Employees> _allEmployees;
        private List<Roles> _allRoles;
        private ICollectionView _employeesView;

        public event Action<Employees> EditEmployeeRequested;
        public event Action<Employees> DeleteEmployeeRequested;

        public EmployeesListPage()
        {
            InitializeComponent();
            LoadData();
            InitializeView();
            InitializeFilters();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    _allEmployees = db.Employees.Include("Roles").ToList();
                    _allRoles = db.Roles.OrderBy(r => r.RoleName).ToList();
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
            _employeesView = CollectionViewSource.GetDefaultView(_allEmployees);
            _employeesView.Filter = EmployeeFilter;
            EmployeesListView.ItemsSource = _employeesView;
        }

        private bool EmployeeFilter(object item)
        {
            var employee = item as Employees;
            if (employee == null) return false;

            string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            bool matchesSearch = string.IsNullOrEmpty(searchText);

            if (!matchesSearch)
            {
                matchesSearch =
                    (employee.LastName?.ToLower().Contains(searchText) ?? false) ||
                    (employee.FirstName?.ToLower().Contains(searchText) ?? false) ||
                    (employee.Patronymic?.ToLower().Contains(searchText) ?? false) ||
                    (employee.PhoneNumber?.ToLower().Contains(searchText) ?? false) ||
                    (employee.Email?.ToLower().Contains(searchText) ?? false) ||
                    (employee.Roles?.RoleName?.ToLower().Contains(searchText) ?? false);
            }

            bool matchesRole = true;
            if (RoleFilterComboBox?.SelectedItem != null)
            {
                var selectedRole = (Roles)RoleFilterComboBox.SelectedItem;
                matchesRole = employee.Roles != null && employee.Roles.RoleID == selectedRole.RoleID;
            }

            return matchesSearch && matchesRole;
        }

        private void InitializeFilters()
        {
            RoleFilterComboBox.ItemsSource = _allRoles;
            RoleFilterComboBox.DisplayMemberPath = "RoleName";
            RoleFilterComboBox.SelectedIndex = -1;
            SortComboBox.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (_employeesView != null)
            {
                _employeesView.Filter = EmployeeFilter;
                ApplySorting();
            }
        }

        private void ApplySorting()
        {
            _employeesView.SortDescriptions.Clear();

            switch (SortComboBox.SelectedIndex)
            {
                case 0: // По фамилии (А-Я)
                    _employeesView.SortDescriptions.Add(
                        new SortDescription("LastName", ListSortDirection.Ascending));
                    break;
                case 1: // По фамилии (Я-А)
                    _employeesView.SortDescriptions.Add(
                        new SortDescription("LastName", ListSortDirection.Descending));
                    break;
                case 2: // По дате приема (новые)
                    _employeesView.SortDescriptions.Add(
                        new SortDescription("HireDate", ListSortDirection.Descending));
                    break;
                case 3: // По дате приема (старые)
                    _employeesView.SortDescriptions.Add(
                        new SortDescription("HireDate", ListSortDirection.Ascending));
                    break;
                case 4: // По должности
                    _employeesView.SortDescriptions.Add(
                        new SortDescription("Roles.RoleName", ListSortDirection.Ascending));
                    break;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            RoleFilterComboBox.SelectedIndex = -1;
            SortComboBox.SelectedIndex = 0;
            ApplyFilters();
        }

        private Employees GetSelectedEmployee()
        {
            return EmployeesListView.SelectedItem as Employees;
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var employee = GetSelectedEmployee();
            if (employee != null)
            {
                EditEmployeeRequested?.Invoke(employee);
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var employee = GetSelectedEmployee();
            if (employee != null)
            {
                DeleteEmployeeRequested?.Invoke(employee);
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EmployeesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var employee = GetSelectedEmployee();
            if (employee != null)
            {
                EditEmployeeRequested?.Invoke(employee);
            }
        }
    }
}