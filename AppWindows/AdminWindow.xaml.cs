using System;
using System.Linq;
using System.Windows;

namespace Retrolink
{
    public partial class AdminWindow : Window
    {
        public AdminWindow(Employees employee)
        {
            InitializeComponent();
            Title = $"Окно Админа - {employee.FirstName} {employee.LastName}";
            LoadEmployeesData();
        }

        private void LoadEmployeesData()
        {
            try
            {
                using (var db = new Entities())
                {
                    
                    var employees = db.Employees.Include("Roles").ToList();
                    EmployeesDataGrid.ItemsSource = employees;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }
    }
}