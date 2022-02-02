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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
	[TestClass]
	public class BioSimInternalModelTest
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


		[TestMethod]
		public void testingEachModel() 
		{
			List<IBioSimPlot> locations = new();
			locations.Add(new BioSimFakeLocation(45, -74, 300));

			List<string> modelList = BioSimClient.GetModelList();
			OrderedDictionary overallOutput = BioSimClient.GenerateWeather(2018,
				2019,
				locations,
				null,
				null,
				modelList,
				1,
				1,
				null);
			OrderedDictionary resultMap = new ();
			foreach (string modelName in modelList) 
			{
				object output = overallOutput[modelName];
				if (output is OrderedDictionary) 
					resultMap.Add(modelName, true);
				else if (output is BioSimClientException) 
					resultMap.Add(modelName, false);
				else 
					throw new Exception("The value of the map should be either a LinkedHashMap or a BioSimClient Exception!");
			}
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);
			String observedString = this.GetJSONObject(resultMap);
			String referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);
			Assert.AreEqual(referenceString, observedString);
		}

		private String GetJSONObject(OrderedDictionary oMap) 
		{
			string outputString = JsonConvert.SerializeObject(oMap);
			return outputString;
		}
	}

}
