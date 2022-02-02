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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace biosimclienttest
{
    [TestClass]
    public class BioSimClientNormalsTest
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
		public void getNormalsFor1981_2010() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals1981_2010, BioSimClientTestSettings.Instance.Plots, null, null);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}


		[TestMethod]
		public void getNormalsFor2051_2080_Hadley_RCP45() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, null, ClimateModel.Hadley);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}

		[TestMethod]
		public void getNormalsFor2051_2080_Hadley_RCP85() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, RCP.RCP85, ClimateModel.Hadley);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}

		[TestMethod]
		public void GetNormalsFor2051_2080_RCM4_RCP45() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, null, null);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}

		[TestMethod]
		public void getNormalsFor2051_2080_RCM4_RCP85() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, RCP.RCP85, null);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}

		[TestMethod]
		public void getNormalsFor2051_2080_GCM4_RCP45() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, RCP.RCP45,	ClimateModel.GCM4);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}

		[TestMethod]
		public void getNormalsFor2051_2080_GCM4_RCP85() 
		{
			OrderedDictionary teleIO = BioSimClient.GetAnnualNormals(Period.FromNormals2051_2080, BioSimClientTestSettings.Instance.Plots, RCP.RCP85, ClimateModel.GCM4);

			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(0);
			string methodName = stackFrame.GetMethod().Name;
			string validationFilename = BioSimClientTestSettings.GetFilename(methodName);

			BioSimDataSet dataSet = BioSimDataSet.ConvertLinkedHashMapToBioSimDataSet(teleIO);
			string observedString = BioSimClientTestSettings.GetJSONObject(dataSet);

			string referenceString = BioSimClientTestSettings.GetReferenceString(validationFilename);

			Assert.AreEqual(referenceString, observedString);
		}


		[TestMethod]
		public void getMonthlyNormalsFor1971_2000() 	
		{
			OrderedDictionary teleIO = BioSimClient.GetMonthlyNormals(Period.FromNormals1971_2000, BioSimClientTestSettings.Instance.Plots, null,	null);

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
