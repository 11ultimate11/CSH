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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lection2_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDBO? dBO;
        private string? query;
        public MainWindow()
        {
            InitializeComponent();
            dBO = new DBO("dbdemo2", "demo", "Florin123");
        }
        public async Task ExecuteQuery()
        {
            if (dBO is not null)
            {
                var result = await dBO.ExecuteQuery(query);
                if (result.Item1 is not null && result.Item2 is not null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        sqlList.Items.Clear();
                        string header = "";
                        result.Item1.ForEach(x => header += $" {x}");
                        sqlList.Items.Add(header);
                        foreach (var item in result.Item2)
                        {
                            sqlList.Items.Add(item);
                        }
                    });


                }
                Debug.WriteLine(result);
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
    }
}
