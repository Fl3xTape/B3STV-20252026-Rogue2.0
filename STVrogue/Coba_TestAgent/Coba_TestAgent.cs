using STVrogue;
using STVrogue.GameLogic;
using STVrogue.Utils;

// Demonstrating the use of a TestAgent to play and test
// the game STVRogue game.

namespace TestProject1;

/// <summary>
/// Implementing my own TestAgent. This agent will simply do
/// the command move-east 3x then quit the game.
/// </summary>
class MyTestAgent : TestAgent
{
    char[] _commands = { 'm', 'e', 'm', 'e', 'm', 'e' };
    int _currentCommand = 0 ;

    public override char NextAction(List<string> consoleOutput, Game gameState)
    {
        // adding a deliberate delay so you can see the output.
        // System.Threading.Thread.Sleep(1000);
        if (_currentCommand < _commands.Length)
        {
            char command = _commands[_currentCommand];
            _currentCommand++;
            return command;
        }
        else
        {
            return 'q'; // Quit after all commands are executed
        }
    }
}

[TestFixture]
public class Coba_TestAgent
{

    /// <summary>
    /// A simple example of how to run the test-agent above on an instance of
    /// the STVRogue game. In this example, no particular property is
    /// checked other than the fact that the game runs without chrashing.
    /// </summary>
    [Test]
    public void Test1()
    {
        STVLogger.Log(">>> Test");
        // (1) reading the configuration of the game-level to generate
        GameConfiguration conf = new GameConfiguration("rogueconfig.txt");
        // (2) create an instance of a Game:
        Game game = new Game(conf);
        // create a test agent
        TestAgent agent = new MyTestAgent();
        // (4) attach the Game and the test-agent to a runner. The runner contains the logic of the game's
        // main-loop.
        GameRunner runner = new GameRunner(game, new GameConsole(), agent);
        // (5) Run the main-loop:
        runner.Run();
    }
}

[TestFixture]
public class TestAgentTests
{
    public int N = 20;

    public GameConfiguration GetRandomConfig(Random rng, DifficultyMode mode, DungeonShapeType shape)
    {
        GameConfiguration config = new GameConfiguration();
        config.DifficultyMode = mode;
        config.DungeonShape = shape;
        config.NumberOfRooms = 15;//rng.Next(20) + 15;
        config.RndSeed = 1000;//rng.Next(int.MaxValue);
        config.InitialNumberOfHealingPots = 1; //rng.Next(5) + 5;
        config.InitialNumberOfMonsters = 1; //rng.Next(config.NumberOfRooms - 2);
        config.InitialNumberOfRagePots = 1; //rng.Next(5) + 5;
        config.MaxRoomCapacity = config.NumberOfRooms;
        return config;
    }
    
    [TestCase(DifficultyMode.NEWBIEmode, DungeonShapeType.LINEAR)]
    [TestCase(DifficultyMode.NEWBIEmode, DungeonShapeType.GRID)]
    [TestCase(DifficultyMode.NEWBIEmode, DungeonShapeType.TREE)]
    [TestCase(DifficultyMode.NORMALmode, DungeonShapeType.LINEAR)]
    [TestCase(DifficultyMode.NORMALmode, DungeonShapeType.GRID)]
    [TestCase(DifficultyMode.NORMALmode, DungeonShapeType.TREE)]
    [TestCase(DifficultyMode.ELITEmode, DungeonShapeType.LINEAR)]
    [TestCase(DifficultyMode.ELITEmode, DungeonShapeType.GRID)]
    [TestCase(DifficultyMode.ELITEmode, DungeonShapeType.TREE)]
    public void RanTest(DifficultyMode mode, DungeonShapeType shape)
    {
        Random rng = new Random();
        GameConfiguration config = GetRandomConfig(rng, mode, shape);
        TestAgent agent = new RandomTestAgent(100);
        for (int i = 0; i < N; i++)
        {
            config.RndSeed = rng.Next(int.MaxValue);
            Game game = new Game(config);
            GameRunner runner = new GameRunner(game, new GameConsole(), agent);
            runner.Run();
        }
    }
}