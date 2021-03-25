using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public enum ChallangeRating
    {
        CR_0,
        CR_1_8th,
        CR_1_4th,
        CR_1_2lf,
        CR_1,
        CR_2,
        CR_3,
        CR_4,
        CR_5,
        CR_6,
        CR_7,
        CR_8,
        CR_9,
        CR_10,
        CR_11,
        CR_12,
        CR_13,
        CR_14,
        CR_15,
        CR_16,
        CR_17,
        CR_18,
        CR_19,
        CR_20,
        CR_21,
        CR_22,
        CR_23,
        CR_24,
        CR_25,
        CR_26,
        CR_27,
        CR_28,
        CR_29,
        CR_30,
    }

    public static class ChallangeRatingExtension
    {
        public static string GetReadableString(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0: return "0";
                case ChallangeRating.CR_1_8th: return "1/8";
                case ChallangeRating.CR_1_4th: return "1/4";
                case ChallangeRating.CR_1_2lf: return "1/2";
                default: return ((int)cr - 3).ToString();  //"Challange Rating {0} not implemented".F(cr)); return 2;
            }
        }


        public static int ProficiencyBonus(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0:
                case ChallangeRating.CR_1_8th:
                case ChallangeRating.CR_1_4th:
                case ChallangeRating.CR_1_2lf:
                case ChallangeRating.CR_1:
                case ChallangeRating.CR_2:
                case ChallangeRating.CR_3:
                case ChallangeRating.CR_4: return 2;
                case ChallangeRating.CR_5:
                case ChallangeRating.CR_6:
                case ChallangeRating.CR_7:
                case ChallangeRating.CR_8: return 3;
                case ChallangeRating.CR_9:
                case ChallangeRating.CR_10:
                case ChallangeRating.CR_11:
                case ChallangeRating.CR_12: return 4;
                case ChallangeRating.CR_13:
                case ChallangeRating.CR_14:
                case ChallangeRating.CR_15:
                case ChallangeRating.CR_16: return 5;
                case ChallangeRating.CR_17:
                case ChallangeRating.CR_18:
                case ChallangeRating.CR_19:
                case ChallangeRating.CR_20: return 6;
                case ChallangeRating.CR_21:
                case ChallangeRating.CR_22:
                case ChallangeRating.CR_23:
                case ChallangeRating.CR_24: return 7;
                case ChallangeRating.CR_25:
                case ChallangeRating.CR_26:
                case ChallangeRating.CR_27:
                case ChallangeRating.CR_28: return 8;
                case ChallangeRating.CR_29:
                case ChallangeRating.CR_30: return 9;
                default: Debug.LogError(QcLog.CaseNotImplemented(cr, context: "Proficiency Bonus")); return 2;//"Challange Rating {0} not implemented".F(cr)); return 2;
            }
        }

        public static int ExpectedArmorClass(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0: return 12;
                case ChallangeRating.CR_1_8th:
                case ChallangeRating.CR_1_4th:
                case ChallangeRating.CR_1_2lf:
                case ChallangeRating.CR_1:
                case ChallangeRating.CR_2:
                case ChallangeRating.CR_3: return 13;
                case ChallangeRating.CR_4: return 14;
                case ChallangeRating.CR_5:
                case ChallangeRating.CR_6:
                case ChallangeRating.CR_7: return 15;
                case ChallangeRating.CR_8:
                case ChallangeRating.CR_9: return 16;
                case ChallangeRating.CR_10:
                case ChallangeRating.CR_11:
                case ChallangeRating.CR_12: return 17;
                case ChallangeRating.CR_13:
                case ChallangeRating.CR_14:
                case ChallangeRating.CR_15:
                case ChallangeRating.CR_16: return 18;
                case ChallangeRating.CR_17:
                case ChallangeRating.CR_18:
                case ChallangeRating.CR_19:
                case ChallangeRating.CR_20:
                case ChallangeRating.CR_21:
                case ChallangeRating.CR_22:
                case ChallangeRating.CR_23:
                case ChallangeRating.CR_24:
                case ChallangeRating.CR_25:
                case ChallangeRating.CR_26:
                case ChallangeRating.CR_27:
                case ChallangeRating.CR_28:
                case ChallangeRating.CR_29:
                case ChallangeRating.CR_30: return 19;
                default: Debug.LogError(QcLog.CaseNotImplemented(cr, "Armor Class")); return 2;
            }
        }

        public static int ExpectedAttackBonus(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0:
                case ChallangeRating.CR_1_8th:
                case ChallangeRating.CR_1_4th:
                case ChallangeRating.CR_1_2lf:
                case ChallangeRating.CR_1:
                case ChallangeRating.CR_2: return 3;
                case ChallangeRating.CR_3: return 4;
                case ChallangeRating.CR_4: return 5;
                case ChallangeRating.CR_5:
                case ChallangeRating.CR_6:
                case ChallangeRating.CR_7: return 6;
                case ChallangeRating.CR_8:
                case ChallangeRating.CR_9:
                case ChallangeRating.CR_10: return 7;
                case ChallangeRating.CR_11:
                case ChallangeRating.CR_12:
                case ChallangeRating.CR_13:
                case ChallangeRating.CR_14:
                case ChallangeRating.CR_15: return 8;
                case ChallangeRating.CR_16: return 9;
                case ChallangeRating.CR_17:
                case ChallangeRating.CR_18:
                case ChallangeRating.CR_19:
                case ChallangeRating.CR_20: return 10;
                case ChallangeRating.CR_21:
                case ChallangeRating.CR_22:
                case ChallangeRating.CR_23: return 11;
                case ChallangeRating.CR_24:
                case ChallangeRating.CR_25:
                case ChallangeRating.CR_26: return 12;
                case ChallangeRating.CR_27:
                case ChallangeRating.CR_28:
                case ChallangeRating.CR_29: return 13;
                case ChallangeRating.CR_30: return 14;
                default: Debug.LogError(QcLog.CaseNotImplemented(cr, "Attack Bonus")); return 2;
            }
        }

        public static int ExpectedSaveDC(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0:
                case ChallangeRating.CR_1_8th:
                case ChallangeRating.CR_1_4th:
                case ChallangeRating.CR_1_2lf:
                case ChallangeRating.CR_1:
                case ChallangeRating.CR_2:
                case ChallangeRating.CR_3: return 13;
                case ChallangeRating.CR_4: return 14;
                case ChallangeRating.CR_5:
                case ChallangeRating.CR_6:
                case ChallangeRating.CR_7: return 15;
                case ChallangeRating.CR_8:
                case ChallangeRating.CR_9:
                case ChallangeRating.CR_10: return 16;
                case ChallangeRating.CR_11:
                case ChallangeRating.CR_12: return 17;
                case ChallangeRating.CR_13:
                case ChallangeRating.CR_14:
                case ChallangeRating.CR_15:
                case ChallangeRating.CR_16:
                case ChallangeRating.CR_17:
                case ChallangeRating.CR_18: return 18;
                case ChallangeRating.CR_19:
                case ChallangeRating.CR_20: return 19;
                case ChallangeRating.CR_21:
                case ChallangeRating.CR_22:
                case ChallangeRating.CR_23: return 20;
                case ChallangeRating.CR_24:
                case ChallangeRating.CR_25:
                case ChallangeRating.CR_26: return 21;
                case ChallangeRating.CR_27:
                case ChallangeRating.CR_28:
                case ChallangeRating.CR_29: return 22;
                case ChallangeRating.CR_30: return 23;
                default: Debug.LogError(QcLog.CaseNotImplemented(cr, context: "Save DC")); return 2;
            }
        }

        public static int GetExperience(this ChallangeRating cr)
        {
            switch (cr)
            {
                case ChallangeRating.CR_0: return 10;
                case ChallangeRating.CR_1_8th: return 25;
                case ChallangeRating.CR_1_4th: return 50;
                case ChallangeRating.CR_1_2lf: return 100;
                case ChallangeRating.CR_1: return 200;
                case ChallangeRating.CR_2: return 450;
                case ChallangeRating.CR_3: return 700;
                case ChallangeRating.CR_4: return 1100;
                case ChallangeRating.CR_5: return 1800;
                case ChallangeRating.CR_6: return 2300;
                case ChallangeRating.CR_7: return 2900;
                case ChallangeRating.CR_8: return 3900;
                case ChallangeRating.CR_9: return 5000;
                case ChallangeRating.CR_10: return 5900;
                case ChallangeRating.CR_11: return 7200;
                case ChallangeRating.CR_12: return 8400;
                case ChallangeRating.CR_13: return 10000;
                case ChallangeRating.CR_14: return 11500;
                case ChallangeRating.CR_15: return 13000;
                case ChallangeRating.CR_16: return 15000;
                case ChallangeRating.CR_17: return 18000;
                case ChallangeRating.CR_18: return 20000;
                case ChallangeRating.CR_19: return 22000;
                case ChallangeRating.CR_20: return 25000;
                case ChallangeRating.CR_21: return 33000;
                case ChallangeRating.CR_22: return 41000;
                case ChallangeRating.CR_23: return 50000;
                case ChallangeRating.CR_24: return 62000;
                case ChallangeRating.CR_25: return 75000;
                case ChallangeRating.CR_26: return 90000;
                case ChallangeRating.CR_27: return 105000;
                case ChallangeRating.CR_28: return 120000;
                case ChallangeRating.CR_29: return 135000;
                case ChallangeRating.CR_30: return 155000;
                default: Debug.LogError(QcLog.CaseNotImplemented(cr, context: "Experience")); return 2;
            }
        }

        private static int Avarage(int min, int max) 
        {
            return Mathf.FloorToInt((max + min) * 0.5f);
        }

        public static int ExpectedMinHp(this ChallangeRating cr) 
        {
            if (cr <= ChallangeRating.CR_0)
                return 1;

            int ind = (int)cr;
            ind -= 1;

            return ExpectedMaxHp((ChallangeRating)ind) + 1;
        }

        public static int ExpectedMaxHp(this ChallangeRating cr) 
        {
            if (cr < ChallangeRating.CR_0)
                return 1;

            switch (cr)
            {
                case ChallangeRating.CR_0:      return 6;
                case ChallangeRating.CR_1_8th:  return 35;
                case ChallangeRating.CR_1_4th:  return 49;
                case ChallangeRating.CR_1_2lf:  return 70;
                case ChallangeRating.CR_1:      return 85;
                case ChallangeRating.CR_2:      return 100;
                case ChallangeRating.CR_3:      return 115;
                case ChallangeRating.CR_4:      return 130;
                case ChallangeRating.CR_5:      return 145;
                case ChallangeRating.CR_6:      return 160;
                case ChallangeRating.CR_7:      return 175;
                case ChallangeRating.CR_8: return 190;
                case ChallangeRating.CR_9: return 205;
                case ChallangeRating.CR_10: return 220;
                case ChallangeRating.CR_11: return 235;
                case ChallangeRating.CR_12: return 250;
                case ChallangeRating.CR_13: return 265;
                case ChallangeRating.CR_14: return 280; 
                case ChallangeRating.CR_15: return 295;
                case ChallangeRating.CR_16: return 310;
                case ChallangeRating.CR_17: return 325;
                case ChallangeRating.CR_18: return 340;
                case ChallangeRating.CR_19: return 355;
                case ChallangeRating.CR_20: return 400;
                case ChallangeRating.CR_21: return 445;
                case ChallangeRating.CR_22: return 490;
                case ChallangeRating.CR_23: return 535;
                case ChallangeRating.CR_24: return 580;
                case ChallangeRating.CR_25: return 625;
                case ChallangeRating.CR_26: return 670;
                case ChallangeRating.CR_27: return 715;
                case ChallangeRating.CR_28: return 760;
                case ChallangeRating.CR_29: return 805;
                case ChallangeRating.CR_30: return 850;
            }

            Debug.LogError(QcLog.CaseNotImplemented(cr, context: nameof(ExpectedMaxHp))); return 850;
        }

    }
}