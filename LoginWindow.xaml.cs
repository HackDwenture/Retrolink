using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Retrolink
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            Auth(LoginTextBox.Text, PasswordTextBox.Password);
        }

        public bool Auth(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string hashedPassword = GetHash(password);

            using (var db = new Entities())
            {
                var account = db.Accounts
                    .AsNoTracking()
                    .FirstOrDefault(a => a.Login == login && a.Password == hashedPassword);

                if (account == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var employee = db.Employees.FirstOrDefault(emp => emp.EmployeeID == account.EmployeeID);

                if (employee == null)
                {
                    MessageBox.Show("Ошибка: Не найден сотрудник, связанный с этой учетной записью.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                bool authSuccess = true;
                switch (employee.RoleID)
                {
                    case 1:
                        AdminWindow adminWindow = new AdminWindow(employee);
                        adminWindow.Show();
                        break;
                    case 2:
                        ManagerWindow managerWindow = new ManagerWindow(employee);
                        managerWindow.Show();
                        break;
                    case 3:
                        EngineerWindow engineerWindow = new EngineerWindow(employee);
                        engineerWindow.Show();
                        break;
                    case 4:
                        SupportWindow supportWindow = new SupportWindow(employee);
                        supportWindow.Show();
                        break;
                    default:
                        MessageBox.Show("Неизвестная должность сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        authSuccess = false;
                        break;
                }

                if (authSuccess)
                {
                    this.Close();
                }

                return true;
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
    }
}