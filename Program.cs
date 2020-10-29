using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fractionator
{
    public class Program
    {
        static void Main(string[] args)
        {
            printMessage("Welcome to Fractionator!\nI will help you perform basic operations (+, -, *, /) with fractional numbers");
            printMessage("Example: ? 1/7 + 3_8/16");

            while (true)
            {
                string input = Console.ReadLine();

                if (input.ToLower().Trim().Equals("exit"))
                    break;

                if (input[0].Equals('?'))
                {
                    FractionNumber f1, f2;
                    string oper;

                    if (FractionExpression.tryParse(input.Trim('?'), out f1, out f2, out oper))
                    {
                        Console.WriteLine("Result: " + FractionExpression.executeOperation(f1, f2, oper));
                    }
                    else
                    {
                        printMessage("Invalid expression!");
                    }
                }
                else
                {
                    printMessage("Wrong input!");
                }
            }
        }

        static void printMessage(string message)
        {
            Console.WriteLine("");
            Console.WriteLine(message);
            Console.WriteLine("");
        }
    }

    public static class FractionExpression
    {
        public static bool tryParse(string expr, out FractionNumber f1, out FractionNumber f2, out string oper)
        {
            f1 = new FractionNumber(0);
            f2 = new FractionNumber(0);
            oper = "";

            string[] pieces = expr.Split(' ');

            List<string> expPieces = new List<string>();
            for (int p = 0; p < pieces.Length; p++)
            {
                if (pieces[p].Trim().Length > 0)
                {
                    expPieces.Add(pieces[p]);
                    if (expPieces.Count == 3)
                        break;
                }
            }

            // Min set for operation
            if (expPieces.Count < 3)
                return false;

            // Valid oper sign
            List<string> operands = new List<string> { "+", "-", "*", "/" };
            if (expPieces[1].Length != 1 || !operands.Contains(expPieces[1]))
                return false;

            oper = expPieces[1];

            // Parse each fraction number
            if (!FractionExpression.tryParse(expPieces[0], out f1) ||
                !FractionExpression.tryParse(expPieces[2], out f2))
                return false;

            return true;
        }

        public static bool tryParse(string fraction, out FractionNumber f)
        {
            f = new FractionNumber(0);

            List<char> valid = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            bool negative = false;

            // First char must be negative sign or number
            if (!fraction[0].Equals('-') && !valid.Contains(fraction[0]))
                return false;

            if (fraction[0].Equals('-'))
                negative = true;

            string number = "", whole = "", num = "", den = "";

            int curPos = 0;
            int endPos = fraction.Length;

            while (curPos < endPos)
            {
                if (curPos == 0 && fraction[curPos].Equals('-'))
                {
                    negative = true;
                }
                else if (valid.Contains(fraction[curPos]))
                {
                    number += fraction[curPos];
                }
                else if (fraction[curPos].Equals('_'))
                {
                    for (int c = 0; c < number.Length; c++)
                        whole += number[c];

                    number = "";
                }
                else if (fraction[curPos].Equals('/'))
                {
                    for (int c = 0; c < number.Length; c++)
                        num += number[c];

                    number = "";
                }

                ++curPos;
            }

            if ((whole.Length == 0 && num.Length == 0) || number.Length == 0)
                return false;

            for (int c = 0; c < number.Length; c++)
                den += number[c];

            if (whole.Length == 0)
                whole = "0";

            // Convert to integers
            int iWhole, iNum, iDen;
            if (!Int32.TryParse(whole, out iWhole) ||
                !Int32.TryParse(num, out iNum) ||
                !Int32.TryParse(den, out iDen))
                return false;

            if (negative)
            {
                if (iWhole != 0)
                    iWhole = iWhole * -1;
                else
                    iDen = iDen * -1;
            }

            // Construct fraction number
            f = new FractionNumber(iWhole, iNum, iDen);

            return true;
        }

        public static string executeOperation(FractionNumber f1, FractionNumber f2, string oper)
        {
            // Executes operation according to operator
            if (oper.Equals("+"))
                return FractionNumber.sum(f1, f2).toString();
            else if (oper.Equals("-"))
                return FractionNumber.diff(f1, f2).toString();
            else if (oper.Equals("*"))
                return FractionNumber.mult(f1, f2).toString();
            else if (oper.Equals("/"))
                return FractionNumber.div(f1, f2).toString();
            else
                throw new Exception("Invalid operator!");
        }
    }

    public class FractionNumber
    {
        public int Whole { get; set; } = 0;
        public int Numerator { get; set; } = 0;
        public int Denominator { get; set; } = 1;

        public FractionNumber(int whole) : this(whole, 0, 1) { }

        public FractionNumber(int num, int den) : this(0, num, den) { }

        public FractionNumber(int whole, int num, int den)
        {
            if (den == 0)
                throw new Exception("Denominator cannot be 0!");

            Whole = whole;
            Numerator = num;
            Denominator = den;

            mixedToImproper(whole, num, den);
            reduce();
        }

        public static FractionNumber sum(FractionNumber f1, FractionNumber f2)
        {
            var num = (f1.Numerator * f2.Denominator) + (f2.Numerator * f1.Denominator);
            var den = f1.Denominator * f2.Denominator;

            FractionNumber fraction = new FractionNumber(num, den);

            fraction.reduce();

            return fraction;
        }

        public static FractionNumber diff(FractionNumber f1, FractionNumber f2)
        {
            var num = (f1.Numerator * f2.Denominator) - (f2.Numerator * f1.Denominator);
            var den = f1.Denominator * f2.Denominator;

            FractionNumber fraction = new FractionNumber(num, den);

            fraction.reduce();

            return fraction;
        }

        public static FractionNumber mult(FractionNumber f1, FractionNumber f2)
        {
            var num = f1.Numerator * f2.Numerator;
            var den = f1.Denominator * f2.Denominator;

            FractionNumber fraction = new FractionNumber(num, den);

            fraction.reduce();

            return fraction;
        }

        public static FractionNumber div(FractionNumber f1, FractionNumber f2)
        {
            var num = f1.Numerator * f2.Denominator;
            var den = f1.Denominator * f2.Numerator;

            FractionNumber fraction = new FractionNumber(num, den);

            fraction.reduce();

            return fraction;
        }

        public int greatestCommonDivisor(int n1, int n2)
        {
            n1 = Math.Abs(n1);
            n2 = Math.Abs(n2);

            if (n2 == 0)
                return n1;
            else if (n1 == 0)
                return n2;

            return greatestCommonDivisor(n2, n1 % n2);
        }

        private void improperToMixed(int num, int den)
        {
            if (Math.Abs(num) > Math.Abs(den))
            {
                Whole = num / den;
                Numerator = Numerator % Denominator;

                if (Numerator < 0)
                    Numerator *= -1;
            }
        }

        private void mixedToImproper(int who, int num, int den)
        {
            if (Math.Abs(who) > 0)
            {
                Whole = 0;
                Numerator = den * Math.Abs(who) + num;
                Denominator *= (who > 0 ? 1 : -1);
            }
        }

        public void reduce()
        {
            int gcd = greatestCommonDivisor(Numerator, Denominator);

            Numerator = Numerator / gcd;
            Denominator = Denominator / gcd;

            if (Denominator < 0)
            {
                Denominator = Denominator * -1;
                Numerator = Numerator * -1;
            }
        }

        public string toString()
        {
            improperToMixed(Numerator, Denominator);

            string result;

            if (Numerator == 0)
            {
                result = Whole.ToString();
            }
            else
            {
                if (Whole == 0)
                    result = Numerator + "/" + Denominator;
                else
                    result = Whole + "_" + Numerator + "/" + Denominator;
            }

            return result;
        }
    }
}
