using static System.Console;
public enum Color { black, white };

// king shows up in random places bug
// pawn can move twice in the beginning

interface Game
{
    int? winner();

    List<Move> possible_moves(Piece p);
    void move(Move m);
    // Game clone();
}

static class Globals
{
    public static Piece[,] winning_board = new Piece[8, 8];
    public static Move? best_move;
}

class Top
{
    static void Main()
    {
        Chess c = new Chess();
        c.read_file();
        WriteLine("who's turn is it to play?");
        int turn = int.Parse(ReadLine()!);
        WriteLine("depth of the program:");
        int depth = int.Parse(ReadLine()!);
        // WriteLine(c.checkmate(Color.white));

        Move? best;
        // WriteLine($"winner is: {Chess.minimax(c, 0, turn, depth, out best)}");
        Chess.minimax(c, 0, turn, depth, out best);
        WriteLine($"winner is {turn}");
        // // WriteLine(c.winning_board);

        Chess.write_matrix(Globals.winning_board);
        best = Globals.best_move;
        // WriteLine(Globals.best_move);
        if (best != null)
        {
            WriteLine($"{best.p.color} {best.p} to ({best.x}, {best.y})");
        }

        // test();
    }


    static void test()
    {
        Chess g = new Chess();
        Piece wn = new Knight(Color.white, new int[] { 3, 5 });
        Piece bn = new Knight(Color.black, new int[] { 5, 5 });
        Piece wp = new Pawn(Color.white, new int[] { 4, 1 });
        // Piece wn1 = new Knight(Color.white, new int[] { 0, 1 });
        // Piece wn2 = new Knight(Color.white, new int[] { 1, 1 });
        // Piece bn = new Knight(Color.black, new int[] { 2, 1 });
        // Piece wb = new Bishop(Color.white, new int[] { 1, 0 });
        g.pieces.Add(wp);
        g.pieces.Add(bn);
        g.pieces.Add(wn);
        // g.pieces.Add(bk);


        // g.pieces.Add(wn1);
        // g.pieces.Add(wb);
        // g.pieces.Add(wn2);
        // g.pieces.Add(bn);
        g.squares = g.build_board();
        // Chess.write_matrix(g.build_attack_matrix(Color.white));


        // WriteLine(g.possible_moves(bq).Count);
        // int counter = 0;

        foreach (Move m in g.possible_moves(wp))
        {
            WriteLine($"{m.x}, {m.y}");
        }
        // g.total_possible_moves(Color.white);
        // g.read_file();
        // WriteLine("final board");
        g.write_board();
        // WriteLine(g.king_is_attacked(Color.white));
        // WriteLine(g.king_is_surrounded(Color.white));
        // WriteLine(g.king_is_attacked(Color.black));
        // WriteLine(g.king_is_surrounded(Color.black));
        // WriteLine(g.checkmate(Color.black));

    }
}

class Move
{
    public int x;
    public int y;
    public Piece p;
    public Move(Piece p, int x, int y)
    {
        this.x = x; this.y = y; this.p = p;
    }
}
class Chess : Game
{
    // contains array which holds the piece on each square of the board

    public Piece[,] squares = new Piece[8, 8];
    public List<Piece> pieces = new List<Piece>();


    public static Piece[,] copy_board(Piece[,] to_copy)
    {
        Piece[,] board = new Piece[8, 8];
        foreach (Piece p in to_copy)
        {
            if (p != null)
                board[p.pos[0], p.pos[1]] = p;
        }
        return board;
    }

    public int? winner()
    {
        if (this.checkmate(Color.white))
        {
            return 2;

        }

        else if (this.checkmate(Color.black))
        {
            // Globals.winning_board = copy_board(squares);
            return 1;
        }
        return null;
    }

    public void read_file()
    {
        using (var reader = new StreamReader("./position.txt"))
        {
            while (reader.ReadLine()! is string s)
            {
                string name = s.Substring(1, 1);
                string color = s.Substring(0, 1);
                string pos = s.Substring(2, 2);
                int[] actual_pos = translate_coords(pos);
                Color clr;
                if (color == "W")
                {
                    clr = Color.white;
                }
                else if (color == "B")
                {
                    clr = Color.black;
                }
                else
                {
                    WriteLine("no such color");
                    continue;
                }

                int xpos = actual_pos[0];
                int ypos = actual_pos[1];

                if (within_bounds(xpos, ypos) && !check_occupied(actual_pos))
                {
                    WriteLine($"name = {name}, x position = {xpos}, y position = {ypos}");
                    switch (name)
                    {
                        case "K":
                            this.pieces.Add(new King(clr, actual_pos));
                            break;
                        case "Q":
                            this.pieces.Add(new Queen(clr, actual_pos));
                            break;
                        case "R":
                            this.pieces.Add(new Rook(clr, actual_pos));
                            break;
                        case "B":
                            this.pieces.Add(new Bishop(clr, actual_pos));
                            break;
                        case "P":
                            this.pieces.Add(new Pawn(clr, actual_pos));
                            break;
                        case "N":
                            this.pieces.Add(new Knight(clr, actual_pos));
                            break;
                        default:
                            WriteLine("no such piece");
                            break;
                    }
                    this.squares = build_board();
                    write_board();
                }
                else
                {
                    WriteLine("position out of bounds");
                }
            }
        }
    }
    public List<Move> total_possible_moves(Color c)
    {
        List<Move> moves = new List<Move>();

        foreach (Piece p in this.squares)
        {
            if (p != null && p.color == c && !p.pinned(this))
            {
                // WriteLine($"{p.GetType().ToString()} can move");
                foreach (Move m in this.possible_moves(p))
                {
                    if (m != null)
                    {
                        moves.Add(m);
                    }
                }
            }
        }
        return moves;
    }

    public Chess clone()
    {
        Chess g = new Chess();
        g.pieces = this.pieces;
        g.squares = (Piece[,])this.squares.Clone();
        return g;
    }
    public static double minimax(Chess g, int depth, int turn, int limit, out Move? best)
    {
        best = null;
        // WriteLine(g.checkmate(Color.black));
        if (g.winner() is int i)
        {
            // write_matrix(g.squares);
            // WriteLine(i);
            // WriteLine(g.king_is_attacked(Color.black));
            // WriteLine(g.king_is_surrounded(Color.black));
            // write_matrix(g.build_attack_matrix(Color.white));
            // ReadLine();
            // WriteLine(g.squares);
            // WriteLine("winning pos");
            Globals.winning_board = g.squares;
            return i == 1 ? 1.0 : i == 2 ? -1.0 : 0.0;

        }

        if (depth >= limit)
        {
            return 0;
        }

        double v = turn == 1 ? double.MinValue : double.MaxValue;
        // List<Move> moves = g.total_possible_moves(turn == 1 ? Color.white : Color.black);

        foreach (Move m in g.total_possible_moves(turn == 1 ? Color.white : Color.black))
        {
            Chess g1 = g.clone();
            g1.move(m);
            // WriteLine("board");
            // g1.write_board();
            double w = minimax(g1, depth + 1, 3 - turn, limit, out Move _);

            if (turn == 1)
            {
                if (w > v)
                {
                    v = w;
                    Globals.best_move = m;
                    // best = m;
                    // Globals.winning_board = copy_board(g.squares);
                    // if (v == 1.0)
                    // {

                    //     WriteLine("white won------------------------------------");
                    //     return v;
                    // }
                }
            }
            else if (turn == 2)
            {
                if (w < v)
                {
                    v = w;
                    Globals.best_move = m;

                    // best = m;
                    // Globals.winning_board = copy_board(g.squares);

                    // if (v == -1.0)
                    // {

                    //     WriteLine("black won------------------------------------");
                    //     return v;
                    // }
                }
            }
        }
        return v;
    }


    public List<Move> possible_moves(Piece p)
    {
        List<Move> moves = new List<Move>();
        if (p != null)
        {
            if (p.GetType().ToString() != "Pawn")
            {
                foreach ((int x, int y) in p.dirs!)
                {
                    int i;
                    int j;
                    for (int ii = 0; ii < 8; ii++)
                    {
                        for (int jj = 0; jj < 8; jj++)
                        {
                            if (p.Equals(this.squares[ii, jj]))
                            {
                                i = ii;
                                j = jj;
                                while (within_bounds(i + x, j + y))
                                {
                                    if (squares[i + x, j + y] == null)
                                    {
                                        i += x;
                                        j += y;
                                        if (p.GetType().ToString() == "King")
                                        {
                                            bool[,] attack_matrix = build_attack_matrix(p.color == Color.white ? Color.black : Color.white);
                                            if (attack_matrix[i, j] == false)
                                            {
                                                moves.Add(new Move(p, i, j));
                                            }
                                        }
                                        else
                                        {
                                            moves.Add(new Move(p, i, j));

                                        }
                                        if (p.GetType().ToString() == "Knight" || p.GetType().ToString() == "King")
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (p.GetType().ToString() == "King")
                                        {
                                            bool[,] attack_matrix = build_attack_matrix(p.color == Color.white ? Color.black : Color.white);
                                            if (attack_matrix[i + x, j + y] == false && this.squares[i + x, j + y].color != p.color)
                                            {
                                                moves.Add(new Move(p, i + x, j + y));
                                            }
                                        }
                                        else if (squares[i + x, j + y] != null && p.GetType().ToString() != "King")
                                        {
                                            if (squares[i + x, j + y].color != p.color)
                                            {
                                                // WriteLine(squares[i + x, j + y].GetType().ToString());
                                                moves.Add(new Move(p, i + x, j + y));
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }



            if (p.GetType().ToString() == "Pawn")
            {
                if (p.color == Color.white)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (p.Equals(this.squares[i, j]))
                            {
                                int[] pos = { i, j };
                                if (pos[1] + 1 >= 0 && pos[1] + 1 < 8)
                                {
                                    if (!check_occupied(new int[] { pos[0], pos[1] + 1 }))
                                    {
                                        moves.Add(new Move(p, pos[0], pos[1] + 1));
                                    }
                                }
                                if (pos[0] + 1 >= 0 && pos[0] + 1 < 8 && pos[1] + 1 >= 0 && pos[1] + 1 < 8)
                                {
                                    if (check_occupied(new int[] { pos[0] + 1, pos[1] + 1 }))
                                    {
                                        if (squares[pos[0] + 1, pos[1] + 1].color == Color.black)
                                        {
                                            moves.Add(new Move(p, pos[0] + 1, pos[1] + 1));
                                        }
                                    }
                                }
                                if (pos[0] - 1 >= 0 && pos[0] - 1 < 8 && pos[1] + 1 >= 0 && pos[1] + 1 < 8)
                                {
                                    if (check_occupied(new int[] { pos[0] - 1, pos[1] + 1 }))
                                    {
                                        if (squares[pos[0] - 1, pos[1] + 1].color == Color.black)
                                        {
                                            moves.Add(new Move(p, pos[0] - 1, pos[1] + 1));
                                        }
                                    }
                                }
                                if (pos[1] == 1)
                                {
                                    if (!check_occupied(new int[] { pos[0], pos[1] + 2 }))
                                    {
                                        moves.Add(new Move(p, pos[0], pos[1] + 2));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (p.color == Color.black)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (p.Equals(this.squares[i, j]))
                            {
                                int[] pos = { i, j };
                                if (pos[1] - 1 >= 0 && pos[1] - 1 < 8)
                                {
                                    if (!check_occupied(new int[] { pos[0], pos[1] - 1 }))
                                    {
                                        moves.Add(new Move(p, pos[0], pos[1] - 1));
                                    }
                                }
                                if (pos[0] + 1 >= 0 && pos[0] + 1 < 8 && pos[1] - 1 >= 0 && pos[1] - 1 < 8)
                                {
                                    if (check_occupied(new int[] { pos[0] + 1, pos[1] - 1 }))
                                    {
                                        if (squares[pos[0] + 1, pos[1] - 1].color == Color.white)
                                        {
                                            moves.Add(new Move(p, pos[0] + 1, pos[1] - 1));
                                        }
                                    }
                                }
                                if (within_bounds(pos[0] + 1, pos[1] - 1))
                                {
                                    if (check_occupied(new int[] { pos[0] + 1, pos[1] - 1 }))
                                    {
                                        if (squares[pos[0] + 1, pos[1] - 1].color == Color.white)
                                        {
                                            moves.Add(new Move(p, pos[0] + 1, pos[1] - 1));
                                        }
                                    }
                                }
                                if (pos[1] == 6)
                                {
                                    if (!check_occupied(new int[] { pos[0], pos[1] - 2 }))
                                    {
                                        moves.Add(new Move(p, pos[0], pos[1] - 2));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return moves;
    }

    public bool[,] build_attack_matrix(Color c)
    {
        bool[,] attack_matrix = new bool[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (this.squares[i, j] != null && this.squares[i, j].color == c)
                {
                    if (this.squares[i, j].GetType().ToString() != "Knight"
                        && this.squares[i, j].GetType().ToString() != "Pawn"
                        && this.squares[i, j].GetType().ToString() != "King"
                    )
                    {
                        foreach ((int x, int y) in this.squares[i, j].dirs!)
                        {
                            for (int z = 1; z < 8; z++)
                            {
                                if (i + (x * z) >= 0 && i + (x * z) < 8 && j + (y * z) >= 0 && j + (y * z) < 8)
                                {
                                    attack_matrix[i + (x * z), j + (y * z)] = true;

                                    if (this.squares[i + (x * z), j + (y * z)] != null && this.squares[i + (x * z), j + (y * z)].GetType().ToString() != "King")
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (this.squares[i, j].GetType().ToString() == "King" || this.squares[i, j].GetType().ToString() == "Knight")
                    {
                        foreach ((int x, int y) in this.squares[i, j].dirs!)
                        {
                            if (i + x >= 0 && i + x < 8 && j + y >= 0 && j + y < 8)
                            {
                                attack_matrix[i + x, j + y] = true;
                            }
                        }
                    }
                    if (this.squares[i, j].GetType().ToString() == "Pawn")
                    {
                        if (this.squares[i, j].color == Color.white)
                        {
                            if (i + 1 >= 0 && i + 1 < 8 && j + 1 >= 0 && j + 1 < 8)
                            {
                                attack_matrix[i + 1, j + 1] = true;
                            }
                            if (i - 1 >= 0 && i - 1 < 8 && j + 1 >= 0 && j + 1 < 8)
                            {
                                attack_matrix[i - 1, j + 1] = true;
                            }
                        }
                        else if (this.squares[i, j].color == Color.black)
                        {
                            if (i + 1 >= 0 && i + 1 < 8 && j - 1 >= 0 && j - 1 < 8)
                            {
                                attack_matrix[i + 1, j - 1] = true;
                            }
                            if (i - 1 >= 0 && i - 1 < 8 && j - 1 >= 0 && j - 1 < 8)
                            {
                                attack_matrix[i - 1, j - 1] = true;
                            }
                        }
                    }
                }
            }
        }

        return attack_matrix;
    }

    public void move(Move m)
    {
        Piece p = m.p;
        Piece[,] board = new Piece[8, 8];
        board[m.x, m.y] = p;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (!p.Equals(squares[i, j]))
                {
                    if (i != m.x || j != m.y)
                    {
                        board[i, j] = squares[i, j];
                    }
                }
            }
        }
        this.squares = board;
    }

    // public void unmove(Move m)
    // {
    //     Piece p = m.p;
    //     move(new Move(p, p.old_pos[0], p.old_pos[1]));
    // }

    public int[] find_piece(Piece p)
    {
        int[] pos = new int[2];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (p.Equals(this.squares[i, j]))
                {
                    pos[0] = i;
                    pos[1] = j;
                }
            }
        }
        return pos;
    }

    // king is checkmated if it is attacked and all surrounding squares are either occupied by friendly
    // pieces or enemy pieces which are protected or all the squares are attacked
    // and no piece can move to block the check

    public bool checkmate(Color c)
    {
        if (king_is_attacked(c) && king_is_surrounded(c))
        {
            if (no_defense(c))
            {
                return true;
            }
        }
        return false;
    }

    public bool king_is_attacked(Color c)
    {
        bool[,] attack_matrix = new bool[8, 8];
        int[] king_pos = new int[2];

        attack_matrix = c == Color.white ? build_attack_matrix(Color.black) : build_attack_matrix(Color.white);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece? p = this.squares[i, j];
                if (p != null && p.GetType().ToString() == "King" && p.color == c)
                {
                    king_pos = new int[] { i, j };
                }
            }
        }

        return attack_matrix[king_pos[0], king_pos[1]];
    }

    public bool no_defense(Color c)
    {
        foreach (Move m in total_possible_moves(c))
        {
            Chess g1 = this.clone();
            g1.move(m);
            if (!g1.king_is_attacked(c))
            {
                // WriteLine($"apparently this move saves: {m.p.color} {m.p} from {m.p.pos[0]}, {m.p.pos[1]} to {m.x}, {m.y}");
                return false;
            }
        }
        return true;
    }

    public bool king_is_surrounded(Color c)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece? p = this.squares[i, j];
                if (p != null && p.GetType().ToString() == "King" && p.color == c)
                {
                    foreach ((int x, int y) in p.dirs!)
                    {
                        bool[,] attack_matrix = c == Color.white ? build_attack_matrix(Color.black) : build_attack_matrix(Color.white);
                        if (i + x >= 0 && i + x < 8 && j + y >= 0 && j + y < 8)
                        {
                            if (!attack_matrix[i + x, j + y])
                            {
                                if (!check_occupied(new int[] { i + x, j + y }))
                                {
                                    return false;
                                }
                                if (this.squares[i + x, j + y].color != c)
                                {
                                    return false;
                                }
                            }

                        }
                    }
                }
            }
        }
        return true;
    }

    int[] translate_coords(string chess_notation)
    {
        int xpos = (int)chess_notation[0] - 97;
        int ypos = int.Parse(chess_notation[1].ToString()) - 1;
        int[] coords = { xpos, ypos };
        return coords;
    }

    bool check_occupied(int[] coords)
    {
        if (within_bounds(coords[0], coords[1]))
        {
            return squares[coords[0], coords[1]] != null ? true : false;
        }
        return true;
    }

    public void setup_board()
    {
        WriteLine("Setup the board");
        WriteLine("To add a piece to the board, simply write the piece and square in chess notation.");
        WriteLine("Example: to add a white knight to the c3 square we would write WNc3");
        WriteLine("Example: to add a black king to the e4 square, we would write BKe4");
        WriteLine("All pieces are represented by the first letter of their name except for the knight \nwhich is represented by N, since K is reserved for the king.");
        while (ReadLine() is string s)
        {
            string name = s.Substring(1, 1);
            string color = s.Substring(0, 1);
            string pos = s.Substring(2, 2);
            int[] actual_pos = translate_coords(pos);
            // WriteLine(check_occupied(actual_pos));
            Color clr;
            if (color == "W")
            {
                clr = Color.white;
            }
            else if (color == "B")
            {
                clr = Color.black;
            }
            else
            {
                WriteLine("no such color");
                continue;
            }

            int xpos = actual_pos[0];
            int ypos = actual_pos[1];

            if (xpos <= 7 && xpos >= 0 && ypos <= 7 && ypos >= 0 && !check_occupied(actual_pos))
            {
                WriteLine($"name = {name}, x position = {xpos}, y position = {ypos}");
                switch (name)
                {
                    case "K":
                        this.pieces.Add(new King(clr, actual_pos));
                        break;
                    case "Q":
                        this.pieces.Add(new Queen(clr, actual_pos));
                        break;
                    case "R":
                        this.pieces.Add(new Rook(clr, actual_pos));
                        break;
                    case "B":
                        this.pieces.Add(new Bishop(clr, actual_pos));
                        break;
                    case "P":
                        this.pieces.Add(new Pawn(clr, actual_pos));
                        break;
                    case "N":
                        this.pieces.Add(new Knight(clr, actual_pos));
                        break;
                    default:
                        WriteLine("no such piece");
                        break;
                }
                this.squares = build_board();
                write_board();
            }
            else
            {
                WriteLine("position out of bounds");
            }
        }
    }

    public Piece[,] build_board()
    {
        Piece[,] board = new Piece[8, 8];
        foreach (Piece p in this.pieces)
        {
            if (within_bounds(p.pos[0], p.pos[1]))
                board[p.pos[0], p.pos[1]] = p;
        }
        return board;
    }

    public void write_board()
    {
        for (int i = 7; i >= 0; i--)
        {
            for (int j = 0; j < 8; j++)
            {
                char c;
                if (this.squares[j, i] != null)
                {
                    if (this.squares[j, i].ToString() != "Knight")
                    {
                        c = this.squares[j, i].ToString()![0];
                    }
                    else
                    {
                        c = this.squares[j, i].ToString()!.ToUpper()[1];
                    }
                }
                else
                {
                    c = '-';
                }

                if (j == 7)
                {
                    Write($"{c}\n");
                }
                else
                {
                    Write($"{c} ");
                }
            }
        }
    }

    public static void write_matrix<T>(T[,] matrix)
    {
        for (int i = matrix.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (j < matrix.GetLength(1) - 1)
                {
                    if (matrix[j, i] != null)
                    {
                        if (matrix[j, i]!.GetType().ToString() != "Knight")
                        {
                            Write($"{matrix[j, i]!.ToString()![0]} ");

                        }
                        else
                        {
                            Write($"{matrix[j, i]!.ToString()![1]} ");

                        }

                    }
                    else
                    {
                        Write("- ");
                    }

                }
                else
                {
                    if (matrix[j, i] != null)
                    {
                        if (matrix[j, i]!.GetType().ToString() != "Knight")
                        {
                            Write($"{matrix[j, i]!.ToString()![0]}\n");
                        }
                        else
                        {
                            Write($"{matrix[j, i]!.ToString()![1]}\n");

                        }
                    }
                    else
                    {
                        Write($"-\n");
                    }
                }
            }
        }
    }

    public static bool within_bounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}

class Piece
{
    public (int, int)[]? dirs;
    public Color color;
    public int[] pos = new int[2];
    public void setDirs((int, int)[] d)
    {
        this.dirs = d;
    }

    public int[] old_pos = new int[2];

    public bool pinned(Chess g)
    {
        if (this.GetType().ToString() != "King")
        {
            foreach (Move m in g.possible_moves(this))
            {
                Chess g1 = g.clone();
                g1.move(m);
                if (!g1.king_is_attacked(this.color))
                {
                    return false;
                }
            }
        }
        return true;
    }
}

class King : Piece
{
    public King(Color c, int[] pos)
    {
        (int, int)[] dirs = { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
        this.color = c;
        this.setDirs(dirs);
        this.pos = pos;
        this.old_pos = pos;
    }
}

class Rook : Piece
{
    public Rook(Color c, int[] pos)
    {
        (int, int)[] dirs = { (1, 0), (0, 1), (-1, 0), (0, -1) };
        this.color = c;
        this.setDirs(dirs);
        this.pos = pos;
        this.old_pos = pos;

    }
}

class Queen : Piece
{
    public Queen(Color c, int[] pos)
    {
        (int, int)[] dirs = { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
        this.color = c;
        this.setDirs(dirs);
        this.pos = pos;
        this.old_pos = pos;



    }
}

class Bishop : Piece
{
    public Bishop(Color c, int[] pos)
    {
        (int, int)[] dirs = { (1, 1), (-1, 1), (-1, -1), (1, -1) };
        this.color = c;
        this.setDirs(dirs);
        this.pos = pos;
        this.old_pos = pos;

    }
}

class Pawn : Piece
{
    public Pawn(Color c, int[] pos)
    {
        this.color = c;
        if (this.color == Color.black)
        {
            (int, int)[] dirs = { (0, -1), (-1, -1), (1, -1) };
        }
        else if (this.color == Color.white)
        {
            (int, int)[] dirs = { (1, 1), (-1, 1), (0, 1) };
        }
        this.setDirs(dirs!);
        this.pos = pos;
        this.old_pos = pos;

    }
}

class Knight : Piece
{
    public Knight(Color c, int[] pos)
    {
        (int, int)[] dirs = { (1, 2), (-1, 2), (-2, -1), (-2, 1), (-1, -2), (1, -2), (2, -1), (2, 1) };
        this.color = c;
        this.setDirs(dirs);
        this.pos = pos;
        this.old_pos = pos;


    }
}
