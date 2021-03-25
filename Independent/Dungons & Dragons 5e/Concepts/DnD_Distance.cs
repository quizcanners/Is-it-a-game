using System;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{


    [Serializable]
    public struct FeetDistance
    {
        public const string SUFFIX = "ft.";
        public const int FEET_PER_MILE = 5280;
        public const int INCHES_PER_FOOT = 12;

        [SerializeField] public int ft;

        public int ToInches => ft * INCHES_PER_FOOT;

        public int ToMiles => ft / FEET_PER_MILE;

        public int Cells
        {
            get => ft / GridDistance.FEET_PER_CELL;
            set => ft = value * GridDistance.FEET_PER_CELL;
        }

        public static FeetDistance operator +(FeetDistance feet, FeetDistance toAdd)
        {
            feet.ft += toAdd.ft;
            return feet;
        }

        public static FeetDistance operator +(FeetDistance feet, GridDistance cells)
        {
            feet += cells.TotalFeet;
            return feet;
        }

        public override string ToString() => "{0} {1}".F(ft, SUFFIX);
    }

    [Serializable]
    public struct GridDistance : IPEGI_ListInspect
    {
        public const int FEET_PER_CELL = 5;

        [SerializeField] private bool _diagonalHalf;
        [SerializeField] private int _nonDIagonal;

        public int TotalCells => _nonDIagonal;

        public FeetDistance TotalFeet => new() { ft = TotalCells * FEET_PER_CELL };

        public void AddCells(int count = 1, bool diagonal = false)
        {
            if (!diagonal)
            {
                _nonDIagonal += count;
                return;
            }

            _nonDIagonal += (count / 2) * 2;

            if (count % 2 > 0)
            {

                if (_diagonalHalf)
                {
                    _nonDIagonal += 1;
                    _diagonalHalf = false;
                }
                else
                {
                    _diagonalHalf = true;
                }
            }
        }

        private void AddFloorToLowest(FeetDistance feet)
        {
            _nonDIagonal += feet.ft / FEET_PER_CELL;
        }

        public static GridDistance operator +(GridDistance cell, FeetDistance feet)
        {
            cell.AddFloorToLowest(feet);
            return cell;
        }

        public static GridDistance FromCells(int count)
        {
            var cd = new GridDistance();
            cd._nonDIagonal += count;
            return cd;
        }

        public GridDistance Half()
        {
            _nonDIagonal /= 2;
            _diagonalHalf = false;
            return this;
        }

        public static GridDistance FromFeet(FeetDistance dist) => new GridDistance() + dist;
        
        public override string ToString() => TotalFeet.ToString();

        public void InspectInList(ref int edited, int ind)
        {
            "Cells".PegiLabel(50).edit(ref _nonDIagonal);
            "Feet: {0}".F(ToString()).PegiLabel().write();
        }
    }
}
