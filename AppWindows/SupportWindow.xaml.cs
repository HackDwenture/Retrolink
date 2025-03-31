using System.Windows;
using System.Windows.Controls;

namespace Retrolink
{
    public partial class SupportWindow : Window
    {
        private Employees _employee;

        public SupportWindow(Employees employee)
        {
            InitializeComponent();
            _employee = employee;
            Title = $"Окно поддержки - {_employee.FirstName} {_employee.LastName}";

            DisplayEmployeeInfo();
        }

        private void DisplayEmployeeInfo()
        {
            EmployeeNameLabel.Content = $"{_employee.FirstName} {_employee.LastName}";
            EmployeePositionLabel.Content = _employee.RoleID;
            EmployeePhoneLabel.Content = _employee.PhoneNumber;
            EmployeeEmailLabel.Content = _employee.Email;
        }
    }
}
