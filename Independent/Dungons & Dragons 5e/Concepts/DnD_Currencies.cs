using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using System.Text;

namespace Dungeons_and_Dragons
{
    public enum Currency 
    {
        Copper = 0,
        Silver = 1,
        Electrum = 2,
        Gold = 3,
        Platinum = 4,
    }

    internal static class DnD_CurrencyExtensions 
    {
        public static int GetToCopperRate(this Currency currency)
        {
            switch (currency) 
            {
                case Currency.Copper: return 1;
                case Currency.Silver: return 10;
                case Currency.Electrum: return 50;
                case Currency.Gold: return 100;
                case Currency.Platinum: return 1000;
                default:
                    UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(currency)); return 1;
            }
        }

        public static string GetShortString(this Currency cur) 
        {
            switch (cur) 
            {
                case Currency.Copper: return "cp";
                case Currency.Silver: return "sp";
                case Currency.Electrum: return "ep";
                case Currency.Gold: return "gp";
                case Currency.Platinum: return "pp";
                default: return cur.ToString();
            }
        } 
    }
 
    [System.Serializable]
    public class Wallet : IPEGI, IGotReadOnlyName
    {
        [UnityEngine.SerializeField] private List<int> _currencies = new List<int>();

        public int TotalCopper() 
        {
            int total = 0;
            for (int i = 0; i < _currencies.Count; i++) 
            {
                int amount = _currencies[i];
                if (amount > 0)
                {
                    total += amount * ((Currency)i).GetToCopperRate();
                }
            }
            return total;
        }

        public int this[Currency cur] 
        {
            get 
            {
                int index = (int)cur;
                if (index >= _currencies.Count)
                    return 0;

                return _currencies[index];
            }

            set 
            {
                int index = (int)cur;
                _currencies.ForceSet(index, value);
            }
        }

        public static bool operator >=(Wallet a, Wallet b) => a.TotalCopper() >= b.TotalCopper();
        public static bool operator <=(Wallet a, Wallet b) => a.TotalCopper() <= b.TotalCopper();
        public static bool operator >(Wallet a, Wallet b) => a.TotalCopper() > b.TotalCopper();
        public static bool operator <(Wallet a, Wallet b) => a.TotalCopper() < b.TotalCopper();

        #region Inspector

        public void Inspect()
        {
            for (int i=0; i<5; i++) 
            {
                var enm = (Currency)i;
                var val = this[enm];

                if (i>0 && icon.Up.Click()) 
                {
                    
                }

                if (enm.ToString().PegiLabel().editDelayed(ref val))
                    this[enm] = val;


                pegi.nl();
            }

        }

        public string GetReadOnlyName()
        {
            var sb = new StringBuilder();
            bool anyMoney = false;

            for (int i=4; i>=0; i--) 
            {
                var cur = (Currency)i;
                var amount = this[cur];
                if (amount > 0)
                {
                    anyMoney = true;
                    if (sb.Length > 0)
                        sb.Append(", ");

                    sb.Append(amount.ToString()).Append(cur.GetShortString());
                }
            }

            return anyMoney ? sb.ToString() : "EMPTY";
        }
        #endregion

    }

}
