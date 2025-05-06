using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Retrolink.Admin_pg
{
    public partial class TariffsListPage : Page
    {
        public event Action<Tariffs> EditTariffRequested;
        public event Action<Tariffs> DeleteTariffRequested;
        public TariffsListPage()
        {
            InitializeComponent();
            LoadTariffs();
        }

        private void LoadTariffs()
        {
            using (var db = new Entities())
            {
                TariffsListView.ItemsSource = db.Tariffs
                    .OrderBy(t => t.TariffName)
                    .ToList();
            }
        }

        private Tariffs GetSelectedTariff()
        {
            return TariffsListView.SelectedItem as Tariffs;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTariff = GetSelectedTariff();
            if (selectedTariff != null)
            {
                EditTariffRequested?.Invoke(selectedTariff);
            }
            else
            {
                MessageBox.Show("Выберите тариф для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTariff = GetSelectedTariff();
            if (selectedTariff != null)
            {
                DeleteTariffRequested?.Invoke(selectedTariff);
            }
            else
            {
                MessageBox.Show("Выберите тариф для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TariffsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedTariff = GetSelectedTariff();
            if (selectedTariff != null)
            {
                EditTariffRequested?.Invoke(selectedTariff);
            }
        }
    }
}