using System;
using System.Collections.Generic;
using STVrogue.GameLogic;

namespace STVrogue;

public class RandomTestAgent : TestAgent
{
    private int _amountOfActions;
    private char currentAction;
    private char lastAction = ' ';
    private int rand;
    
    public RandomTestAgent(int amountOfActions) {
        _amountOfActions = amountOfActions;
    }
    
    // completely random agent that performs a random action until it runs out of actions.
    public override char NextAction(List<string> consoleOutput, Game gameState) {
        switch (lastAction)
        {
            case 'm':
                // need direction
                lastAction = ' ';
                rand = new Random().Next(0, 4);
                switch (rand)
                {
                    case 0:
                        return 'n';
                    case 1:
                        return 'e';
                    case 2:
                        return 's';
                    case 3:
                        return 'w';
                }
                break;
            case 'u':
                // what item?
                break;
            case 'a':
                // which monster?
                break;
        }
        
        if (_amountOfActions <= 0)
        {
            return 'q';
        }
        _amountOfActions--;
        
        rand = new Random().Next(0, 6);
        
        switch (rand)
        {
            case 0:
                currentAction = ' ';
                break;
            case 1:
                currentAction = 'm';
                break;
            case 2:
                currentAction = 'p';
                break;
            case 3:
                currentAction = 'a';
                break;
            case 4:
                currentAction = 'f';
                break;
            case 5:
                currentAction = 'u';
                break;
        }
        
        lastAction = currentAction;
        return currentAction;
    }
}