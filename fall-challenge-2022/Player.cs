using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using static Player;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine()?.Split(' ') ?? Array.Empty<string>();
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        var center = new Point(width/ 2, height / 2);

        Console.Error.WriteLine("width: " + width);
        Console.Error.WriteLine("height: " + height);
        // game loop
        while (true)
        {
            inputs = Console.ReadLine()?.Split(' ') ?? Array.Empty<string>();
                        
            int myMatter = int.Parse(inputs[0]);
            int oppMatter = int.Parse(inputs[1]);
            var tiles = new List<Tile>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    inputs = Console.ReadLine()?.Split(' ') ?? Array.Empty<string>();
                    var tile = new Tile(x, y, inputs);
                    tiles.Add(tile);
                }                
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            var tileManager = new TileManager(tiles);
            var commands = new List<string>();
            foreach (var tile in tiles)
            {
                //if (tile.CanBuild && !tile.InRangeOfRecycler && tileManager.GetScrapValue(tile) > 10)
                //{
                //    commands.Add($"BUILD {tile.Coordinates.X} {tile.Coordinates.Y}");
                //    tile.CanBuild = false;
                //    tile.InRangeOfRecycler = true;
                //    break;
                //}
                
                if (tileManager.IsMyClosestTileToCenter(tile, center) && tile.CanSpawn && myMatter >= 50 && ((tile.InRangeOfRecycler && tile.ScrapAmount > 1) || !tile.InRangeOfRecycler))
                {
                    var botsToSpawn = (int)(Math.Floor((decimal)(myMatter / 10)));
                    commands.Add($"SPAWN {botsToSpawn} {tile.Coordinates.X} {tile.Coordinates.Y}");
                    tile.CanSpawn = false;
                    myMatter -= (botsToSpawn * 10);
                }
                //I own tile and have units on it
                else if (tile.Owner == Owner.Me && tile.UnitCount > 0)
                {
                    for (int i = 0; i < tile.UnitCount; i++)
                    {
                        var closestEmptyTile = tileManager.GetClosestEmptyTile(tile);
                        Console.Error.WriteLine($"{closestEmptyTile?.Coordinates.X}-{closestEmptyTile?.Coordinates.Y}, Incoming bots {closestEmptyTile?.IncomingBots ?? 0}");
                        //if noone near to take over, move to center
                        if (closestEmptyTile != null)
                        {
                            commands.Add($"MOVE 1 {tile.Coordinates.X} {tile.Coordinates.Y} {closestEmptyTile.Coordinates.X} {closestEmptyTile.Coordinates.Y}");
                            closestEmptyTile.IncomingBots += 1;
                            Console.Error.WriteLine($"2! {closestEmptyTile?.Coordinates.X}-{closestEmptyTile?.Coordinates.Y}, Incoming bots {closestEmptyTile?.IncomingBots ?? 0}");
                        }
                        else
                        {
                            commands.Add($"MOVE 1 {tile.Coordinates.X} {tile.Coordinates.Y} {center.X} {center.Y}");
                        }
                    }

                        
                    tile.CanBuild = false;
                }


            }
            //Console.Error.WriteLine(string.Join(";", commands.ToArray()));
            Console.WriteLine(string.Join(";", commands.ToArray()));
        }
    }

    public enum Owner 
    { 
        Neutral = -1,
        Foe = 0, 
        Me = 1
    }

    public class Tile
    {
        public Tile(int x, int y, string[] inputs)
        {
            Coordinates = new Point(x, y);
            ScrapAmount = int.Parse(inputs[0]);
            Owner = (Owner)int.Parse(inputs[1]);
            UnitCount = int.Parse(inputs[2]);
            IsRecycler = int.Parse(inputs[3]) == 1;
            CanBuild = int.Parse(inputs[4]) == 1;
            CanSpawn = int.Parse(inputs[5]) == 1;
            InRangeOfRecycler = int.Parse(inputs[6]) == 1;
        }

        public Point Coordinates { get; set; }

        public int ScrapAmount { get; set; }

        public bool IsGrass => ScrapAmount == 0;

        public Owner Owner { get; set; }

        public int UnitCount { get; set; }

        public bool IsRecycler { get; set; }

        /// <summary>
        /// Can only build on tiles owned by me, and that are empty
        /// </summary>
        public bool CanBuild { get; set; }

        /// <summary>
        /// Can only spawn on tiles owned by me
        /// </summary>
        public bool CanSpawn { get; set; }

        public bool InRangeOfRecycler { get; set; }

        public int IncomingBots { get; set; } = 0;

        public override string ToString() =>
            $"{nameof(Coordinates)}:{Coordinates.X}:{Coordinates.Y}, " +
            $"{nameof(ScrapAmount)}:{ScrapAmount}, " +
            $"{nameof(IsGrass)}:{IsGrass}, " +
            $"{nameof(Owner)}:{Owner}, " +
            $"{nameof(UnitCount)}:{UnitCount}, " +
            $"{nameof(IsRecycler)}:{IsRecycler}, " +
            $"{nameof(CanBuild)}:{CanBuild}, " +
            $"{nameof(CanSpawn)}:{CanSpawn}, " +
            $"{nameof(InRangeOfRecycler)}:{InRangeOfRecycler}, " +
            $"{nameof(IncomingBots)}:{IncomingBots}";

        public Point North() => new(Coordinates.X, Coordinates.Y + 1);

        public Point South() => new(Coordinates.X, Coordinates.Y - 1);

        public Point East() => new(Coordinates.X + 1, Coordinates.Y);

        public Point West() => new(Coordinates.X - 1, Coordinates.Y);

        public ICollection<Point> CloseTiles => new List<Point> { North(), South(), East(), West() };

        public double Distance(Tile tile) => Distance(tile.Coordinates);

        public double Distance(Point coordinates) => Math.Sqrt(
                           Math.Pow(Math.Abs(coordinates.X - Coordinates.X), 2) +
                           Math.Pow(Math.Abs(coordinates.Y - Coordinates.Y), 2));
    }

    public class TileManager
    {
        public ICollection<Tile> Tiles { get; set; } = new List<Tile>();

        public ICollection<Tile> TilesToConquer => Tiles.Where(tile => ((tile.InRangeOfRecycler && tile.ScrapAmount > 1) || !tile.InRangeOfRecycler) && tile.ScrapAmount > 0 && tile.Owner != Owner.Me).ToList();

        public ICollection<Tile> MyTiles => Tiles.Where(tile => tile.CanSpawn && ((tile.InRangeOfRecycler && tile.ScrapAmount > 1) || !tile.InRangeOfRecycler)).ToList();

        public TileManager(ICollection<Tile> tiles) { Tiles = tiles; }

        public int GetScrapValue(Tile tile)
        {
            var nearbyTiles = GetNearbyTiles(tile);
            if (nearbyTiles.Any(x => x.InRangeOfRecycler))
                return 0;
            foreach (var nearbyTile in nearbyTiles)
                nearbyTile.InRangeOfRecycler = true;
            
            return nearbyTiles.Sum(x => x.ScrapAmount);
        }

        private ICollection<Tile> GetNearbyTiles(Tile tile) => 
            Tiles.Where(x => (x.Coordinates.X == tile.Coordinates.X && x.Coordinates.Y == tile.Coordinates.Y + 1)
                || (x.Coordinates.X == tile.Coordinates.X && x.Coordinates.Y == tile.Coordinates.Y - 1)
                || (x.Coordinates.X == tile.Coordinates.X + 1 && x.Coordinates.Y == tile.Coordinates.Y)
                || (x.Coordinates.X == tile.Coordinates.X - 1 && x.Coordinates.Y == tile.Coordinates.Y)).ToList();

        public Tile? GetTile(Point point) => Tiles.FirstOrDefault(x => x.Coordinates == point);

        public Tile? GetClosestEmptyTile(Tile tile) => TilesToConquer.OrderBy(x => x.IncomingBots).ThenBy(tile.Distance).FirstOrDefault();

        public bool IsMyClosestTileToCenter(Tile tile, Point center) => tile == MyTiles.OrderBy(x => x.IncomingBots).ThenBy(x => x.Distance(center)).First();
    }
}