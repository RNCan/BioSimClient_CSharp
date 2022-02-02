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
    public class BioSimClientPastAndFutureDailyClimateTest
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
		 * Tests if the weather generation over past and future time intervals.
		 */
		[TestMethod]
		public void testingWithDailyOverlappingPastAndFuture()
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;
			int initialDateYr = 2000;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary teleIORefs = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, 2040, locations, null, null, new List<string>(new string[] { modelName }), null)[modelName];
			OrderedDictionary teleIORefs2 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, 2040, locations, null, null, new List<string>(new string[] { modelName }), null)[modelName];

			foreach (IBioSimPlot plot in teleIORefs.Keys)
			{
				BioSimDataSet firstDataSet = (BioSimDataSet)teleIORefs[plot];
				BioSimDataSet secondDataSet = (BioSimDataSet)teleIORefs2[plot];
				Assert.IsTrue(firstDataSet.GetNumberOfObservations() > 0);
				Console.WriteLine("There is at least one observation");
				Assert.AreEqual(firstDataSet.GetNumberOfObservations(), secondDataSet.GetNumberOfObservations());
				Console.WriteLine("Same number of observations in each dataset");

				int dateFieldIndex = firstDataSet.GetFieldNames().IndexOf("Year");
				int ddFieldIndex = firstDataSet.GetFieldNames().IndexOf("DD");
				int dataTypeIndex = firstDataSet.GetFieldNames().IndexOf("DataType");
				for (int i = 0; i < firstDataSet.GetNumberOfObservations(); i++)
				{
					double d1 = (double)firstDataSet.GetValueAt(i, ddFieldIndex);
					double d2 = (double)secondDataSet.GetValueAt(i, ddFieldIndex);
					string dataType1 = (string)firstDataSet.GetValueAt(i, dataTypeIndex);
					string dataType2 = (string)secondDataSet.GetValueAt(i, dataTypeIndex);
					int dateYr = (int)firstDataSet.GetValueAt(i, dateFieldIndex);
					if (dataType1.Equals("Real_Data"))
					{
						Assert.IsTrue(dataType2.Equals("Real_Data"));
						Assert.AreEqual(d1, d2, 1E-8);  // Testing if the degree-days are the same before 2020 

					}
					else if (dataType1.Equals("Simulated"))
					{
						Assert.IsTrue(dataType2.Equals("Simulated"));
						Assert.IsTrue(Math.Abs(d1 - d2) > 1E-8);  // Testing that the degree-days are different for 2022 and after
					}
				}
				Console.WriteLine("Degree-days before 2020 are the same and those after vary.");
			}

		}


		/*
		 * Tests future climate with default climate model and RCP.
		 */
		[TestMethod]
		public void testingFutureDegreeDaysWithDefaultValuesOfRCPsandClimateModels()
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;
			int initialDateYr = 2090;
			int finalDateYr = 2091;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary oRCP45def_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, finalDateYr, locations, null, null, new List<string>(new String[] { modelName }), null)[modelName];
			OrderedDictionary oRCP45_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, finalDateYr, locations, RCP.RCP45, null, new List<string>(new String[] { modelName }), null)[modelName];
			OrderedDictionary oRCP45def_RCM4 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, finalDateYr, locations, null, ClimateModel.RCM4, new List<string>(new String[] { modelName }), null)[modelName];

			foreach (IBioSimPlot plot in oRCP45def_RCM4def.Keys)
			{
				BioSimDataSet firstDataSet = (BioSimDataSet)oRCP45def_RCM4def[plot];
				BioSimDataSet secondDataSet = (BioSimDataSet)oRCP45_RCM4def[plot];
				BioSimDataSet thirdDataSet = (BioSimDataSet)oRCP45def_RCM4[plot];
				Assert.IsTrue(firstDataSet.GetNumberOfObservations() > 0);
				Console.WriteLine("There is at least one observation");
				Assert.AreEqual(firstDataSet.GetNumberOfObservations(), secondDataSet.GetNumberOfObservations());
				Assert.AreEqual(secondDataSet.GetNumberOfObservations(), thirdDataSet.GetNumberOfObservations());
				Console.WriteLine("Same number of observations in each dataset");
				int ddFieldIndex = firstDataSet.GetFieldNames().IndexOf("DD");
				int dataTypeIndex = firstDataSet.GetFieldNames().IndexOf("DataType");
				for (int i = 0; i < firstDataSet.GetNumberOfObservations(); i++)
				{
					double d1 = (double)firstDataSet.GetValueAt(i, ddFieldIndex);
					//				double d2 = ((Number)secondDataSet.getValueAt(i, ddFieldIndex);
					double d2 = (double)secondDataSet.GetValueAt(i, ddFieldIndex);
					double d3 = (double)thirdDataSet.GetValueAt(i, ddFieldIndex);
					string dataType1 = (string)firstDataSet.GetValueAt(i, dataTypeIndex);
					string dataType2 = (string)secondDataSet.GetValueAt(i, dataTypeIndex);
					string dataType3 = (string)thirdDataSet.GetValueAt(i, dataTypeIndex);
					Assert.AreEqual(d1, d2, 450);   // Testing if the degree-days are equal between first and second datasets in spite of the stochastic simulation 
					Assert.AreEqual(d2, d3, 450);
					Assert.IsTrue(dataType1.Contains("Simulated"));
					Assert.IsTrue(dataType2.Contains("Simulated"));
					Assert.IsTrue(dataType3.Contains("Simulated"));
				}
				Console.WriteLine("Degree-days tested for default values.");
			}
		}



		/*
		 * Tests future climate with RCP 8.5 and default climate model.
		 */
		[TestMethod]
		public void testingFutureDegreeDaysWithRCP85andClimateModels()
		{
			List<IBioSimPlot> locations = BioSimClientTestSettings.Instance.Plots;
			int initialDateYr = 2090;
			int finalDateYr = 2091;
			string modelName = "DegreeDay_Annual";
			OrderedDictionary oRCP85_RCM4def = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, finalDateYr, locations, RCP.RCP85, null, new List<string>(new string[] { modelName }), null)[modelName];
			OrderedDictionary oRCP85_RCM4 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, finalDateYr, locations, RCP.RCP85, ClimateModel.RCM4, new List<string>(new string[] { modelName }), null)[modelName];

			foreach (IBioSimPlot plot in oRCP85_RCM4def.Keys)
			{
				BioSimDataSet firstDataSet = (BioSimDataSet)oRCP85_RCM4def[plot];
				BioSimDataSet secondDataSet = (BioSimDataSet)oRCP85_RCM4[plot];
				Assert.IsTrue(firstDataSet.GetNumberOfObservations() > 0);
				Console.WriteLine("There is at least one observation");
				Assert.AreEqual(firstDataSet.GetNumberOfObservations(), secondDataSet.GetNumberOfObservations());
				Console.WriteLine("Same number of observations in each dataset");
				int ddFieldIndex = firstDataSet.GetFieldNames().IndexOf("DD");
				int dataTypeIndex = firstDataSet.GetFieldNames().IndexOf("DataType");
				for (int i = 0; i < firstDataSet.GetNumberOfObservations(); i++)
				{
					double d1 = (double)firstDataSet.GetValueAt(i, ddFieldIndex);
					//		double d2 = ((Number)secondDataSet.getValueAt(i, ddFieldIndex)).doubleValue();
					double d2 = (double)secondDataSet.GetValueAt(i, ddFieldIndex);
					string dataType1 = (string)firstDataSet.GetValueAt(i, dataTypeIndex);
					String dataType2 = (string)secondDataSet.GetValueAt(i, dataTypeIndex);
					Assert.AreEqual(d1, d2, 480);
					Assert.IsTrue(dataType1.Contains("Simulated"));
					Assert.IsTrue(dataType2.Contains("Simulated"));
				}
				Console.WriteLine("Degree-days tested for default values.");
			}
		}


		/*
		 * Tests if the weather generation over past and future time intervals.
		 */
		[TestMethod]
		public void testingWithForceClimateGenerationEnabled()
		{
			BioSimClient.SetForceClimateGenerationEnabled(true);
			List<IBioSimPlot> locations = new();
			locations.Add(BioSimClientTestSettings.Instance.Plots[0]);

			int initialDateYr = 2000;

			string modelName = "DegreeDay_Annual";
			OrderedDictionary teleIORefs = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, 2040, locations, null, null, new List<string>(new string[] { modelName }), null)[modelName];
			OrderedDictionary teleIORefs2 = (OrderedDictionary)BioSimClient.GenerateWeather(initialDateYr, 2040, locations, null, null, new List<string>(new string[] { modelName }), null)[modelName];

			foreach (IBioSimPlot plot in teleIORefs.Keys)
			{
				BioSimDataSet firstDataSet = (BioSimDataSet)teleIORefs[plot];
				BioSimDataSet secondDataSet = (BioSimDataSet)teleIORefs2[plot];
				Assert.IsTrue(firstDataSet.GetNumberOfObservations() > 0);
				Console.WriteLine("There is at least one observation");
				Assert.AreEqual(firstDataSet.GetNumberOfObservations(), secondDataSet.GetNumberOfObservations());
				Console.WriteLine("Same number of observations in each dataset");

				int ddFieldIndex = firstDataSet.GetFieldNames().IndexOf("DD");
				int dataTypeIndex = firstDataSet.GetFieldNames().IndexOf("DataType");
				for (int i = 0; i < firstDataSet.GetNumberOfObservations(); i++)
				{
					double d1 = (double)firstDataSet.GetValueAt(i, ddFieldIndex);
					double d2 = (double)secondDataSet.GetValueAt(i, ddFieldIndex); ;
					string dataType1 = (string)firstDataSet.GetValueAt(i, dataTypeIndex);
					string dataType2 = (string)secondDataSet.GetValueAt(i, dataTypeIndex);
					Assert.IsTrue(Math.Abs(d1 - d2) > 1E-8);  // Testing that the degree-days are different for all years 
					Assert.IsTrue(dataType1.Contains("Simulated"));  // Testing if was simulated 
					Assert.IsTrue(dataType2.Contains("Simulated"));  // Testing if was simulated
				}
				Console.WriteLine("Degree-days all vary because climate generation is enabled.");
			}
			BioSimClient.SetForceClimateGenerationEnabled(false);       // set it back to default value 
		}





	}


}
