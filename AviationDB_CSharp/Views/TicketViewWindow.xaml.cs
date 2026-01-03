using AviationDB_CSharp.Models;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class TicketViewWindow : Window
    {
        private readonly Ticket _ticket;

        public TicketViewWindow(Ticket ticket)
        {
            InitializeComponent();
            _ticket = ticket;
            DataContext = ticket;
            LoadContactData();
        }

        private void LoadContactData()
        {
            if (_ticket != null)
            {
                ContactDataTextBlock.Text = _ticket.GetFormattedContactData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TicketEditWindow(_ticket);
            if (editWindow.ShowDialog() == true)
            {
                DataContext = null;
                DataContext = _ticket;
                LoadContactData();
                DialogResult = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}