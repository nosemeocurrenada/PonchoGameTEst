using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonchoGameTest
{
    public static class Ext
    {
        static Random rng = new Random();
        public static T Choose<T>(this List<T> self)
        {
            var index = rng.Next(self.Count);
            return self.ElementAt(index);
        }

        public static bool In<T>(this IEnumerable<T> self, IEnumerable<T> other) {
            return self.All(x => other.Contains(x));
        }

        public static void Scramble<TKey,TValue>(this IDictionary<TKey, TValue> self) {
            int n = rng.Next(self.Count);

            for (int i = 0; i < n; i++) {
                var k1 = self.Keys.ToList().Choose();
                var k2 = self.Keys.ElementAt(i);
                self.Swap(k1, k2);
            }
        }

        public static void Swap<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey k1, TKey k2) {
            TValue aux = self[k1];
            self[k1] = self[k2];
            self[k2] = aux;
        }
    }

    public class Program
    {
        Dictionary<Color, Person[]> persons;
        Color goal;
        bool won = false;
        Dictionary<int, Color> colorTable;
        Dictionary<int, Accion> actionTable;
        public Program(Dictionary<Color, Person[]> persons, Color goal, Dictionary<int, Color> colorTable, Dictionary<int, Accion> actionTable) {
            this.persons = persons;
            this.goal = goal;
            this.colorTable = colorTable;
            this.actionTable = actionTable;
        }

        // No uso el nombre Action por que es parte del framework de .NET
        public enum Accion { Up, Down }

        public enum Color
        {
            Blanco, Naranja, Rosa, Celeste, Verde
        }

        public static string ColorToString(Color color) {
            return color.ToString()[0].ToString();
        }

        public class Person
        {
            public Color color { get; private set; }
            public void SetColor(Color color)
            {
                this.color = color;
            }

            public override string ToString()
            {
                return ColorToString(this.color);
            }
        }

        public class Choice
        {
            public Accion accion;
            public Color hat;
            public Color poncho;
        }

        Choice Read() {
            //Your code goes here
            Console.WriteLine("Elija tres glifos");
            Console.WriteLine("1- △");
            Console.WriteLine("2- □");
            Console.WriteLine("3- X");
            Console.WriteLine("4- O");
            string line;
            int a, b, c;
            bool check = false;
            do
            {
                Console.Write(">>> ");
                line = Console.ReadLine();

                bool areOptsOk = int.TryParse(line[0].ToString(), out a);
                areOptsOk &= int.TryParse(line[1].ToString(), out b);
                areOptsOk &= int.TryParse(line[2].ToString(), out c);

                check = new[] { a, b, c }.In(new[] { 1, 2, 3, 4 }) && areOptsOk;
            } while (line.Count() != 3 || !check);

            Color hat = colorTable[a];
            Color poncho = colorTable[b];
            Accion accion = actionTable[c];

            return new Choice() { hat = hat, poncho = poncho, accion = accion };
        }

        void CheckWinCondition() {
            if (persons.All(pair =>
                    pair.Value.All(person => person.color == goal)))
            {
                won = true;
            }

        }

        void Eval(Choice choice) {
            var accion = choice.accion;
            var poncho = choice.poncho;
            var selectedRow = persons[choice.hat];

            foreach (var person in selectedRow)
            {
                //Poncho blanco
                if (person.color == Color.Blanco)
                {
                    if (accion == Accion.Up)
                        person.SetColor(poncho);
                    if (accion == Accion.Down)
                    {
                        var colors = ((Color[])Enum.GetValues(typeof(Color))).Except(new[] { Color.Blanco, poncho }).ToList();
                        var selected = colors.Choose();
                        person.SetColor(selected);
                    }
                }
                //Matchea
                else if (person.color == poncho)
                {
                    if (accion == Accion.Down)
                        person.SetColor(Color.Blanco);
                }
                // No matchea
                else
                {
                    if (accion == Accion.Up)
                        person.SetColor(Color.Blanco);
                }
            }

            CheckWinCondition();
        }

        void Print(Dictionary<Color, Person[]> persons) {
            foreach (var color in persons.Keys) {
                Console.Write("Hat {0} - ", ColorToString(color));
                foreach (var person in persons[color]) {
                    Console.Write("{0} ", person);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Su objetivo es tener todos los ponchos de color {0}", goal.ToString().ToLower());
            if (won) Console.WriteLine("Ganó");
        }

        void Run() {
            Print(persons);

            while (! won)
            {
                var choice = Read();

                Eval(choice);

                Print(persons);
            }

            Console.WriteLine("Finalizó el juego. Presione ENTER para salir si está corriendo desde un Visual.");
            Console.ReadLine();
        }

        public static void Main(string[] args)
        {
            int personsPerRow = 3;
            bool scramble = true;


            var persons = new Dictionary<Color, Person[]>();
            var nonWhiteColors = new List<Color>((Color[])Enum.GetValues(typeof(Color))).Where(color => color != Color.Blanco).ToList();
            foreach (var color in nonWhiteColors) {
                persons[color] = new Person[personsPerRow];
                for (int i = 0; i < persons[color].Length; i++) {
                    var person = new Person();
                    var c = nonWhiteColors.Choose();
                    person.SetColor(c);
                    persons[color][i] = person;
                }
            }
            var goal = nonWhiteColors.Choose();

            Dictionary<int, Color> colorTable = new Dictionary<int, Color> {
                {1, Color.Naranja},
                {2, Color.Verde},
                {3, Color.Rosa},
                {4, Color.Celeste}
            };
            Dictionary<int, Accion> actionTable = new Dictionary<int, Accion> {
                {1, Accion.Down},
                {2, Accion.Up},
                {3, Accion.Down},
                {4, Accion.Up}
            };

            if (scramble) {
                colorTable.Scramble();
                actionTable.Scramble();
            }

            new Program(persons, goal, colorTable, actionTable).Run();
        }
    }
}