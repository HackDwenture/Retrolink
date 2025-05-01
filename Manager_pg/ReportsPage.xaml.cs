using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ClosedXML.Excel;
using System.Windows.Media;

namespace Retrolink.Manager_pg
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            GenerateRegistrationsReport();
            GenerateTicketsReport();
            GeneratePaymentsReport();
        }

        private void GenerateRegistrationsReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateRegistrationsReport();
        }

        private void GenerateRegistrationsReport()
        {
            using (var db = new Entities())
            {
                var query = db.Customers.AsQueryable();

                if (RegistrationsStartDate.SelectedDate != null && RegistrationsEndDate.SelectedDate != null)
                {
                    query = query.Where(c => c.RegistrationDate >= RegistrationsStartDate.SelectedDate &&
                                            c.RegistrationDate <= RegistrationsEndDate.SelectedDate);
                }

                RegistrationsDataGrid.ItemsSource = query.OrderByDescending(c => c.RegistrationDate).ToList();
            }
        }

        private void GenerateTicketsReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateTicketsReport();
        }

        private void GenerateTicketsReport()
        {
            using (var db = new Entities())
            {
                var query = db.SupportTickets.Include(t => t.Customers).Include(t => t.Employees).AsQueryable();

                if (TicketsStartDate.SelectedDate != null && TicketsEndDate.SelectedDate != null)
                {
                    query = query.Where(t => t.TicketDate >= TicketsStartDate.SelectedDate &&
                                           t.TicketDate <= TicketsEndDate.SelectedDate);
                }

                if (TicketStatusComboBox.SelectedIndex > 0)
                {
                    var status = ((ComboBoxItem)TicketStatusComboBox.SelectedItem).Content.ToString();
                    query = query.Where(t => t.Status == status);
                }

                TicketsDataGrid.ItemsSource = query.OrderByDescending(t => t.TicketDate).ToList();
            }
        }

        private void GeneratePaymentsReport_Click(object sender, RoutedEventArgs e)
        {
            GeneratePaymentsReport();
        }

        private void GeneratePaymentsReport()
        {
            using (var db = new Entities())
            {
                var query = db.Payments.Include(p => p.Customers).AsQueryable();

                if (PaymentsStartDate.SelectedDate != null && PaymentsEndDate.SelectedDate != null)
                {
                    query = query.Where(p => p.PaymentDate >= PaymentsStartDate.SelectedDate &&
                                           p.PaymentDate <= PaymentsEndDate.SelectedDate);
                }

                if (PaymentMethodComboBox.SelectedIndex > 0)
                {
                    var method = ((ComboBoxItem)PaymentMethodComboBox.SelectedItem).Content.ToString();
                    query = query.Where(p => p.PaymentMethod == method);
                }

                PaymentsDataGrid.ItemsSource = query.OrderByDescending(p => p.PaymentDate).ToList();
            }
        }

        private void ExportRegistrationsToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(worksheet =>
            {
                // Заголовки
                worksheet.Cell(1, 1).Value = "Дата регистрации";
                worksheet.Cell(1, 2).Value = "Фамилия";
                worksheet.Cell(1, 3).Value = "Имя";
                worksheet.Cell(1, 4).Value = "Телефон";
                worksheet.Cell(1, 5).Value = "Email";
                worksheet.Cell(1, 6).Value = "Адрес";

                // Данные
                int row = 2;
                foreach (Customers customer in RegistrationsDataGrid.Items)
                {
                    worksheet.Cell(row, 1).Value = customer.RegistrationDate;
                    worksheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";
                    worksheet.Cell(row, 2).Value = customer.LastName;
                    worksheet.Cell(row, 3).Value = customer.FirstName;
                    worksheet.Cell(row, 4).Value = customer.PhoneNumber;
                    worksheet.Cell(row, 5).Value = customer.Email;
                    worksheet.Cell(row, 6).Value = customer.Address;
                    row++;
                }

                // Стиль для заголовков
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            }, "Регистрации_клиентов");
        }

        private void ExportTicketsToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(worksheet =>
            {
                worksheet.Cell(1, 1).Value = "Дата обращения";
                worksheet.Cell(1, 2).Value = "Клиент";
                worksheet.Cell(1, 3).Value = "Статус";
                worksheet.Cell(1, 4).Value = "Описание";
                worksheet.Cell(1, 5).Value = "Сотрудник";

                int row = 2;
                foreach (SupportTickets ticket in TicketsDataGrid.Items)
                {
                    worksheet.Cell(row, 1).Value = ticket.TicketDate;
                    worksheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";
                    worksheet.Cell(row, 2).Value = $"{ticket.Customers.LastName} {ticket.Customers.FirstName}";
                    worksheet.Cell(row, 3).Value = ticket.Status;
                    worksheet.Cell(row, 4).Value = ticket.Description;
                    worksheet.Cell(row, 5).Value = ticket.Employees != null ?
                        $"{ticket.Employees.LastName} {ticket.Employees.FirstName}" : "";
                    row++;
                }

                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            }, "Обращения_в_техподдержку");
        }

        private void ExportPaymentsToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(worksheet =>
            {
                worksheet.Cell(1, 1).Value = "Дата платежа";
                worksheet.Cell(1, 2).Value = "Клиент";
                worksheet.Cell(1, 3).Value = "Сумма";
                worksheet.Cell(1, 4).Value = "Метод оплаты";
                worksheet.Cell(1, 5).Value = "ID платежа";

                int row = 2;
                foreach (Payments payment in PaymentsDataGrid.Items)
                {
                    worksheet.Cell(row, 1).Value = payment.PaymentDate;
                    worksheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";
                    worksheet.Cell(row, 2).Value = $"{payment.Customers.LastName} {payment.Customers.FirstName}";
                    worksheet.Cell(row, 3).Value = payment.Amount;
                    worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 ₽";
                    worksheet.Cell(row, 4).Value = payment.PaymentMethod;
                    worksheet.Cell(row, 5).Value = payment.PaymentID;
                    row++;
                }

                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            }, "Платежи_клиентов");
        }

        private void ExportToExcel(Action<IXLWorksheet> fillData, string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = defaultFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Отчет");

                        // Заполняем данные
                        fillData(worksheet);

                        // Автонастройка ширины столбцов
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Отчет успешно экспортирован", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}