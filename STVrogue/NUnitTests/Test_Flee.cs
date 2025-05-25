using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace NUnitTests
{
    [TestFixture]
    public class Test_Flee
    {
        [Test]
        public void Test_flee( DifficultyMode dif )
        {
            // Setup
            var r1 = new Room("r1", RoomType.ORDINARYroom, 2);
            var r2 = new Room("r2", RoomType.ORDINARYroom, 1);
            var exit = new Room("exit", RoomType.EXITroom, 0);
            
            r1.Connect(exit, Direction.SOUTH);
            
            var dungeon = new DummyDungeon();
            
            dungeon.Rooms.Add(r1);
            dungeon.Rooms.Add(exit);
            dungeon.ExitRoom = exit;
            
            var player = new Player("P0", "Stitch");
            var m1 = new Monster("m1", "Lilo");
            var config = new GameConfiguration();
            var game = new Game(config);
            
            m1.Location = r1;
            player.Location = r1;
            
            // r1 -- exit
            // The player can't go to the end with Flee
            Assert.IsFalse(player.Flee(game, new RandomGenerator())); // The generator is a required input, but unused
            
            r1.Disconnect(exit);
            r1.Connect(r2, Direction.EAST);
            dungeon.Rooms.Add(r2);
            dungeon.Rooms.Remove(exit);
            
            // r1 -- r2
            // The player can flee in basic situations (didn't use item and not enraged and no exit to flee towards)
            // in every gamemode
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            
            // The player can't flee in NormalMode and above if it just used an item
            player.TurnsUntilFlee = 2; // Arbitrary, above 0
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsFalse(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsFalse(player.Flee(game, new RandomGenerator()));
            
            // The player can't flee in EliteMode if they are enraged
            player.TurnsUntilFlee = 0;
            player.EnragedTurns = 4; // Arbitrary, above 0
            config.DifficultyMode = DifficultyMode.NEWBIEmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.NORMALmode;
            Assert.IsTrue(player.Flee(game, new RandomGenerator()));
            config.DifficultyMode = DifficultyMode.ELITEmode;
            Assert.IsFalse(player.Flee(game, new RandomGenerator()));
            
            // Add a monster to room 1 and test flee to room 2
            var m2 = new Monster("m3", "Nani");
            m2.Location = r1;
            Assert.IsTrue(m1.Flee(game, new RandomGenerator()));
            
            // Flee the other monster to a full capacity room
            Assert.IsTrue(m2.Flee(game, new RandomGenerator()));
        }
    }
    
}