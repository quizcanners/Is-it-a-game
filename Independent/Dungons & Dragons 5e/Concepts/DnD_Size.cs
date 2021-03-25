using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public static class DungeonsAndDragonsUtils 
    {
        public static Dice GetHitDice(this Size size)
        {
            switch (size) 
            {
                case Size.Tiny: return Dice.D4;
                case Size.Small: return Dice.D6;
                case Size.Medium: return Dice.D8;
                case Size.Large: return Dice.D10;
                case Size.Huge: return Dice.D12;
                case Size.Gargantuan: return Dice.D20;
                default: Debug.LogError("Size {0} not implemented".F(size)); return Dice.D6;
            }
        }

        public static GridDistance GetCellDistance(this Size size)
        {
            switch (size)
            {
                case Size.Tiny: 
                case Size.Small:
                case Size.Medium: return GridDistance.FromCells(1);
                case Size.Large: return GridDistance.FromCells(2);
                case Size.Huge: return GridDistance.FromCells(3);
                case Size.Gargantuan: return GridDistance.FromCells(4);
                default: Debug.LogError("Size {0} not implemented".F(size)); return GridDistance.FromCells(1);
            }
        }

        public static string ToSignedNumber(this int value) => value < 0 ? value.ToString() : "+{0}".F(value.ToString());

      

    }





    public enum Size
    {
        Tiny = 0, Small = 1, Medium = 2, Large = 3, Huge = 4, Gargantuan = 5
    }

}