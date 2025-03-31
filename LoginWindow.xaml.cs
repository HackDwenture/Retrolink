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
            if (string.IsNullOrEmpty(LoginTextBox.Text) || string.IsNullOrEmpty(PasswordTextBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            string hashedPassword = GetHash(PasswordTextBox.Password);

            using (var db = new Entities())
            {
                var account = db.Accounts
                                .AsNoTracking()
                                .FirstOrDefault(a => a.Login == LoginTextBox.Text && a.Password == hashedPassword);

                if (account == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    return;
                }

                var employee = db.Employees.FirstOrDefault(emp => emp.EmployeeID == account.EmployeeID);

                if (employee == null)
                {
                    MessageBox.Show("Ошибка: Не найден сотрудник, связанный с этой учетной записью.");
                    return;
                }

                
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
                        MessageBox.Show("Неизвестная должность сотрудника.");
                        return;
                }

                this.Close(); 
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
