using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Engineer_pg
{
    public partial class AddEditInstalledEquipmentPage : Page
    {
        private InstalledEquipment _currentInstalled;
        public event Action SaveCompleted;
        public event Action CancelClicked;

        public new string Title => _currentInstalled == null ? "Добавить установку оборудования" : "Редактировать установку оборудования";

        public AddEditInstalledEquipmentPage(InstalledEquipment installed)
        {
            InitializeComponent();
            _currentInstalled = installed ?? new InstalledEquipment();
            DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    var equipmentList = db.Equipment.ToList();
                    EquipmentComboBox.ItemsSource = equipmentList;
                    EquipmentComboBox.DisplayMemberPath = "EquipmentName";
                    EquipmentComboBox.SelectedValuePath = "EquipmentID";

                    var contractList = db.Contracts.ToList();
                    ContractComboBox.ItemsSource = contractList;
                    ContractComboBox.DisplayMemberPath = "ContractID";
                    ContractComboBox.SelectedValuePath = "ContractID";

                    if (_currentInstalled.InstalledEquipmentID != 0)
                    {
                        EquipmentComboBox.SelectedValue = _currentInstalled.EquipmentID;
                        ContractComboBox.SelectedValue = _currentInstalled.ContractID;
                        InstallationDatePicker.SelectedDate = _currentInstalled.InstallationDate;
                    }
                    else
                    {
                        InstallationDatePicker.SelectedDate = DateTime.Today;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите оборудование", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ContractComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите контракт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (InstallationDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату установки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    var selectedEquipment = (Equipment)EquipmentComboBox.SelectedItem;
                    var selectedContract = (Contracts)ContractComboBox.SelectedItem;

                    if (_currentInstalled.InstalledEquipmentID == 0) 
                    {
                        var newInstalled = new InstalledEquipment
                        {
                            EquipmentID = selectedEquipment.EquipmentID,
                            ContractID = selectedContract.ContractID,
                            InstallationDate = InstallationDatePicker.SelectedDate.Value
                        };
                        db.InstalledEquipment.Add(newInstalled);
                    }
                    else 
                    {
                        var installedToUpdate = db.InstalledEquipment.Find(_currentInstalled.InstalledEquipmentID);
                        if (installedToUpdate != null)
                        {
                            installedToUpdate.EquipmentID = selectedEquipment.EquipmentID;
                            installedToUpdate.ContractID = selectedContract.ContractID;
                            installedToUpdate.InstallationDate = InstallationDatePicker.SelectedDate.Value;
                        }
                    }

                    db.SaveChanges();
                    SaveCompleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke();
        }
    }
}