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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
	[TestClass]
    public class BioSimClientModelWithParametersTest
    {

        [ClassInitialize]
        public static void InitalizeClass(TestContext c)
        {
            BioSimClient.IsLocal = true;
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            BioSimClient.IsLocal = false;
        }

		/*
		 * Testing parameter map conversion to String
		 */
		[TestMethod]
		public void testingParametersWithDegreeDays() 
		{
			BioSimParameterMap parameterMap = BioSimClient.GetModelDefaultParameters("DegreeDay_Annual");
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			string observedString = BioSimClientTestSettings.GetJSONObject(parameterMap);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}



		/*
		 * Tests if the weather generation over past and future time intervals.
		 */
		[TestMethod]
		public void testingWithDegreeDaysAbove5CAnEmptyParameters()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			BioSimParameterMap parms = new BioSimParameterMap();
			string modelName = "DegreeDay_Annual";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2001,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					new List<BioSimParameterMap>(new BioSimParameterMap[] { parms }))[modelName];

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}


		/*
		 * Tests if the weather generation over past and future time intervals.
		 */
		// TODO MF2022-01-31 reenable this test when the model is enabled on the server side
		//		[TestMethod]
		public void testingWithDegreeDaysAndGrowingSeason()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			BioSimParameterMap parms = new BioSimParameterMap();
			parms.AddParameter("LowerThreshold", 5);

			string[] modelNames = new string[] { "GrowingSeason", "DegreeDay_Annual" };
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2001,
					locations,
					null,
					null,
					new List<string>(modelNames),
					new List<BioSimParameterMap>(new BioSimParameterMap[] { null, parms }));

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			OrderedDictionary finalMap = new();
			foreach (string modelName in teleIO.Keys)
				finalMap.Add(modelName, BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet((OrderedDictionary)teleIO[modelName]));

			string observedString = BioSimClientTestSettings.GetJSONObject(finalMap);

			String referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}

		/*
		 * Tests if the weather generation over past and future time intervals.
		 */
		[TestMethod]
		public void testingWithDegreeDaysAbove5C()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			BioSimParameterMap parms = new();
			parms.AddParameter("LowerThreshold", 5);
			string modelName = "DegreeDay_Annual";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
				2001,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				new List<BioSimParameterMap>(new BioSimParameterMap[] { parms }))[modelName];
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}


		/*
		 * Tests if the weather generation over past and future time intervals.
		 */
		[TestMethod]
		public void testingWithDegreeDaysAbove5CLong()
		{
			int initialDateYr = 1980;
			BioSimParameterMap parms = new();
			parms.AddParameter("LowerThreshold", 5);
			string modelName = "DegreeDay_Annual";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2020,
					BioSimClientTestSettings.Instance.Plots,
					null,
					null,
					new List<string>(new String[] { modelName }),
					new List<BioSimParameterMap>(new BioSimParameterMap[] { parms }))[modelName];
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			String validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}



	}
}
