using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;

namespace Server.Dueling
{
    public class DuelPoints
    {
        private Mobile _Mobile;

        public Mobile Mobile { get { return _Mobile; } }

        private Dictionary<int, DuelInfo> _FastestWins;
        private Dictionary<int, DuelInfo> _LongestWins;

        public Dictionary<int, DuelInfo> FastestWins { get { return _FastestWins; } }
        public Dictionary<int, DuelInfo> LongestWins { get { return _LongestWins; } }

        private Dictionary<int, DuelInfo> _FastestLoses;
        private Dictionary<int, DuelInfo> _LongestLoses;

        public Dictionary<int, DuelInfo> FastestLoses { get { return _FastestLoses; } }
        public Dictionary<int, DuelInfo> LongestLoses { get { return _LongestLoses; } }

        private Dictionary<int, int> _Wins;
        private Dictionary<int, int> _Loses;

        public Dictionary<int, int> Wins { get { return _Wins; } }
        public Dictionary<int, int> Loses { get { return _Loses; } }

        public DuelPoints( Mobile mobile )
        {
            _Mobile = mobile;

            _Wins = new Dictionary<int,int>();
            _Loses = new Dictionary<int,int>();

            _FastestWins = new Dictionary<int, DuelInfo>();
            _LongestWins = new Dictionary<int, DuelInfo>();

            _FastestLoses = new Dictionary<int, DuelInfo>();
            _LongestLoses = new Dictionary<int, DuelInfo>();
        }

        public DuelPoints( GenericReader reader )
        {
            int version = reader.ReadInt();

            switch( version )
            {
                case 0:
                    {
                        _Mobile = reader.ReadMobile();
                        _FastestWins = DuelController.ReadPointsDictionary( reader );
                        _LongestWins = DuelController.ReadPointsDictionary( reader );
                        _FastestLoses = DuelController.ReadPointsDictionary( reader );
                        _LongestLoses = DuelController.ReadPointsDictionary( reader );
                        _Wins = DuelController.ReadIntDictionary( reader );
                        _Loses = DuelController.ReadIntDictionary( reader );

                        break;
                    }
            }
        }

        public void AddWin( int numPerTeam, DuelInfo duelInfo )
        {
            if( _Wins.ContainsKey( numPerTeam ) )
                _Wins[numPerTeam]++;
            else
                _Wins.Add( numPerTeam, 1 );

            if( _FastestWins.ContainsKey( numPerTeam ))
                if(_FastestWins[numPerTeam].DuelTime.Seconds > duelInfo.DuelTime.Seconds)
                    _FastestWins[numPerTeam] = duelInfo;
            else
                _FastestWins[numPerTeam] = duelInfo ;

            if( _LongestWins.ContainsKey( numPerTeam ))
                if(_LongestWins[numPerTeam].DuelTime.Seconds < duelInfo.DuelTime.Seconds)
                    _LongestWins[numPerTeam] = duelInfo;
            else
                _LongestWins[numPerTeam] = duelInfo;
        }

        public void AddLoss( int numPerTeam, DuelInfo duelInfo )
        {
            if( _Loses.ContainsKey( numPerTeam ) )
                _Loses[numPerTeam]++;
            else
                _Loses[numPerTeam] = 1 ;

            if( _FastestLoses.ContainsKey( numPerTeam ))
                if(_FastestLoses[numPerTeam].DuelTime.Seconds > duelInfo.DuelTime.Seconds)
                    _FastestLoses[numPerTeam] = duelInfo;
            else
                _FastestLoses[numPerTeam] = duelInfo;

            if( _LongestLoses.ContainsKey( numPerTeam ))
                if(_LongestLoses[numPerTeam].DuelTime.Seconds < duelInfo.DuelTime.Seconds)
                    _LongestLoses[numPerTeam] = duelInfo;
            else
                _LongestLoses.Add( numPerTeam, duelInfo );
        }

        internal void Serialize( GenericWriter writer )
        {
            writer.Write( ( int )0 );

            writer.Write( ( Server.Mobile )_Mobile );

            DuelController.WritePointsDictionary( _FastestWins, writer );
            DuelController.WritePointsDictionary( _LongestWins, writer );
            DuelController.WritePointsDictionary( _FastestLoses, writer );
            DuelController.WritePointsDictionary( _LongestLoses, writer );
            DuelController.WriteIntDictionary( _Wins, writer );
            DuelController.WriteIntDictionary( _Loses, writer );
        }
    }
}
