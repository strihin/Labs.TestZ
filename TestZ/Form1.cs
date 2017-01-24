using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace TestZ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();  
        }        
        
        int Beta()
        {
            int beta = 0;
            var rb = new List<RadioButton> {rb1, rb2, rb3, rb4};
            foreach (RadioButton t in rb)
            {
                beta += 2;
                if (t.Checked)
                    return beta;
            }
            return 2;
        }

        public double M(double x)
        {
            return Math.Pow(16, Math.Pow(Math.Sin(x), Math.Cos(x))) + 3 * Math.Pow(4, Math.Sin(2 * x) / 2) - Beta();
        }

#region Approximation
       double MethodInterval(double x, double[] nodes, int k) { return  x * Math.Pow(nodes[k], x); }

       double SKO(double x, double[] nodes, int k, double[] xval)
       {
           return  Math.Pow(Math.Pow(M(x) - MethodPoint(x, nodes, k, xval), 2)*Math.Pow(nodes.Count(),-1),0.5);
       }
        double MethodPoint(double x, double[] nodes, int k, double[] xval)
        {

            double mi = Math.Pow(nodes.Count()-1, -1) + nodes[k] * xval[k] / x;
            return mi;
        }
        public double Apr(double x, double[] nodes, int k, double[] xval)
        {
           // return m(x)*xval[k]/x + Math.Pow(nodes.Count(), -1);
             return MethodPoint(x, nodes, k,xval);         
        }
        

        double Ee(double[] nodes, double[] xval, int xmax, ref int outcount)
        {
            
            int k = 0;
            double ma=0;
            var yStar = new double[3];
            var y = new double[3];
            var Em = new double[7];
            yStar[0] = (xval[0] + xval[xmax-2]) / 2;
            yStar[1] = Math.Sqrt(Math.Abs(xval[0]) * xval[xmax-2]);
            yStar[2] = 2 * (xval[0] * xval[xmax-2]) / xval[0] + xval[xmax-2];
            y[0] = (nodes[0] + nodes[xmax-2]) / 2;
            y[1] = Math.Sqrt(Math.Abs(nodes[0] * nodes[xmax-2]));
            y[2] = 2 * (nodes[0] * nodes[xmax-2]) / nodes[0] + nodes[xmax-2];

            for (int i = 0; i < 7; i++)
            {
                if(i>=0 && i<=2)
                    Em[i] = Math.Abs(yStar[0] - y[i]);
                
                if (i==3)
                {
                    Em[i] = Math.Abs(yStar[1] - y[0]);
                }
                if (i > 3 && i<7)
                {

                    Em[i] = Math.Abs(yStar[2] - y[k]);
                    k++;
                }
                ma = Em.Min();                
                
            }

            for (int i = 0; i < 7; i++)
            {
                if (ma == Em[i])
                {
                    outcount = i;
                }
            }
            return ma;
        }
        
#endregion

       public double PnLagrange(double x, double[] nodes, double[] xval)
       {
            double p = 1;
            double pn = 0;
            
           for (int i = 0; i < xval.Count(); i++)
           {
               double currentX = xval[i];
               for (int j = 0; j < xval.Count(); j++)
               {
                   if (currentX != xval[j])
                   {
                      
                       p*=(x-xval[j])/(currentX-xval[j]);
                   }
               }
               pn +=nodes[i]*p;
               p = 1;
           }

           return pn;
         }
       public double Newton(double[] xval, double[] node, double x)
       {
           
           double res = node[0], f, den;
           int i, j, k;
           for (i = 1; i < node.Count(); i++)
           {
               f = 0;
               for (j = 0; j <= i; j++)
               {//следующее слагаемое полинома
                   den = 1;
                   //считаем знаменатель разделенной разности
                   for (k = 0; k <= i; k++)
                   {
                       if (k != j) den *= (xval[j] - xval[k]);
                   }
                   //считаем разделенную разность
                   f += node[j] / den;
               }
               
               //домножаем разделенную разность на скобки (x-x[0])...(x-x[i-1])
               for (k = 0; k < i; k++) f *= (Math.Round(x,5) - xval[k]);
               res += f;//полином
           }
           return res;
       } 
#region Drawing
         public void DrawGraph()
         {
             int k = 0;
             double tempsko = 0;
             double x0 = 0.01936;
             double step = 2;
             int xmax = Convert.ToInt16(comboBox1.Text) + 1;
             dataGridView1.RowCount = xmax - 1;
             var nodes = new double[xmax];
             var xval = new double[xmax];
             const string outp1 = "SKO<<Emin=> Аппроксимация физически недостоверна!";
             const string outp2 = "SKO>>Emin=> Аппроксимация физически недостоверна!";
             const string equation = "  =>y=a+b/x\nSKO=";

             GraphPane pane = zedGraph1.GraphPane;
             pane.CurveList.Clear();
             pane.Title.Text = "";
           
             var mlist = new PointPairList();
             var LGList = new PointPairList();
             var NTList = new PointPairList();
             var MNKList = new PointPairList();
  
             step /= xmax;
            
             for (double i =x0; i <= 2; i+=step)
             {
                 nodes[k] = M(i);
                 xval[k] = i;
                 k++;
             }
             k = 0;
             double[] ELN =new double[xmax-1];
             double[] ENT = new double[xmax-1];
             double[] EMNK = new double[xmax-1];
             for (double x =x0; x <= 2; x +=step)
             {                              
                 LGList.Add(x, PnLagrange(x, nodes,xval)); 
                 NTList.Add(x, Newton(xval,nodes,x));
                 MNKList.Add(x, Apr(x, nodes,k,xval));
                 tempsko+= SKO(x, nodes, k, xval);
                 k++;
             }

             tempsko /= xmax;
             for (double x = x0; x <= 2; x += 0.1)
             {
                 mlist.Add(x, M(x));
             }

             for (int x = 0; x < xmax - 1; x++)
             {
                 dataGridView1[0, x].Value = xval[x];
                 dataGridView1[1, x].Value = mlist[x].Y;
                 dataGridView1[2, x].Value = LGList[x].Y;
                 dataGridView1[3, x].Value = NTList[x].Y;
                 dataGridView1[4, x].Value = MNKList[x].Y;

                 ELN[x] = Math.Abs(mlist[x].Y - LGList[x].Y);
                 ENT[x] = Math.Abs(mlist[x].Y - NTList[x].Y);
                 EMNK[x] = Math.Abs(mlist[x].Y - MNKList[x].Y);
             }
             double EmaxLN = ELN.Max();
             double EmaxNT = ENT.Max();
             double EmaxMNK = EMNK.Max();
             lblE.Text = "Emax(LG)=" + EmaxLN.ToString("N3") + "\nEmax(NT)=" + EmaxNT.ToString("N3") + "\nEmax(MNK)=" + EmaxMNK.ToString("N3");

             pane.AddCurve("f(x)", mlist, Color.Thistle, SymbolType.None);
             pane.AddCurve("LaGrange(x)", LGList, Color.Red, SymbolType.Triangle);
             pane.AddCurve("Newton(x)", NTList, Color.Blue, SymbolType.TriangleDown);
             LineItem curvemy=  pane.AddCurve("MNK(x)", MNKList, Color.Indigo, SymbolType.Star);
             curvemy.Line.IsVisible = false;

             int outcount=0;
             string outE = Ee(nodes, xval, xmax, ref outcount).ToString();
             
             pane.XAxis.Scale.Min = 0;
             pane.XAxis.Scale.Max = 2;
             pane.YAxis.Scale.Min = -5;
             pane.YAxis.Scale.Max = 20;
        

             label2.Text = @"Emin(" + (outcount+1) + @")=" + outE + equation+tempsko;
             if (Convert.ToDouble(tempsko)*100 < Convert.ToDouble(outE))
                 label3.Text = outp1;
             if (Convert.ToDouble(tempsko)  > Convert.ToDouble(outE))
                 label3.Text = outp2;
             zedGraph1.AxisChange();
             zedGraph1.Invalidate();                       
         }
#endregion
         private void button1_Click(object sender, EventArgs e)
         {
             dataGridView1.Rows.Clear();
                 
            zedGraph1.Visible = true;
            DrawGraph();
            
         }

         private void Form1_Load(object sender, EventArgs e)
         {
             comboBox1.Text = @"10";
         }

    }
}
