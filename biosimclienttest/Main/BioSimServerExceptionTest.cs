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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
	[TestClass]
	public class BioSimServerExceptionTest
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


		[TestMethod]
		public void incorrectNormalsRequestWithNaN()
		{
			BioSimFakeLocation fakeLocation = new(double.NaN, double.NaN, double.NaN);
			List<IBioSimPlot> locations = new();
			locations.Add(fakeLocation);
			try
			{
				BioSimClient.GetMonthlyNormals(Period.FromNormals1951_1980, locations, RCP.RCP45, ClimateModel.RCM4);
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
			catch (BioSimClientException e)
			{
				string errMsg = e.Message;
				Assert.IsTrue(errMsg.Contains("the lat parameter cannot be parsed") || errMsg.Contains("argument lat could not be parsed to a NaN"));
				Assert.IsTrue(errMsg.Contains("the long parameter cannot be parsed") || errMsg.Contains("argument long could not be parsed to a NaN"));

			}
			catch (Exception)
			{
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
		}

		[TestMethod]
		public void incorrectNormalsRequestWithInconsistentLatitudeAndLongitudeValues()
		{
			BioSimFakeLocation fakeLocation = new(-2000, 2000, Double.NaN);
			List<IBioSimPlot> locations = new();
			locations.Add(fakeLocation);
			try
			{
				BioSimClient.GetMonthlyNormals(Period.FromNormals1951_1980, locations, RCP.RCP45, ClimateModel.RCM4);
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
			catch (BioSimClientException e)
			{
				string errMsg = e.Message;
				Assert.IsTrue(errMsg.Contains("lat is out of range") || errMsg.Contains("the latitude must range"));
				Assert.IsTrue(errMsg.Contains("long is out of range") || errMsg.Contains("the longitude must range"));
			}
			catch (Exception)
			{
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
		}


		[TestMethod]
		public void incorrectWeatherGenerationRequestWithInconsistentLatitudeAndLongitudeValues()
		{
			BioSimFakeLocation fakeLocation = new(-2000, 2000, Double.NaN);
			List<IBioSimPlot> locations = new();
			locations.Add(fakeLocation);
			try
			{
				BioSimClient.GenerateWeather(2000, 2001, locations, RCP.RCP45, ClimateModel.RCM4, new List<string>(new string[] { "DegreeDay_Annual" }), null);
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
			catch (BioSimClientException e)
			{
				string errMsg = e.Message;
				Assert.IsTrue(errMsg.Contains("lat is out of range") || errMsg.Contains("the latitude must range"));
				Assert.IsTrue(errMsg.Contains("long is out of range") || errMsg.Contains("the longitude must range"));
			}
			catch (Exception)
			{
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
		}

		[TestMethod]
		public void incorrectModelHelpRequest()
		{
			try
			{
				BioSimClient.GetModelHelp("Blabla");
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
			catch (BioSimClientException e)
			{
				string errMsg = e.Message;
				Assert.IsTrue(errMsg.Contains("Error: Model Blabla does not exist"));
			}
			catch (Exception)
			{
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
		}

		[TestMethod]
		public void incorrectModelDefaultParametersRequest()
		{
			try
			{
				BioSimClient.GetModelDefaultParameters("Blabla");
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
			catch (BioSimClientException e)
			{
				string errMsg = e.Message;
				Assert.IsTrue(errMsg.Contains("Error: Model Blabla does not exist"));
			}
			catch (Exception)
			{
				Assert.Fail("Should have thrown a BioSimClientException instance");
			}
		}

	}
}
