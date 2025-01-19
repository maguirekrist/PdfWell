namespace PDFParser.Parser.Math;

public class Matrix<T> where T : struct
{
    private readonly T[,] _data;

    // Properties for dimensions
    public int Rows { get; }
    public int Columns { get; }

    // Constructor for creating an empty matrix
    public Matrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Rows and columns must be greater than zero.");

        Rows = rows;
        Columns = columns;
        _data = new T[rows, columns];
    }

    // Constructor to initialize the matrix with data
    public Matrix(T[,] data)
    {
        Rows = data.GetLength(0);
        Columns = data.GetLength(1);
        _data = (T[,])data.Clone(); // Clone to prevent external modification
    }

    // Indexer for accessing elements
    public T this[int row, int col]
    {
        get
        {
            ValidateIndices(row, col);
            return _data[row, col];
        }
        set
        {
            ValidateIndices(row, col);
            _data[row, col] = value;
        }
    }

    // Validate indices
    private void ValidateIndices(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns)
            throw new IndexOutOfRangeException($"Invalid indices: ({row}, {col}).");
    }

    // Add two matrices
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new InvalidOperationException("Matrices must have the same dimensions to add.");

        var result = new Matrix<T>(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Columns; j++)
                result[i, j] = Add(a[i, j], b[i, j]);

        return result;
    }

    // Multiply two matrices
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Columns != b.Rows)
            throw new InvalidOperationException("Number of columns in A must match number of rows in B.");

        var result = new Matrix<T>(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < b.Columns; j++)
                for (int k = 0; k < a.Columns; k++)
                    result[i, j] = Add(result[i, j], Multiply(a[i, k], b[k, j]));

        return result;
    }

    // Helper methods for generic arithmetic
    private static T Add(T a, T b) => (dynamic)a + b;
    private static T Multiply(T a, T b) => (dynamic)a * b;

    // Transpose the matrix
    public Matrix<T> Transpose()
    {
        var result = new Matrix<T>(Columns, Rows);
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Columns; j++)
                result[j, i] = _data[i, j];
        return result;
    }

    // String representation for debugging
    public override string ToString()
    {
        var result = "";
        for (int i = 0; i < Rows; i++)
        {
            result += "[ ";
            for (int j = 0; j < Columns; j++)
            {
                result += $"{_data[i, j]} ";
            }
            result += "]\n";
        }
        return result;
    }
}