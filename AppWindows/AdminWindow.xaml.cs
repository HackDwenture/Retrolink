using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Retrolink
{
    public partial class AdminWindow : Window
    {
        private Employees _currentEmployee;

        public AdminWindow(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            Title = $"Администратор - {employee.FirstName} {employee.LastName}";
            ShowEmployeesList();
        }

        private void ShowEmployeesList_Click(object sender, RoutedEventArgs e) => ShowEmployeesList();
        private void ShowAddEmployee_Click(object sender, RoutedEventArgs e) => ShowAddEditEmployee(null);
        private void ShowRolesList_Click(object sender, RoutedEventArgs e) => ShowRolesList();
        private void ShowAddRole_Click(object sender, RoutedEventArgs e) => ShowAddEditRole(null);
        private void ShowServicesList_Click(object sender, RoutedEventArgs e) => ShowServicesList();
        private void ShowAddService_Click(object sender, RoutedEventArgs e) => ShowAddEditService(null);
        private void ShowTariffsList_Click(object sender, RoutedEventArgs e) => ShowTariffsList();
        private void ShowAddTariff_Click(object sender, RoutedEventArgs e) => ShowAddEditTariff(null);

        private void ShowEmployeesList()
        {
            var page = new Admin_pg.EmployeesListPage();
            page.EditEmployeeRequested += (employee) => ShowAddEditEmployee(employee);
            page.DeleteEmployeeRequested += (employee) => DeleteEmployee(employee);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditEmployee(Employees employee)
        {
            var page = new Admin_pg.AddEditEmployeePage(employee);
            page.SaveCompleted += () => ShowEmployeesList();
            page.CancelClicked += () => ShowEmployeesList();
            MainFrame.Navigate(page);
        }

        private void ShowRolesList()
        {
            var page = new Admin_pg.RolesListPage();
            page.EditRoleRequested += (role) => ShowAddEditRole(role);
            page.DeleteRoleRequested += (role) => DeleteRole(role);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditRole(Roles role)
        {
            var page = new Admin_pg.AddEditRolePage(role);
            page.SaveCompleted += () => ShowRolesList();
            page.CancelClicked += () => ShowRolesList();
            MainFrame.Navigate(page);
        }

        private void ShowTariffsList()
        {
            var page = new Admin_pg.TariffsListPage();
            page.EditTariffRequested += (tariff) => ShowAddEditTariff(tariff);
            page.DeleteTariffRequested += (tariff) => DeleteTariff(tariff);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditTariff(Tariffs tariff)
        {
            var page = new Admin_pg.AddEditTariffPage(tariff);
            page.SaveCompleted += () => ShowTariffsList();
            page.CancelClicked += () => ShowTariffsList();
            MainFrame.Navigate(page);
        }

        private void DeleteEmployee(Employees employee)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить сотрудника {employee.LastName} {employee.FirstName} и все связанные данные?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var employeeToDelete = db.Employees
                            .Include(e => e.Accounts)
                            .Include(e => e.ProvidedServices)
                            .Include(e => e.SupportTickets)
                            .FirstOrDefault(emp => emp.EmployeeID == employee.EmployeeID);

                        if (employeeToDelete == null)
                        {
                            MessageBox.Show("Сотрудник не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (employeeToDelete.Accounts != null && employeeToDelete.Accounts.Any())
                        {
                            db.Accounts.RemoveRange(employeeToDelete.Accounts);
                        }

                        var providedServices = db.ProvidedServices
                            .Where(ps => ps.EmployeeID == employeeToDelete.EmployeeID)
                            .ToList();

                        foreach (var service in providedServices)
                        {
                            db.ProvidedServices.Remove(service);
                        }

                        var supportTickets = db.SupportTickets
                            .Where(st => st.EmployeeID == employeeToDelete.EmployeeID)
                            .ToList();

                        foreach (var ticket in supportTickets)
                        {
                            db.SupportTickets.Remove(ticket);
                        }

                        db.Employees.Remove(employeeToDelete);
                        db.SaveChanges();

                        MessageBox.Show("Сотрудник и все связанные данные успешно удалены", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        ShowEmployeesList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении сотрудника: {ex.Message}\n\n" +
                        "Убедитесь, что нет других связанных записей, которые препятствуют удалению.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteTariff(Tariffs tariff)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить тариф {tariff.TariffName}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var contractsWithTariff = db.Contracts.Any(c => c.TariffID == tariff.TariffID);
                        if (contractsWithTariff)
                        {
                            MessageBox.Show("Невозможно удалить тариф, так как есть активные контракты с этим тарифом",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var tariffToDelete = db.Tariffs.FirstOrDefault(t => t.TariffID == tariff.TariffID);
                        if (tariffToDelete != null)
                        {
                            db.Tariffs.Remove(tariffToDelete);
                            db.SaveChanges();
                            ShowTariffsList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteRole(Roles role)
        {
            try
            {
                using (var db = new Entities())
                {
                    var employeesWithRole = db.Employees.Any(emp => emp.RoleID == role.RoleID);
                    if (employeesWithRole)
                    {
                        MessageBox.Show("Невозможно удалить роль, так как есть сотрудники с этой ролью", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (MessageBox.Show($"Вы уверены, что хотите удалить роль {role.RoleName}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var roleToDelete = db.Roles.FirstOrDefault(r => r.RoleID == role.RoleID);
                        if (roleToDelete != null)
                        {
                            db.Roles.Remove(roleToDelete);
                            db.SaveChanges();
                            ShowRolesList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowServicesList()
        {
            var page = new Admin_pg.ServicesListPage();
            page.EditServiceRequested += (service) => ShowAddEditService(service);
            page.DeleteServiceRequested += (service) => DeleteService(service);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditService(Services service)
        {
            var page = new Admin_pg.AddEditServicePage(service);
            page.SaveCompleted += () => ShowServicesList();
            page.CancelClicked += () => ShowServicesList();
            MainFrame.Navigate(page);
        }

        private void DeleteService(Services service)
        {
            try
            {
                using (var db = new Entities())
                {
                    var providedServices = db.ProvidedServices.Any(ps => ps.ServiceID == service.ServiceID);
                    if (providedServices)
                    {
                        MessageBox.Show("Невозможно удалить услугу, так как она уже была предоставлена клиентам", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (MessageBox.Show($"Вы уверены, что хотите удалить услугу {service.ServiceName}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var serviceToDelete = db.Services.FirstOrDefault(s => s.ServiceID == service.ServiceID);
                        if (serviceToDelete != null)
                        {
                            db.Services.Remove(serviceToDelete);
                            db.SaveChanges();
                            ShowServicesList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}