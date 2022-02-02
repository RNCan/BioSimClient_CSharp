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
	public class BioSimClientReplicateTest
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
		public void test2085to2090_2rep() 
		{
			testingFutureDegreeDaysWithRCP85andClimateModels(2085,2090,2);
		}

		[TestMethod]
		public void test2085to2090_10rep() 
		{
			testingFutureDegreeDaysWithRCP85andClimateModels(2085,2090,10);
		}

		[TestMethod]
		public void test2065to2090_2rep() 
		{
			testingFutureDegreeDaysWithRCP85andClimateModels(2065,2090,2);
		}

		[TestMethod]
		public void test2015to2030_3rep() 
		{
			testingFutureDegreeDaysWithRCP85andClimateModels(2015,2030,3);
		}


		private static void testingFutureDegreeDaysWithRCP85andClimateModels(int initialDateYr, int finalDateYr, int nbReplicates)
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;

			int expectedObservationsPerPlot = ((finalDateYr - initialDateYr) + 1) * nbReplicates;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary oRCP85_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr,
					finalDateYr,
					locations,
					RCP.RCP85,
					null,
					new List<string>(new string[] { modelName }),
					nbReplicates,
					null)[modelName];

			foreach (IBioSimPlot plot in oRCP85_RCM4def.Keys)
			{
				BioSimDataSet firstDataSet = (BioSimDataSet)oRCP85_RCM4def[plot];
				Assert.AreEqual(expectedObservationsPerPlot, firstDataSet.GetNumberOfObservations());
			}
		}

		[TestMethod]
		public void test2015to2025_2repForcedClimateGeneration() 
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			BioSimClient.SetForceClimateGenerationEnabled(true);
			string modelName = "DegreeDay_Annual";
			OrderedDictionary oRCP85_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(2015,
				2025,
				locations,
				RCP.RCP85,
				null,
				new List<string>(new string[] { modelName }),
				2,
				null)[modelName];
			BioSimDataSet dataset = (BioSimDataSet)oRCP85_RCM4def[locations[0]];
			int repIndex = dataset.GetFieldNames().IndexOf("Rep");
			int yearIndex = dataset.GetFieldNames().IndexOf("Year");
			int refRep = -1;
			int refYear = -1;
			foreach (Observation obs in dataset.GetObservations())
			{
				int rep = (int)obs.values[repIndex];
				int year = (int)obs.values[yearIndex];
				if (rep < refRep)
					Assert.Fail("The ascending order was not repected in the Rep field");
				else if (rep == refRep)
				{
					if (year <= refYear)
						Assert.Fail("The ascending order was not repected in the Year field");
					else
						refYear = year;
				}
				else
				{
					refRep = rep;
					refYear = year;
				}
			}
			Console.WriteLine("Ascending order tested in replicated generated climate!");
			BioSimClient.ResetClientConfiguration();
		}

		[TestMethod]
		public void test2012to2016_1repForcedClimateGeneration() 
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			BioSimClient.SetForceClimateGenerationEnabled(true);
			string modelName = "DegreeDay_Annual";
			OrderedDictionary oRCP85_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(2012,
				2016,
				locations,
				RCP.RCP85,
				null,
				new List<string>(new string[] { modelName }),
				null)[modelName];
			BioSimDataSet dataset = (BioSimDataSet)oRCP85_RCM4def[locations[0]];
			Assert.AreEqual(5, dataset.GetNumberOfObservations());
			BioSimClient.ResetClientConfiguration();
		}


		[TestMethod]
		public void test1981to2010_2repForcedClimateGeneration() 
		{
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);
			BioSimClient.SetForceClimateGenerationEnabled(true);
			string modelName = "ClimaticQc_Annual";
			OrderedDictionary oRCP85_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(1981,
				2010,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				2,
				null)[modelName];
			BioSimDataSet dataset = (BioSimDataSet)oRCP85_RCM4def[locations[0]];
			Assert.AreEqual(30 * 2, dataset.GetNumberOfObservations());
			BioSimClient.ResetClientConfiguration();
		}


		[TestMethod]
		public void test1981to2010_2repOnTheModelEnd()
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary climateOutput = (OrderedDictionary)BioSimClient.GenerateWeather(1981,
				2010,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				1,
				2,
				null)[modelName];
			foreach (IBioSimPlot l in locations)
			{
				BioSimDataSet dataset = (BioSimDataSet)climateOutput[l];
				Assert.AreEqual(30 * 2, dataset.GetNumberOfObservations());
			}
		}
	
		[TestMethod]
		public void test1981to2010_2repOnWGPlus2repTheModelEnd() 
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary climateOutput = (OrderedDictionary)BioSimClient.GenerateWeather(1981,
				2010,
				locations,
				null,
				null,
				new List<string>(new string[] { modelName }),
				2,
				2,
				null)[modelName];
			foreach (IBioSimPlot l in locations)
			{
				BioSimDataSet dataset = (BioSimDataSet)climateOutput[l];
				Assert.AreEqual(30 * 4, dataset.GetNumberOfObservations());
			}
		}
	
	}

}
