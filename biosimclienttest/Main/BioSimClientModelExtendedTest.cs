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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace biosimclienttest
{
    [TestClass]
    public class BioSimClientModelExtendedTest
    {

        [ClassInitialize]
        public static void InitalizeClass(TestContext c)
        {
            BioSimClientTestSettings.SetForTest(true);
        }

        [ClassCleanup]
        public static void CleanUp()
        {
			BioSimClientTestSettings.SetForTest(false);
		}


		/*
		 * Testing ClimaticQc_Annual model
		*/
		[TestMethod]
		public void testingWithClimaticQc_Annual()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			string modelName = "ClimaticQc_Annual";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
				2000,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				null)[modelName];
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Console.WriteLine(referenceString);
			Console.WriteLine(observedString);
			Assert.AreEqual(referenceString, observedString);
		}

		/*
		 * Testing Climatic_Annual model
		 */
		[TestMethod]
		public void testingWithClimatic_Annual()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			string modelName = "Climatic_Annual";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
				2000,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				null)[modelName];

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
		 * Testing Climatic_Monthly model
		 */
		[TestMethod]
		public void testingWithClimatic_Monthly()
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			int initialDateYr = 2000;
			string modelName = "Climatic_Monthly";
			OrderedDictionary teleIO = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2000,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					null)[modelName];
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}

	}
}
