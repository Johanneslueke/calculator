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

namespace Calculater_eXtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringBuilder Expression;

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
                    Expression.Append(",");
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
                    Expression.Append("tau");
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
                        var ArithParser = (new Parser(Expression.ToString()));
                        Output.Text = ArithParser.EVAL().ToString();
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
            }
                
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
