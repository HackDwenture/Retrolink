using System.Windows;
using System.Windows.Controls;

namespace Retrolink
{
    public partial class EngineerWindow : Window
    {
        private Employees _employee;

        public EngineerWindow(Employees employee)
        {
            InitializeComponent();
            _employee = employee;
            Title = $"Окно инженера - {_employee.FirstName} {_employee.LastName}";

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
