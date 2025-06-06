﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Retrolink.Manager_pg;

namespace Retrolink
{
    public partial class ManagerWindow : Window
    {
        private Employees _currentEmployee;

        public ManagerWindow(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            Title = $"Менеджер - {employee.FirstName} {employee.LastName}";
            ShowCustomersList();
        }

        private void ShowCustomersList_Click(object sender, RoutedEventArgs e) => ShowCustomersList();
        private void ShowAddCustomer_Click(object sender, RoutedEventArgs e) => ShowAddEditCustomer(null);

        private void ShowCustomersList()
        {
            var page = new Manager_pg.CustomersListPage();
            page.EditCustomerRequested += (customer) => ShowAddEditCustomer(customer);
            page.DeleteCustomerRequested += (customer) => DeleteCustomer(customer);
            page.ViewCustomerDetailsRequested += (customer) => ShowCustomerDetails(customer);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditCustomer(Customers customer)
        {
            var page = new Manager_pg.AddEditCustomerPage(customer);
            page.SaveCompleted += () => ShowCustomersList();
            page.CancelClicked += () => ShowCustomersList();
            MainFrame.Navigate(page);
        }

        private void ShowCustomerDetails(Customers customer)
        {
            var page = new Manager_pg.CustomerDetailsPage(customer);
            page.BackRequested += () => ShowCustomersList();
            page.EditContractRequested += (contract) => ShowAddEditContract(contract, customer);
            page.AddContractRequested += () => ShowAddEditContract(null, customer);
            page.DeleteContractRequested += (contract) => DeleteContract(contract);
            page.AddProvidedServiceRequested += (service) => ShowAddEditProvidedService(service, customer);
            page.EditProvidedServiceRequested += (service) => ShowAddEditProvidedService(service, customer);
            page.DeleteProvidedServiceRequested += (service) => DeleteProvidedService(service, customer);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditContract(Contracts contract, Customers customer)
        {
            var page = new Manager_pg.AddEditContractPage(contract, customer);
            page.SaveCompleted += () => ShowCustomerDetails(customer);
            page.CancelClicked += () => ShowCustomerDetails(customer);
            MainFrame.Navigate(page);
        }

        private void ShowAddEditProvidedService(ProvidedServices service, Customers customer)
        {
            var page = new Manager_pg.AddEditProvidedServicePage(service, customer);
            page.SaveCompleted += () => ShowCustomerDetails(customer);
            page.CancelClicked += () => ShowCustomerDetails(customer);
            MainFrame.Navigate(page);
        }

        private void ShowReports_Click(object sender, RoutedEventArgs e)
        {
            var page = new Manager_pg.ReportsPage();
            MainFrame.Navigate(page);
        }

        private void DeleteCustomer(Customers customer)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить клиента {customer.LastName} {customer.FirstName} и все связанные данные?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var customerToDelete = db.Customers.FirstOrDefault(c => c.CustomerID == customer.CustomerID);
                        if (customerToDelete != null)
                        {
                            var contracts = db.Contracts.Where(c => c.CustomerID == customerToDelete.CustomerID).ToList();
                            foreach (var contract in contracts)
                            {
                                var installedEquipment = db.InstalledEquipment.Where(ie => ie.ContractID == contract.ContractID).ToList();
                                db.InstalledEquipment.RemoveRange(installedEquipment);

                                var providedServices = db.ProvidedServices.Where(ps => ps.ContractID == contract.ContractID).ToList();
                                db.ProvidedServices.RemoveRange(providedServices);

                                db.Contracts.Remove(contract);
                            }

                            var payments = db.Payments.Where(p => p.CustomerID == customerToDelete.CustomerID).ToList();
                            db.Payments.RemoveRange(payments);

                            var supportTickets = db.SupportTickets.Where(st => st.CustomerID == customerToDelete.CustomerID).ToList();
                            db.SupportTickets.RemoveRange(supportTickets);

                            db.Customers.Remove(customerToDelete);
                            db.SaveChanges();
                            ShowCustomersList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteContract(Contracts contract)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить контракт от {contract.ContractDate.ToShortDateString()}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Entities())
                {
                    var contractToDelete = db.Contracts.FirstOrDefault(c => c.ContractID == contract.ContractID);
                    if (contractToDelete != null)
                    {
                        var installedEquipment = db.InstalledEquipment.Where(ie => ie.ContractID == contractToDelete.ContractID).ToList();
                        db.InstalledEquipment.RemoveRange(installedEquipment);

                        var providedServices = db.ProvidedServices.Where(ps => ps.ContractID == contractToDelete.ContractID).ToList();
                        db.ProvidedServices.RemoveRange(providedServices);

                        db.Contracts.Remove(contractToDelete);
                        db.SaveChanges();

                        if (MainFrame.Content is CustomerDetailsPage detailsPage)
                        {
                            ShowCustomerDetails(detailsPage.CurrentCustomer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении контракта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteProvidedService(ProvidedServices service, Customers customer)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту предоставленную услугу?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Entities())
                {
                    var serviceToDelete = db.ProvidedServices.FirstOrDefault(s => s.ProvidedServiceID == service.ProvidedServiceID);
                    if (serviceToDelete != null)
                    {
                        db.ProvidedServices.Remove(serviceToDelete);
                        db.SaveChanges();

                        if (MainFrame.Content is CustomerDetailsPage detailsPage)
                        {
                            ShowCustomerDetails(detailsPage.CurrentCustomer);
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