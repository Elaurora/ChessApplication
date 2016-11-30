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

using ChessClasses;

namespace ChessApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        private Board gameBoard;

        private pieceColour turn;

        private pieceColour perspective = pieceColour.light;

        private ChessPiece liftedPiece;

        private BoardLocation liftedFrom;

        private ChessMove lastMove;

        public MainWindow()
        {
            InitializeComponent();
            initializeBoardObjects();
            
            
        }

        /// <summary>
        /// Function that is called by every button on the MainWindow representing a board square.
        /// </summary>
        /// <param name="sender">Button object of the button that was pressed</param>
        /// <param name="e"></param>
        private void Square_Pressed(object sender, RoutedEventArgs e)
        {
            Button Sender = (System.Windows.Controls.Button)sender;
            string squarePressedUid = Sender.Uid;
            BoardLocation locationPressed;

            locationPressed = new BoardLocation(squarePressedUid[0], squarePressedUid[1]);

            if (turn == pieceColour.dark)
            {
                locationPressed.rotate180Degrees();
            }

            BoardSquare squarePressed = gameBoard.getSquare(locationPressed);

            if (this.liftedPiece == null)
            {
                attemptToLiftPiece(squarePressed);
            }
            else if (this.liftedPiece != null)
            {
                attemptToPlacePiece(squarePressed);
            }
        }

        /// <summary>
        /// Attemps to lift the piece from the given square as long as it is that players turn
        /// </summary>
        /// <param name="?"></param>
        private void attemptToLiftPiece(BoardSquare square)
        {
            ChessPiece pieceToLift = square.getPiece();

            if (pieceToLift == null || pieceToLift.getColour() != this.turn)
            {
                return;
            }

            this.liftedPiece = square.liftPiece();

            this.liftedFrom = square.getLocation();

            setCursorToLiftedPiece();


            
        }

        private void attemptToPlacePiece(BoardSquare square)
        {
            ChessMove attemptedMove = new ChessMove(liftedFrom, square.getLocation(), liftedPiece);

            if (square.getLocation() == this.liftedFrom || !gameBoard.getState().isLegalMove(attemptedMove, lastMove))
            { // If the piece is getting placed back to where it was lifted from just put it back, or if the move is illegal
                placePieceBack();
                return;
            }


            if (checkForAndApplyPawnPromotion(attemptedMove)) { return; }
            checkForAndApplyenPassant(attemptedMove);
            gameBoard.getSquare(square.getLocation()).placePiece(this.liftedPiece);
            setCursorToDefault();
            checkForAndApplyCastle(attemptedMove);
            forceUIUpdate();
            updateCastleingPossibilities();

            this.liftedPiece = null;
            this.liftedFrom = null;

            nextTurn();

            
            gameState currentState = this.gameBoard.getState().calculateGameState(this.turn, lastMove);
            if (currentState == gameState.lightWin)
            {
                gameCompleteAndRestart(currentState);
                return;
            }
            else if (currentState == gameState.darkWin)
            {
                gameCompleteAndRestart(currentState);
                return;
            }
            else if (currentState == gameState.draw)
            {
                gameCompleteAndRestart(currentState);
                return;
            }

            lastMove = attemptedMove;
        }

        /// <summary>
        /// Opens the pawn promotion window and return what piece the user chose to promote his pawn to. Does not return
        /// until the pawn promotion window is closed.
        /// </summary>
        /// <param name="colour">The colour of the pawn being promoted</param>
        /// <returns>What the pawn will be promoted to, or a pawn if there will be no promotion</returns>
        private pieceType pawnPromotion(pieceColour colour)
        {
            pieceType returned = pieceType.pawn;

            PawnPromotionWindow pawnPromotionWindow = new PawnPromotionWindow();

            pawnPromotionWindow.setPromotionPieceColours(colour);

            pawnPromotionWindow.promotedToPiece += value => returned = value;

            pawnPromotionWindow.ShowDialog();

            pawnPromotionWindow.Close();

            return returned;
        }

        private void gameCompleteAndRestart(gameState currentState){

            GameCompletionWindow completionWindow = new GameCompletionWindow();

            completionWindow.setGameOutcomeText(currentState);

            completionWindow.ShowDialog();

            resetBoardToInitialState();

        }

        /// <summary>
        /// Places the lifted piece back to where is was lifted from.
        /// </summary>
        private void placePieceBack()
        {
            if (this.liftedPiece != null && this.liftedFrom != null)
            {
                setCursorToDefault();
                gameBoard.getSquare(this.liftedFrom).placePiece(this.liftedPiece);
                this.liftedPiece = null;
                this.liftedFrom = null;
            }
        }

        /// <summary>
        /// Changes the cursor to match the currently lifted piece, as long as a piece has been lifted
        /// </summary>
        private void setCursorToLiftedPiece()
        {
            if (this.liftedPiece != null)
            {
                System.Windows.Resources.StreamResourceInfo pieceCURStream = Application.GetResourceStream(
                    new Uri(FileLocations.getCURLocation(this.liftedPiece.getType(), this.liftedPiece.getColour()), UriKind.Relative));

                this.Cursor = new System.Windows.Input.Cursor(pieceCURStream.Stream);
            }
        }

        /// <summary>
        /// Sets the cursor back to the default mouse pointer
        /// </summary>
        private void setCursorToDefault()
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        /// <summary>
        /// Initializes the objects needed to play the game. The following objects are initialized:
        /// this.gameBoard, this.liftedPiece, this.liftedFrom, 
        /// Sets the perspective to lights perspective
        /// Places piece to their initial location
        /// </summary>
        private void initializeBoardObjects()
        {
            this.gameBoard = new Board();
            this.liftedPiece = null;
            this.liftedFrom = null;
            setPerspectiveLight();
            setInitialBoardState();

        }

        /// <summary>
        /// Sets all pieces to their initial locations on the board and clears all other locations
        /// </summary>
        private void setInitialBoardState()
        {
            this.gameBoard.getSquare(InitialPieceLocations.leftRookInitialPosition(pieceColour.light)).placePiece(new Rook(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.leftKnightInitialPosition(pieceColour.light)).placePiece(new Knight(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.leftBishopInitialPosition(pieceColour.light)).placePiece(new Bishop(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.queenInitialPosition(pieceColour.light)).placePiece(new Queen(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.kingInitialPosition(pieceColour.light)).placePiece(new King(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.rightBishopInitialPosition(pieceColour.light)).placePiece(new Bishop(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.rightKnightInitialPosition(pieceColour.light)).placePiece(new Knight(pieceColour.light));
            this.gameBoard.getSquare(InitialPieceLocations.rightRookInitialPosition(pieceColour.light)).placePiece(new Rook(pieceColour.light));

            this.gameBoard.getSquare(StaticBoardLocations.A2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.B2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.C2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.D2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.E2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.F2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.G2).placePiece(new Pawn(pieceColour.light));
            this.gameBoard.getSquare(StaticBoardLocations.H2).placePiece(new Pawn(pieceColour.light));

            this.gameBoard.getSquare(StaticBoardLocations.A3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.B3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.C3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.D3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.E3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.F3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.G3).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.H3).placePiece(null);

            this.gameBoard.getSquare(StaticBoardLocations.A4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.B4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.C4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.D4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.E4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.F4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.G4).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.H4).placePiece(null);

            this.gameBoard.getSquare(StaticBoardLocations.A5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.B5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.C5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.D5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.E5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.F5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.G5).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.H5).placePiece(null);

            this.gameBoard.getSquare(StaticBoardLocations.A6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.B6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.C6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.D6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.E6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.F6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.G6).placePiece(null);
            this.gameBoard.getSquare(StaticBoardLocations.H6).placePiece(null);

            this.gameBoard.getSquare(StaticBoardLocations.A7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.B7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.C7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.D7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.E7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.F7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.G7).placePiece(new Pawn(pieceColour.dark));
            this.gameBoard.getSquare(StaticBoardLocations.H7).placePiece(new Pawn(pieceColour.dark));

            this.gameBoard.getSquare(InitialPieceLocations.leftRookInitialPosition(pieceColour.dark)).placePiece(new Rook(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.leftKnightInitialPosition(pieceColour.dark)).placePiece(new Knight(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.leftBishopInitialPosition(pieceColour.dark)).placePiece(new Bishop(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.queenInitialPosition(pieceColour.dark)).placePiece(new Queen(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.kingInitialPosition(pieceColour.dark)).placePiece(new King(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.rightBishopInitialPosition(pieceColour.dark)).placePiece(new Bishop(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.rightKnightInitialPosition(pieceColour.dark)).placePiece(new Knight(pieceColour.dark));
            this.gameBoard.getSquare(InitialPieceLocations.rightRookInitialPosition(pieceColour.dark)).placePiece(new Rook(pieceColour.dark));

            this.turn = pieceColour.light;
        }

        /// <summary>
        /// Checks if the attemptedMove is a pawn reaching the end rank. If so, opens a pawnPromotion window that
        /// prompts the player to choose a piece to promote the pawn to. If the player chooses to promote to a pawn
        /// his move is disallowed as a pawn cannot be promoted to a pawn on the final rank
        /// </summary>
        /// <param name="attemptedMove"></param>
        /// <returns>True if a pawn was promoted, false otherwise.</returns>
        private bool checkForAndApplyPawnPromotion(ChessMove attemptedMove)
        {
            if (attemptedMove.piece.getType() == pieceType.pawn)
            {
                if (attemptedMove.final.rank() == '8')
                {
                    pieceType promotedTo = pawnPromotion(pieceColour.light);
                    if (promotedTo == pieceType.pawn)
                    {
                        placePieceBack();
                        return true;
                    }
                    this.liftedPiece = ChessPiece.newChessPiece(promotedTo, pieceColour.light);
                }
                else if (attemptedMove.final.rank() == '1')
                {
                    pieceType promotedTo = pawnPromotion(pieceColour.dark);
                    if (promotedTo == pieceType.pawn)
                    {
                        placePieceBack();
                        return true;
                    }
                    this.liftedPiece = ChessPiece.newChessPiece(promotedTo, pieceColour.dark);
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the attemptedMove was en-passant. If so, "captures" (removes from board) the "passed" pawn
        /// </summary>
        /// <param name="attemptedMove"></param>
        private void checkForAndApplyenPassant(ChessMove attemptedMove)
        {
            if (lastMove != null && lastMove.piece.getType() == pieceType.pawn
                && attemptedMove.piece.getType() == pieceType.pawn
                && this.gameBoard.getSquare(attemptedMove.final).getPiece() == null
                && attemptedMove.final.file() != attemptedMove.init.file())//Confirms an en-passant
            {
                if (lastMove.piece.getColour() == pieceColour.light)
                {
                    gameBoard.getSquare(new BoardLocation(lastMove.final.file(), lastMove.final.rank())).placePiece(null);
                }
                else
                {
                    gameBoard.getSquare(new BoardLocation(lastMove.final.file(), lastMove.final.rank())).placePiece(null);
                }
            }
        }

        /// <summary>
        /// Checks to see if the attemptedMove was a castle. If it is, moves the rook accordingly
        /// </summary>
        /// <param name="attemptedMove">The move made</param>
        private void checkForAndApplyCastle(ChessMove attemptedMove)
        {
            if (liftedPiece.getType() == pieceType.king)
            {
                if (liftedFrom == InitialPieceLocations.kingInitialPosition(attemptedMove.piece.getColour()))
                {

                    if (attemptedMove.final == InitialPieceLocations.rightKnightInitialPosition(attemptedMove.piece.getColour()))//castling kingside
                    {
                        ChessPiece LiftedRook = gameBoard.getSquare(InitialPieceLocations.rightRookInitialPosition(attemptedMove.piece.getColour())).liftPiece();
                        gameBoard.getSquare(InitialPieceLocations.rightBishopInitialPosition(attemptedMove.piece.getColour())).placePiece(LiftedRook);
                    }
                    else if (attemptedMove.final == InitialPieceLocations.leftBishopInitialPosition(attemptedMove.piece.getColour()))//castling queenside
                    {
                        ChessPiece LiftedRook = gameBoard.getSquare(InitialPieceLocations.leftRookInitialPosition(attemptedMove.piece.getColour())).liftPiece();
                        gameBoard.getSquare(InitialPieceLocations.queenInitialPosition(attemptedMove.piece.getColour())).placePiece(LiftedRook);
                    }
                }

            }
        }

        /// <summary>
        /// Checks to see if the move made violates any of the players castling possibilities, and updates the 
        /// this.gameBoard accordingly
        /// </summary>
        private void updateCastleingPossibilities()
        {
            
            if (liftedPiece.getType() == pieceType.rook)
            {
                if (liftedFrom == InitialPieceLocations.rightRookInitialPosition(liftedPiece.getColour()))
                {
                    if (liftedPiece.getColour() == pieceColour.light)
                    {
                        gameBoard.setCanLightCastleKingSide(false);
                    }
                    else
                    {
                        gameBoard.setCanDarkCastleKingSide(false);
                    }
                }
                else if (liftedFrom == InitialPieceLocations.leftRookInitialPosition(liftedPiece.getColour()))
                {
                    if (liftedPiece.getColour() == pieceColour.light)
                    {
                        gameBoard.setCanLightCastleQueenSide(false);
                    }
                        
                    else
                    {
                        gameBoard.setCanDarkCastleQueenSide(false);
                    }

                }
            }
            else if (liftedPiece.getType() == pieceType.king)
            {
                if (liftedFrom == InitialPieceLocations.kingInitialPosition(liftedPiece.getColour()))
                {
                    if (liftedPiece.getColour() == pieceColour.light)
                    {
                        gameBoard.setCanLightCastleKingSide(false);
                        gameBoard.setCanLightCastleQueenSide(false);
                    }
                    else
                    {
                        gameBoard.setCanDarkCastleKingSide(false);
                        gameBoard.setCanDarkCastleQueenSide(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// Has the main threat wait 200 milliseconds, then changes whos turn it is and changes the perspective to
        /// the new players turn
        /// </summary>
        private void nextTurn()
        {
            if (this.turn == pieceColour.light)
            {
                this.turn = pieceColour.dark;
                setPerspective(pieceColour.dark);
            }
            else
            {
                this.turn = pieceColour.light;
                setPerspective(pieceColour.light);
            }
        }

        /// <summary>
        /// Resets the board to its initial state. Restarts the game.
        /// </summary>
        private void resetBoardToInitialState()
        {
            initializeBoardObjects();
            turn = pieceColour.light;
        }

        /// <summary>
        /// Sets the perspective to that of the specified colour 
        /// If the perspective is changed, it has the same effect as rotating the board 180 degrees
        /// </summary>
        /// <param name="colour">The perspective to set to</param>
        private void setPerspective(pieceColour colour)
        {
            if (colour == pieceColour.light)
            {
                setPerspectiveLight();
            }
            else
            {
                setPerspectiveDark();
            }
        }

        /// <summary>
        /// Links the BoardSquares to the appropriate buttons to represent lights view of the game.
        /// </summary>
        private void setPerspectiveLight()
        {
            this.gameBoard.getSquare(StaticBoardLocations.A1).LinkToButton(A1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B1).LinkToButton(B1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C1).LinkToButton(C1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D1).LinkToButton(D1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E1).LinkToButton(E1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F1).LinkToButton(F1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G1).LinkToButton(G1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H1).LinkToButton(H1_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A2).LinkToButton(A2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B2).LinkToButton(B2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C2).LinkToButton(C2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D2).LinkToButton(D2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E2).LinkToButton(E2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F2).LinkToButton(F2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G2).LinkToButton(G2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H2).LinkToButton(H2_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A3).LinkToButton(A3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B3).LinkToButton(B3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C3).LinkToButton(C3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D3).LinkToButton(D3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E3).LinkToButton(E3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F3).LinkToButton(F3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G3).LinkToButton(G3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H3).LinkToButton(H3_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A4).LinkToButton(A4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B4).LinkToButton(B4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C4).LinkToButton(C4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D4).LinkToButton(D4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E4).LinkToButton(E4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F4).LinkToButton(F4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G4).LinkToButton(G4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H4).LinkToButton(H4_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A5).LinkToButton(A5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B5).LinkToButton(B5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C5).LinkToButton(C5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D5).LinkToButton(D5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E5).LinkToButton(E5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F5).LinkToButton(F5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G5).LinkToButton(G5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H5).LinkToButton(H5_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A6).LinkToButton(A6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B6).LinkToButton(B6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C6).LinkToButton(C6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D6).LinkToButton(D6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E6).LinkToButton(E6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F6).LinkToButton(F6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G6).LinkToButton(G6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H6).LinkToButton(H6_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A7).LinkToButton(A7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B7).LinkToButton(B7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C7).LinkToButton(C7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D7).LinkToButton(D7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E7).LinkToButton(E7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F7).LinkToButton(F7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G7).LinkToButton(G7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H7).LinkToButton(H7_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A8).LinkToButton(A8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B8).LinkToButton(B8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C8).LinkToButton(C8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D8).LinkToButton(D8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E8).LinkToButton(E8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F8).LinkToButton(F8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G8).LinkToButton(G8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H8).LinkToButton(H8_Button);

        }

        /// <summary>
        /// Links the BoardSquares to the appropriate buttons to represent darks view of the game.
        /// </summary>
        private void setPerspectiveDark()
        {
            
            this.gameBoard.getSquare(StaticBoardLocations.A1).LinkToButton(H8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B1).LinkToButton(G8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C1).LinkToButton(F8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D1).LinkToButton(E8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E1).LinkToButton(D8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F1).LinkToButton(C8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G1).LinkToButton(B8_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H1).LinkToButton(A8_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A2).LinkToButton(H7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B2).LinkToButton(G7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C2).LinkToButton(F7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D2).LinkToButton(E7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E2).LinkToButton(D7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F2).LinkToButton(C7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G2).LinkToButton(B7_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H2).LinkToButton(A7_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A3).LinkToButton(H6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B3).LinkToButton(G6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C3).LinkToButton(F6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D3).LinkToButton(E6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E3).LinkToButton(D6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F3).LinkToButton(C6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G3).LinkToButton(B6_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H3).LinkToButton(A6_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A4).LinkToButton(H5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B4).LinkToButton(G5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C4).LinkToButton(F5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D4).LinkToButton(E5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E4).LinkToButton(D5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F4).LinkToButton(C5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G4).LinkToButton(B5_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H4).LinkToButton(A5_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A5).LinkToButton(H4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B5).LinkToButton(G4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C5).LinkToButton(F4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D5).LinkToButton(E4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E5).LinkToButton(D4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F5).LinkToButton(C4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G5).LinkToButton(B4_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H5).LinkToButton(A4_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A6).LinkToButton(H3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B6).LinkToButton(G3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C6).LinkToButton(F3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D6).LinkToButton(E3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E6).LinkToButton(D3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F6).LinkToButton(C3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G6).LinkToButton(B3_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H6).LinkToButton(A3_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A7).LinkToButton(H2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B7).LinkToButton(G2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C7).LinkToButton(F2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D7).LinkToButton(E2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E7).LinkToButton(D2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F7).LinkToButton(C2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G7).LinkToButton(B2_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H7).LinkToButton(A2_Button);

            this.gameBoard.getSquare(StaticBoardLocations.A8).LinkToButton(H1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.B8).LinkToButton(G1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.C8).LinkToButton(F1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.D8).LinkToButton(E1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.E8).LinkToButton(D1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.F8).LinkToButton(C1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.G8).LinkToButton(B1_Button);
            this.gameBoard.getSquare(StaticBoardLocations.H8).LinkToButton(A1_Button);
        }

        /// <summary>
        /// Forces the UI to update itself when this functions is called
        /// The current Thread is blocked until the update has completed
        /// </summary>
        private void forceUIUpdate()
        {
            this.Dispatcher.Invoke(delegate { }, System.Windows.Threading.DispatcherPriority.Render);
        }


    }

    
    
}
