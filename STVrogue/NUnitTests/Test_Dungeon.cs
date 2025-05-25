using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;
using static STVrogue.Utils.HelperPredicates;

namespace NUnitTests
{
    
    [TestFixture]
    public class Test_Dungeon
    {
        private IRandomGenerator rnd;

        // This a parameterized test for the constructor of the Dungeon class, in particular for
        // the linear shaped dungeon. The test is parameterized over the number of rooms N and
        // the maximum capacity of the rooms.
        [TestCase(2, 1)]     // not enough rooms (exception path)
        [TestCase(3, 0)]     // not enough capacity (exception path)
        [TestCase(4, 2)]     // should cover all other paths
        [TestCase(100, 100)] // testing consistency
        public void test_LinearDungeon(int N, int capacity)
        {
            rnd = RandomGenerator.Instance;
            
            // we cannot have a linear dungeon with less than 3 rooms
            if (N < 3)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.LINEAR, N, capacity));
                return;
            }
            // we cannot have a dungeon with less than 1 maximum capacity
            if (capacity < 1)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.LINEAR, N, capacity));
                return;
            }

            Dungeon dungeon = new Dungeon(rnd, DungeonShapeType.LINEAR, N, capacity);
            // we have N rooms:
            Assert.IsTrue(dungeon.Rooms.Count == N);
            // each room has a unique ID:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.Rooms.Count(r2 => r2.Id == r.Id) == 1));
            // each room has a capacity between 0 and capacity:
            Assert.That(Forall(dungeon.Rooms, r => r.Capacity >= 0 && r.Capacity <= capacity));
            // there is a unique startroom:
            Assert.IsTrue(dungeon.StartRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.StartRoom) == 1);
            // there is a unique exitroom:
            Assert.IsTrue(dungeon.ExitRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.ExitRoom) == 1);
            // all rooms are reachable from the startroom:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.StartRoom.ReachableRooms().Contains(r)));
            // start and exit rooms have capacity 0:
            Assert.IsTrue(dungeon.StartRoom.Capacity == 0);
            Assert.IsTrue(dungeon.ExitRoom.Capacity == 0);
            // rooms neighbouring the exit room have maximum capacity:
            Assert.IsTrue(Forall(dungeon.ExitRoom.Neighbors, x => x.Item1.Capacity == capacity));
            // each room has at most 2 neighbours:
            Assert.IsTrue(Forall(dungeon.Rooms, r => r.Neighbors.Count <= 2));
            // the dungeon is linear, meaning there are no corners:
            Assert.IsTrue(Forall(dungeon.Rooms, r => r == dungeon.StartRoom ||
                                                     r == dungeon.ExitRoom ||
                                                     r.Neighbors[0].Item2 == Opposite(r.Neighbors[1].Item2)));
        }
        
        [TestCase(4, 1)]     // not enough rooms (exception path)
        [TestCase(5, 0)]     // not enough capacity (exception path)
        [TestCase(8, 2)]     // should cover all other paths
        [TestCase(100, 100)] // testing consistency
        public void test_TreeDungeon(int N, int capacity)
        {
            RandomGenerator.SetSeed(123);
            rnd = RandomGenerator.Instance;
            
            // we cannot have a tree dungeon with less than 5 rooms
            if (N < 5)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.TREE, N, capacity));
                return;
            }
            // we cannot have a dungeon with less than 1 maximum capacity
            if (capacity < 1)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.TREE, N, capacity));
                return;
            }

            Dungeon dungeon = new Dungeon(rnd, DungeonShapeType.TREE, N, capacity);
            // we have N rooms:
            Assert.IsTrue(dungeon.Rooms.Count == N);
            // each room has a unique ID:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.Rooms.Count(r2 => r2.Id == r.Id) == 1));
            // each room has a capacity between 0 and capacity:
            Assert.That(Forall(dungeon.Rooms, r => r.Capacity >= 0 && r.Capacity <= capacity));
            // there is a unique startroom:
            Assert.IsTrue(dungeon.StartRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.StartRoom) == 1);
            // there is a unique exitroom:
            Assert.IsTrue(dungeon.ExitRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.ExitRoom) == 1);
            // all rooms are reachable from the startroom:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.StartRoom.ReachableRooms().Contains(r)));
            // start and exit rooms have capacity 0:
            Assert.IsTrue(dungeon.StartRoom.Capacity == 0);
            Assert.IsTrue(dungeon.ExitRoom.Capacity == 0);
            // rooms neighbouring the exit room have maximum capacity:
            Assert.IsTrue(Forall(dungeon.ExitRoom.Neighbors, x => x.Item1.Capacity == capacity));
            // dungeon may not contain a cycle, use DFS to check for cycles:
            HashSet<Room> visited = new HashSet<Room>();
            Assert.IsFalse(HasCycle(dungeon.StartRoom, null, ref visited));
            // the dungeon is not linear, meaning there is a corner somewhere:
            Assert.IsTrue(dungeon.Rooms.Any(r => r.Neighbors.Count > 2 || r.Neighbors[0].Item2 != Opposite(r.Neighbors[1].Item2)));
            // exit room may only have 1 neighbour
            Assert.IsTrue(dungeon.ExitRoom.Neighbors.Count == 1);
        }

        [TestCase(3, 1)]     // not enough rooms (exception path)
        [TestCase(4, 0)]     // not enough capacity (exception path)
        [TestCase(9, 2)]     // testing square grid
        [TestCase(10, 3)]    // testing row with 1 room where exit has only 1 neighbour
        [TestCase(15, 3)]
        [TestCase(100, 100)] // testing consistency
        public void test_GridDungeon(int N, int capacity)
        {
            RandomGenerator.SetSeed(123);
            rnd = RandomGenerator.Instance;
            
            // we cannot have a tree dungeon with less than 4 rooms
            if (N < 4)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.GRID, N, capacity));
                return;
            }

            // we cannot have a dungeon with less than 1 maximum capacity
            if (capacity < 1)
            {
                Assert.Throws<ArgumentException>(() =>
                    new Dungeon(rnd, DungeonShapeType.GRID, N, capacity));
                return;
            }

            Dungeon dungeon = new Dungeon(rnd, DungeonShapeType.GRID, N, capacity);
            // we have N rooms:
            Assert.IsTrue(dungeon.Rooms.Count == N);
            // each room has a unique ID:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.Rooms.Count(r2 => r2.Id == r.Id) == 1));
            // each room has a capacity between 0 and capacity:
            Assert.That(Forall(dungeon.Rooms, r => r.Capacity >= 0 && r.Capacity <= capacity));
            // there is a unique startroom:
            Assert.IsTrue(dungeon.StartRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.StartRoom) == 1);
            // there is a unique exitroom:
            Assert.IsTrue(dungeon.ExitRoom != null);
            Assert.IsTrue(dungeon.Rooms.Count(r => r == dungeon.ExitRoom) == 1);
            // all rooms are reachable from the startroom:
            Assert.IsTrue(Forall(dungeon.Rooms, r => dungeon.StartRoom.ReachableRooms().Contains(r)));
            // start and exit rooms have capacity 0:
            Assert.IsTrue(dungeon.StartRoom.Capacity == 0);
            Assert.IsTrue(dungeon.ExitRoom.Capacity == 0);
            // rooms neighbouring the exit room have maximum capacity:
            Assert.IsTrue(Forall(dungeon.ExitRoom.Neighbors, x => x.Item1.Capacity == capacity));
        }

        Direction Opposite(Direction dir)
        {
            switch (dir)
            {
                case Direction.EAST:
                    return Direction.WEST;
                case Direction.WEST:
                    return Direction.EAST;
                case Direction.NORTH:
                    return Direction.SOUTH;
                case Direction.SOUTH:
                    return Direction.NORTH;
            }
            throw new Exception("Invalid direction");
        }
    }
}
