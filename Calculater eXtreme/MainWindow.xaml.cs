using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Calculater_eXtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringBuilder Expression;
       // private Interpreter Lisp = new Interpreter();
        private Parser ArithParser = new Parser("");
        private bool hasConvert = false;

        public bool Radian
        {
            get => hasConvert;
            set => hasConvert = value;
        }

        private void ConcatinateMathTerm(object sender) { 
            Button sen = sender as Button;
            if (sen == null)
                throw new Exception(); 

            switch(sen.Name){

                case "ButtonUndo":
                    if(Expression.Length > 0)
                        Expression.Remove(Expression.Length-1, 1);
                    Output.Text = Expression.ToString();
                    break;
                case "Button1":
                    Expression.Append("1");
                    Output.Text = Expression.ToString();
                    break;
                case "Button2":
                    Expression.Append("2");
                    Output.Text = Expression.ToString();
                    break;
                case "Button3":
                    Expression.Append("3");
                    Output.Text = Expression.ToString();
                    break;
                case "Button4":
                    Expression.Append("4");
                    Output.Text = Expression.ToString();
                    break;
                case "Button5":
                    Expression.Append("5");
                    Output.Text = Expression.ToString();
                    break;
                case "Button6":
                    Expression.Append("6");
                    Output.Text = Expression.ToString();
                    break;
                case "Button7":
                    Expression.Append("7");
                    Output.Text = Expression.ToString();
                    break;
                case "Button8":
                    Expression.Append("8");
                    Output.Text = Expression.ToString();
                    break;
                case "Button9":
                    Expression.Append("9");
                    Output.Text = Expression.ToString();
                    break;
                case "Button0":
                    Expression.Append("0");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonComma":
                    Expression.Append(".");
                    Output.Text = Expression.ToString();
                    break;

                case "ButtonLParenthis":
                    Expression.Append("(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonRParenthis":
                    Expression.Append(")");
                    Output.Text = Expression.ToString();
                    break;

                case "ButtonAdd":
                    Expression.Append("+");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonSub":
                    Expression.Append("-");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonDiv":
                    Expression.Append("/");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonMul":
                    Expression.Append("*");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonMod":
                    Expression.Append("%");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonPow":
                    Expression.Append("^(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonPow2":
                    Expression.Append("^(2)");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonPow3":
                    Expression.Append("^(3)");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonSqrt":
                    Expression.Append("sqrt(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonPi":
                    Expression.Append("pi");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonTau":
                    Expression.Append("tau(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonSin":
                    Expression.Append("sin(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonCos":
                    Expression.Append("cos(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonSinH":
                    Expression.Append("sinh(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonCosH":
                    Expression.Append("cosh(");
                    Output.Text = Expression.ToString();
                    break;

                case "ButtonTan":
                    Expression.Append("tan(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonTanH":
                    Expression.Append("tanh(");
                    Output.Text = Expression.ToString();
                    break;

                case "ButtonLN":
                    Expression.Append("ln(");
                    Output.Text = Expression.ToString();
                    break;
                case "ButtonLog":
                    Expression.Append("log(");
                    Output.Text = Expression.ToString();
                    break;
                case "Button10":
                    Expression.Append("10^(");
                    Output.Text = Expression.ToString();
                    break;
            }
 
        }

        public MainWindow()
        {
            InitializeComponent();
            Expression = new StringBuilder();

           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConcatinateMathTerm(sender);

            if (sender == ButtonReturn)
            {
                if(Expression.Length > 0)
                {
                    try
                    {
                       
                        Output.Text = ArithParser.EvaluateExpression(Expression.ToString()).ToString();
                        OutputAst.Text = JsonHelper.FormatJson(ArithParser.ast);
                    }
                    catch(Exception error)
                    {
                        Console.WriteLine(error.ToString());
                        Output.Text = "";
                    }
                   
                    Expression.Clear();
                    Expression.Insert(0, Output.Text);
                }
               
            }else if(sender == ButtonClear)
            {
                Expression.Clear();
                Output.Text = Expression.ToString();
            }else if(sender == ButtonConv)
            {
                if (!Radian)
                {
                    double value = double.Parse(Output.Text);
                    value = (value * 180) / Math.PI;
                    Output.Text = value.ToString();
                    hasConvert = true;
                }
                else
                {
                    double value = double.Parse(Output.Text);
                    value = (value * Math.PI ) /  180;
                    Output.Text = value.ToString();
                    hasConvert = false;
                }
            }
                
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItem_Click_Compile(object sender, RoutedEventArgs e)
        {
            Expression.Append(Script.ToString().Substring(32));

            try
            {
                Output.Text = ArithParser.EvaluateExpression(Expression.ToString()).ToString();
            }catch(Exception error)
            {
                Console.WriteLine(error.ToString());
                Output.Text = "";
            }
            Expression.Clear();
        }

        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
           
        }



        private void MenuItem_Click_Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = "d:\\",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == true)
            {
                String script =  File.ReadAllText(dialog.FileName);
                Script.Text = script;
            }
        }

        private void RADIAN_Checked(object sender, RoutedEventArgs e)
        {
            Radian = true;
        }

        private void DEGREE_Checked(object sender, RoutedEventArgs e)
        {
            Radian = false;
        }
    }
}
