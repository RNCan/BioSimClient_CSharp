/*
 * This file is part of the C# client for BioSIM Web API.
 *
 * Copyright (C) 2020-2022 Her Majesty the Queen in right of Canada
 * Authors: Mathieu Fortin and Jean-Francois Lavoie, 
 *          (Canadian Wood Fibre Centre, Canadian Forest Service)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed with the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * Please see the license at http://www.gnu.org/copyleft/lesser.html.
 */
using biosimclient.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
    internal class BioSimClientTestSettings
    {
        internal static bool Validation = true;

        internal static readonly BioSimClientTestSettings Instance = new();
		internal string ProjectRootPath { get; private set; }

        internal List<IBioSimPlot> Plots = new();


        internal BioSimClientTestSettings()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            while (!path.EndsWith("biosimclienttest"))
            {
                DirectoryInfo d = Directory.GetParent(path);
                path = d.ToString();
            }
            ProjectRootPath = path;

            Plots.Clear();
            Plots.Add(new BioSimPlotImpl(46.87, -71.25, 114));
            Plots.Add(new BioSimPlotImpl(46.03, -73.12, 15));

        }

        internal static string GetFilename(string methodName)
        {
            return BioSimClientTestSettings.Instance.ProjectRootPath + Path.DirectorySeparatorChar + "testData" + Path.DirectorySeparatorChar + methodName + "Ref.json";
        }

        internal static string GetReferenceString(string validationFilename)
        {
            using StreamReader r = new(validationFilename);
            string referenceString = r.ReadToEnd();
            return referenceString;
        }


        private static OrderedDictionary ConvertBioSimDataSetToMap(BioSimDataSet dataSet)
        {
            OrderedDictionary dict = new();
            for (int i = 0; i < dataSet.GetObservations().Count; i++)
            {
                Observation o = dataSet.GetObservations()[i];
                OrderedDictionary innerMap = new();
                dict.Add("" + i, innerMap);
                for (int j = 0; j < o.values.Count; j++)
                    innerMap.Add(dataSet.GetFieldNames()[j], o.values[j]);
            }
            return dict;
        }

        internal static string GetJSONObject(BioSimDataSet dataSet)
        {
            string str = JsonConvert.SerializeObject(ConvertBioSimDataSetToMap(dataSet));
            return str;
        }

        internal static string GetJSONObject(OrderedDictionary dataSets) 
        {
            OrderedDictionary mainObj = new();
	    	foreach (string modelName in dataSets.Keys) 
			    mainObj.Add(modelName, ConvertBioSimDataSetToMap((BioSimDataSet)dataSets[modelName]));
            string outputString = JsonConvert.SerializeObject(mainObj);
            return outputString;
	}

        internal static String GetJSONObject(BioSimParameterMap parmMap)
        {
            string outputString = JsonConvert.SerializeObject(parmMap.InnerMap);
            return outputString;
        }
    }
}
