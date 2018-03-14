using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using M_money.Models;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace M_money
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = new ViewModels.M_moneyItemViewModel();
            this.Tile();
        }
        ViewModels.M_moneyItemViewModel ViewModel { get; set; }

       protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           /* if (e.Parameter.GetType() == typeof(ViewModels.M_moneyItemViewModel))
            {
                this.ViewModel = (ViewModels.M_moneyItemViewModel)(e.Parameter);
            }*/
            // UpdateTile();
            DataTransferManager.GetForCurrentView().DataRequested += DataRequested;

        }

        private void MoneyItem_Click(object sender, ItemClickEventArgs e)
        {

            
            ViewModel.SelectedItem = (Models.M_moneyItem)(e.ClickedItem);
            if (ViewModel.SelectedItem != null) CreateButton.Content = "Update";
            if (InlineToDoItemViewGrid.Visibility != Visibility.Collapsed)
            {
                cost.Text = ViewModel.SelectedItem.cost.ToString();
                detail.Text = ViewModel.SelectedItem.description;
                date.Date = ViewModel.SelectedItem.time;
                //image1.Source = ViewModel.SelectedItem.imgs;
                Clothes.IsChecked = Fun.IsChecked = Study.IsChecked = Transport.IsChecked = Other.IsChecked = Food.IsChecked = false;
                if (ViewModel.SelectedItem.type == "Clothes") Clothes.IsChecked = true;
                if (ViewModel.SelectedItem.type == "Study") Study.IsChecked = true;
                if (ViewModel.SelectedItem.type == "Transport") Transport.IsChecked = true;
                if (ViewModel.SelectedItem.type == "Food") Food.IsChecked = true;
                if (ViewModel.SelectedItem.type == "Fun") Fun.IsChecked = true;
                if (ViewModel.SelectedItem.type == "Other") Other.IsChecked = true;
            }

            /*  else
              {
                  Frame.Navigate(typeof(NewPage), ViewModel);
              }
            */

        }
        private void Month_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedMonth = (Models.MonthItem)(e.ClickedItem);
            Frame.Navigate(typeof(MonthPage), ViewModel);
        }

        private void Year_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedYear = (Models.YearItem)(e.ClickedItem);
            Frame.Navigate(typeof(YearPage), ViewModel);
        }
        private void Combobox_click(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedItem != null)
            {
                ViewModel.SelectedYear = (Models.YearItem)(comboBox.SelectedItem);
                ViewModel.MonthItems.Clear();
                for (int i = 0; i < 12; i++)
                {
                    string num = Convert.ToString(i + 1);
                    var target = Convert.ToString(ViewModel.SelectedYear.year) + "/" + num + "/%";
                    double total = 0;
                    using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ?"))
                    {
                        monthsearch.Bind(1, target);
                        if (monthsearch.ColumnCount != 0)
                        {
                            while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                            {
                                total += (double)monthsearch[1];
                            }
                        }
                    }
                    ViewModel.MonthItems.Add(new Models.MonthItem(ViewModel.SelectedYear.year, i + 1, total));
                }
            }
        }

        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //type....???先假装默认只选一个
            string type = "";
            bool chosen = false;
            if (Clothes.IsChecked == true) type = "Clothes";
            if (Study.IsChecked == true) type = "Study";
            if (Transport.IsChecked == true) type = "Transport";
            if (Food.IsChecked == true) type = "Food";
            if (Fun.IsChecked == true) type = "Fun";
            if (Other.IsChecked == true) type = "Other";

            if (cost.Text == "")
            {
                var i = new MessageDialog("Please enter cost!").ShowAsync();
            }
            if (detail.Text == "")
            {
                var i = new MessageDialog("Please enter descriptions!").ShowAsync();
            }
            if (date.Date > System.DateTime.Now)
            {
                var i = new MessageDialog("CreateDate is invalid, please choose again!").ShowAsync();
            }
            if (Clothes.IsChecked == false && Study.IsChecked == false && Transport.IsChecked == false && Food.IsChecked == false && Fun.IsChecked == false && Other.IsChecked == false)
            {
                var i = new MessageDialog("Please choose a type!").ShowAsync();
            } else
            {
                chosen = true;
            }
            if (cost.Text != "" && detail.Text != "" && date.Date <= DateTime.Now && chosen == true)
            {
                if (CreateButton.Content.ToString() == "Update" && ViewModel.SelectedItem != null)
                {
                    //数据库update
                    //.......todo.......
                    var db = App.conn;
                    using (var updatedata = db.Prepare("UPDATE Message SET Cost = ?, Type = ?, Description = ?, Time = ? WHERE Id = ?"))
                    {
                        updatedata.Bind(1, Convert.ToDouble(cost.Text));
                        updatedata.Bind(2, type);
                        updatedata.Bind(3, detail.Text);
                        updatedata.Bind(4, Convert.ToString(date.Date.DateTime));
                        updatedata.Bind(5, ViewModel.SelectedItem.id);
                        updatedata.Step();
                    }

                    ViewModel.UpdateM_moneyItem(Convert.ToDouble(cost.Text), type, detail.Text, date.Date.DateTime);
                    //刷新页面
                    Frame.Navigate(typeof(MainPage), ViewModel);
                }
                else if (CreateButton.Content.ToString() == "Save" && ViewModel.SelectedItem == null)
                {
                    //数据库insert
                    //.......todo.......
                    var db = App.conn;
                    int id;
                    if (ViewModel.AllItems.Count == 0) id = 1;
                    else id = ViewModel.AllItems[0].id + 1;
                    using (var insertdata = db.Prepare("INSERT INTO Message(Id, Cost, Type, Description, Time) VALUES(?, ?, ?, ?, ?)"))
                    {
                        insertdata.Bind(1, id);
                        insertdata.Bind(2, Convert.ToDouble(cost.Text));
                        insertdata.Bind(3, type);
                        insertdata.Bind(4, detail.Text);
                        insertdata.Bind(5, Convert.ToString(date.Date.DateTime));
                        insertdata.Step();
                    }

                    ViewModel.AddM_moneyItem(id, Convert.ToDouble(cost.Text), type, detail.Text, date.Date.DateTime);
                    switch(type)
                    {
                        case "Clothes":
                            ViewModel.ClothesItems.Add(ViewModel.AllItems[0]);
                            break;
                        case "Study":
                            ViewModel.StudyItems.Add(ViewModel.AllItems[0]);
                            break;
                        case "Transport":
                            ViewModel.TransportItems.Add(ViewModel.AllItems[0]);
                            break;
                        case "Food":
                            ViewModel.FoodItems.Add(ViewModel.AllItems[0]);
                            break;
                        case "Fun":
                            ViewModel.FunItems.Add(ViewModel.AllItems[0]);
                            break;
                        case "Other":
                            ViewModel.OtherItems.Add(ViewModel.AllItems[0]);
                            break;
                    }
                    //刷新页面
                    chosen = false;
                    Frame.Navigate(typeof(MainPage), ViewModel);
                }
            }
        }

        void CheckBox_click(object sender, RoutedEventArgs e)
        {
            Clothes.IsChecked = Study.IsChecked = Transport.IsChecked = Food.IsChecked = Fun.IsChecked = Other.IsChecked = false;
            CheckBox temp = (CheckBox)sender;
            temp.IsChecked = true;
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var db = App.conn;
            dynamic ori = e.OriginalSource;
            ViewModel.SelectedItem = (M_moneyItem)ori.DataContext;
            using (var statement = db.Prepare("DELETE FROM Message WHERE Id = ?"))
            {
                statement.Bind(1, ViewModel.SelectedItem.id);
                statement.Step();
            }
            ViewModel.RemoveM_moneyItem(ViewModel.SelectedItem.id);
            ViewModel.SelectedItem = null;
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Clothes.IsChecked = false;
            Study.IsChecked = false;
            Transport.IsChecked = false;
            Food.IsChecked = false;
            Fun.IsChecked = false;
            Other.IsChecked = false;

            cost.Text = "";
            detail.Text = "";
            date.Date = System.DateTime.Now;
        }
        void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic a = e.OriginalSource;
            ViewModel.SelectedItem = (M_moneyItem)a.DataContext;
            //启动共享UI
            DataTransferManager.ShowShareUI();
        }

        private async void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share bill: " + ViewModel.SelectedItem.type;
            request.Data.Properties.Description = ViewModel.SelectedItem.description;
            request.Data.SetText("Consumption bill\n" + "Consumption type: "+ ViewModel.SelectedItem.type +
                "\nConsumption amount: ￥"+ViewModel.SelectedItem.cost + "\nConsumption time: "+ViewModel.SelectedItem.time);
            //share pictures
            var photoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/"+ViewModel.SelectedItem.type+".png"));
            request.Data.SetStorageItems(new List<StorageFile> { photoFile });
        }

        private void Tile()
        {
            String s = File.ReadAllText("Tile.xml");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            XmlNodeList xn1 = doc.GetElementsByTagName("text");
            XmlNodeList xn2 = doc.GetElementsByTagName("binding");
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            
            int i;
            for (i = ViewModel.AllItems.Count-1; i >= 0; i--)
            {
               
                xn1[0].InnerText = " ¥" + ViewModel.AllItems[i].cost.ToString();
                xn1[1].InnerText = ViewModel.AllItems[i].time.Month.ToString() + "." + ViewModel.AllItems[i].time.Day.ToString();
                xn1[3].InnerText = ViewModel.AllItems[i].time.Year.ToString() + "年" + ViewModel.AllItems[i].time.Month.ToString() + "月"
                    + ViewModel.AllItems[i].time.Day.ToString() + "日";
                xn1[4].InnerText = ViewModel.AllItems[i].type.ToString();
                xn1[5].InnerText = " ¥" + ViewModel.AllItems[i].cost.ToString();
                xn1[2].InnerText = ViewModel.AllItems[i].type.ToString() +  " ¥" + ViewModel.AllItems[i].cost.ToString();
                TileNotification notification = new TileNotification(doc);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
        }

    }
}




/* 按月查找记录示例（按年查找类似）,以一月为例
 *              var target = "%/1/%";
                using (var statement = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ?"))
                {
                    statement.Bind(1, target);
                    if (statement.ColumnCount != 0)
                    {
                        double total = 0;
                        while (statement.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            total += (string)statement[1];
                        }
                    }
                }
*/
