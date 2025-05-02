using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;

namespace Retrolink.Admin_pg
{
    public partial class AddEditEmployeePage : Page
    {
        private Employees _employee;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditEmployeePage(Employees employee)
        {
            InitializeComponent();

            if (employee != null)
            {
                _employee = employee;
                _isEditMode = true;
                DataContext = new AddEditEmployeeViewModel(employee);
            }
            else
            {
                _employee = new Employees { HireDate = DateTime.Now };
                _isEditMode = false;
                DataContext = new AddEditEmployeeViewModel(null);
            }
        }

        private string GetHash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditEmployeeViewModel)DataContext;

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(viewModel.LastName) ||
                string.IsNullOrWhiteSpace(viewModel.FirstName) ||
                string.IsNullOrWhiteSpace(viewModel.PhoneNumber) ||
                string.IsNullOrWhiteSpace(viewModel.Email) ||
                viewModel.HireDate == null ||
                viewModel.SelectedRoleId == 0)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка пароля если он введен
            bool passwordChanged = !string.IsNullOrWhiteSpace(PasswordBox.Password);
            if (passwordChanged)
            {
                if (PasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        // Обновление данных сотрудника
                        var employeeToUpdate = db.Employees.FirstOrDefault(emp => emp.EmployeeID == _employee.EmployeeID);
                        if (employeeToUpdate != null)
                        {
                            employeeToUpdate.LastName = viewModel.LastName;
                            employeeToUpdate.FirstName = viewModel.FirstName;
                            employeeToUpdate.Patronymic = viewModel.Patronymic;
                            employeeToUpdate.PhoneNumber = viewModel.PhoneNumber;
                            employeeToUpdate.Email = viewModel.Email;
                            employeeToUpdate.HireDate = viewModel.HireDate.Value;
                            employeeToUpdate.RoleID = viewModel.SelectedRoleId;
                        }

                        // Обновление учетной записи
                        var account = db.Accounts.FirstOrDefault(a => a.EmployeeID == _employee.EmployeeID);
                        if (account != null)
                        {
                            // Проверка и обновление логина
                            if (!string.IsNullOrWhiteSpace(viewModel.Login) && account.Login != viewModel.Login)
                            {
                                if (db.Accounts.Any(a => a.Login == viewModel.Login && a.EmployeeID != _employee.EmployeeID))
                                {
                                    MessageBox.Show("Этот логин уже занят другим пользователем", "Ошибка",
                                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                                account.Login = viewModel.Login;
                            }

                            // Обновление пароля если он изменен
                            if (passwordChanged)
                            {
                                account.Password = GetHash(PasswordBox.Password);
                            }
                        }
                    }
                    else
                    {
                        // Добавление нового сотрудника
                        if (string.IsNullOrWhiteSpace(viewModel.Login) || PasswordBox.Password.Length < 6)
                        {
                            MessageBox.Show("Для нового сотрудника необходимо указать логин и пароль (минимум 6 символов)",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (db.Accounts.Any(a => a.Login == viewModel.Login))
                        {
                            MessageBox.Show("Этот логин уже занят", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _employee.LastName = viewModel.LastName;
                        _employee.FirstName = viewModel.FirstName;
                        _employee.Patronymic = viewModel.Patronymic;
                        _employee.PhoneNumber = viewModel.PhoneNumber;
                        _employee.Email = viewModel.Email;
                        _employee.HireDate = viewModel.HireDate.Value;
                        _employee.RoleID = viewModel.SelectedRoleId;

                        db.Employees.Add(_employee);
                        db.SaveChanges();

                        // Создание учетной записи
                        var account = new Accounts
                        {
                            Login = viewModel.Login,
                            Password = GetHash(PasswordBox.Password),
                            EmployeeID = _employee.EmployeeID
                        };
                        db.Accounts.Add(account);
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные сотрудника успешно сохранены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    SaveCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke();
        }
    }

    public class AddEditEmployeeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string WindowTitle { get; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? HireDate { get; set; }
        public string Login { get; set; }
        public System.Collections.Generic.List<Roles> Roles { get; }
        public int SelectedRoleId { get; set; }

        public AddEditEmployeeViewModel(Employees employee)
        {
            using (var db = new Entities())
            {
                Roles = db.Roles.ToList();
            }

            if (employee != null)
            {
                WindowTitle = "Редактирование сотрудника";
                LastName = employee.LastName;
                FirstName = employee.FirstName;
                Patronymic = employee.Patronymic;
                PhoneNumber = employee.PhoneNumber;
                Email = employee.Email;
                HireDate = employee.HireDate;
                SelectedRoleId = employee.RoleID ?? 0;

                using (var db = new Entities())
                {
                    var account = db.Accounts.FirstOrDefault(a => a.EmployeeID == employee.EmployeeID);
                    if (account != null)
                    {
                        Login = account.Login;
                    }
                }
            }
            else
            {
                WindowTitle = "Добавление сотрудника";
                HireDate = DateTime.Now;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}