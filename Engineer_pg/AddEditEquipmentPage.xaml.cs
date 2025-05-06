using System;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Engineer_pg
{
    public partial class AddEditEquipmentPage : Page
    {
        private Equipment _currentEquipment;
        public event Action SaveCompleted;
        public event Action CancelClicked;

        public new string Title => _currentEquipment == null ? "Добавить оборудование" : "Редактировать оборудование";

        public AddEditEquipmentPage(Equipment equipment)
        {
            InitializeComponent();
            _currentEquipment = equipment ?? new Equipment();
            DataContext = this;
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            if (_currentEquipment != null)
            {
                EquipmentNameTextBox.Text = _currentEquipment.EquipmentName;
                ModelTextBox.Text = _currentEquipment.Model;
                SerialNumberTextBox.Text = _currentEquipment.SerialNumber;
                DescriptionTextBox.Text = _currentEquipment.Description;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EquipmentNameTextBox.Text))
            {
                MessageBox.Show("Введите название оборудования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_currentEquipment.EquipmentID == 0) 
                    {
                        var newEquipment = new Equipment
                        {
                            EquipmentName = EquipmentNameTextBox.Text,
                            Model = ModelTextBox.Text,
                            SerialNumber = SerialNumberTextBox.Text,
                            Description = DescriptionTextBox.Text
                        };
                        db.Equipment.Add(newEquipment);
                    }
                    else 
                    {
                        var equipmentToUpdate = db.Equipment.Find(_currentEquipment.EquipmentID);
                        if (equipmentToUpdate != null)
                        {
                            equipmentToUpdate.EquipmentName = EquipmentNameTextBox.Text;
                            equipmentToUpdate.Model = ModelTextBox.Text;
                            equipmentToUpdate.SerialNumber = SerialNumberTextBox.Text;
                            equipmentToUpdate.Description = DescriptionTextBox.Text;
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