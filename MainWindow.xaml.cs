using System;
using System.Collections.Generic;
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
using System.Windows.Shell;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace ScreenScraper {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ChromeDriver driver = null;
        List<Game> games;
        public MainWindow() {
            InitializeComponent();
            DataFetcher df = new DataFetcher();
            df.FetchData();
            games = Database.Instance.GetGames();
            txtTotal.Text = "Total Games: " + games.Count;
            dataGrid.ItemsSource = games;            
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e) {
            if(dataGrid.SelectedItem != null) {
                if(driver == null) {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArguments("user-data-dir=C:\\Users\\Stalker\\AppData\\Local\\Google\\Chrome\\User Data\\Default");
                    driver = new ChromeDriver(options);
                }                
                
                var cellInfo = dataGrid.SelectedCells[1];
                string url = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                driver.Url = url;
                driver.Navigate();
            } else {
                Log("Please select a row.");
            }
            
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e) {
            if(dataGrid.SelectedItem != null) {
                var cellInfo = dataGrid.SelectedCells[0];
                string name = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                Database.Instance.DeleteGame(name);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e) {
            if(ignoredGrid.SelectedItem != null) {
                var cellInfo = ignoredGrid.SelectedCells[0];
                string name = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                Database.Instance.RestoreIgnored(name);
                ignoredGrid.ItemsSource = Database.Instance.GetIgnored();
            }
        }

        private void BtnDeleteIgnored_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void BtnIgnore_Click(object sender, RoutedEventArgs e) {
            if(dataGrid.SelectedItem != null) {
                var cellInfo = dataGrid.SelectedCells[0];
                string gameTitle = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                Database.Instance.IgnoreGame(gameTitle);
                RefreshGrid(dataGrid);
            } else {
                Log("Please select a row.");
            }           
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(e.Source is TabControl) {
                TabControl tabControl = (TabControl) e.Source;
                TabItem tab = (TabItem) tabControl.SelectedItem;
                if(tab.Header.ToString() == "Ignored") {
                    RefreshGrid(ignoredGrid);
                } else if (tab.Header.ToString() == "Games") {
                    RefreshGrid(dataGrid);
                    if(games != null) {
                        txtTotal.Text = "Total Games: " + games.Count;
                    }
                }

            }
        }

        public async void Log(string message) {
            txtError.Text = message;
            await Task.Delay(5000);
            txtError.Text = "";
        }

        private void RefreshGrid(DataGrid grid) {
            if(grid == dataGrid) {
                if(games != null) {
                    txtTotal.Text = "Total Games: " + games.Count;
                    games = Database.Instance.GetGames();
                    dataGrid.ItemsSource = games;
                    dataGrid.Items.Refresh();
                }
            } else if (grid == ignoredGrid) {
                ignoredGrid.ItemsSource = Database.Instance.GetIgnored();
                txtTotal.Text = "Total Games: " + Database.Instance.GetIgnored().Count;
            } else {
                Log("ERROR: Grid does not exist.");
            }
            
        }
    }
}
