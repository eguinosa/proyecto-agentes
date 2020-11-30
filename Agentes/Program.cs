using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initial Information
            Console.WriteLine("3er Proyecto de Simulacion: Agentes");
            Console.WriteLine("Simulacion de un Robot que se encarga de limpiar y cuidar un mapa, recogiendo " +
                              "todos los niños que sueltos y ensuciando dentro del mapa para colocarlos dentro de un corral.\n");

            while (true)
            {
                Console.WriteLine("\n--- Datos Necesarios para la Simulacion ---");
                Console.Write("Intruzca el ancho del mapa: ");
                var m = int.Parse(Console.ReadLine());
                Console.Write("El largo del mapa: ");
                var n = int.Parse(Console.ReadLine());
                Console.Write("La cantidad de niños en el mapa: ");
                var kids = int.Parse(Console.ReadLine());
                Console.Write("La porciento de casillas sucias en el mapa: ");
                var dirty = double.Parse(Console.ReadLine());
                Console.Write("El porciento de obstaculos en el mapa: ");
                var obstacles = double.Parse(Console.ReadLine());
                Console.Write("El tiempo de duracion del ciclo [milliseconds]: ");
                var waiting = long.Parse(Console.ReadLine());
                Console.Write("El Tipo de Agente que desea usar [A/B]: ");
                var input = Console.ReadLine().ToLower();
                RobotType agent;
                if (input == "a" || input == "agenta" || input == "agent a") agent = RobotType.AgentA;
                else if (input == "b" || input == "agentb" || input == "agent b") agent = RobotType.AgentA;
                else
                {
                    Console.WriteLine("La informacion introducida es incorrecta, vuelva a intentarlo...\n");
                    continue;
                }

                //Creating the Environment && Agent
                var simulation = new Environment(m, n, kids, dirty, obstacles, agent, waiting);

                Console.WriteLine("\n<<<< Mapa Inicial >>>>");
                foreach (var item in simulation.Cells())
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine();

                //Doing the Simulation
                var finerun = true;
                var message = "";
                while (!simulation.Done && finerun)
                {
                    try
                    {
                        var reports = simulation.Cicle();
                        Console.WriteLine("**************\nCiclo: " + simulation.Count + "\n**************");
                        foreach (var item in reports)
                        {
                            Console.WriteLine(item);
                        }
                        Console.WriteLine("\n<<<< Mapa Actual >>>>");
                        foreach (var item in simulation.Cells())
                        {
                            Console.WriteLine(item);
                        }
                        Console.WriteLine();
                        //Console.ReadLine();
                    }
                    catch(Exception e)
                    {
                        finerun = false;
                        message = e.Message;
                    }
                }
                if (!finerun)
                {
                    Console.WriteLine("El Robot fue despedido por funcionamiento defectuoso.");
                    Console.WriteLine(message);
                }
                else if (simulation.Success)
                {
                    Console.WriteLine("El Robot ubico a los niños en el corral y limpio toda la casa en {0} ciclos.", simulation.Count);
                }
                else if (simulation.Fired)
                {
                    Console.WriteLine("El Robot fue despedido.");
                }
                else
                {
                    Console.WriteLine("El robot no tuvo suficiente tiempo para terminar.");
                }

                Console.WriteLine("\n[q/quit] para salir");
                input = Console.ReadLine().ToLower(); ;
                if (input == "q" || input == "quit") break;
            }
        }
    }
}
