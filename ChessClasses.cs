using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using ChessApp;

namespace ChessClasses
{

    public enum pieceType { pawn, bishop, knight, rook, queen, king };
    public enum pieceColour { light, dark };
    public enum gameState { lightWin, darkWin, draw, unfinished};

    class IllegalBoardLocationException : Exception
    {
        public IllegalBoardLocationException(string message)
            : base(message)
        {

        }

        public IllegalBoardLocationException()
            : base()
        {}
    }

    class ChessMove
    {
        public BoardLocation init;
        public BoardLocation final;
        public ChessPiece piece;

        public ChessMove(BoardLocation init, BoardLocation final, ChessPiece piece)
        {
            this.init = init;
            this.final = final;
            this.piece = piece;
        }
    }

    /// <summary>
    /// Represents a specific location on the board
    /// </summary>
    class BoardLocation
    {
        /// <summary>
        /// This identifies which file (a.k.a columns, vertical) this square is on
        /// value is 'A' through 'H'
        /// </summary>
        private char File;

        /// <summary>
        /// This identifies which rank (a.k.a rows, horizontal) this square is on
        /// value is '1' through '8'
        /// </summary>
        private char Rank;

        public BoardLocation(char file, char rank)
        {
            
            if (file < 'A' || file > 'H'
                || rank < '1' || rank > '8')
            {
                throw new IllegalBoardLocationException();
            }
            this.File = file;
            this.Rank = rank;
        }

        public char file()
        {
            return this.File;
        }

        public char rank()
        {
            return this.Rank;
        }

        /// <summary>
        /// Changes this board location object to represent the location that the initial location would represent were the 
        /// board rotated 180 degrees
        /// </summary>
        public void rotate180Degrees()
        {
            this.File = (char)( 'A' + ('H' - this.File));
            this.Rank = (char)( '1' +  ( '8' - this.Rank));
        }

        public static bool operator ==(BoardLocation left, BoardLocation right)
        {
            bool leftIsNull = Object.ReferenceEquals(left, null);
            bool rightIsNull = Object.ReferenceEquals(right, null);
            if (leftIsNull && rightIsNull) { return true; }
            if (leftIsNull || rightIsNull) { return false; }
            if (left.file() == right.file() && left.rank() == right.rank()) { return true; }
            else { return false; }
        }

        public static bool operator !=(BoardLocation left, BoardLocation right)
        {
            bool leftIsNull = Object.ReferenceEquals(left, null);
            bool rightIsNull = Object.ReferenceEquals(right, null);
            if (leftIsNull && rightIsNull) { return false; }
            if (leftIsNull || rightIsNull) { return true; }
            if (left.file() == right.file() && left.rank() == right.rank()) { return false; }
            else { return true; }
        }

        public override bool Equals(object o)
        {
            if (o == null) { return false; }
            BoardLocation objAsBoardLocation = o as BoardLocation;
            if (objAsBoardLocation == null) { return false; }
            else { return this.Equals(objAsBoardLocation); }
        }

        public bool Equals(BoardLocation location)
        {
            return this == location;
        }
    }

    /// <summary>
    /// Represents a specific square on the board
    /// </summary>
    class BoardSquare
    {

        /// <summary>
        /// 
        /// </summary>
        private BoardLocation location;

        /// <summary>
        /// The piece currently placed on this square. If no piece is present the value will be null
        /// </summary>
        private ChessPiece piece;

        /// <summary>
        /// Button object that this Square is linked to
        /// </summary>
        private Button squareButton;

        public BoardSquare(BoardLocation location)
        {
            this.location = location;
            this.piece = null;
        }

        /// <summary>
        /// Links the button to this class so the button may add content to it
        /// </summary>
        /// <param name="squareButton">The button to link the square to</param>
        public void LinkToButton(Button squareButton)
        {
            this.squareButton = squareButton;
            setPicture();
        }

        /// <summary>
        /// Places the given chess piece onto this board square and sets the content of the button it it linked to to
        /// refect the piece being placed there
        /// </summary>
        /// <param name="piece">The piece being placed</param>
        public void placePiece(ChessPiece piece)
        {
            this.piece = piece;
            this.setPicture();
        }

        /// <summary>
        /// Removes the piece on the BoardSquare and sets the content on the button it is linked
        /// to back to null.
        /// </summary>
        /// <returns>The ChessPiece that was on this square2</returns>
        public ChessPiece liftPiece()
        {
            ChessPiece returned = this.piece;
            this.piece = null;
            this.removePicture();
            return returned;
        }


        /// <returns>The chess piece on this Square</returns>
        public ChessPiece getPiece()
        {
            return this.piece;
        }

        /// <summary>
        /// Sets the picture on the button linked to this Square to refect the piece placed on it.
        /// If there is no piece on this Square it does nothing
        /// </summary>
        public void setPicture()
        {
            if (this.piece != null)
            {
                squareButton.Content = new Image
                {
                    Source = new BitmapImage(new Uri(FileLocations.getPNGLocation(this.piece.getType(), this.piece.getColour()), UriKind.Relative)),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }
            else
            {
                removePicture();
            }
        }

        /// <summary>
        /// Removes any picture on the button linked with this square
        /// </summary>
        private void removePicture()
        {
            squareButton.Content = null;
        }

        /// <returns>The location on the board this Square corresponds to</returns>
        public BoardLocation getLocation()
        {
            return this.location;
        }
    }

    /// <summary>
    /// Represents the entire board of a game.
    /// </summary>
    class Board
    {
        /// <summary>
        /// First index indicates rank, second index indicates file
        /// </summary>
        private BoardSquare[][] board;

        private bool darkCastleKingSide;
        private bool darkCastleQueenSide;
        private bool lightCastleKingSide;
        private bool lightCastleQueenSide;

        /// <summary>
        /// Initializes all the boards squares
        /// </summary>
        public Board()
        {

            this.board = new BoardSquare[8][];
            for (int i = 0; i != 8; i++)
            {

                this.board[i] = new BoardSquare[8];
                for (int j = 0; j != 8; j++)
                {
                    this.board[i][j] = new BoardSquare(new BoardLocation((char)((int)'A' + j), (char)((int)'1' + i)));
                }
            }

            darkCastleKingSide = true;
            darkCastleQueenSide = true;
            lightCastleKingSide = true;
            lightCastleQueenSide = true;

        }

        /// <summary>
        /// Returns a boardSquare object corresponding to the given location
        /// </summary>
        /// <param name="rank">Indicates which rank you want</param>
        /// <param name="file">Indicates which file you want</param>
        /// <returns>The BoardSquare object corresponding to the given rank and file</returns>
        public BoardSquare getSquare(BoardLocation location)
        {
            return this.board[(location.rank() - '1')][(location.file() - 'A')];
        }

        public BoardState getState()
        {
            return new BoardState(this);
        }

        public bool canDarkCastleKingSide()
        {
            return this.darkCastleKingSide;
        }
        public bool canDarkCastleQueenSide()
        {
            return this.darkCastleQueenSide;
        }
        public bool canLightCastleKingSide()
        {
            return this.lightCastleKingSide;
        }
        public bool canLightCastleQueenSide()
        {
            return this.lightCastleQueenSide;
        }
        public void setCanDarkCastleKingSide(bool Bool)
        {
            this.darkCastleKingSide = Bool;
        }
        public void setCanDarkCastleQueenSide(bool Bool)
        {
            this.darkCastleQueenSide = Bool;
        }
        public void setCanLightCastleKingSide(bool Bool)
        {
            this.lightCastleKingSide = Bool;
        }
        public void setCanLightCastleQueenSide(bool Bool)
        {
            this.lightCastleQueenSide = Bool;
        }
    }

    /// <summary>
    /// Hold information about the type and colour of a piece on a square
    /// </summary>
    class SquareState
    {
        public pieceType type;
        public pieceColour colour;

        public SquareState(pieceType type, pieceColour colour)
        {
            this.type = type;
            this.colour = colour;
        }

        public SquareState(SquareState copyFrom)
        {
            this.type = copyFrom.type;
            this.colour = copyFrom.colour;
        }
    }

    /// <summary>
    /// information about 
    /// </summary>
    class BoardState
    {
        
        /// <summary>
        /// state[rank][file] 
        /// Null values represent squares with no piece on them
        /// </summary>
        private SquareState[][] state;

        private List<BoardLocation> locationsThreatenedByLight = null;
        private List<BoardLocation> locationsThreatenedByDark = null;

        private bool darkCastleKingSide;
        private bool darkCastleQueenSide;
        private bool lightCastleKingSide;
        private bool lightCastleQueenSide;

        public BoardState(Board board)
        {
            this.state = new SquareState[8][];

            for (int i = 0; i != 8; i++)
            {
                this.state[i] = new SquareState[8];
                for (int j = 0; j != 8; j++)
                {
                    ChessPiece pieceAt = board.getSquare(new BoardLocation((char)('A' + j), (char)('1' + i))).getPiece();
                    if (pieceAt != null)
                    {
                        this.state[i][j] = new SquareState(pieceAt.getType(), pieceAt.getColour());
                    }
                    else
                    {
                        this.state[i][j] = null;
                    }
                    
                }
            }
            this.darkCastleKingSide = board.canDarkCastleKingSide();
            this.darkCastleQueenSide = board.canDarkCastleQueenSide();
            this.lightCastleKingSide = board.canLightCastleKingSide();
            this.lightCastleQueenSide = board.canLightCastleQueenSide();
        }

        public BoardState(BoardState copyFrom)
        {
            this.state = new SquareState[8][];

            for (int rank = 0; rank != 8; rank++)
            {
                this.state[rank] = new SquareState[8];
                for (int file = 0; file != 8; file++)
                {
                    SquareState oldState = copyFrom.stateAt(new BoardLocation((char)('A' + file), (char)('1' + rank)));
                    if (oldState != null)
                    {
                        this.state[rank][file] = new SquareState(oldState);
                    }
                    else
                    {
                        this.state[rank][file] = oldState;
                    }

                }
            }
            this.darkCastleKingSide = copyFrom.canDarkCastleKingSide();
            this.darkCastleQueenSide = copyFrom.canDarkCastleQueenSide();
            this.lightCastleKingSide = copyFrom.canLightCastleKingSide();
            this.lightCastleQueenSide = copyFrom.canLightCastleQueenSide();
        }

        public SquareState stateAt(BoardLocation location)
        {
            return this.state[(location.rank() - '1')][(location.file() - 'A')];
        }

        public void setStateAt(BoardLocation location, SquareState state)
        {
            this.state[(location.rank() - '1')][(location.file() - 'A')] = state;
            locationsThreatenedByLight = null;
            locationsThreatenedByDark = null;
        }

        /// <summary>
        /// Applies the specified move to the board. This method assumes that the move being
        /// applied has already been checked for legality and therefor will not check itself. 
        /// It is recommended to call the isLehalMove function prior to this.
        /// </summary>
        /// <param name="move">The move to be applied</param>
        private void applyMove(ChessMove move)
        {
            locationsThreatenedByLight = null;
            locationsThreatenedByDark = null;

            if (move.piece.getType() == pieceType.pawn
                && move.init.file() != move.final.file()
                && stateAt(move.final) == null)//Confirms an en-passant
            {
                if (move.piece.getColour() == pieceColour.light)
                {
                    this.setStateAt(new BoardLocation(move.final.file(), (char)(move.final.rank() - 1)), null);
                }
                else
                {
                    this.setStateAt(new BoardLocation(move.final.file(), (char)(move.final.rank() + 1)), null);
                }
            }
            else if (move.piece.getType() == pieceType.king)
            {
                if (move.piece.getColour() == pieceColour.light)
                {
                    if (this.lightCastleKingSide == true
                        && move.init == InitialPieceLocations.kingInitialPosition(pieceColour.light))
                    {
                        if (move.final == InitialPieceLocations.rightKnightInitialPosition(pieceColour.light))// Means the move is a kingside castle by light
                        {
                            this.setStateAt(InitialPieceLocations.rightRookInitialPosition(pieceColour.light), null);
                            this.setStateAt(InitialPieceLocations.rightBishopInitialPosition(pieceColour.light), new SquareState(pieceType.rook, pieceColour.light));
                        }
                    }
                    else if (this.lightCastleQueenSide == true
                        && move.init == InitialPieceLocations.kingInitialPosition(pieceColour.light))
                    {
                        if (move.final == InitialPieceLocations.leftBishopInitialPosition(pieceColour.light))//Means the move is a queenside castle by light
                        {
                            this.setStateAt(InitialPieceLocations.leftRookInitialPosition(pieceColour.light), null);
                            this.setStateAt(InitialPieceLocations.queenInitialPosition(pieceColour.light), new SquareState(pieceType.rook, pieceColour.light));
                        }
                    }
                    this.setCanLightCastleKingSide(false);
                    this.setCanLightCastleQueenSide(false);
                }
                else
                {
                    if (this.darkCastleKingSide == true
                        && move.init == InitialPieceLocations.kingInitialPosition(pieceColour.dark))
                    {
                        if (move.final == InitialPieceLocations.rightKnightInitialPosition(pieceColour.dark))// Means the move is a kingside castle by light
                        {
                            this.setStateAt(InitialPieceLocations.rightRookInitialPosition(pieceColour.dark), null);
                            this.setStateAt(InitialPieceLocations.rightBishopInitialPosition(pieceColour.dark), new SquareState(pieceType.rook, pieceColour.dark));
                        }
                    }
                    else if (this.darkCastleQueenSide == true
                        && move.init == InitialPieceLocations.kingInitialPosition(pieceColour.dark))
                    {
                        if (move.final == InitialPieceLocations.leftBishopInitialPosition(pieceColour.dark))//Means the move is a queenside castle by light
                        {
                            this.setStateAt(InitialPieceLocations.leftRookInitialPosition(pieceColour.dark), null);
                            this.setStateAt(InitialPieceLocations.queenInitialPosition(pieceColour.dark), new SquareState(pieceType.rook, pieceColour.dark));
                        }
                    }
                    this.setCanDarkCastleQueenSide(false);
                    this.setCanDarkCastleKingSide(false);
                }
            }
            else if (move.piece.getType() == pieceType.rook)
            {
                if (move.piece.getColour() == pieceColour.light)
                {
                    if (move.init == InitialPieceLocations.rightRookInitialPosition(pieceColour.light))
                    {
                        this.setCanLightCastleKingSide(false);
                    }
                    else if (move.init == InitialPieceLocations.leftRookInitialPosition(pieceColour.light))
                    {
                        this.setCanLightCastleQueenSide(false);
                    }
                }
                else
                {
                    if (move.init == InitialPieceLocations.rightRookInitialPosition(pieceColour.dark))
                    {
                        this.setCanDarkCastleKingSide(false);
                    }
                    else if (move.init == InitialPieceLocations.leftRookInitialPosition(pieceColour.dark))
                    {
                        this.setCanDarkCastleQueenSide(false);
                    }
                }
            }

            this.setStateAt(move.init, null);
            this.setStateAt(move.final, new SquareState(move.piece.getType(), move.piece.getColour()));

        }

        /// <summary>
        /// Calculates whether or not the player of the specified colour is in check for this BoardState
        /// </summary>
        /// <param name="colour">The colour in question</param>
        /// <returns>True if they player is in check, false otherwise</returns>
        public bool isInCheck(pieceColour colour)
        {

            BoardLocation kingLocation = this.getKingLocation(colour);
            List<BoardLocation> enemyLocationsThreatened;

            if (colour == pieceColour.light)
            {
                enemyLocationsThreatened = getLocationsThreatened(pieceColour.dark);
            }
            else
            {
                enemyLocationsThreatened = getLocationsThreatened(pieceColour.light);
            }

            if (enemyLocationsThreatened.Exists(x => x == kingLocation))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns all the locations that are threatened by the pieces of the given colour
        /// </summary>
        /// <param name="colour">The colour of the pieces being checked</param>
        /// <returns>A list of BoardLocations threatened by the colour</returns>
        public List<BoardLocation> getLocationsThreatened(pieceColour colour)
        {
            if (colour == pieceColour.light)
            {
                if (locationsThreatenedByLight == null)
                {
                    locationsThreatenedByLight = new List<BoardLocation>();

                    List<BoardLocation> pieceLocations = getPieceLocations(pieceColour.light);

                    foreach (BoardLocation piece in pieceLocations)
                    {
                        locationsThreatenedByLight.AddRange(ChessPiece.getThreatenedLocations(this, piece, this.stateAt(piece).type, pieceColour.light));
                    }
                }

                return locationsThreatenedByLight;
            }
            else
            {
                if (locationsThreatenedByDark == null)
                {
                    locationsThreatenedByDark = new List<BoardLocation>();

                    List<BoardLocation> pieceLocations = getPieceLocations(pieceColour.dark);

                    foreach (BoardLocation piece in pieceLocations)
                    {
                        locationsThreatenedByDark.AddRange(ChessPiece.getThreatenedLocations(this, piece, this.stateAt(piece).type, pieceColour.dark));
                    }
                }

                return locationsThreatenedByDark;
            }
        }

        /// <summary>
        /// Checks to see if a given move is a legal move on this BoardState
        /// </summary>
        /// <param name="thisMove">The move in question</param>
        /// <param name="previousMove">The previously made move, needed for en-passant calculation</param>
        /// <returns>true if the move is legal, false otherwise</returns>
        public bool isLegalMove(ChessMove thisMove, ChessMove previousMove)
        {
            List<BoardLocation> potentialMoves = ChessPiece.getPotentialMoves(this, thisMove.init, previousMove, thisMove.piece.getType(), thisMove.piece.getColour());

            if (!potentialMoves.Exists(x => x == thisMove.final))
            {
                return false;
            }

            BoardState temp = new BoardState(this);

            temp.applyMove(thisMove);

            return !temp.isInCheck(thisMove.piece.getColour());

        }

        /// <summary>
        /// Gets the location of the king of the specified colour on this BoardState
        /// </summary>
        /// <param name="colour">The colour of the king being searched for</param>
        /// <returns>The location of the king of the specified colour</returns>
        private BoardLocation getKingLocation(pieceColour colour)
        {
            if (colour == pieceColour.light)
            {
                for (int rank = 0; rank != 8; rank++)
                {
                    for (int file = 0; file != 8; file++)
                    {
                        SquareState stateAt = this.state[rank][file];
                        if (stateAt != null)
                        {
                            if (stateAt.type == pieceType.king && stateAt.colour == colour)
                            {
                                return new BoardLocation((char)(file + 'A'), (char)(rank + '1'));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int rank = 7; rank != -1; rank--)//Makes more sense to start looking for the black king on the 8th rank first, as he is almost always there (index of 8th rank is 7)
                {
                    for (int file = 0; file != 8; file++)
                    {
                        SquareState stateAt = this.state[rank][file];
                        if (stateAt != null)
                        {
                            if (stateAt.type == pieceType.king && stateAt.colour == colour)
                            {
                                return new BoardLocation((char)(file + 'A'), (char)(rank + '1'));
                            }
                        }
                    }
                }
            }
            throw new System.Exception("No king found on the board, unable to continue");
            //if this point is reached, it was either in error due to a bug or there is no king of the specified colour on the board
        }

        /// <summary>
        /// Gets a list of all pieces of the given colour
        /// </summary>
        /// <param name="colour">The colour requested</param>
        /// <returns>A list of all the locations of pieces of the specified colour on this BoardState</returns>
        public List<BoardLocation> getPieceLocations(pieceColour colour)
        {
            List<BoardLocation> pieceLocations = new List<BoardLocation>();

            for (int rank = 0; rank != 8; rank++)
            {
                for (int file = 0; file != 8; file++)
                {
                    SquareState stateAt = this.state[rank][file];
                    if (stateAt != null && stateAt.colour == colour)
                    {
                        pieceLocations.Add(new BoardLocation((char)(file + 'A'), (char)(rank + '1')));
                    }
                }
            }

            return pieceLocations;
        }

        /// <summary>
        /// Checks to see if the given colour is in checkmate or has no remaining moves.
        /// </summary>
        /// <param name="colour">The colour in question</param>
        /// <param name="previousMove">The previously made move. Needed for en-passant calculation</param>
        /// <returns>The state of the game, either black win, white win, draw or unfinished</returns>
        public gameState calculateGameState(pieceColour colour, ChessMove previousMove)
        {
            List<BoardLocation> pieces = this.getPieceLocations(colour);

            foreach (BoardLocation piece in pieces)
            {
                List<BoardLocation> potentialMoves = ChessPiece.getPotentialMoves(this, piece, previousMove, this.stateAt(piece).type, this.stateAt(piece).colour);
                foreach (BoardLocation move in potentialMoves)
                {
                    ChessMove attemptedMove = new ChessMove(piece, move, ChessPiece.newChessPiece(this.stateAt(piece).type, colour));
                    BoardState temp = new BoardState(this);
                    temp.applyMove(attemptedMove);
                    if (!temp.isInCheck(colour))//If they have even one move it is not checkmate or a draw
                    {
                        return gameState.unfinished;
                    }
                }
            }

            if (this.isInCheck(colour))
            {
                if (colour == pieceColour.light)
                {
                    return gameState.darkWin;
                }
                else
                {
                    return gameState.lightWin;
                }
            }
            return gameState.draw;
            
        }

        public bool canDarkCastleKingSide()
        {
            return this.darkCastleKingSide;
        }
        public bool canDarkCastleQueenSide()
        {
            return this.darkCastleQueenSide;
        }
        public bool canLightCastleKingSide()
        {
            return this.lightCastleKingSide;
        }
        public bool canLightCastleQueenSide()
        {
            return this.lightCastleQueenSide;
        }
        public void setCanDarkCastleKingSide(bool Bool)
        {
            this.darkCastleKingSide = Bool;
        }
        public void setCanDarkCastleQueenSide(bool Bool)
        {
            this.darkCastleQueenSide = Bool;
        }
        public void setCanLightCastleKingSide(bool Bool)
        {
            this.lightCastleKingSide = Bool;
        }
        public void setCanLightCastleQueenSide(bool Bool)
        {
            this.lightCastleQueenSide = Bool;
        }

    }

    /// <summary>
    /// Abstract class representing a generic chess piece, holds a type and a colour
    /// </summary>
    abstract class ChessPiece
    {

        private pieceType type;
        private pieceColour colour;

        public ChessPiece(pieceType type, pieceColour colour)
        {
            this.type = type;
            this.colour = colour;
            
        }

        public pieceType getType()
        {
            return this.type;
        }

        public pieceColour getColour()
        {
            return this.colour;
        }

        /// <summary>
        /// Calculates a list of all the locations threatend by a piece on a given board state. Note that a piece may 
        /// threaten locations that hold another piece of the same colour
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <param name="type">The type of the piece in question</param>
        /// <param name="colour">The colour of the piece in question</param>
        /// <returns>A list of locations threatened by the piece</returns>
        public static List<BoardLocation> getThreatenedLocations(BoardState boardState, BoardLocation pieceLocation, pieceType type, pieceColour colour)
        {
            switch (type)
            {
                case(pieceType.pawn):
                    return Pawn.locationsThreatened(boardState, pieceLocation, colour);
                
                case(pieceType.knight):
                    return Knight.locationsThreatened(pieceLocation);

                case(pieceType.bishop):
                    return Bishop.locationsThreatened(boardState, pieceLocation);

                case(pieceType.rook):
                    return Rook.locationsThreatened(boardState, pieceLocation);

                case(pieceType.queen):
                    return Queen.locationsThreatened(boardState, pieceLocation);

                case (pieceType.king):
                    return King.locationsThreatened(boardState, pieceLocation);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Calculates a list of moves that a piece of the given type and colour may move to. It should be noted
        /// that this function will return moves that may put the player in, or leave the player in check. It will 
        /// not return moves that will cause the piece to capture a piece of its own colour
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <param name="previousMove">The move made last turn. Needed for en-passant calculation</param>
        /// <param name="type">The type of piece in question</param>
        /// <param name="colour">The colour of piece in question</param>
        /// <returns>A list of moves the piece may move to</returns>
        public static List<BoardLocation> getPotentialMoves(BoardState boardState, BoardLocation pieceLocation, ChessMove previousMove, pieceType type, pieceColour colour)
        {
            switch (type)
            {
                case (pieceType.pawn):
                    return Pawn.potentialMoves(boardState, pieceLocation, previousMove, colour);

                case (pieceType.knight):
                    return Knight.potentialMoves(boardState, pieceLocation, colour);

                case (pieceType.bishop):
                    return Bishop.potentialMoves(boardState, pieceLocation, colour);

                case (pieceType.rook):
                    return Rook.potentialMoves(boardState, pieceLocation, colour);

                case (pieceType.queen):
                    return Queen.potentialMoves(boardState, pieceLocation, colour);

                case (pieceType.king):
                    return King.potentialMoves(boardState, pieceLocation, colour);

                default:
                    return null;
            }
        }

        /// <summary></summary>
        /// <param name="type">Type of the new piece</param>
        /// <param name="colour">Colour of the new piece</param>
        /// <returns>A new chess piece of the specified type and colour</returns>
        public static ChessPiece newChessPiece(pieceType type, pieceColour colour)
        {
            switch (type)
            {
                case(pieceType.pawn):
                    return new Pawn(colour);
                case(pieceType.bishop):
                    return new Bishop(colour);
                case(pieceType.knight):
                    return new Knight(colour);
                case(pieceType.rook):
                    return new Rook(colour);
                case(pieceType.queen):
                    return new Queen(colour);
                case (pieceType.king):
                    return new King(colour);
                default:
                    return null;
            }
        }
    }

    class Pawn : ChessPiece
    {


        public Pawn(pieceColour colour)
            : base( pieceType.pawn, colour)
        {
            
        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check if any of these move put the player in, or leave the player in check
        /// 
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="previousMove">The previously made move. Needed to check if en-passant is an option</param>
        /// <param name="colour">The colour of the pawn</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, ChessMove previousMove, pieceColour colour)
        {
            List<BoardLocation> potentialMoves = new List<BoardLocation>();
            BoardLocation toMoveTo;
            SquareState stateAt;


            if (colour == pieceColour.light)
            {
                // the space in front of it, and the space 2 in front of it if it and the one in front are not taken
                
                toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + 1));
                if (boardState.stateAt(toMoveTo) == null)
                {
                    potentialMoves.Add(toMoveTo);
                    if (pieceLocation.rank() == '2' )//Check to see if it can move 2 spaces up
                    {
                        toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + 2));
                        if (boardState.stateAt(toMoveTo) == null)
                        {
                            potentialMoves.Add(toMoveTo);
                        }
                    }
                }
               

                //above-left square, if there is any enemy piece there, or en-passant is a thing
                if (pieceLocation.file() != 'A')
                {
                    toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 1));
                    stateAt = boardState.stateAt(toMoveTo);
                    if (stateAt != null && stateAt.colour == pieceColour.dark)
                    {
                        potentialMoves.Add(toMoveTo);
                    }
                    else if (pieceLocation.rank() == '5')//If im on the 5th rank
                    {
                        // AND the last move was a pawn AND that move was on the file to the left of me
                        if (previousMove.piece.getType() == pieceType.pawn && previousMove.init.file() == toMoveTo.file())
                        {
                            // AND the pawn moved from the 7th to fifth rank
                            if (previousMove.init.rank() == '7' && previousMove.final.rank() == '5')
                            {
                                //Only then may i en-passante
                                potentialMoves.Add(toMoveTo);
                            }
                        }
                    }
                }


                //above-right square, if there is any enemy piece there, or en-passant is a thing
                if (pieceLocation.file() != 'H')
                {
                    toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 1));
                    stateAt = boardState.stateAt(toMoveTo);
                    if (stateAt != null && stateAt.colour == pieceColour.dark)
                    {
                        potentialMoves.Add(toMoveTo);
                    }
                    else if (pieceLocation.rank() == '5')//If im on the 5th rank
                    {
                        // AND the last move was a pawn AND that move was on the file to the right of me
                        if (previousMove.piece.getType() == pieceType.pawn && previousMove.init.file() == toMoveTo.file())
                        {
                            // AND the pawn moved from the 7th to fifth rank
                            if (previousMove.init.rank() == '7' && previousMove.final.rank() == '5')
                            {
                                //Only then may i en-passante
                                potentialMoves.Add(toMoveTo);
                            }
                        }
                    }
                }
                
            }
            else
            {
                // the space below it, and the space 2 below it if it and the one below are not taken
                
                toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - 1));
                if (boardState.stateAt(toMoveTo) == null)
                {
                    potentialMoves.Add(toMoveTo);
                    if (pieceLocation.rank() == '7')//Check to see if it can move 2 spaces up
                    {
                        toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - 2));
                        if (boardState.stateAt(toMoveTo) == null)
                        {
                            potentialMoves.Add(toMoveTo);
                        }
                    }
                }


                if (pieceLocation.file() != 'A')//below-left square, if there is any enemy piece there, or en-passant is a thing
                {
                    toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 1));
                    stateAt = boardState.stateAt(toMoveTo);
                    if (stateAt != null && stateAt.colour == pieceColour.light)
                    {
                        potentialMoves.Add(toMoveTo);
                    }
                    else if (pieceLocation.rank() == '4')//If im on the 4th rank
                    {
                        // AND the last move was a pawn AND that move was on the file to the left of me
                        if (previousMove.piece.getType() == pieceType.pawn && previousMove.init.file() == toMoveTo.file())
                        {
                            // AND the pawn moved from the 2nd to fourth rank
                            if (previousMove.init.rank() == '2' && previousMove.final.rank() == '4')
                            {
                                //Only then may i en-passante
                                potentialMoves.Add(toMoveTo);
                            }
                        }
                    }
                }


                if (pieceLocation.file() != 'H')//below-right square, if there is any enemy piece there, or en-passant is a thing
                {
                    toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 1));
                    stateAt = boardState.stateAt(toMoveTo);
                    if (stateAt != null && stateAt.colour == pieceColour.light)
                    {
                        potentialMoves.Add(toMoveTo);
                    }
                    else if (pieceLocation.rank() == '4')//If im on the 4th rank
                    {
                        // AND the last move was a pawn AND that move was on the file to the right of me
                        if (previousMove.piece.getType() == pieceType.pawn && previousMove.init.file() == toMoveTo.file())
                        {
                            // AND the pawn moved from the 2nd to fourth rank
                            if (previousMove.init.rank() == '2' && previousMove.final.rank() == '4')
                            {
                                //Only then may i en-passante
                                potentialMoves.Add(toMoveTo);
                            }
                        }
                    }
                }
               
            }

            return potentialMoves;
        }

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <param name="colour">The colour of the pawn</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> threatenedLocations = new List<BoardLocation>();

            if (colour == pieceColour.light)
            {
                if(pieceLocation.file() != 'A') //up-left square
                {
                    BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 1));
                    threatenedLocations.Add(threatenedLocation);
                }
                

                if(pieceLocation.file() != 'H') //up-right square
                {
                    BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 1));
                    threatenedLocations.Add(threatenedLocation);
                }
                
            }
            else
            {
                if(pieceLocation.file() != 'A') //down-left square
                {
                    BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 1));
                    threatenedLocations.Add(threatenedLocation);
                }
                

                if(pieceLocation.file() != 'H') //down-right square
                {
                    BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 1));
                    threatenedLocations.Add(threatenedLocation);
                }
                
            }

            return threatenedLocations;
        }
    }

    class Bishop : ChessPiece
    {


        public Bishop(pieceColour colour)
            : base(pieceType.bishop, colour)
        {

        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check 
        /// if any of these move put the player in, or leave the player in check
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="colour">The colour of the piece</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> potentialMoves = new List<BoardLocation>();
            BoardLocation toMoveTo;
            SquareState potentialMoveToSquareState;

            int displacement = 1;

            while (((pieceLocation.file() + displacement) != 'I') && (pieceLocation.rank() + displacement) != '9')//Check all locations to the up-right diagonal of the bishop
            {
                
                toMoveTo = new BoardLocation((char)(pieceLocation.file() + displacement), (char)(pieceLocation.rank() + displacement));
                potentialMoveToSquareState = boardState.stateAt(toMoveTo);
                if (potentialMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveTo);
                }
                else
                {
                    if (potentialMoveToSquareState.colour != colour)//Add it if i can take the piece (i.e. it is the opposite colour
                    {
                        potentialMoves.Add(toMoveTo);
                    }

                    break;
                }
                
                
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() - displacement) != '@') && (pieceLocation.rank() + displacement) != '9')//Check all locations to the up-left diagonal of the bishop
            {
                
                toMoveTo = new BoardLocation((char)(pieceLocation.file() - displacement), (char)(pieceLocation.rank() + displacement));
                potentialMoveToSquareState = boardState.stateAt(toMoveTo);
                if (potentialMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveTo);
                }
                else
                {
                    if (potentialMoveToSquareState.colour != colour)//Add it if i can take the piece (i.e. it is the opposite colour
                    {
                        potentialMoves.Add(toMoveTo);
                    }

                    break;
                }
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() + displacement) != 'I') && (pieceLocation.rank() - displacement) != '0')//Check all locations to the down-right diagonal of the bishop
            {
                
                toMoveTo = new BoardLocation((char)(pieceLocation.file() + displacement), (char)(pieceLocation.rank() - displacement));
                potentialMoveToSquareState = boardState.stateAt(toMoveTo);
                if (potentialMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveTo);
                }
                else
                {
                    if (potentialMoveToSquareState.colour != colour)//Add it if i can take the piece (i.e. it is the opposite colour
                    {
                        potentialMoves.Add(toMoveTo);
                    }

                    break;
                }
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() - displacement) != '@') && (pieceLocation.rank() - displacement) != '0')//Check all locations to the down-left diagonal of the bishop
            {
                
                toMoveTo = new BoardLocation((char)(pieceLocation.file() - displacement), (char)(pieceLocation.rank() - displacement));
                potentialMoveToSquareState = boardState.stateAt(toMoveTo);
                if (potentialMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveTo);
                }
                else
                {
                    if (potentialMoveToSquareState.colour != colour)//Add it if i can take the piece (i.e. it is the opposite colour
                    {
                        potentialMoves.Add(toMoveTo);
                    }

                    break;
                }
                displacement++;
            }

            return potentialMoves;
        }

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardState boardState, BoardLocation pieceLocation)
        {
            List<BoardLocation> threatenedLocations = new List<BoardLocation>();

            int displacement = 1;

            while (((pieceLocation.file() + displacement) != 'I') && (pieceLocation.rank() + displacement) != '9')//Check all locations to the up-right diagonal of the bishop
            {
                
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + displacement), (char)(pieceLocation.rank() + displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() - displacement) != '@') && (pieceLocation.rank() - displacement) != '0')//Check all locations to the down - left diagonal of the bishop
            {
    
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - displacement), (char)(pieceLocation.rank() - displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() - displacement) != '@') && (pieceLocation.rank() + displacement) != '9')//Check all locations to the up-left diagonal of the bishop
            {
                
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - displacement), (char)(pieceLocation.rank() + displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                displacement++;
            }

            displacement = 1;

            while (((pieceLocation.file() + displacement) != 'I') && (pieceLocation.rank() - displacement) != '0')//Check all locations to the down - right diagonal of the bishop
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + displacement), (char)(pieceLocation.rank() - displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                
                displacement++;
            }

            return threatenedLocations;
        }

    }

    class Knight : ChessPiece
    {


        public Knight(pieceColour colour)
            : base(pieceType.knight, colour)
        {

        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check if any of these move put the player in, or leave the player in check
        /// 
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="colour">The colour of the piece</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {

            List<BoardLocation> potentialMoves = new List<BoardLocation>();

            if((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() + 2) < '9')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 2));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() - 2) > '@' && (pieceLocation.rank() + 1) < '9')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 2), (char)(pieceLocation.rank() + 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() - 2) > '@' && (pieceLocation.rank() - 1) > '0')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 2), (char)(pieceLocation.rank() - 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() - 2) > '0')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 2));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() - 2) > '0')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 2));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 2) < 'I' && (pieceLocation.rank() - 1) > '0')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 2), (char)(pieceLocation.rank() - 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 2) < 'I' && (pieceLocation.rank() + 1) < '9')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 2), (char)(pieceLocation.rank() + 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() + 2) < '9')
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 2));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            return potentialMoves;
        }

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardLocation pieceLocation)
        {
            List<BoardLocation> threatenedLocations = new List<BoardLocation>();

            if((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() + 2) < '9')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 2));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() - 2) > '@' && (pieceLocation.rank() + 1) < '9')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 2), (char)(pieceLocation.rank() + 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() - 2) > '@' && (pieceLocation.rank() - 1) > '0')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 2), (char)(pieceLocation.rank() - 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() - 2) > '0')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 2));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() - 2) > '0')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 2));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 2) < 'I' && (pieceLocation.rank() - 1) > '0')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 2), (char)(pieceLocation.rank() - 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 2) < 'I' && (pieceLocation.rank() + 1) < '9')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 2), (char)(pieceLocation.rank() + 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() + 2) < '9')
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 2));
                threatenedLocations.Add(threatenedLocation);
            }


            return threatenedLocations;
        }
    }

    class Rook : ChessPiece
    {

        public Rook(pieceColour colour)
            : base(pieceType.rook, colour)
        {

        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check if any of these move put the player in, or leave the player in check
        /// 
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="colour">The colour of the piece</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> potentialMoves = new List<BoardLocation>();

            int displacement = 1;

            while ((pieceLocation.rank() + displacement) < '9')//Check all locations above the rook
            {
                BoardLocation toMoveto = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + displacement));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveto);
                if (toMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveto);
                }
                else
                {
                    if (toMoveToSquareState.colour != colour)
                    {
                        potentialMoves.Add(toMoveto);
                    }
                    break;
                }
                displacement++;
               
            }

            displacement = 1;

            while ((pieceLocation.rank() - displacement) > '0')//Check all locations below the rook
            {
                
                BoardLocation toMoveto = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - displacement));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveto);
                if (toMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveto);
                }
                else
                {
                    if (toMoveToSquareState.colour != colour)
                    {
                        potentialMoves.Add(toMoveto);
                    }
                    break;
                }
                
                displacement++;
            }

            displacement = 1;

            while ((pieceLocation.file() - displacement) > '@')//Check all locations left the rook
            {
                BoardLocation toMoveto = new BoardLocation((char)(pieceLocation.file() - displacement), pieceLocation.rank());
                SquareState toMoveToSquareState = boardState.stateAt(toMoveto);
                if (toMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveto);
                }
                else
                {
                    if (toMoveToSquareState.colour != colour)
                    {
                        potentialMoves.Add(toMoveto);
                    }
                    break;
                }
                displacement++;
            }

            displacement = 1;

            while ((pieceLocation.file() + displacement) < 'I')//Check all locations right the rook
            {
                
                BoardLocation toMoveto = new BoardLocation((char)(pieceLocation.file() + displacement), pieceLocation.rank());
                SquareState toMoveToSquareState = boardState.stateAt(toMoveto);
                if (toMoveToSquareState == null)
                {
                    potentialMoves.Add(toMoveto);
                }
                else
                {
                    if (toMoveToSquareState.colour != colour)
                    {
                        potentialMoves.Add(toMoveto);
                    }
                    break;
                }
                displacement++;
            }

            return potentialMoves;
        }

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardState boardState, BoardLocation pieceLocation)
        {
            List<BoardLocation> threatenedLocations = new List<BoardLocation>();

            int displacement = 1;

            while ((pieceLocation.rank() + displacement) < '9')//Check all locations above the rook
            {
                BoardLocation threatenedLocation = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                displacement++;
            }

            displacement = 1;

            while ((pieceLocation.rank() - displacement) > '0')//Check all locations below the rook
            {
                BoardLocation threatenedLocation = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - displacement));
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                displacement++;
            }

            displacement = 1;

            while ((pieceLocation.file() + displacement) < 'I')//Check all locations to the right of the rook
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + displacement),pieceLocation.rank());
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                
                displacement++;
            }

            displacement = 1;

            while ((pieceLocation.file() - displacement) > '@')//Check all locations to the left of the rook
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - displacement), pieceLocation.rank());
                threatenedLocations.Add(threatenedLocation);
                SquareState threatenedSquareState = boardState.stateAt(threatenedLocation);
                if (!Object.ReferenceEquals(threatenedSquareState, null))
                {
                    break;
                }
                displacement++;
            }

            return threatenedLocations;
        }


    }

    class Queen : ChessPiece
    {
        public Queen(pieceColour colour)
            : base(pieceType.queen, colour)
        {

        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check if any of these move put the player in, or leave the player in check
        /// 
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="colour">The colour of the piece</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> potentialMoves = Rook.potentialMoves(boardState, pieceLocation, colour);
            potentialMoves.AddRange(Bishop.potentialMoves(boardState, pieceLocation, colour));
            return potentialMoves;
        }

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardState boardState, BoardLocation pieceLocation)
        {
            List<BoardLocation> threatenedLocations = Rook.locationsThreatened(boardState, pieceLocation);
            threatenedLocations.AddRange(Bishop.locationsThreatened(boardState, pieceLocation));
            return threatenedLocations;
        }

    }

    class King : ChessPiece
    {


        public King(pieceColour colour)
            : base(pieceType.king, colour)
        {

        }

        /// <summary>
        /// Gets all the moves that the piece can potentially move to. Does not check if any of these move put the player in, or leave the player in check
        /// 
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece</param>
        /// <param name="colour">The colour of the piece</param>
        /// <returns>A list of all the BoardLocations the piece may move to</returns>
        public static List<BoardLocation> potentialMoves(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> potentialMoves = new List<BoardLocation>();

            if ((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() + 1) < '9')// to the above-left square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if ((pieceLocation.rank() + 1) < '9') // to the above square
            {
                BoardLocation toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if ((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() + 1) < '9')//to the above-right square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 1) < 'I')//to the right square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), pieceLocation.rank());
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() - 1) > '0')//to the below-right square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.rank() - 1) > '0')//to the below square
            {
                BoardLocation toMoveTo = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() - 1) > '0')//to the below-left square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 1));
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            if((pieceLocation.file() - 1) > '@')//to the left square
            {
                BoardLocation toMoveTo = new BoardLocation((char)(pieceLocation.file() - 1), pieceLocation.rank());
                SquareState toMoveToSquareState = boardState.stateAt(toMoveTo);
                if (toMoveToSquareState == null || toMoveToSquareState.colour != colour)
                {
                    potentialMoves.Add(toMoveTo);
                }
            }

            potentialMoves.AddRange(findCastleableLocations(boardState, pieceLocation, colour));
            
            return potentialMoves;
        }

        /// <summary>
        /// Calculates and returns a list of locations the king of the specified colour is allowed to castle to. Will check
        /// to make sure that the king does not move into or through check.
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The current location of the king</param>
        /// <param name="colour">The colour of the king</param>
        /// <returns>A list of board locations the king is allowed to castle to</returns>
        public static List<BoardLocation> findCastleableLocations(BoardState boardState, BoardLocation pieceLocation, pieceColour colour)
        {
            List<BoardLocation> castleableLocations = new List<BoardLocation>();
            pieceColour oppositeColour;

            if (colour == pieceColour.light) {      oppositeColour = pieceColour.dark; }
            else {  oppositeColour = pieceColour.light; }

            if (boardState.canLightCastleKingSide() || boardState.canDarkCastleKingSide())
            {
                if (boardState.stateAt(InitialPieceLocations.rightBishopInitialPosition(colour)) == null
                    && boardState.stateAt(InitialPieceLocations.rightKnightInitialPosition(colour)) == null)// If there are no pieces between the king and the rook on the kingside
                {
                    if (!boardState.getLocationsThreatened(oppositeColour).Exists(x => ((x == InitialPieceLocations.rightBishopInitialPosition(colour))
                                                                                        || (x == InitialPieceLocations.rightKnightInitialPosition(colour)))))// And the locations the could would move to are not threatened
                    {
                        castleableLocations.Add(InitialPieceLocations.rightKnightInitialPosition(colour));//the location the king would move to if light was to castle kingside
                    }

                }
            }
            if (boardState.canLightCastleQueenSide() || boardState.canDarkCastleQueenSide())
            {
                if (boardState.stateAt(InitialPieceLocations.queenInitialPosition(colour)) == null
                    && boardState.stateAt(InitialPieceLocations.leftBishopInitialPosition(colour)) == null
                    && boardState.stateAt(InitialPieceLocations.leftKnightInitialPosition(colour)) == null)// If there are no pieces between the king and the rook on the queenside
                {
                    if (!boardState.getLocationsThreatened(oppositeColour).Exists(x => ((x == InitialPieceLocations.queenInitialPosition(colour))
                                                                                        || (x == InitialPieceLocations.leftBishopInitialPosition(colour)))))// And the locations the king would move to are not threatened
                    {
                        castleableLocations.Add(InitialPieceLocations.leftBishopInitialPosition(colour));//the location the king would move to if light was to castle queenside
                    }
                }
            }

            return castleableLocations;
        }
        

        /// <summary>
        /// Calculates a list of all the BoardLocations a piece is threatening for a given board state
        /// </summary>
        /// <param name="boardState">The current state of the board</param>
        /// <param name="pieceLocation">The location of the piece in question</param>
        /// <param name="colour">The colour of the pawn</param>
        /// <returns>A list of the boardlocations that are currently "threatened" by the piece(i.e. a threatened location is a location that
        /// the piece is either attacking or defending, a threatened location may have another piece of either colour on it) </returns>
        public static List<BoardLocation> locationsThreatened(BoardState boardState, BoardLocation pieceLocation)
        {
            List<BoardLocation> threatenedLocations = new List<BoardLocation>();

            if ((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() + 1) < '9')// to the above-left square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() + 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.rank() + 1) < '9') // to the above square
            {
                BoardLocation threatenedLocation = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() + 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() + 1) < '9')//to the above-right square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() + 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 1) < 'I')//to the right square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), pieceLocation.rank());
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() + 1) < 'I' && (pieceLocation.rank() - 1) > '0')//to the below-right square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() + 1), (char)(pieceLocation.rank() - 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.rank() - 1) > '0')//to the below square
            {
                BoardLocation threatenedLocation = new BoardLocation(pieceLocation.file(), (char)(pieceLocation.rank() - 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() - 1) > '@' && (pieceLocation.rank() - 1) > '0')//to the below-left square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), (char)(pieceLocation.rank() - 1));
                threatenedLocations.Add(threatenedLocation);
            }

            if ((pieceLocation.file() - 1) > '@')//to the left square
            {
                BoardLocation threatenedLocation = new BoardLocation((char)(pieceLocation.file() - 1), pieceLocation.rank());
                threatenedLocations.Add(threatenedLocation);
            }

            return threatenedLocations;
        }
    }

    static class InitialPieceLocations
    {
        private static BoardLocation leftRookLightInitialPosition = StaticBoardLocations.A1;
        private static BoardLocation leftKnightLightInitialPosition = StaticBoardLocations.B1;
        private static BoardLocation leftBishopLightInitialPosition = StaticBoardLocations.C1;
        private static BoardLocation queenLightInitialPosition = StaticBoardLocations.D1;
        private static BoardLocation kingLightInitialPosition = StaticBoardLocations.E1;
        private static BoardLocation rightBishopLightInitialPosition = StaticBoardLocations.F1;
        private static BoardLocation rightKnightLightInitialPosition = StaticBoardLocations.G1;
        private static BoardLocation rightRookLightInitialPosition = StaticBoardLocations.H1;

        private static BoardLocation leftRookDarkInitialPosition = StaticBoardLocations.A8;
        private static BoardLocation leftKnightDarkInitialPosition = StaticBoardLocations.B8;
        private static BoardLocation leftBishopDarkInitialPosition = StaticBoardLocations.C8;
        private static BoardLocation queenDarkInitialPosition = StaticBoardLocations.D8;
        private static BoardLocation kingDarkInitialPosition = StaticBoardLocations.E8;
        private static BoardLocation rightBishopDarkInitialPosition = StaticBoardLocations.F8;
        private static BoardLocation rightKnightDarkInitialPosition = StaticBoardLocations.G8;
        private static BoardLocation rightRookDarkInitialPosition = StaticBoardLocations.H8;

       public static BoardLocation leftRookInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return leftRookLightInitialPosition;
           }
           return leftRookDarkInitialPosition;
        }
       public static BoardLocation leftKnightInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return leftKnightLightInitialPosition;
           }
           return leftKnightDarkInitialPosition;
        }
       public static BoardLocation leftBishopInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return leftBishopLightInitialPosition;
           }
           return leftBishopDarkInitialPosition;
        }
       public static BoardLocation queenInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return queenLightInitialPosition;
           }
           return queenDarkInitialPosition;
        }
       public static BoardLocation kingInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return kingLightInitialPosition;
           }
           return kingDarkInitialPosition;
        }
       public static BoardLocation rightBishopInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return rightBishopLightInitialPosition;
           }
           return rightBishopDarkInitialPosition;
        }
       public static BoardLocation rightKnightInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return rightKnightLightInitialPosition;
           }
           return rightKnightDarkInitialPosition;
        }
       public static BoardLocation rightRookInitialPosition(pieceColour colour){
           if(colour == pieceColour.light){
               return rightRookLightInitialPosition;
           }
           return rightRookDarkInitialPosition;
        }


    }

    static class FileLocations
    {
        private const string piecePNGFolderLocation = "ChessPiecePNGs/";
        private const string blackPawn_PNGFile = "pawn_black.png";
        private const string whitePawn_PNGFile = "pawn_white.png";
        private const string blackBishop_PNGFile = "bishop_black.png";
        private const string whiteBishop_PNGFile = "bishop_white.png";
        private const string blackKnight_PNGFile = "knight_black.png";
        private const string whiteKnight_PNGFile = "knight_white.png";
        private const string blackRook_PNGFile = "rook_black.png";
        private const string whiteRook_PNGFile = "rook_white.png";
        private const string blackQueen_PNGFile = "queen_black.png";
        private const string whiteQueen_PNGFile = "queen_white.png";
        private const string blackKing_PNGFile = "king_black.png";
        private const string whiteKing_PNGFile = "king_white.png";

        private const string pieceCURFolderLocation = "ChessPieceCURs/";
        private const string blackPawn_CURFile = "pawn_black.cur";
        private const string whitePawn_CURFile = "pawn_white.cur";
        private const string blackBishop_CURFile = "bishop_black.cur";
        private const string whiteBishop_CURFile = "bishop_white.cur";
        private const string blackKnight_CURFile = "knight_black.cur";
        private const string whiteKnight_CURFile = "knight_white.cur";
        private const string blackRook_CURFile = "rook_black.cur";
        private const string whiteRook_CURFile = "rook_white.cur";
        private const string blackQueen_CURFile = "queen_black.cur";
        private const string whiteQueen_CURFile = "queen_white.cur";
        private const string blackKing_CURFile = "king_black.cur";
        private const string whiteKing_CURFile = "king_white.cur";

        public static string getPNGLocation(pieceType type, pieceColour colour)
        {
            string returned = piecePNGFolderLocation;

            switch (type)
            {
                case (pieceType.pawn):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whitePawn_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackPawn_PNGFile;
                            break;
                    }
                    break;

                case (pieceType.bishop):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteBishop_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackBishop_PNGFile;
                            break;
                    }
                    break;

                case (pieceType.knight):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteKnight_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackKnight_PNGFile;
                            break;
                    }
                    break;

                case (pieceType.rook):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteRook_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackRook_PNGFile;
                            break;
                    }
                    break;

                case (pieceType.queen):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteQueen_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackQueen_PNGFile;
                            break;
                    }
                    break;

                case (pieceType.king):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteKing_PNGFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackKing_PNGFile;
                            break;
                    }
                    break;
            }

            return returned;
        }

        public static string getCURLocation(pieceType type, pieceColour colour)
        {
            string returned = pieceCURFolderLocation;

            switch (type)
            {
                case (pieceType.pawn):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whitePawn_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackPawn_CURFile;
                            break;
                    }
                    break;

                case (pieceType.bishop):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteBishop_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackBishop_CURFile;
                            break;
                    }
                    break;

                case (pieceType.knight):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteKnight_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackKnight_CURFile;
                            break;
                    }
                    break;

                case (pieceType.rook):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteRook_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackRook_CURFile;
                            break;
                    }
                    break;

                case (pieceType.queen):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteQueen_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackQueen_CURFile;
                            break;
                    }
                    break;

                case (pieceType.king):
                    switch (colour)
                    {
                        case (pieceColour.light):
                            returned += whiteKing_CURFile;
                            break;

                        case (pieceColour.dark):
                            returned += blackKing_CURFile;
                            break;
                    }
                    break;
            }

            return returned;
        }
    }

    static class StaticBoardLocations
    {

        public static BoardLocation A1 = new BoardLocation('A', '1');
        public static BoardLocation B1 = new BoardLocation('B', '1');
        public static BoardLocation C1 = new BoardLocation('C', '1');
        public static BoardLocation D1 = new BoardLocation('D', '1');
        public static BoardLocation E1 = new BoardLocation('E', '1');
        public static BoardLocation F1 = new BoardLocation('F', '1');
        public static BoardLocation G1 = new BoardLocation('G', '1');
        public static BoardLocation H1 = new BoardLocation('H', '1');

        public static BoardLocation A2 = new BoardLocation('A', '2');
        public static BoardLocation B2 = new BoardLocation('B', '2');
        public static BoardLocation C2 = new BoardLocation('C', '2');
        public static BoardLocation D2 = new BoardLocation('D', '2');
        public static BoardLocation E2 = new BoardLocation('E', '2');
        public static BoardLocation F2 = new BoardLocation('F', '2');
        public static BoardLocation G2 = new BoardLocation('G', '2');
        public static BoardLocation H2 = new BoardLocation('H', '2');

        public static BoardLocation A3 = new BoardLocation('A', '3');
        public static BoardLocation B3 = new BoardLocation('B', '3');
        public static BoardLocation C3 = new BoardLocation('C', '3');
        public static BoardLocation D3 = new BoardLocation('D', '3');
        public static BoardLocation E3 = new BoardLocation('E', '3');
        public static BoardLocation F3 = new BoardLocation('F', '3');
        public static BoardLocation G3 = new BoardLocation('G', '3');
        public static BoardLocation H3 = new BoardLocation('H', '3');
   
        public static BoardLocation A4 = new BoardLocation('A', '4');
        public static BoardLocation B4 = new BoardLocation('B', '4');
        public static BoardLocation C4 = new BoardLocation('C', '4');
        public static BoardLocation D4 = new BoardLocation('D', '4');
        public static BoardLocation E4 = new BoardLocation('E', '4');
        public static BoardLocation F4 = new BoardLocation('F', '4');
        public static BoardLocation G4 = new BoardLocation('G', '4');
        public static BoardLocation H4 = new BoardLocation('H', '4');
    
        public static BoardLocation A5 = new BoardLocation('A', '5');
        public static BoardLocation B5 = new BoardLocation('B', '5');
        public static BoardLocation C5 = new BoardLocation('C', '5');
        public static BoardLocation D5 = new BoardLocation('D', '5');
        public static BoardLocation E5 = new BoardLocation('E', '5');
        public static BoardLocation F5 = new BoardLocation('F', '5');
        public static BoardLocation G5 = new BoardLocation('G', '5');
        public static BoardLocation H5 = new BoardLocation('H', '5');

        public static BoardLocation A6 = new BoardLocation('A', '6');
        public static BoardLocation B6 = new BoardLocation('B', '6');
        public static BoardLocation C6 = new BoardLocation('C', '6');
        public static BoardLocation D6 = new BoardLocation('D', '6');
        public static BoardLocation E6 = new BoardLocation('E', '6');
        public static BoardLocation F6 = new BoardLocation('F', '6');
        public static BoardLocation G6 = new BoardLocation('G', '6');
        public static BoardLocation H6 = new BoardLocation('H', '6');

        public static BoardLocation A7 = new BoardLocation('A', '7');
        public static BoardLocation B7 = new BoardLocation('B', '7');
        public static BoardLocation C7 = new BoardLocation('C', '7');
        public static BoardLocation D7 = new BoardLocation('D', '7');
        public static BoardLocation E7 = new BoardLocation('E', '7');
        public static BoardLocation F7 = new BoardLocation('F', '7');
        public static BoardLocation G7 = new BoardLocation('G', '7');
        public static BoardLocation H7 = new BoardLocation('H', '7');

        public static BoardLocation A8 = new BoardLocation('A', '8');
        public static BoardLocation B8 = new BoardLocation('B', '8');
        public static BoardLocation C8 = new BoardLocation('C', '8');
        public static BoardLocation D8 = new BoardLocation('D', '8');
        public static BoardLocation E8 = new BoardLocation('E', '8');
        public static BoardLocation F8 = new BoardLocation('F', '8');
        public static BoardLocation G8 = new BoardLocation('G', '8');
        public static BoardLocation H8 = new BoardLocation('H', '8'); 
    }
}