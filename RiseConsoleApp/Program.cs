using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace RiseConsoleApp
{
    public class Point
    {
        private int x;
        private int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
    } 

    public static class PointFunctions
    {
        public static double DistanceTo(Point point1, Point point2)
        {
            /*Calculate distance between two points*/
            var a = (int)(point2.X - point1.X);
            var b = (int)(point2.Y - point1.Y);

            return Math.Sqrt(a * a + b * b);
        }

        public static double TriangleArea(Point point1, Point point2, Point point3)
        {
            /*Calculate area of triangle*/
            return Math.Abs((point1.X * (point2.Y - point3.Y) +
                             point2.X * (point3.Y - point1.Y) +
                             point3.X * (point1.Y - point2.Y)) / 2.0);
        }

        public static bool IsInsideTriangle(Point point1, Point point2, Point point3, Point pointP)
        {
            /* Calculate area of triangle ABC */
            double A = TriangleArea(point1, point2, point3);

            /* Calculate area of triangle PBC */
            double A1 = TriangleArea(pointP, point2, point3);

            /* Calculate area of triangle PAC */
            double A2 = TriangleArea(point1, pointP, point3);

            /* Calculate area of triangle PAB */
            double A3 = TriangleArea(point1, point2, pointP);

            /* Check if sum of A1, A2 and A3 is same as A */
            return (A == A1 + A2 + A3);
        }

        public static List<Point> SortByDistance(List<Point> lst, Point pt)
        {
            List<Point> output = new List<Point>();
            output.Add(lst[NearestPoint(pt, lst)]);
            lst.Remove(output[0]);
            int x = 0;
            for (int i = 0; i < lst.Count + x; i++)
            {
                output.Add(lst[NearestPoint(output[output.Count - 1], lst)]);
                lst.Remove(output[output.Count - 1]);
                x++;
            }
            return output;
        }

        public static int NearestPoint(Point srcPt, List<Point> lookIn)
        {
            KeyValuePair<double, int> smallestDistance = new KeyValuePair<double, int>();
            for (int i = 0; i < lookIn.Count; i++)
            {
                double distance = Math.Sqrt(Math.Pow(srcPt.X - lookIn[i].X, 2) + Math.Pow(srcPt.Y - lookIn[i].Y, 2));
                if (i == 0)
                {
                    smallestDistance = new KeyValuePair<double, int>(distance, i);
                }
                else
                {
                    if (distance < smallestDistance.Key)
                    {
                        smallestDistance = new KeyValuePair<double, int>(distance, i);
                    }
                }
            }
            return smallestDistance.Value;
        }

    }

    class Program
    {

        static void Main(string[] args)
        {
            List<Point> dogPath = new List<Point>() {
                new Point(0, 0)
            };
            List<Point> trees = new List<Point>();
            List<Point> pathPoints = new List<Point>() { 
                new Point(0, 0)
            };

            Console.WriteLine("Enter path for the input file");
            string path = Console.ReadLine();

            try
            {
                if (File.Exists(path))
                {
                    /*Read number of toys and trees*/
                    string line = File.ReadLines(path).Skip(0).Take(1).First();
                    var lineParts = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int numOfToys = int.Parse(lineParts[0]);
                    int numOfTrees = int.Parse(lineParts[1]);
                    
                    /*Create list of toys coordinates*/
                    for (int i = 1; i <= numOfToys; i++)
                    {
                        line = File.ReadLines(path).Skip(i).Take(1).First();
                        lineParts = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        int x = int.Parse(lineParts[0]);
                        int y = int.Parse(lineParts[1]);
                        Point point = new Point(x, y);
                        dogPath.Add(point);
                    }
                    Console.WriteLine(dogPath);

                    /*Create list of trees coordinates*/
                    for (int i = numOfToys + 1; i <= numOfToys + numOfTrees; i++)
                    {
                        line = File.ReadLines(path).Skip(i).Take(1).First();
                        lineParts = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        int x = int.Parse(lineParts[0]);
                        int y = int.Parse(lineParts[1]);
                        Point point = new Point(x, y);
                        trees.Add(point);
                    }

                    /*Find points in path*/
                    for (int i = 1; i < numOfToys; i++ )
                    {
                        /*Remove points where the leash no longer pivots*/
                        int k = i;
                        while (pathPoints.Count() > 1)
                        {
                            
                            if (PointFunctions.IsInsideTriangle(dogPath[k], dogPath[k-1], pathPoints[pathPoints.Count - 2], pathPoints[pathPoints.Count - 1]))
                            {
                                pathPoints.RemoveAt(pathPoints.Count - 1);
                                k -= 1;
                            } else
                            {
                                break;
                            }
                        }

                        /*Identify pontential trees where the leash might pivot*/
                        List<Point> potentialPathPoints = new List<Point>();
                        for (int j = 0; j < numOfTrees; j++)
                        {
                            if (PointFunctions.IsInsideTriangle(dogPath[i-1], dogPath[i], dogPath[i+1], trees[j]))
                            {
                                if (!trees[j].Equals(pathPoints.Last()))
                                {
                                    potentialPathPoints.Add(trees[j]);
                                }                               
                            } 
                        }

                        /*Evaluate if leash would pivot on potential trees*/
                        List<Point> orderedPotentialPathPoints = new List<Point>();
                        if (potentialPathPoints.Count > 1)
                        { 
                            orderedPotentialPathPoints = PointFunctions.SortByDistance(potentialPathPoints, dogPath[i]);
                            for (int l = 0; l < orderedPotentialPathPoints.Count - 1; l++)
                            {
                                for (int m = l + 1; m < orderedPotentialPathPoints.Count - 1; m++)
                                {
                                    if (PointFunctions.IsInsideTriangle(dogPath[i - 1], dogPath[i + 1], orderedPotentialPathPoints[l], orderedPotentialPathPoints[m]))
                                    {
                                        orderedPotentialPathPoints.RemoveAt(m);
                                    }
                                }
                            }
                            if (orderedPotentialPathPoints.Count > 1)
                            {
                                List<Point> orderedActualPathPoints = new List<Point>();
                                orderedActualPathPoints = PointFunctions.SortByDistance(orderedPotentialPathPoints, dogPath[i - 1]);
                                pathPoints.AddRange(orderedActualPathPoints);
                            } else
                            {
                                pathPoints.AddRange(orderedPotentialPathPoints);
                            }
                        }else
                        {
                            pathPoints.AddRange(potentialPathPoints);
                        }
                    }
                    /*add last toy*/
                    pathPoints.Add(dogPath.Last());

                    /*Calculate leash length*/
                    double leashLength = 0;

                    for (int i = 1; i < pathPoints.Count(); i++)
                    {
                        leashLength += PointFunctions.DistanceTo(pathPoints[i-1], pathPoints[i]);
                    }

                    Console.WriteLine(leashLength);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }         
        }
    }
}
