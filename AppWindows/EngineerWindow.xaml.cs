using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink
{
    public partial class EngineerWindow : Window
    {
        private Employees _currentEmployee;

        public EngineerWindow(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            Title = $"Инженер - {employee.FirstName} {employee.LastName}";
            ShowEquipmentList();
        }

        private void ShowEquipmentList_Click(object sender, RoutedEventArgs e) => ShowEquipmentList();
        private void ShowAddEquipment_Click(object sender, RoutedEventArgs e) => ShowAddEditEquipment(null);
        private void ShowInstalledEquipmentList_Click(object sender, RoutedEventArgs e) => ShowInstalledEquipmentList();
        private void ShowAddInstalledEquipment_Click(object sender, RoutedEventArgs e) => ShowAddEditInstalledEquipment(null);
        private void ShowMyServices_Click(object sender, RoutedEventArgs e) => ShowMyServices();

        private void ShowEquipmentList()
        {
            var page = new Engineer_pg.EquipmentListPage();
            page.EditEquipmentRequested += (equipment) => ShowAddEditEquipment(equipment);
            page.DeleteEquipmentRequested += (equipment) => DeleteEquipment(equipment);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditEquipment(Equipment equipment)
        {
            var page = new Engineer_pg.AddEditEquipmentPage(equipment);
            page.SaveCompleted += () => ShowEquipmentList();
            page.CancelClicked += () => ShowEquipmentList();
            MainFrame.Navigate(page);
        }

        private void ShowInstalledEquipmentList()
        {
            var page = new Engineer_pg.InstalledEquipmentListPage();
            page.EditInstalledEquipmentRequested += (installed) => ShowAddEditInstalledEquipment(installed);
            page.DeleteInstalledEquipmentRequested += (installed) => DeleteInstalledEquipment(installed);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditInstalledEquipment(InstalledEquipment installed)
        {
            var page = new Engineer_pg.AddEditInstalledEquipmentPage(installed);
            page.SaveCompleted += () => ShowInstalledEquipmentList();
            page.CancelClicked += () => ShowInstalledEquipmentList();
            MainFrame.Navigate(page);
        }

        private void ShowMyServices()
        {
            var page = new Engineer_pg.MyServicesPage(_currentEmployee.EmployeeID);
            page.ViewDetailsRequested += (service) => ShowServiceDetails(service);
            MainFrame.Navigate(page);
        }

        private void ShowServiceDetails(ProvidedServices service)
        {
            var page = new Engineer_pg.ServiceDetailsPage(service);
            page.BackRequested += () => ShowMyServices();
            MainFrame.Navigate(page);
        }

        private void DeleteEquipment(Equipment equipment)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить оборудование {equipment.EquipmentName}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var installedEquipment = db.InstalledEquipment.Any(ie => ie.EquipmentID == equipment.EquipmentID);
                        if (installedEquipment)
                        {
                            MessageBox.Show("Невозможно удалить оборудование, так как оно установлено у клиентов",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var equipmentToDelete = db.Equipment.FirstOrDefault(e => e.EquipmentID == equipment.EquipmentID);
                        if (equipmentToDelete != null)
                        {
                            db.Equipment.Remove(equipmentToDelete);
                            db.SaveChanges();
                            ShowEquipmentList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteInstalledEquipment(InstalledEquipment installed)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить запись об установленном оборудовании?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var installedToDelete = db.InstalledEquipment.FirstOrDefault(ie => ie.InstalledEquipmentID == installed.InstalledEquipmentID);
                        if (installedToDelete != null)
                        {
                            db.InstalledEquipment.Remove(installedToDelete);
                            db.SaveChanges();
                            ShowInstalledEquipmentList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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