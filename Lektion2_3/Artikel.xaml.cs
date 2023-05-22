using CSHlibrary;
using CSHlibrary.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lektion2_3
{
    /// <summary>
    /// Interaction logic for Artikel.xaml
    /// </summary>
    public partial class Artikel : Window, INotifyPropertyChanged
    {

        /*
         * Dieses Fenster wird zum Abrufen, Hinzufügen und Löschen von Elementen aus der Datenbank verwendet.
         * Es benötigt das IDBO interface , es in CSHLibrary sich befindet.
         * Dieses Fenster implementiert INotifyPropertyChanged, um die Benutzeroberfläche zu aktualisieren
          */
        private ArtikelModel _model = new();
        private readonly IDBO _dbo;
        private DataSet? _dataset;
        public ArtikelModel Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddCommand { get; set; }
        public ICommand DellCommand { get; set; }
        public ICommand BackCommand { get; set; }

        private readonly string addQuery = "insert into artikel values(? , ? , ? , ? , ? , ? , ?)";
        private readonly string dellQuery = "delete from artikel where id = ?";
        private readonly string getQuery = "select * from artikel";
        private string? error;
        private int delId;
        /// <summary>
        /// Benötige die IDBO-Schnittstellen, um zu funktionieren
        /// </summary>
        /// <param name="dbo"></param>
        public Artikel(IDBO dbo)
        {
            InitializeComponent();
            DataContext = this;
            AddCommand = new RelayCommand(async () => await AddAsync());
            DellCommand = new RelayCommand(DeleteArtikel);
            BackCommand = new RelayCommand(() => Dispatcher.Invoke(() => this.Close()));
            _dbo = dbo;
            Task.Run(GetArtikelAsync);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Wird verwendet, um der Benutzeroberfläche mitzuteilen, dass sich etwas geändert hat.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Validieren Sie das Modell, das in dbo eingefügt werden soll
        /// </summary>
        /// <returns></returns>
        private bool ValidateModel()
        {
            bool valid = true;
            var properties = Model.GetType().GetProperties().ToList();
            if (properties is not null && properties.Any())
            {
                foreach (var property in properties)
                {
                    if (property.GetValue(Model) is null)
                    {
                        valid = false;
                        error = $"Diese Field darf nicht leer sein : {property.Name}";
                        break;
                    }
                }
            }
            return valid;
        }
        private Dictionary<string, object> GenerateDic()
        {
            Dictionary<string, object> dic = new();
            var properties = Model.GetType().GetProperties().ToList();
            if (properties is not null && properties.Any())
            {
                foreach (var property in properties)
                {
                    if (property.GetValue(Model) is not null)
                    {
                        dic.Add(property.Name.ToLower(), property.GetValue(Model)!);
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// Diese Methode fügt asynchron das Element in dbo
        /// </summary>
        /// <returns></returns>
        private async Task AddAsync()
        {
            if (!ValidateModel())
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(error);
                });
                return;
            }
            var result = await _dbo.InsertInto(addQuery, GenerateDic());
            Dispatcher.Invoke(() =>
            {
                var table = _dataset.Tables[0];
                DataRow row = table.NewRow();
                PopulateTable(row);
                table.Rows.InsertAt(row ,0);
                collection.Items.Refresh();
                Model = new() { id = Model.id +1 };
                OnPropertyChanged(nameof(Model));
                MessageBox.Show("Artikel wurde hinzugefügt");
            });
        }
        /// <summary>
        /// Diese Methode holt alle Elemente aus dbo
        /// </summary>
        /// <returns></returns>
        private async Task GetArtikelAsync()
        {
            var result = await _dbo.ExecuteQueryDataSetASync(getQuery);
            Dispatcher.Invoke(() =>
            {
                _dataset = result;
                collection.SelectedValuePath = "id";
                collection.ItemsSource = result.Tables[0].DefaultView;
                var table = _dataset.Tables[0];
                var lastID = table.AsEnumerable().Max(x => x.Field<int>("id"));
                Model.id = lastID +1;
                OnPropertyChanged(nameof(Model));
            });
        }
        private void DeleteArtikel()
        {
            Dispatcher.Invoke(() =>
            {
                if (collection.SelectedItem is null)
                {
                    MessageBox.Show("Kein Artikel ausgewählt");
                    return;
                }
                else
                {
                    var result = MessageBox.Show("Artikel löschen", "Delete the item from database", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;
                    delId = Convert.ToInt32(collection.SelectedValue.ToString());
                    Task.Run(DeleteAsync);
                }
            });
        }
        /// <summary>
        /// Diese Methode löscht das Element aus dbo
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAsync()
        {
            var result = await _dbo.DeleteFrom(dellQuery, new Dictionary<string, int>() { { "id", delId } });
            if (result)
            {
                Dispatcher.Invoke(() =>
                {
                    var table = _dataset.Tables[0];
                    var rows = table.Select($"id = {delId}");
                    table.Rows.Remove(rows[0]);
                    collection.Items.Refresh();
                });
            }
        }
        private void PopulateTable(DataRow row)
        {
            var properties = Model.GetType().GetProperties().ToList();
            if (properties is not null && properties.Any())
            {
                foreach (var property in properties)
                {
                    if (property.GetValue(Model) is not null)
                    {
                        row[property.Name.ToLower()] = property.GetValue(Model);
                    }
                }
            }
        }
    }

}
