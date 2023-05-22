using CSHlibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lektion2_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private readonly IDBO dbo = new DBO("dbdemo2", "demo", "Florin123");
        private int value;
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(LoadModels);
        }
        private async Task LoadModels()
        {
            try
            {

                var dataset = await dbo.ExecuteQueryDataSetASync("select * from personen");
                Dispatcher.Invoke(() =>
                {
                    combo.DisplayMemberPath = "nachname";
                    combo.SelectedValuePath = "id";
                    combo.ItemsSource = dataset.Tables[0].DefaultView;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private async Task LoadArtikles()
        {
            if (value == 0) { return; }
            var query = "select a.name , a.beschreibung , p.nachname , p.vorname " +
                "from artikel a , personen p , bestellungen b , bestellpositionen bp  , kunden k " +
                $"where p.id = {value} and k.personID = p.id and b.kundenID = k.id and bp.bestellID = b.id and a.id = bp.artikelID";
            var dataset = await dbo.ExecuteQueryDataSetASync(query);
            Dispatcher.Invoke(() =>
            {
                lbBestellungen.ItemsSource = dataset.Tables[0].DefaultView;
            });
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            value = Convert.ToInt32(combo.SelectedValue.ToString());
            Task.Run(LoadArtikles);
        }

        private void Button_Click_beenden(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_open_lif(object sender, RoutedEventArgs e)
        {
            Lieferanten lieferanten = new (dbo);
            lieferanten.Show();
        }

        private void Button_Click_Artikel(object sender, RoutedEventArgs e)
        {
            Artikel artikel = new Artikel(dbo);
            artikel.Show();
        }
    }
}
