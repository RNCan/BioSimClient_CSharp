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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
    [TestClass]
    public class BioSimClientModelNbNearestNeighboursTest
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
		 * Testing ClimaticQc_Annual model and ensuring that the default nb of nearest neighbour is 4.
		*/
		[TestMethod]
		public void testingWithDefaultFourClimateStations()
		{
			BioSimClient.ResetClientConfiguration();
			BioSimClientTestSettings.SetForTest(true);
			List<IBioSimPlot> locations = new();
			IBioSimPlot plot = BioSimClientTestSettings.Instance.Plots[0];
			locations.Add(plot);
			int initialDateYr = 2000;
			string modelName = "ClimaticQc_Annual";
			OrderedDictionary teleIORefs = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2000,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					null)[modelName];
			BioSimDataSet bsds1 = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIORefs);
			string referenceString = BioSimClientTestSettings.GetJSONObject(bsds1);

			BioSimClient.SetNbNearestNeighbours(4);

			OrderedDictionary teleIORefs2 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2000,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					null)[modelName];
			BioSimDataSet bsds2 = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIORefs2);
			String observedString = BioSimClientTestSettings.GetJSONObject(bsds2);

			Assert.AreEqual(referenceString, observedString);
			BioSimClient.ResetClientConfiguration();
			BioSimClientTestSettings.SetForTest(true);
		}

		[TestMethod]
		public void testingWithTwentyClimateStations()
		{
			BioSimClient.ResetClientConfiguration();
			BioSimClientTestSettings.SetForTest(true);
			List<IBioSimPlot> locations = new();
			IBioSimPlot plot = BioSimClientTestSettings.Instance.Plots[0];
			locations.Add(plot);
			int initialDateYr = 2000;
			string modelName = "ClimaticQc_Annual";
			OrderedDictionary teleIORefs = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2000,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					null)[modelName];
			BioSimDataSet bsds1 = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIORefs);
			string referenceString = BioSimClientTestSettings.GetJSONObject(bsds1);

			BioSimClient.SetNbNearestNeighbours(20);

			OrderedDictionary teleIORefs2 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					2000,
					locations,
					null,
					null,
					new List<string>(new string[] { modelName }),
					null)[modelName];
			BioSimDataSet bsds2 = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIORefs2);
			String observedString = BioSimClientTestSettings.GetJSONObject(bsds2);

			Assert.IsFalse(referenceString.Equals(observedString));
			BioSimClient.ResetClientConfiguration();
			BioSimClientTestSettings.SetForTest(true);
		}



	}
}
