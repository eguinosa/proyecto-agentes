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
            
            
            var dirtyCellsA = new double[30];
            var dirtyCellsB = new double[30];
            var ciclesA = new int[30];
            var ciclesB = new int[30];
            int firedA = 0;
            int firedB = 0;
            int successA = 0;
            int successB = 0;
                        
            for (int i = 0; i < 30; i++)
            {
                //Creating the Environment && Agent
                var simulationA = new Environment(9, 9, 6, 20, 15, RobotType.AgentA, 0);
                var simulationB = simulationA.CopyStartEnviroment(RobotType.AgentB);

                //Console.WriteLine("*** Imprimiendo el Mapa: ***");
                //foreach (var item in simulationA.Cells())
                //{
                //    Console.WriteLine(item);
                //}
                //Console.WriteLine();

                //Doing the Simulation
                Console.WriteLine("\nSimulation of AgentA:");
                while (!simulationA.Done)
                {
                    var reports = simulationA.Cicle();
                    //Console.WriteLine("**************\nCicle: " + simulationA.Count + "\n**************");
                    //foreach (var item in reports)
                    //{
                    //    Console.WriteLine(item);
                    //}
                    //Console.WriteLine("\n<<<< Updated Map >>>>");
                    //foreach (var item in simulationA.Cells())
                    //{
                    //    Console.WriteLine(item);
                    //}
                    //Console.WriteLine();
                    //Console.ReadLine();
                }

                if (simulationA.Success)
                {
                    successA++;
                    Console.WriteLine("The Robot was able to clean the house and put the Kid in the Corrals in {0} cicles", simulationA.Count);
                }
                else if (simulationA.Fired)
                {
                    firedA++;
                    Console.WriteLine("The Robot was Fired");
                }
                else
                {
                    Console.WriteLine("The Robot didn't have enough time to finish his work.");
                }
                ciclesA[i] = simulationA.Count;
                dirtyCellsA[i] = simulationA.PercentDirty();


                //********** Simulation B ************
                Console.WriteLine("\nSimulation of AgentB:");

                //Console.WriteLine("*** Imprimiendo el Mapa: ***");
                //foreach (var item in simulationB.Cells())
                //{
                //    Console.WriteLine(item);
                //}
                //Console.WriteLine();

                //Doing the Simulation
                while (!simulationB.Done)
                {
                    var reports = simulationB.Cicle();
                    //Console.WriteLine("**************\nCicle: " + simulationB.Count + "\n**************");
                    //foreach (var item in reports)
                    //{
                    //    Console.WriteLine(item);
                    //}
                    //Console.WriteLine("\n<<<< Updated Map >>>>");
                    //foreach (var item in simulationB.Cells())
                    //{
                    //    Console.WriteLine(item);
                    //}
                    //Console.WriteLine();
                    //Console.ReadLine();
                }

                if (simulationB.Success)
                {
                    successB++;
                    Console.WriteLine("The Robot was able to clean the house and put the Kid in the Corrals in {0} cicles", simulationB.Count);
                }
                else if (simulationB.Fired)
                {
                    firedB++;
                    Console.WriteLine("The Robot was Fired");
                }
                else
                {
                    Console.WriteLine("The Robot didn't have enough time to finish his work.");
                }

                ciclesB[i] = simulationB.Count;
                dirtyCellsB[i] = simulationB.PercentDirty();
            }

            var sumDirtyCellsA = dirtyCellsA.Aggregate((e1, e2) => e1 + e2);
            var meanDirtyA = sumDirtyCellsA / 30;
            var sumDirtyCellsB = dirtyCellsB.Aggregate((e1, e2) => e1 + e2);
            var meanDirtyB = sumDirtyCellsB / 30;

            var meanCiclesA = (double) ciclesA.Aggregate((e1, e2) => e1 + e2) / 30;
            var meanCiclesB = (double) ciclesB.Aggregate((e1, e2) => e1 + e2) / 30;

            Console.WriteLine("Estadisticas Agente A:");
            Console.WriteLine("Success: {0}", successA);
            Console.WriteLine("Fired: {0}", firedA);
            Console.WriteLine("Promedio de Ciclos: {0}", meanCiclesA);
            Console.WriteLine("Promedio de Casillas Sucias: {0}\n", meanDirtyA);

            Console.WriteLine("Estadisticas Agente B:");
            Console.WriteLine("Success: {0}", successB);
            Console.WriteLine("Fired: {0}", firedB);
            Console.WriteLine("Promedio de Ciclos: {0}", meanCiclesB);
            Console.WriteLine("Promedio de Casillas Sucias: {0}", meanDirtyB);


            Console.ReadLine();
        }
    }
}
