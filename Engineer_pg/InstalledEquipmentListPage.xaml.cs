using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Retrolink.Engineer_pg
{
    public partial class InstalledEquipmentListPage : Page
    {
        private List<InstalledEquipment> _allInstalledEquipment;
        private ICollectionView _installedEquipmentView;

        public event Action<InstalledEquipment> EditInstalledEquipmentRequested;
        public event Action<InstalledEquipment> DeleteInstalledEquipmentRequested;

        public InstalledEquipmentListPage()
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
                   
                    _allInstalledEquipment = db.InstalledEquipment
                        .Include("Equipment")  
                        .ToList();
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
            _installedEquipmentView = CollectionViewSource.GetDefaultView(_allInstalledEquipment);
            _installedEquipmentView.Filter = InstalledEquipmentFilter;

            InstalledEquipmentListView.ItemsSource = _installedEquipmentView;
        }

        private bool InstalledEquipmentFilter(object item)
        {
            var installed = item as InstalledEquipment;
            if (installed == null) return false;

            string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText)) return true;

            return (installed.Equipment?.EquipmentName?.ToLower().Contains(searchText) ?? false) ||
                   (installed.ContractID.ToString().Contains(searchText)) ||
                   (installed.InstallationDate.ToString("dd.MM.yyyy").Contains(searchText));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _installedEquipmentView?.Refresh();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _installedEquipmentView?.Refresh();
        }

        private InstalledEquipment GetSelectedInstalledEquipment()
        {
            return InstalledEquipmentListView.SelectedItem as InstalledEquipment;
        }

        private void EditInstalledEquipment_Click(object sender, RoutedEventArgs e)
        {
            var installed = GetSelectedInstalledEquipment();
            if (installed != null)
            {
                EditInstalledEquipmentRequested?.Invoke(installed);
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteInstalledEquipment_Click(object sender, RoutedEventArgs e)
        {
            var installed = GetSelectedInstalledEquipment();
            if (installed != null)
            {
                DeleteInstalledEquipmentRequested?.Invoke(installed);
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InstalledEquipmentListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var installed = GetSelectedInstalledEquipment();
            if (installed != null)
            {
                EditInstalledEquipmentRequested?.Invoke(installed);
            }
        }
    }
}