using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace M_money.ViewModels
{
    class M_moneyItemViewModel
    {
        // 每个对应一个Pivot
        private ObservableCollection<Models.M_moneyItem> allItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> AllItems { get { return this.allItems; } }

        private ObservableCollection<Models.MonthItem> monthItems = new ObservableCollection<Models.MonthItem>();
        public ObservableCollection<Models.MonthItem> MonthItems { get { return this.monthItems; } }

        private ObservableCollection<Models.YearItem> yearItems = new ObservableCollection<Models.YearItem>();
        public ObservableCollection<Models.YearItem> YearItems { get { return this.yearItems; } }

        private ObservableCollection<Models.M_moneyItem> foodItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> FoodItems { get { return this.foodItems; } }

        private ObservableCollection<Models.M_moneyItem> transportItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> TransportItems { get { return this.transportItems; } }

        private ObservableCollection<Models.M_moneyItem> funItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> FunItems { get { return this.funItems; } }

        private ObservableCollection<Models.M_moneyItem> clothesItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> ClothesItems { get { return this.clothesItems; } }

        private ObservableCollection<Models.M_moneyItem> studyItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> StudyItems { get { return this.studyItems; } }

        private ObservableCollection<Models.M_moneyItem> otherItems = new ObservableCollection<Models.M_moneyItem>();
        public ObservableCollection<Models.M_moneyItem> OtherItems { get { return this.otherItems; } }

        // 选中时的3种情况（即有可能选中3种不同的item)
        // year和month也可以不设选中，因为选中也没什么用。。除非要在点击后触发对某年或者某月的消费类型占比分析，就需要这个
        private Models.M_moneyItem selectedItem = default(Models.M_moneyItem);
        public Models.M_moneyItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        private Models.MonthItem selectedMonth = default(Models.MonthItem);
        public Models.MonthItem SelectedMonth { get { return selectedMonth; } set { this.selectedMonth = value; } }

        private Models.YearItem selectedYear = default(Models.YearItem);
        public Models.YearItem SelectedYear { get { return selectedYear; } set { this.selectedYear = value; } }

        public M_moneyItemViewModel()
        {
            using (var statement = App.conn.Prepare("SELECT* FROM Message"))
            {
                if (statement.ColumnCount != 0)
                {
                    while (statement.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.AddM_moneyItem((int)(long)statement[0], (double)statement[1], (string)statement[2], (string)statement[3], Convert.ToDateTime(statement[4]));
                    }
                    allItems = new ObservableCollection<Models.M_moneyItem>(allItems.Reverse());
                }
                
            }
            

            //  初始化月份的items

            for (int i = 0; i < 12; i++)
            {
                string num = Convert.ToString(i+1);
                var target = "2017/" + num + "/%";
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
                this.monthItems.Add(new Models.MonthItem(2017, i+1, total));
            }


            // 年份Items初始化
            List<string> year = new List<string>();

            using (var yearsearchprepare = App.conn.Prepare("SELECT* FROM Message"))
            {
                if (yearsearchprepare.ColumnCount != 0)
                {
                    while (yearsearchprepare.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        string src = (string)yearsearchprepare[4];
                        string cut = src.Substring(0, 4);
                        if (!year.Contains(cut))
                        {
                            year.Add(cut);
                        }
                    }
                }
            }

            year.Sort();

            for (int i = 0; i < year.Count; i++)
            {
                var yeartarget = year[i] + "/%";
                int num = 0;
                double total = 0;
                using (var yearsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ?"))
                {
                    yearsearch.Bind(1, yeartarget);
                    if (yearsearch.ColumnCount != 0)
                    {
                        while (yearsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            total += (double)yearsearch[1];
                        }
                        num = Convert.ToInt32(year[i]);
                        this.yearItems.Add(new Models.YearItem(num, total));
                    }
                }
            }
            // Transport类初始化
            using (var transporttarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                transporttarget.Bind(1, "Transport");
                if (transporttarget.ColumnCount != 0)
                {
                    while (transporttarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.transportItems.Add(new Models.M_moneyItem((int)(long)transporttarget[0], (double)transporttarget[1], (string)transporttarget[2], (string)transporttarget[3], Convert.ToDateTime(transporttarget[4])));
                    }
                }
            }

            // Clothes类初始化
            using (var clothestarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                clothestarget.Bind(1, "Clothes");
                if (clothestarget.ColumnCount != 0)
                {
                    while (clothestarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.clothesItems.Add(new Models.M_moneyItem((int)(long)clothestarget[0], (double)clothestarget[1], (string)clothestarget[2], (string)clothestarget[3], Convert.ToDateTime(clothestarget[4])));
                    }
                }
            }

            //Study类初始化
            using (var studytarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                studytarget.Bind(1, "Study");
                if (studytarget.ColumnCount != 0)
                {
                    while (studytarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.studyItems.Add(new Models.M_moneyItem((int)(long)studytarget[0], (double)studytarget[1], (string)studytarget[2], (string)studytarget[3], Convert.ToDateTime(studytarget[4])));
                    }
                }
            }

            //Food类初始化
            using (var foodtarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                foodtarget.Bind(1, "Food");
                if (foodtarget.ColumnCount != 0)
                {
                    while (foodtarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.foodItems.Add(new Models.M_moneyItem((int)(long)foodtarget[0], (double)foodtarget[1], (string)foodtarget[2], (string)foodtarget[3], Convert.ToDateTime(foodtarget[4])));
                    }
                }
            }

            //Fun类初始化
            using (var funtarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                funtarget.Bind(1, "Fun");
                if (funtarget.ColumnCount != 0)
                {
                    while (funtarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.funItems.Add(new Models.M_moneyItem((int)(long)funtarget[0], (double)funtarget[1], (string)funtarget[2], (string)funtarget[3], Convert.ToDateTime(funtarget[4])));
                    }
                }
            }

            //Other类初始化
            using (var othertarget = App.conn.Prepare("SELECT* FROM Message WHERE Type LIKE ?"))
            {
                othertarget.Bind(1, "Other");
                if (othertarget.ColumnCount != 0)
                {
                    while (othertarget.Step() == SQLitePCL.SQLiteResult.ROW)
                    {
                        this.otherItems.Add(new Models.M_moneyItem((int)(long)othertarget[0], (double)othertarget[1], (string)othertarget[2], (string)othertarget[3], Convert.ToDateTime(othertarget[4])));
                    }
                }
            }
        }
       
        
        public void AddM_moneyItem(int id, double cost, string type, string description, DateTime time)
        {
            this.allItems.Add(new Models.M_moneyItem(id, cost, type, description, time));
        }

        public void RemoveM_moneyItem(int id)
        {
            this.allItems.Remove(selectedItem);
            this.selectedItem = null;
        }

        public void UpdateM_moneyItem(double cost, string type, string description, DateTime time)
        {
            this.selectedItem.cost = cost;
            this.selectedItem.type = type;
            this.selectedItem.description = description;
            this.selectedItem.time = time;
        }
    }
}
