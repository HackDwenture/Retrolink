using System.Windows.Controls;


namespace Retrolink.Manager_pg
{
   
    public partial class ContractsView : Page
    {
        private Contracts _contract;
        public ContractsView(Contracts contract)
        {
            InitializeComponent();
            _contract = contract;
            DisplayContractInfo();
        }

        private void DisplayContractInfo()
        {
            ContractLabel.Content = _contract.ContractID;
            CustomerLabel.Content = _contract.CustomerID;
            TariffLabel.Content = _contract.TariffID;
            ContractDateLabel.Content = _contract.ContractDate;
            InstallationDateLabel.Content = _contract.InstallationDate;
        }
    }
}
