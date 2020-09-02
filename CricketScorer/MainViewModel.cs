using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CricketScorer.Extensions;

namespace CricketScorer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            _currentMatch = new Match();
            CurrentMatch.HomeTeam = new Team();
        }

        private Ball _proposedBall;
        public Ball ProposedBall
        {
            get => _proposedBall;
            set
            {
                if (value == _proposedBall) return;
                _proposedBall = value;
                OnPropertyChanged();
            }
        }

        private Match _currentMatch;
        public Match CurrentMatch
        {
            get => _currentMatch;
            set
            {
                if (value == _currentMatch) return;
                _currentMatch = value;
                OnPropertyChanged();
            }
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Commands

        private ICommand _runsScoredCommand;
        public ICommand RunsScoredCommand
        {
            get
            {
                return _runsScoredCommand ??= new SimpleDelegateCommand(prm =>
                {
                    if (CurrentMatch == null)
                    {
                        Trace.TraceError("Match is still null. Start a match before attempting to add runs.");
                        return;
                    }

                    object[] args = (object[]) prm;
                    ProposedBall = SetRunsScored((int) args[0], (RunType) args[1]);
                });
            }
        }

        private ICommand _outCommand;
        public ICommand OutCommand
        {
            get
            {
                return _outCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    if (CurrentMatch == null)
                    {
                        Trace.TraceError("Match is still null. Start a match before attempting to add outs.");
                        return;
                    }

                    if (AcceptBall() != null)
                    {
                        ProposedBall = null;
                    }
                    else
                    {
                        Trace.TraceError("Failed to add new ball : " + ProposedBall);
                    }
                });
            }
        }

        private ICommand _runsShortCommand;
        public ICommand RunsShortCommand
        {
            get
            {
                return _runsShortCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    if (CurrentMatch == null)
                    {
                        Trace.TraceError("Match is still null. Start a match before attempting to add runs short.");
                    }

                    if (!int.TryParse((string) prm, out int result)) return;

                    RunsShort(result);
                    OnPropertyChanged(nameof(ProposedBall));
                });
            }
        }

        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    Trace.WriteLine("Clearing ball.");
                    ProposedBall = null;
                });
            }
        }

        private ICommand _newMatchCommand;
        public ICommand NewMatchCommand
        {
            get
            {
                return _newMatchCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    Trace.WriteLine("New game.");
                });
            }
        }

        private ICommand _loadMatchCommand;
        public ICommand LoadMatchCommand
        {
            get
            {
                return _loadMatchCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    Trace.WriteLine("Load game.");
                });
            }
        }

        private ICommand _QuitCommand;
        public ICommand QuitCommand
        {
            get
            {
                return _loadMatchCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    Trace.WriteLine("Quit.");
                    Environment.Exit(0);
                });
            }
        }

        private ICommand _aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                return _aboutCommand ??= new SimpleDelegateCommand((prm) =>
                {
                    Trace.WriteLine("About.");
                });
            }
        }

        #endregion

        public Ball AcceptBall()
        {
            Trace.WriteLine("Ball is being accepted : " + ProposedBall);

            return CurrentMatch.CurrentInnings.acceptBall(ProposedBall);
        }

        public Ball RunsShort(int numRunsShort)
        {
            Trace.WriteLine("Runs short : " + numRunsShort);

            if (ProposedBall != null)
            {
                ProposedBall.RunsScored.RunsShort = numRunsShort;
            }
            return ProposedBall;
        }

        /// <summary>
        /// This command will take an amount of runs of a type and create a new ball, assigning to it the runs scored.
        /// If the ball is already in existence, replacing the runs element of the ball.
        /// This allows updating of a ball while scoring (i.e. correcting runs -> byes, runs -> 4 etc).
        /// </summary>
        /// <param name="runCount">The number of runs scored.</param>
        /// <param name="runType">The <see cref="RunType"/> scored.</param>
        public Ball SetRunsScored(int runCount, RunType runType)
        {
            Trace.WriteLine("Run Scored " + runCount + ", runtype = " + runType);
            return _currentMatch.CurrentInnings.IncrementBall(runCount, runType);
        }

        public void SetOut(Player playerOut, OutType outType, Player dismisser, Player dismisser2, string dismissalNotes, bool batsmenCrossed)
        {
            var curBall = _currentMatch.CurrentInnings.CurrentBall ?? _currentMatch.CurrentInnings.IncrementBall(outType, playerOut, dismisser, dismisser2, "", batsmenCrossed);

            curBall.BatsmanOut = new BatsmanOut
            {
                BatterOut = playerOut,
                OutType = outType,
                DismisserOne = dismisser,
                DismisserTwo = dismisser2,
                DismissalNotes = dismissalNotes
            };
        }

        public void NewOver(Player bowler)
        {
            _currentMatch.CurrentInnings.IncrementOver(bowler);
        }

        public void StartNewGame() { }

    }

    public class Match : Base
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }

        public Team TossWinners { get; set; }
        public TossDecision TossDecision { get; set; }

        public string Competition { get; set; }
        public string Location { get; set; }

        public string EndOneName { get; set; } = "End One";
        public string EndTwoName { get; set; }  = "End Two";

        public string Grade { get; set; }
        public string Round { get; set; }

        public DateTime MatchStartDate { get; set; } = DateTime.Now;
        public DateTime MatchEndDate { get; set; }

        public string MatchResult { get; set; }

        public int CurrentInningsNumber { get; set; } = 0;

        public readonly List<Innings> InningsList = new List<Innings>();

        public Innings CurrentInnings
        {
            get => InningsList.At(CurrentInningsNumber);
            set => InningsList.Add(value);
        }

        public Dictionary<(DateTime, DateTime), String> Interruptions = new Dictionary<(DateTime, DateTime), string>();

        public Match(Team homeTeam, Team awayTeam, string competition, string location, string grade, string round)
        {
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            Competition = competition;
            Location = location;
            Grade = grade;
            Round = round;

            CurrentInnings = new Innings();
        }

        public Match()
        {
            CurrentInnings = new Innings();
            HomeTeam = new Team() {CaptainsName = "Lachy", TeamName = "Beecroft"};
            AwayTeam = new Team() { CaptainsName = "Bob", TeamName = "Castle Hill" };
            Location = "Howson Oval";
            MatchStartDate = DateTime.Now;
        }

        public void SetEndNames(string endOne, string endTwo)
        {
            EndOneName = endOne;
            EndTwoName = endTwo;
        }
    }

    public class Team : Base
    {
        public int TeamNumber { get; set; }
        
        public string TeamName { get; set; }
        
        public string CaptainsName { get; set; }
        
        public string Colour { get; set; }
        
        /// <summary>
        /// This player list represents the players in batting order for both teams.
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class Player : Base
    {
        public int PlayerTeamNumber { get; set; }
        public string PlayerName { get; set; }
    }

    /// <summary>
    /// This represents an innings that has been played, overs are added to this and balls added to the overs.
    /// There will be initially two batsmen and two bowlers supplied and those will be assigned to an end, the balls
    /// will then dictate how they swap etc. Abnormal operations such as mid-over bowler change are not yet supported.
    /// todo: Mid-over bowler change.
    /// </summary>
    public class Innings : Base
    {
        public int InningsNumber { get; set; }
        public int Order { get; set; }

        public DateTime InningsStartDate { get; set; }
        public DateTime InningsFinishDate { get; set; }

        public Player BowlerEndOne { get; set; }
        public Player BowlerEndTwo { get; set; }

        public Player Striker { get; set; }
        public Player NonStriker { get; set; }
        
        /// <summary>
        /// This represents the current over for this innings, if it's even it's the first bowler/end, odd = 2nd.
        /// </summary>
        public int CurrentOverNumber { get; set; } = 0;

        /// <summary>
        /// This represents the current ball number of the over, it's derived from the current over & number of balls.
        /// </summary>
        public int CurrentBallNumber => CurrentOver.BallCount;

        public List<Over> Overs { get; set; } = new List<Over>();

        public Team BattingTeam { get; set; }
        public Team FieldingTeam { get; set; }

        public string UmpireOneName { get; set; } = "Player Umpire";
        public string UmpireTwoName { get; set; } = "Player Umpire";

        public string Notes { get; set; }

        public Over CurrentOver => Overs[CurrentOverNumber];
        public Ball CurrentBall => CurrentOver.BallsInOver.At(CurrentBallNumber);
        public Player CurrentBowler => CurrentOverNumber % 2 == 0 ? BowlerEndOne : BowlerEndTwo;

        public Innings()
        {
            Overs.Add(new Over());
        }

        /// <summary>
        /// This function is used to increment the current ball in play. Runs should be added after the ball has been
        /// played, including the number of runs, type of runs (normal, bye, leg bye, no ball, wide etc).
        /// </summary>
        /// <returns>The new ball currently being played.</returns>
        public Ball IncrementBall(int runCount, RunType runType)
        {
            var newBall = new Ball(Striker, NonStriker, new RunsScored(runCount, runType));
            return newBall;
        }

        public Ball acceptBall(Ball toAccept)
        {
            Trace.WriteLine("Ball to be accepted : " + toAccept);

            return CurrentOver.addBall(toAccept);
        }

        /// <summary>
        /// This function is used to increment the current ball in play. Out type is required, as is the player out and
        /// the dismisser, if the out type has a second person (i.e. catch), this can be entered.
        /// </summary>
        /// <param name="outType"></param>
        /// <param name="playerOut"></param>
        /// <param name="dismisserOne"></param>
        /// <param name="dismisserTwo"></param>
        /// <param name="dismissalNotes"></param>
        /// <param name="batsmenSwapped"></param>
        /// <returns></returns>
        public Ball IncrementBall(OutType outType, Player playerOut, Player dismisserOne, Player dismisserTwo, string dismissalNotes, bool batsmenSwapped = false)
        {
            var playerNotOut = playerOut == Striker ? NonStriker : Striker;
            
            var newBall = new Ball(
                Striker, 
                NonStriker,
                new BatsmanOut(outType,
                    playerOut,
                    playerNotOut, 
                    dismisserOne,
                    dismisserTwo,
                    dismissalNotes));
            
            if (batsmenSwapped) SwapBatsmen();
            
            CurrentOver.BallsInOver.Add(newBall);
            return newBall;
        }

        /// <summary>
        /// This will increment the over with a bowler, if the bowler is not a current bowler then the bowler will be
        /// changed.
        /// </summary>
        /// <param name="bowler">The bowler to bowl the over.</param>
        /// <returns>The new over object.</returns>
        public Over IncrementOver(Player bowler)
        {
            // Set the previous over's finish time
            CurrentOver.OverFinish = DateTime.Now;
            
            // Increment the overs by one.
            CurrentOverNumber += 1;

            // Check if we're dealing with a new bowler
            // If this is end one then check the bowler is the same
            if (CurrentOverNumber % 2 == 0)
            {
                if (BowlerEndOne != bowler)
                {
                    // Swap bowler one
                    BowlerEndOne = bowler;
                }
            }
            // if this is end two
            else
            {
                if (BowlerEndTwo != bowler)
                {
                    // swap bowler two
                    BowlerEndTwo = bowler;
                }
            }

            var newOver = new Over {Bowler = CurrentBowler, OverNumber = CurrentOverNumber};
            
            Overs.Add(newOver);
            return newOver;
        }

        public void SwapBatsmen()
        {
            var temp = Striker;
            Striker = NonStriker;
            NonStriker = temp;
        }
    }

    public class Over : Base
    {
        public int OverNumber { get; set; }
        
        public Player Bowler { get; set; }

        public int BowlerPlayerNumber => Bowler.PlayerTeamNumber;
        public string BowlersName => Bowler.PlayerName;

        public DateTime OverStart = DateTime.Now;
        public DateTime OverFinish;

        public int BallCount => BallsInOver.Count;

        public List<Ball> BallsInOver { get; set; } = new List<Ball>();

        public Ball CurrentBall => BallsInOver.Count > 0 ? BallsInOver[BallCount - 1] : null;

        public int TotalOuts => BallsInOver.Count(b => b.BatsmanOut != null);

        public int TotalRunsScored
        {
            get
            {
                var totalRunsScored = BallsInOver.Sum(b => b.RunsScored.RunCount);
                var totalRunsShort = BallsInOver.Where(b => b.RunsScored.RunCount > 0).Sum(b => b.RunsScored.RunsShort);
                
                // If somehow there's no runs scored but the umpire has signalled runs short then return 0
                if (totalRunsScored == 0 && totalRunsShort > 0)
                    return 0;

                // Otherwise return the number of runs scored in the over
                return totalRunsScored - (totalRunsShort ?? 0);
            }
        }

        public Ball addBall(Ball toAdd)
        {
            if (toAdd == null)
                return null;

            BallsInOver.Add(toAdd);

            Trace.WriteLine("Balls in over now has : " + BallsInOver.Count + " balls.");

            return toAdd;
        }
    }

    public class Ball : Base, INotifyPropertyChanged
    {
        public int BallIndex { get; set; }
        public Player Striker { get; set; }
        public Player NonStriker { get; set; }
        public DateTime BallDateTime => Created;
        public RunsScored RunsScored { get; set; }
        public BatsmanOut BatsmanOut { get; set; }

        public Ball(Player striker, Player nonStriker, RunsScored runsScored)
        {
            Striker = striker;
            NonStriker = nonStriker;
            RunsScored = runsScored;
        }

        public Ball(Player striker, Player nonStriker, BatsmanOut batsmanOut)
        {
            Striker = striker;
            NonStriker = nonStriker;
            BatsmanOut = batsmanOut;
        }

        public override string ToString()
        {
            return Striker + " " 
                + RunsScored.RunCount + " " + RunsScored.RunType 
                + (RunsScored?.RunsShort > 0 ? "/" + RunsScored.RunsShort + " short" : "");
        }

        public string GetBallSymbol()
        {
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RunsScored : Base, INotifyPropertyChanged
    {
        public int RunCount { get; set; }
        public int? RunsShort { get; set; }
        public RunType RunType { get; set; }

        public RunsScored()
        {
        }

        public RunsScored(int runCount, RunType runType)
        {
            RunCount = runCount;
            RunType = runType;
        }

        public RunsScored(int runCount, int? runsShort, RunType runType)
        {
            RunCount = runCount;
            RunsShort = runsShort;
            RunType = runType;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class BatsmanOut : Base
    {
        public OutType OutType { get; set; }
        public Player BatterOut { get; set; }
        public Player BatterNotOut { get; set; }
        public Player DismisserOne { get; set; }
        public Player? DismisserTwo { get; set; }
        public string DismissalNotes { get; set; }

        public BatsmanOut()
        {
        }

        public BatsmanOut(OutType outType, Player batterOut, Player batterNotOut, Player dismisserOne, Player dismisserTwo, string dismissalNotes)
        {
            OutType = outType;
            BatterOut = batterOut;
            BatterNotOut = batterNotOut;
            DismisserOne = dismisserOne;
            DismisserTwo = dismisserTwo;
            DismissalNotes = dismissalNotes;
        }
    }

    public enum RunType
    {
        Dot = 0,
        Batted = 1,
        Byes = 2,
        LegByes = 3,
        Wides = 4,
        NoBalls = 5,
        Penalty = 6
    }

    public enum OutType
    {
        Bowled = 0,
        Lbw = 1,
        Caught = 2,
        CaughtAndBowled = 3,
        CaughtBehind = 4,
        Stumped = 5,
        RunOut = 6,
        HitWicket = 7,
        HitBallTwice = 8,
        ObstructingField = 9,
        TimedOut = 10,
        Retired = 11
    }

    public enum TossDecision
    {
        Bat = 0,
        Field = 1
    }

    public class Base 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
    }

    public class SimpleDelegateCommand : ICommand
    {
        Action<object> _executeDelegate;

        public SimpleDelegateCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public bool CanExecute(object parameter) { return true; }
        public event EventHandler CanExecuteChanged;
    }
}
