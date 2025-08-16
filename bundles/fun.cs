using System;
using System.Threading;
using System.Collections.Generic;

public class Fun
{
    public void Matrix(string[] args, CancellationToken token)
    {
        ConsoleColor color = ConsoleColor.Green;
        string style = "rainfall";

        foreach (var arg in args)
        {
            if (arg.StartsWith("--color="))
            {
                var colorName = arg.Substring(8);
                if (Enum.TryParse(colorName, true, out ConsoleColor parsedColor))
                    color = parsedColor;
            }
            else if (arg.StartsWith("--style="))
            {
                style = arg.Substring(8).ToLower();
            }
        }

        Console.CursorVisible = false;
        Random r = new Random();
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        char[] blockChars = new char[] { '█', '░', '▒', '▓' };

        int[] columnPositions = new int[width];
        for (int i = 0; i < width; i++)
            columnPositions[i] = r.Next(height);

        ConsoleColor DarkShade(ConsoleColor c)
        {
            return c switch
            {
                ConsoleColor.Blue => ConsoleColor.DarkBlue,
                ConsoleColor.Cyan => ConsoleColor.DarkCyan,
                ConsoleColor.Gray => ConsoleColor.DarkGray,
                ConsoleColor.Green => ConsoleColor.DarkGreen,
                ConsoleColor.Magenta => ConsoleColor.DarkMagenta,
                ConsoleColor.Red => ConsoleColor.DarkRed,
                ConsoleColor.Yellow => ConsoleColor.DarkYellow,
                ConsoleColor.White => ConsoleColor.Gray,
                _ => c
            };
        }

        while (true)
        {
            if (token.IsCancellationRequested)
            {
                Console.ResetColor();
                Console.CursorVisible = true;
                return;
            }

            switch (style)
            {
                case "chars.print":
                    Console.ForegroundColor = color;
                    for (int i = 0; i < width; i++)
                        Console.Write((char)r.Next(33, 126));
                    Console.WriteLine();
                    break;

                case "chars.random":
                    Console.ForegroundColor = color;
                    Console.SetCursorPosition(r.Next(width), r.Next(height));
                    Console.Write((char)r.Next(33, 126));
                    break;

                case "blocks.print":
                    Console.ForegroundColor = color;
                    for (int i = 0; i < width; i++)
                        Console.Write(blockChars[r.Next(blockChars.Length)]);
                    Console.WriteLine();
                    break;

                case "blocks.random":
                    Console.ForegroundColor = color;
                    Console.SetCursorPosition(r.Next(width), r.Next(height));
                    Console.Write(blockChars[r.Next(blockChars.Length)]);
                    break;

                default:
                    for (int i = 0; i < width; i++)
                    {
                        Console.SetCursorPosition(i, columnPositions[i]);
                        Console.ForegroundColor = color;
                        Console.Write((char)r.Next(33, 126));

                        if (columnPositions[i] > 0)
                        {
                            Console.SetCursorPosition(i, columnPositions[i] - 1);
                            Console.ForegroundColor = DarkShade(color);
                            Console.Write((char)r.Next(33, 126));
                        }

                        if (columnPositions[i] > 1)
                        {
                            Console.SetCursorPosition(i, columnPositions[i] - 2);
                            Console.Write(' ');
                        }

                        columnPositions[i]++;
                        if (columnPositions[i] >= height)
                            columnPositions[i] = 0;
                    }
                    break;
            }

            Thread.Sleep(5);
        }
    }

    public void Pipes(string[] args, CancellationToken token)
    {
        Console.CursorVisible = false;
        Console.Clear();
        Random r = new Random();
        ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        char? customChar = null;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--char=") && arg.Length > 7)
            {
                customChar = arg[7];
            }
        }

        Queue<ConsoleColor> lastColors = new Queue<ConsoleColor>();

        while (!token.IsCancellationRequested)
        {
            int x, y;
            int dirX = 0, dirY = 0;
            int edge = r.Next(4);
            switch (edge)
            {
                case 0:
                    x = r.Next(width);
                    y = 0;
                    dirX = 0; dirY = 1;
                    break;
                case 1:
                    x = r.Next(width);
                    y = height - 1;
                    dirX = 0; dirY = -1;
                    break;
                case 2:
                    x = 0;
                    y = r.Next(height);
                    dirX = 1; dirY = 0;
                    break;
                default:
                    x = width - 1;
                    y = r.Next(height);
                    dirX = -1; dirY = 0;
                    break;
            }

            ConsoleColor color;
            do
            {
                color = colors[r.Next(colors.Length)];
            } while (color == ConsoleColor.Black || lastColors.Contains(color));

            lastColors.Enqueue(color);
            if (lastColors.Count > 3)
                lastColors.Dequeue();

            int prevDirX = dirX;
            int prevDirY = dirY;

            while (x > 0 && x < width && y > 0 && y < height)
            {
                if (token.IsCancellationRequested) break;

                int nextDirX = dirX;
                int nextDirY = dirY;
                if (r.NextDouble() < 0.2)
                {
                    if (dirX != 0)
                    {
                        nextDirX = 0;
                        nextDirY = r.Next(2) == 0 ? 1 : -1;
                    }
                    else
                    {
                        nextDirY = 0;
                        nextDirX = r.Next(2) == 0 ? 1 : -1;
                    }
                }

                char c;
                if (customChar.HasValue)
                {
                    c = customChar.Value;
                }
                else
                {
                    if (dirX == 0 && dirY != 0 && nextDirX == 0) c = '║';
                    else if (dirY == 0 && dirX != 0 && nextDirY == 0) c = '═';
                    else if (dirX == 0 && dirY == 1 && nextDirX == 1) c = '╚';
                    else if (dirX == 0 && dirY == 1 && nextDirX == -1) c = '╝';
                    else if (dirX == 0 && dirY == -1 && nextDirX == 1) c = '╔';
                    else if (dirX == 0 && dirY == -1 && nextDirX == -1) c = '╗';
                    else if (dirX == 1 && dirY == 0 && nextDirY == 1) c = '╗';
                    else if (dirX == 1 && dirY == 0 && nextDirY == -1) c = '╝';
                    else if (dirX == -1 && dirY == 0 && nextDirY == 1) c = '╔';
                    else if (dirX == -1 && dirY == 0 && nextDirY == -1) c = '╚';
                    else c = '╬';
                }

                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = color;
                Console.Write(c);

                prevDirX = dirX;
                prevDirY = dirY;
                dirX = nextDirX;
                dirY = nextDirY;

                x += dirX;
                y += dirY;

                Thread.Sleep(1);
            }
        }

        Console.ResetColor();
        Console.CursorVisible = true;
    }
}