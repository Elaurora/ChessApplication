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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PawnPromotionWindow : Window
    {
        public event Action<pieceType> promotedToPiece;

        pieceColour colour;

        public PawnPromotionWindow()
        {
            InitializeComponent();
        }

        private void SelectPromotion(object sender, RoutedEventArgs e)
        {
            Button Sender = (System.Windows.Controls.Button)sender;
            string squarePressedUid = Sender.Uid;

            switch (squarePressedUid)
            {
                case("NewQueen"):
                    promoteToQueen();
                    break;
                case("NewRook"):
                    promoteToRook();
                    break;
                case("NewBishop"):
                    promoteToBishop();
                    break;
                case("NewKnight"):
                    promoteToKnight();
                    break;
                default:
                    dontPromote();
                    break;
            }

            this.Close();
        }

        void promoteToQueen(){
            promotedToPiece(pieceType.queen);
        }

        void promoteToRook()
        {
            promotedToPiece(pieceType.rook);
        }

        void promoteToBishop()
        {
            promotedToPiece(pieceType.bishop);
        }

        void promoteToKnight()
        {
            promotedToPiece(pieceType.knight);
        }

        void dontPromote()
        {
            promotedToPiece(pieceType.pawn);
        }

        
        public void setPromotionPieceColours(pieceColour colour)
        {
            this.colour = colour;

            this.NewQueen.Content = new Image
            {
                Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(pieceType.queen, colour), UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.NewRook.Content = new Image
            {
                Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(pieceType.rook, colour), UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.NewBishop.Content = new Image
            {
                Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(pieceType.bishop, colour), UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.NewKnight.Content = new Image
            {
                Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(pieceType.knight, colour), UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.OldPawn.Content = new Image
            {
                Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(pieceType.pawn, colour), UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

        }
    }
}
