# Poker game tree generator with CFR+ trainer

The project contains library which generates configurable poker game tree and performs training using Cfr+ algorithm. The result is written to DB (MySql provider is written). After that, it can be used to perform poker game play. Note that right now it contains only simple approximation of real NL Texas Holdem game.

## The PokerGraphgen application

### Command line
You can run PokerGraphgen with the following arguments:
```

  -c, --config     Path to json file with config. If not provided, default configuration is used (use
                   [-g] option to generate it to json file).

  -x, --xml        If set, only xml is generated.

  -i, --iter       (Default: 5000) Number of training iterations. The default value is 5000.

  -t, --tblName    (Default: nodes1) Name of output table name. Default is nodes1.

  -g, --genConf    Generate example config file to default.json and exit the program. It can be used to
                   build your own configuration.

  -s, --silent     No messages written on console

  --help           Display this help screen.

  --version        Display version information.
```

### Example configuration for relative betting
Use ```-g``` option to generate example ```default.json``` file and configure values in this file. Then run the program with ```-c``` option including configuration file name/path.

```javascript
{
  "PossibleRaises": [
    [
      150,	// bet 1,5x on pre-flop
	  200,	// bet 2x on pre-flop
    ],
    [
      150,	// bet 1,5x on flop
	  180,	// bet 1,8x on flop
    ],
    [
      200,	// bet 2x on turn
    ],
    [
      250,	// bet 2,5x on river
    ]
  ],
  // raiser can be re-raised at least one time by opponent, but he can raise re-raise
  "ReraiseAmount": 2,
  
  // should be always 2 (in current version)
  "NumPlayers": 2,
  
  // small blind value
  "SbValue": 1,
  
  // big blind value
  "BbValue": 2,
  
  // bankroll
  "Bankroll": 100,
  
  // relative betting enabled
  "RelativeBetting": true
}
```

### Example configuration for absolute betting

In *absolute betting mode*, bets are hardcoded for each round and not depends on current pot.

```javascript
{
  "PossibleRaises": [
    [
      4,	// just raise by 4
	  8,
    ],
    [
      12,
	  16,
    ],
    [
      20,
	  24,
    ],
    [
      24,
	  32,
    ]
  ],
  // in this case raiser can be re-raised at least one time by opponent and he can't re-raise again
  "ReraiseAmount": 1,
  
  // should be always 2 (in current version)
  "NumPlayers": 2,
  
  // small blind value
  "SbValue": 1,
  
  // big blind value
  "BbValue": 2,
  
  // bankroll
  "Bankroll": 100,
  
  // relative betting disabled
  "RelativeBetting": false
}
```

### Note about complexity

Adding only one possible bet can increase number of game tree possibilities exponentially. The same rule is for ```ReraiseAmount``` param. This is very important if you want to build precise poker game tree - the cost of building a bit more accurate model is growing incredibly.

The similar rule is applicable to possible hands, but increasing number of hands by 10x, increases number of db entries by 10x, so it's growing linearly by number of possible hands.

## The PokerPlayer application

PokerPlayer allows you to test your tree generated by CFR alghorithm and compare two different results. Player just performs poker deals with random hands and shows current bankroll for each player. Actions are performed randomly with respect to action weights generated by PokerGraphgen application.

### Command line arguments

```
  -a, --dbName1    Required. Name of first player db.

  -b, --dbName2    Required. Name of second player db.

  -t, --table      Table output.

  -h, --history    Detailed playing history.

  --help           Display this help screen.

  --version        Display version information.
```

## Train & Test

Perform following operations:

1. Prepare your config.json file with config
2. Run PokerGraphgen.exe -c config.json -t nodes10k -i 10000
3. Run PokerGraphgen.exe -c config.json -t nodes100k -i 100000
4. Run PokerPlayer.exe -a nodes10k -b nodes100k -t > results.txt
5. Wait 30 sec and press escape to end the player
6. Open results.txt, copy to excel and compare columns with current bankroll (relative to 0)

Note that starting amount is 0. For each deal player can win/lost $0 to $100. Play is performed until user press escape. In the end result for both players are displayed. 