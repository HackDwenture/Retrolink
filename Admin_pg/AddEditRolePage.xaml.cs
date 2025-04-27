using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Retrolink.Admin_pg
{
    public partial class AddEditRolePage : Page
    {
        private Roles _role;
        private bool _isEditMode;

        public event Action SaveCompleted;
        public event Action CancelClicked;

        public AddEditRolePage(Roles role)
        {
            InitializeComponent();

            if (role != null)
            {
                _role = role;
                _isEditMode = true;
                DataContext = new AddEditRoleViewModel(role);
            }
            else
            {
                _role = new Roles();
                _isEditMode = false;
                DataContext = new AddEditRoleViewModel(null);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddEditRoleViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(viewModel.RoleName))
            {
                MessageBox.Show("Введите название роли", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    if (_isEditMode)
                    {
                        var roleToUpdate = db.Roles.FirstOrDefault(r => r.RoleID == _role.RoleID);
                        if (roleToUpdate != null)
                        {
                            roleToUpdate.RoleName = viewModel.RoleName;
                        }
                    }
                    else
                    {
                        _role.RoleName = viewModel.RoleName;
                        db.Roles.Add(_role);
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

    public class AddEditRoleViewModel
    {
        public string WindowTitle { get; }
        public string RoleName { get; set; }

        public AddEditRoleViewModel(Roles role)
        {
            if (role != null)
            {
                WindowTitle = "Редактирование роли";
                RoleName = role.RoleName;
            }
            else
            {
                WindowTitle = "Добавление роли";
            }
        }
    }
}