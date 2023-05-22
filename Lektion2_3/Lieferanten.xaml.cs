using CSHlibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lektion2_3
{
    /// <summary>
    /// Interaction logic for Lieferanten.xaml
    /// </summary>
    public partial class Lieferanten : Window
    {
        private readonly IDBO dbo;
        private DataSet? dataset;
        private int id;
        private string? name;
        private int selectedFirma;
        public Lieferanten(IDBO dbo)
        {
            InitializeComponent();
            this.dbo = dbo;
            Task.Run(GetLieferanten).ContinueWith(t => GetPersons());
            //Task.Run(GetPersons);
        }

        private void btnZurueck_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.MainWindow.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbFirma.Text))
                Task.Run(InsertAsync);
            else
            {
                MessageBox.Show("Keine Firma angegeben");
            }
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Wollen Sie wirklich den\r\n Lieferanten aus der Datenbank löschen?", "\"Bitte\r\n bestätigen Sie den Löschvorgang", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                Task.Run(DeleteAsync);
        }
        private void tbFirma_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            name = box.Text;
        }
        private void cbPersonen_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            id = Convert.ToInt32(cb.SelectedValue.ToString());
        }

        private void lbLieferanten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            if (lb.SelectedValue is not null)
                selectedFirma = Convert.ToInt32(lb.SelectedValue.ToString());
        }
        private async Task GetLieferanten()
        {
            var query = "select * from lieferanten";
            var result = await dbo.ExecuteQueryDataSetASync(query);
            Dispatcher.Invoke(() =>
            {
                lbLieferanten.ItemsSource = result.Tables[0].DefaultView;
                lbLieferanten.SelectedValuePath = "id";
                dataset = result;
            });
        }
        private async Task GetPersons()
        {
            var query = "select * from dbdemo2.personen";
            var dataset = await dbo.ExecuteQueryDataSetASync(query);
            Dispatcher.Invoke(() =>
            {
                cbPersonen.DisplayMemberPath = "nachname";
                cbPersonen.SelectedValuePath = "id";
                cbPersonen.ItemsSource = dataset.Tables[0].DefaultView;
            });
        }
        private async Task InsertAsync()
        {
            var query = "INSERT INTO lieferanten VALUES (NULL, ? , ?) ";
            var result = await dbo.InsertInto(query, new Dictionary<string, object>()
            {
                { "personId" , id },
                { "firma" , name }
            });
            if (result)
            {
                Dispatcher.Invoke(() =>
                {
                    DataTable dt = dataset!.Tables[0];
                    DataRow dr = dt.NewRow();
                    dr["personId"] = cbPersonen.SelectedValue;
                    dr["firma"] = tbFirma.Text;
                    dt.Rows.Add(dr);
                    lbLieferanten.Items.Refresh();
                });
            }
            else
            {
                MessageBox.Show("Diese Firma kann nicht eingefügt werden");
            }
        }
        private async Task DeleteAsync()
        {
            var query = "delete from lieferanten where id = ?";
            var result = await dbo.DeleteFrom(query, new Dictionary<string, int>()
            {
                { "id" , selectedFirma }
            });
            if (result)
            {
                Dispatcher.Invoke(() =>
                {
                    DataTable dt = dataset!.Tables[0];
                    var rows = dt.Select($"id = {selectedFirma}");
                    dt.Rows.Remove(rows[0]);
                    lbLieferanten.Items.Refresh();
                });
            }
        }

    }
}
