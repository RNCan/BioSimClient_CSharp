A C# client for the BioSIM Web API

This library is related to the BioSIM application. BioSIM is a software developed by the Canadian Forest Service. It contains a weather generator and various models. 
More information about the application is available at https://cfs.nrcan.gc.ca/projects/133. The Web API is an online service that can process http request and return 
the generated weather to the client. This library is a C# client for the Web API.

Copyright (C) 2020-2022 Her Majesty the Queen in right of Canada
Authors: Mathieu Fortin and Jean-Francois Lavoie (Canadian Wood Fibre Centre, Canadian Forest Service)

The biosimclient library is licensed under the GNU Lesser Public General License 3.0:

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed with the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied
warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU Lesser General Public
License for more details.

Please see the license at http://www.gnu.org/copyleft/lesser.html.

The biosimclient library has a dependence on 
  - The Newtonsoft JSON.NET library available at https://www.newtonsoft.com/json. This library is licensed under MIT (Copyright (c) 2007 James Newton-King). 
  
An example of implementation is:

    List<string> modelList = BioSimClient.GetModelList();   // get all the available models
    BioSimPlotImpl plot = new(50.0, -72.0, 300);
    OrderedDictionary myWeather = BioSimClient.GenerateWeather(2000,
        2010,
        new List<IBioSimPlot>(new IBioSimPlot[] { plot }),
        RCP.RCP45,
        ClimateModel.RCM4,
        new List<string>(new string[] { "DegreeDay_Annual" }),
        null);      // no additional parameters

The myWeather object is an OrderedDictionary with model names as keys and OrderedDictionary instances as values. These nested OrderedDictionary instances have 
the BioSimPlot as keys and BioSimDataSet instances as values. The BioSimDataSet instance contains the climate data.