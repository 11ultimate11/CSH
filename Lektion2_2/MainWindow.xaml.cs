using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lektion2_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private IDBO? dBO;
        private string? query;
        private DataTable? _table;
        public DataTable DataTable
        {
            get => _table ?? new DataTable();
            set
            {
                _table = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            dBO = new DBO("dbdemo2", "demo", "Florin123");

        }
        public async Task ExecuteQuery()
        {
            if (dBO is not null)
            {
                try
                {
                    var result = await dBO.ExecuteQueryDataSetASync(query);
                    Dispatcher.Invoke(() =>
                    {
                        sqlDG.ItemsSource = result.Tables[0].DefaultView;
                    });
                    
                    Debug.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                
            }
        }

        private void sqlEnter_Click(object sender, RoutedEventArgs e)
        {
            query = sqlEntry.Text;
            Task.Run(ExecuteQuery);
        }

        private void sqlEnd_Click(object sender, RoutedEventArgs e)
        {
            dBO?.Dispose();
            Application.Current.Shutdown();
        }
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
