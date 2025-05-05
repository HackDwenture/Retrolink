using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Engineer_pg
{
    public partial class EquipmentListPage : Page
    {
        private List<Equipment> _allEquipment;
        private ICollectionView _equipmentView;

        public event Action<Equipment> EditEquipmentRequested;
        public event Action<Equipment> DeleteEquipmentRequested;

        public EquipmentListPage()
        {
            InitializeComponent();
            LoadData();
            InitializeView();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    _allEquipment = db.Equipment.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeView()
        {
            _equipmentView = CollectionViewSource.GetDefaultView(_allEquipment);
            _equipmentView.Filter = EquipmentFilter;
            EquipmentListView.ItemsSource = _equipmentView;
        }

        private bool EquipmentFilter(object item)
        {
            var equipment = item as Equipment;
            if (equipment == null) return false;

            string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText)) return true;

            return (equipment.EquipmentName?.ToLower().Contains(searchText) ?? false) ||
                   (equipment.Model?.ToLower().Contains(searchText) ?? false) ||
                   (equipment.SerialNumber?.ToLower().Contains(searchText) ?? false) ||
                   (equipment.Description?.ToLower().Contains(searchText) ?? false);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _equipmentView?.Refresh();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _equipmentView?.Refresh();
        }

        private Equipment GetSelectedEquipment()
        {
            return EquipmentListView.SelectedItem as Equipment;
        }

        private void EditEquipment_Click(object sender, RoutedEventArgs e)
        {
            var equipment = GetSelectedEquipment();
            if (equipment != null)
            {
                EditEquipmentRequested?.Invoke(equipment);
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            var equipment = GetSelectedEquipment();
            if (equipment != null)
            {
                DeleteEquipmentRequested?.Invoke(equipment);
            }
            else
            {
                MessageBox.Show("Выберите оборудование для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EquipmentListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var equipment = GetSelectedEquipment();
            if (equipment != null)
            {
                EditEquipmentRequested?.Invoke(equipment);
            }
        }
    }
}