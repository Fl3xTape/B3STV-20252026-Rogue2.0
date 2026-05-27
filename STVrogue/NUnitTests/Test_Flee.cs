using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace NUnitTests
{
    class DummyGame : Game
    {
        public DummyGame(GameConfiguration config) : base()
        {
            RandomGenerator.SetSeed(0);
            rnd = RandomGenerator.Instance;
            Config = config;
        }
    }
    
    [TestFixture]
    public class Test_Flee
    {
        // r1 -- exit
        // The player can't go to the end with Flee
        [Test]
        public void Test_flee_exit()
        {
            // Setup
            Room r1 = new Room("r1", RoomType.ORDINARYroom, 1);
            Room exit = new Room("exit", RoomType.EXITroom, 0);
            
            r1.Connect(exit, Direction.EAST);
            
            var dungeon = new DummyDungeon();
            
            dungeon.Rooms.Add(r1);
            dungeon.Rooms.Add(exit);
            dungeon.ExitRoom = exit;
            
            var player = new Player("P0", "Stitch");
            var config = new GameConfiguration();
            var game = new DummyGame(config);
            
            player.Location = r1;
            
            Assert.IsFalse(player.Flee(game));
            
            
        }

        [Test]
        public void Test_flee()
        {
            Room r1 = new Room("r1", RoomType.ORDINARYroom, 1);
            Room r2 = new Room("r2", RoomType.ORDINARYroom, 1);
            
            r1.Connect(r2, Direction.EAST);
            
            var dungeon = new DummyDungeon();
            
            dungeon.Rooms.Add(r1);
            dungeon.Rooms.Add(r2);
            
            var player = new Player("P0", "Stitch");
            var config = new GameConfiguration();
            var game = new DummyGame(config);
            
            player.Location = r1;
            // r1 -- r2
            // The player can flee in basic situations (didn't use item and not enraged and no exit to flee towards)
            // in every gamemode
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsTrue(player.Flee(game));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsTrue(player.Flee(game));
            
            // The player can't flee in NormalMode and above if it just used an item
            player.TurnsUntilFlee = 1; // Arbitrary, above 0
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsFalse(player.Flee(game));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsFalse(player.Flee(game));
            
            // The player can't flee in EliteMode if they are enraged
            player.TurnsUntilFlee = 0;
            player.EnragedTurns = 4; // Arbitrary, above 0
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsTrue(player.Flee(game));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsFalse(player.Flee(game));
            
            // Add a monster to room 1 and test flee to room 2
            var m1 = new Monster("m1", "Omae wa mou shindeiru");
            var m2 = new Monster("m2", "Nani");

            m1.Location = r1;
            Assert.IsTrue(m1.Flee(game));
            
            // Flee the other monster to a full capacity room
            m2.Location = r1;
            Assert.IsFalse(m2.Flee(game));
        }
    }
    
}