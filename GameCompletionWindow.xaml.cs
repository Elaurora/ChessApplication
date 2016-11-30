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
using System.Windows.Shapes;

using ChessClasses;

namespace ChessApp
{
    /// <summary>
    /// Interaction logic for GameCompletionWindow.xaml
    /// </summary>
    public partial class GameCompletionWindow : Window
    {
        private static string whiteWonText = "WHITE WINS";
        private static string blackWonText = "BLACK WINS";
        private static string drawText = "DRAW";
        public GameCompletionWindow()
        {
            InitializeComponent();
        }

        private void pressPlayAgain(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        public void setGameOutcomeText(gameState state)
        {
            switch (state)
            {
                case(gameState.darkWin):
                    this.Text.Text = blackWonText;
                    break;
                case(gameState.lightWin):
                    this.Text.Text = whiteWonText;
                    break;
                default:
                    this.Text.Text = drawText;
                    break;
            }
        }
        
       
    }
}
