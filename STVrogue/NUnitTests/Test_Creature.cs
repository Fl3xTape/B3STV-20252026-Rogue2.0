using System;
using System.Diagnostics;
using NUnit.Framework;
using STVrogue.GameLogic;
using STVrogue.Utils;

namespace NUnitTests
{
    [TestFixture]
    public class Test_Creature
    {
        [TestCase(Direction.NORTH)]
        [TestCase(Direction.EAST)]
        [TestCase(Direction.SOUTH)]
        [TestCase(Direction.WEST)]
        public void TestPlayerMove(Direction dir)
        {
            Room start = new Room("start", RoomType.ORDINARYroom, 5);
            Room neighbor = new Room(dir.ToString().ToLower(), RoomType.ORDINARYroom, 5);
            start.Connect(neighbor, dir);
            
            Player p = new Player("P0", "Player0");
            p.Location = start;
            p.Move(neighbor);

            Assert.IsTrue(p.Location == neighbor);
        }
        
        [TestCase(Direction.NORTH)]
        [TestCase(Direction.EAST)]
        [TestCase(Direction.SOUTH)]
        [TestCase(Direction.WEST)]
        public void TestMonterMove(Direction dir)
        {
            Room start = new Room("start", RoomType.ORDINARYroom, 5);
            Room neighbor = new Room(dir.ToString().ToLower(), RoomType.ORDINARYroom, 5);
            start.Connect(neighbor, dir);
            
            Monster m = new Monster("M0", "Monster0");
            m.Location = start;
            m.Move(neighbor);

            Assert.IsTrue(m.Location == neighbor);
        }
        
        [Test]
        public void TestPlayerAttack()
        {
            Room room = new Room("room", RoomType.ORDINARYroom, 5);
            Player p = new Player("P0", "Player0");
            Monster m = new Monster("M0", "Monster0");
            
            p.Attack(m);
            
            Assert.IsTrue(m.Hp == 2);
        }
        
        [Test]
        public void TestMonsterAttack()
        {
            Room room = new Room("room", RoomType.ORDINARYroom, 5);
            Player p = new Player("P0", "Player0");
            Monster m = new Monster("M0", "Monster0");
            
            m.Attack(p);
            
            Assert.IsTrue(p.Hp == 19);
        }

        [Test]
        public void TestPlayerUse()
        {
            Player p = new Player("P0", "Player0");
            p.Bag.Add( new HealingPotion("H0",2));
        }
    }
}