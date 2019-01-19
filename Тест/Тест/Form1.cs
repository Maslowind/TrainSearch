using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Web;

namespace Тест
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void TakeXMLtickets(string from, string to, string date, string time)
        {
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["from"] = from;
                data["to"] = to;
                data["date"] = date;
                data["time"] = time;
                var response = wb.UploadValues(@"https://booking.uz.gov.ua/ru/train_search/", "POST", data);
                string responseText = Encoding.UTF8.GetString(response);
                XNode node = JsonConvert.DeserializeXNode(responseText, "Root");
                System.IO.File.WriteAllText(TakePath()+@"\КодBooking.xml", Convert.ToString(node));
            }
        }
        private void TakeStationList(string from, string to, string date, string time)
        {
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["routes[0][from]"] = from;
                data["routes[0][to]"] = to;
                data["routes[0][date]"] = date;
                data["routes[0][train]"] = time;
                var response = wb.UploadValues(@"https://booking.uz.gov.ua/ru/route/", "POST", data);
                string responseText = Encoding.UTF8.GetString(response);
                XNode node = JsonConvert.DeserializeXNode(responseText, "Root");
                System.IO.File.WriteAllText(TakePath() + @"\КодBooking3.xml", Convert.ToString(node));
            }
        }
        private string TakeStationNumber(string StationName)
        {
            string HttpAd = Uri.EscapeDataString(StationName);
            string HtmlText = string.Empty;
            HttpWebRequest myHttwebrequest = (HttpWebRequest)HttpWebRequest.Create(@"https://booking.uz.gov.ua/ru/train_search/station/?term="+HttpAd);
            HttpWebResponse myHttpWebresponse = (HttpWebResponse)myHttwebrequest.GetResponse();
            StreamReader strm = new StreamReader(myHttpWebresponse.GetResponseStream());
            HtmlText = strm.ReadToEnd();
            bool b = false;
            XmlNode xRoot = JsonConvert.DeserializeXmlNode("{\"data\":{\"list\":" + HtmlText + "}}");
            foreach (XmlNode xRoot1 in xRoot)
            {
                foreach (XmlNode xnode in xRoot1.ChildNodes)
                {
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "title" && childnode.InnerText==StationName)
                        {
                            b = true;
                        }
                        if (childnode.Name == "value" &&b)
                        {
                            return childnode.InnerText;
                        }
                        

                    } 
                }
            }
            return null;
        }
        private string TakePath()
        {
            string executableLocation = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);
            return Path.Combine(executableLocation);
        }
        private string[,] TakeInformation(ref int i)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(TakePath() + @"\КодBooking.xml");
            XmlElement xRoot = xDoc.DocumentElement;
            int arrayl = 2;
            string[,] table = new string[arrayl, 6];
           
            foreach (XmlNode xRoot1 in xRoot)
            {
                foreach (XmlNode xnode in xRoot1.ChildNodes)
                {
                   foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "num")
                        {
                            table[i, 0] = childnode.InnerText;                   
                        }
                        if (childnode.Name == "travelTime")
                        {
                            table[i, 4] = childnode.InnerText;
                        }
                        if (childnode.Name == "types")
                        {
                            foreach (XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "id")
                                {
                                    table[i, 5] += (childnode2.InnerText + ":");
                                }
                                if (childnode2.Name == "places")
                                {
                                    table[i, 5] += (childnode2.InnerText + "; ");
                                }
                            }
                        }
                        if (childnode.Name == "from")
                        {
                            string g="";
                            foreach (XmlNode childnode2 in childnode.ChildNodes)
                            {
                                
                                if (childnode2.Name == "stationTrain")
                                {
                                    table[i, 1] = (childnode2.InnerText);
                                }
                                if (childnode2.Name == "time")
                                {
                                    table[i, 2] =g.Insert(0,childnode2.InnerText + " ");
                                }
                                if (childnode2.Name == "date")
                                {
                                    g = (childnode2.InnerText);
                                }
                            }
                        }
                        if (childnode.Name == "to")
                        {
                            string g = "";
                            foreach (XmlNode childnode2 in childnode.ChildNodes)
                            {

                                if (childnode2.Name == "stationTrain")
                                {
                                    table[i,1] +=" - "+ (childnode2.InnerText);
                                }
                                if (childnode2.Name == "time")
                                {
                                    table[i, 3] = g.Insert(0, childnode2.InnerText + " ");
                                }
                                if (childnode2.Name == "date")
                                {
                                    g = (childnode2.InnerText);
                                }
                            }
                        }
                    }
                  if(xnode.Name!="warning")
                  {
                      i++;
                    table = (string[,])ResizeArray(table, new int[] { arrayl++, 6 }); }
                    
                } 
            }
            return table;
        }
        private string[,] TakeTrainStations(ref int i)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(TakePath() + @"\КодBooking3.xml");
            XmlElement xRoot = xDoc.DocumentElement;
            int arrayl = 2;
            string[,] table = new string[arrayl, 2];

            foreach (XmlNode xRoot2 in xRoot)
            {
                foreach (XmlNode xRoot1 in xRoot2)
            {
                     
                            foreach (XmlNode xnode in xRoot1.ChildNodes)
                            {
                                if (xnode.Name != "tpl")
                                {
                                    foreach (XmlNode childnode in xnode.ChildNodes)
                                    {
                                        if (childnode.Name == "name")
                                        {
                                            table[i, 0] = childnode.InnerText;
                                        }
                                        if (childnode.Name == "departureTime")
                                        {
                                            table[i, 1] = childnode.InnerText;
                                        }

                                    }
                                    if (xnode.Name != "train" && xnode.Name != "#text")
                                    {
                                        i++;
                                        table = (string[,])ResizeArray(table, new int[] { arrayl++, 2 });
                                    }
                                }
                           
                }
                }
            }
            return table;
        }
            private static Array ResizeArray(Array arr, int[] newSizes)
   {
      if (newSizes.Length != arr.Rank)
         throw new ArgumentException("arr must have the same number of dimensions " +
                                     "as there are elements in newSizes", "newSizes"); 

      var temp = Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
      int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
      Array.ConstrainedCopy(arr, 0, temp, 0, length);
      return temp;
   }

            private int AddToTable()
            {
                int i = 0;
                string[,] table = TakeInformation(ref i);
                int m = 0;
                
                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < 6; k++)
                        { 
                            dataGridView1[k, j].Value = "";
                        } dataGridView1.RowCount++;
                    
                }
                for (int j = 0; j < i; j++)
                {
                    if (table[j, 5] != null)
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            dataGridView1[k, j-m].Value = table[j, k];
                        } //dataGridView1.RowCount++;
                        
                    }else m++;
                }
                return m;
            }
            private int AddToTable2(int n,double sec1)
            {
                int i = 0;
                string[,] table = TakeInformation(ref i);
                int m = 0; dataGridView1.RowCount++;
                
                for (int j = 0; j < i; j++)
                {
                    if (table[j, 5] != null)
                    {
                        string[] mas = table[j, 3].Split(' ');
                        double sec2 = TimeSpan.Parse(mas[0]).TotalSeconds;
                        DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                        for (int k = 0; k < 6; k++)
                        {
                            
                            row.Cells[k].Value =table[j, k];
                            
                        }
                        if (sec2 >= sec1)
                        {
                            this.dataGridView1.Rows.Add(row);
                        }


                    }
                    else m++;
                }
                return  m;
            }            
                    
                
            
        public void toTable()
        {
            
                dataGridView1.ColumnCount = 6;
                dataGridView1.RowCount = 2;
                dataGridView1.Columns[0].HeaderText = "Номер";
                dataGridView1.Columns[1].HeaderText = "Откуда/Куда";
                dataGridView1.Columns[2].HeaderText = "Время отправления";
                dataGridView1.Columns[3].HeaderText = "Время прибытия";
                dataGridView1.Columns[4].HeaderText = "В пути";
                dataGridView1.Columns[5].HeaderText = "Тип мест";
                this.dataGridView1.DefaultCellStyle.WrapMode =DataGridViewTriState.True;
                dataGridView1.RowTemplate.MinimumHeight = 60;
                DataGridViewColumn column = dataGridView1.Columns[1];
                column.Width = 120;
           
        }
        public int ChooseTrain()
        {
            int i = 0, j = 0;
            int MaxValue = -2147483647, MaxK=0;
            string[,] train = TakeInformation(ref i); 
            for(int k=0; k<i; k++)
            {
                TakeStationList(TakeStationNumber(Convert.ToString(textBox2.Text)),
         TakeStationNumber(Convert.ToString(textBox3.Text)), Convert.ToString(textBox6.Text) +
     "-" + Convert.ToString(textBox5.Text) + "-" +
     Convert.ToString(textBox4.Text),
     train[k, 0]);
                j = 0;
                string[,] trainStation = TakeTrainStations(ref j);
                if (MaxValue < j) { MaxValue = j; MaxK = k; }
            }
            return MaxK;
            
        }

        private string ConvertDate(string OldDate)
        {
            string[] mas = OldDate.Split('.');
            return mas[2]+"-"+mas[1]+"-"+mas[0];
        }
       private void AlternateTicket(string[,]train)
        {
            int k = 0;
            string[,] trainStation = TakeTrainStations(ref k);
            
                int t = ChooseTrain();
                TakeStationList(TakeStationNumber(Convert.ToString(textBox2.Text)),
                     TakeStationNumber(Convert.ToString(textBox3.Text)), Convert.ToString(textBox6.Text) +
                 "-" + Convert.ToString(textBox5.Text) + "-" +
                 Convert.ToString(textBox4.Text),
                 train[t, 0]);
                
                for (int j = 1; j < k; j++)
                {int i = 0;
                   string[,] train2 =  TakeInfAboutConnection(TakeStationNumber(Convert.ToString(textBox2.Text)),
                     TakeStationNumber(trainStation[k-1-j,0]),
                     Convert.ToString(textBox6.Text) + "-" + Convert.ToString(textBox5.Text) + "-" + Convert.ToString(textBox4.Text),
                     ref i);
                     string[] mas = train[t, 3].Split(' ');
                     string NewDate = ConvertDate(mas[2]);
                     double sec1 = TimeSpan.Parse(mas[0]).TotalSeconds;

                    toTable();
                    int m = AddToTable();
                    if(m!=i)
                    {
                        int l= 0;
                        string[,] train3 = TakeInfAboutConnection(TakeStationNumber(trainStation[k - 1 - j, 0]),
                          TakeStationNumber(Convert.ToString(textBox3.Text)),
                          //Convert.ToString(textBox6.Text) + "-" + Convert.ToString(textBox5.Text) + "-" + Convert.ToString(textBox4.Text),
                          NewDate,
                          ref l);
                        label7.Text="На станции "+ trainStation[k - 1 - j, 0]+ " у Вас пересадка!";
                        toTable();
                         int n=AddToTable2(m,sec1);
                        if(l!=n)
                        {
                            break;
                        }
                    }
                }           
        }
        private string [,] TakeInfAboutConnection(string s1,string s2, string date, ref int i)
       {
           TakeXMLtickets(s1,s2,date,"00:00");
           return TakeInformation(ref i);
       }
        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            string [,] train=TakeInfAboutConnection(TakeStationNumber(Convert.ToString(textBox2.Text)),
                     TakeStationNumber(Convert.ToString(textBox3.Text)),
                     Convert.ToString(textBox6.Text) + "-" + Convert.ToString(textBox5.Text) + "-" + Convert.ToString(textBox4.Text),
                     ref i);
            
            
            toTable();
            int m=AddToTable();            
            if (m == i)
            {
            AlternateTicket(train);
            }
        }
        }

       
}
            
            
        
    

