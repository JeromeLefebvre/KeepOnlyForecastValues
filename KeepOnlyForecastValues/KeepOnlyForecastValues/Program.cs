#region Copyright
//  Copyright 2016 OSIsoft, LLC
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

// code originally taken from
// https://pisquare.osisoft.com/community/developers-club/blog/2014/10/06/using-data-pipes-with-future-data-in-pi-af-sdk-27
namespace KeepOnlyForecastValues
{
    public class DataPipeObserver : IObserver<AFDataPipeEvent>
    {
        public void OnCompleted()
        {
            Console.WriteLine("Completed");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("Error");
        }

        public void OnNext(AFDataPipeEvent value)
        {
            Console.WriteLine("\n{0} Deleting value from PI Point: {1}\n => Value: {2} and Timestamp is {3}.", DateTime.Now.ToString(), value.Value.PIPoint.Name, value.Value.Value, value.Value.Timestamp.LocalTime);
            value.Value.PIPoint.UpdateValue(value.Value, AFUpdateOption.Remove);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            PISystem myPISystem = new PISystems()["localhost"];
            AFElement myElement = AFObject.FindObject(@"\\localhost\KeepOnlyForecast\Forecast") as AFElement;

            // Sign up for updates for attributes  
            using (AFDataPipe myDataPipe = new AFDataPipe())
            {
                myDataPipe.EventHorizonMode = AFEventHorizonMode.TimeOffset;
                myDataPipe.AddSignups(myElement.Attributes);
                IObserver<AFDataPipeEvent> observer = new DataPipeObserver();
                myDataPipe.Subscribe(observer);
                Console.WriteLine("Starting (press any key to exit)");

                // create a cancellation source to terminate the update thread when the user is done  
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                Task task = Task.Run(() =>
                {
                    // keep polling while the user hasn't requested cancellation  
                    while (!cancellationSource.IsCancellationRequested)
                    {
                        // Get updates from pipe and process them  
                        AFErrors<AFAttribute> myErrors = myDataPipe.GetObserverEvents();

                        // wait for 1 second using the handle provided by the cancellation source  
                        cancellationSource.Token.WaitHandle.WaitOne(1000);
                    }
                }, cancellationSource.Token);

                Console.ReadKey(); // this will block until the user presses a key   
                cancellationSource.Cancel(); // when this is called the update loop will terminate  
                task.Wait(); // wait for the task to complete before taking down the pipe  
            }
        }
    }
}
