using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Admin_pg
{
    public partial class RolesListPage : Page
    {
        public event Action<Roles> EditRoleRequested;
        public event Action<Roles> DeleteRoleRequested;

        public RolesListPage()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                using (var db = new Entities())
                {
                    var roles = db.Roles.ToList();
                    RolesDataGrid.ItemsSource = roles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ролей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Roles GetSelectedRole()
        {
            return RolesDataGrid.SelectedItem as Roles;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRole = GetSelectedRole();
            if (selectedRole == null)
            {
                MessageBox.Show("Выберите роль для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditRoleRequested?.Invoke(selectedRole);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRole = GetSelectedRole();
            if (selectedRole == null)
            {
                MessageBox.Show("Выберите роль для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeleteRoleRequested?.Invoke(selectedRole);
        }
    }
}