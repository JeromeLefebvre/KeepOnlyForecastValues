# Automatic deletion of forecast event

How to use this example.

1. Create an AF Database called KeepOnlyForecast
2. Import the XML file KeepOnlyForecast.xml into the database
3. In the element DataGeneration, right-click the Forecast attribute and select Create Or Update PI Point, this creates a future tag called OSIDemo_Forecast
4. Verify that the analysis FutureSinusoid is started and writing data
5. By default it will write data 2 minutes in the future
6. Run the program using Visual Studio 2015
7. Once any event becomes older than current time, that event will be printed in the console and deleted from the archive.

This sample code was only tested with the following:
AFSDK 2015 R2, 2.7.5.7166
Visual Studio 2015 Community
.Net Framework 4.5.2