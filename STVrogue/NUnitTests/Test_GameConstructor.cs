using System;
using System.Linq;
using NUnit.Framework;
using STVrogue.GameLogic;
using static STVrogue.Utils.HelperPredicates;

namespace NUnitTests
{  
    public class Test_GameConstructor
    {
        [Test, Pairwise]
        public void combinatoric_Game(
            [Values(DungeonShapeType.LINEAR, DungeonShapeType.TREE, DungeonShapeType.GRID)] DungeonShapeType shape,
            [Values(-1, 0,5,15,50,100)]int N,
            [Values(-1, 0,5,15,30, int.MaxValue)]int y,
            [Values(-1, 0, 5, 10, 30, 50)]int M,
            [Values(-1, 0, 5, 10, 30, 50)]int H,
            [Values(-1, 0, 5, 10, 30, 50)]int R,
            [Values(DifficultyMode.NEWBIEmode, DifficultyMode.NORMALmode, DifficultyMode.ELITEmode)]DifficultyMode dif
            )
        {    
            // We feed the parameters to the Game-constructor through a new config
            GameConfiguration config = new GameConfiguration();
            config.DungeonShape = shape;
            config.NumberOfRooms = N;
            config.MaxRoomCapacity = y;
            config.InitialNumberOfMonsters = M;
            config.InitialNumberOfHealingPots = H;
            config.InitialNumberOfRagePots = R;
            config.DifficultyMode = dif;

            if (N < 3 || y <= 0)
            {
                Game fail;
                Assert.Throws<ArgumentOutOfRangeException>(() => fail = new Game(config));
                return;
            }
            
            // The game makes a dungeon, which we can test for rooms, shape, and numbers passed (see above)
            Game game = new Game(config);
            // Number of rooms is correct
            Assert.IsTrue(game.Dungeon.Rooms.Count == N);
            // Shape is correct
            if (shape == DungeonShapeType.LINEAR)
                Assert.IsTrue(IsLinear(game.Dungeon));
            else if (shape == DungeonShapeType.TREE)
                Assert.IsTrue(IsTree(game.Dungeon));
            else if (shape == DungeonShapeType.GRID)
                Assert.IsTrue(IsGrid(game.Dungeon));
                
            // Number of monsters is correct
            Assert.IsTrue(game.Dungeon.Creatures.Count(c => c is Monster) == M);
            Assert.IsTrue(Forall(game.Dungeon.Creatures, creature => creature.Alive));
            // Number of healing potions is correct
            Assert.IsTrue(game.Dungeon.Items.Count(i => i is HealingPotion) == H);
            // Number of rage potions is correct
            Assert.IsTrue(game.Dungeon.Items.Count(i => i is RagePotion) == R);
            
            // Difficulty must be correct
            Assert.IsTrue(game.Config.DifficultyMode == dif);
            
            // Check the player
            Assert.IsTrue(game.Player.Location == game.Dungeon.StartRoom);
            Assert.IsTrue(game.Player.Hp == game.Player.HpMax);
            Assert.IsTrue(game.Player.Hp > 0);
        }
    }
}