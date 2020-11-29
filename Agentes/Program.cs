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
            Console.WriteLine("3er Proyecto de Simulacion: Agentes\n");

            //Creating the Environment && Agent
            var simulation = new Environment(4, 4, 2, 0, 11, RobotType.AgentA, 2000);
            Console.WriteLine("*** Imprimiendo el Mapa: ***");
            foreach (var item in simulation.Cells())
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();

            //Doing the Simulation
            while (!simulation.Done)
            {
                var reports = simulation.Cicle();
                Console.WriteLine("**************\nCicle: " + simulation.Count + "\n**************");
                foreach (var item in reports)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("\n<<<< Updated Map >>>>");
                foreach (var item in simulation.Cells())
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine();
                //Console.ReadLine();
            }

            if (simulation.Success)
            {
                Console.WriteLine("The Robot was able to clean the house and put the Kid in the Corrals in {0} cicles", simulation.Count);
            }
            else if(simulation.Fired)
            {
                Console.WriteLine("The Robot was Fired");
            }
            else
            {
                Console.WriteLine("The Robot didn't have enough time to finish his work.");
            }

            Console.ReadLine();
        }
    }
}
