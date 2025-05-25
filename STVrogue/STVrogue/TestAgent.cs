using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using STVrogue.GameLogic;

namespace STVrogue;

public class TestAgent
{
    public TestAgent() {
        // Constructor for the TestAgent class.
        // You can initialize any necessary variables or data structures here.
    }
    
    /// <summary>
    /// This method is called to determine the next action for the agent. It takes the console output
    /// from the game and the current game state as input.
    /// </summary>
    public virtual char NextAction(List<string> consoleOutput, Game gameState) {
        // This method should be overridden in subclasses to provide specific behavior.
        throw new NotImplementedException("Not implemented, use a subclass.");
    }
}