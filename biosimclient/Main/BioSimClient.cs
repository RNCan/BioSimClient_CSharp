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
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace biosimclient.Main
{

	internal class InetSocketAddress
    {
		internal string Hostname { get; private set; }
		internal int Port { get; private set; }

		internal InetSocketAddress(string hostName, int port)
        {
			Hostname = hostName;
			Port = port;
        }
    }

	/// <summary>
	/// A client for the BioSIM Web API.
	/// </summary>
	public sealed class BioSimClient
	{
		private static readonly int Revision = 1;

		private static int MAXIMUM_NB_LOCATIONS_PER_BATCH_WEATHER_GENERATION = -1; // not set yet
		private static int MAXIMUM_NB_LOCATIONS_PER_BATCH_NORMALS = -1; // not set yet
		private static int MAXIMUM_NB_LOCATIONS_IN_A_SINGLE_REQUEST = 1000; // not set yet
		private static bool? IS_CLIENT_SUPPORTED = null;
		private static string CLIENT_MESSAGE;


		static readonly string FieldSeparator = ",";
	
		private static readonly InetSocketAddress REpiceaAddress = new("repicea.dynu.net", 80);
		private static readonly InetSocketAddress LocalAddress = new("192.168.0.194", 88);

		private static readonly string SPACE_IN_REQUEST = "%20";

		static readonly List<Month> AllMonths = new((Month[])Enum.GetValues(typeof(Month))); 

		private static readonly string NORMAL_API = "BioSimNormals";
		private static readonly string MODEL_LIST_API = "BioSimModelList";
		private static readonly string BIOSIMSTATUS = "BioSimStatus";
		private static readonly string BIOSIMMODELHELP = "BioSimModelHelp";
		private static readonly string BIOSIMMODELDEFAULTPARAMETERS = "BioSimModelDefaultParameters";
		private static readonly string BIOSIMWEATHER = "BioSimWeather";

		private static Stopwatch SW = new();

		private static readonly HttpClient httpClient = new();

		private static List<string> ReferenceModelList;

		private static double totalServerRequestDuration = 0.0;

		private static bool IsLocal = false;       
		private static bool IsTesting = false;

		static bool ForceClimateGenerationEnabled = false;  // default value

		static int? NbNearestNeighbours = null;


		private static string AddQueryIfAny(string urlString, string query)
		{
			bool isThereQuery = false;
			string finalUrlQuery;
			if (query != null && query.Length > 0)
            {
				isThereQuery = true;
				finalUrlQuery = urlString.Trim() + "?" + query;
			}
			else
				finalUrlQuery = urlString;

			if (BioSimClient.IsTesting)
				finalUrlQuery = isThereQuery ? finalUrlQuery + "&cid=testJava" : finalUrlQuery + "?cid=testJava";

			return finalUrlQuery;
		}


		[MethodImpl(MethodImplOptions.Synchronized)]
		private static BioSimStringList GetStringFromConnection(string api, string query) // TODO should throws BioSimClientException, BioSimServerException
		{
			//		long initTime = System.currentTimeMillis();
			InetSocketAddress address = IsLocal ? BioSimClient.LocalAddress : BioSimClient.REpiceaAddress;
			String urlString = "http://" + address.Hostname + ":" + address.Port + "/" + api;
			urlString = AddQueryIfAny(urlString, query);
			HttpResponseMessage response = null;
			try
			{
				Uri bioSimURL = new(urlString);
				SW.Restart();
				SW.Start();
				HttpRequestMessage message = new(HttpMethod.Get, bioSimURL);
				response = httpClient.Send(message);
				int code = (int)response.StatusCode;
				SW.Stop();
				totalServerRequestDuration += SW.ElapsedMilliseconds * 0.001;
				if (code >= 400 && code < 500)
				{ // client error
					String msg = GetCompleteString(response, true).ToString();
					throw new BioSimClientException("Code " + code + ": " + msg);
				}
				if (code >= 500 && code < 600)
				{ // server error
					String msg = GetCompleteString(response, true).ToString();
					throw new BioSimServerException("Code " + code + ": " + msg);
				}
				// TODO MF2022-01-18 Handle other codes here
				//			System.out.println("Time for server to process request: " + (System.currentTimeMillis() - initTime) + " ms");
				return GetCompleteString(response, false);
			}
			catch (UriFormatException e)
			{
				throw new BioSimClientException("Malformed URI: " + e.Message);
			}
			catch (HttpRequestException e)
			{
				throw new BioSimClientException("Unknown host: " + e.Message);
			}
			//catch (CertificateException e)
			//{
			//	throw new BioSimClientException("Unable to confirm certificate for secure connection!");
			//}
			catch (IOException)
			{
				throw new BioSimClientException("Unable to connect to the server!");
			} finally
            {
				if (response != null)
					response.Dispose();
            }
		}

		private static int getMaximumNbLocationsPerBatchNormals() //throws BioSimClientException, BioSimServerException {
		{
			return MAXIMUM_NB_LOCATIONS_PER_BATCH_NORMALS;
		}

		/// <summary>
		/// Reset the configuration to its initial values.
		/// </summary>
		public static void ResetClientConfiguration()
		{
			NbNearestNeighbours = null;
			ForceClimateGenerationEnabled = false;
			IsLocal = false;
			IsTesting = false;
		}


		private static BioSimStringList GetCompleteString(HttpResponseMessage connection, bool isError)
		{
			BioSimStringList stringList = new();
			try
			{
				Stream s = connection.Content.ReadAsStream();
				StreamReader br = new(s);
				string lineStr;
				while ((lineStr = br.ReadLine()) != null)
				{
					stringList.Add(lineStr);
				}
			}
			catch (IOException e)
			{
				stringList.Add(e.Message);
			}
			return stringList;
		}


		/// <summary>
		/// Check if the server supports the client. 
		/// <br></br>
		/// <br></br>
		/// If the client is not supported, an exception is thrown. The server 
		/// can also provide a message that is embedded in the exception if the client is not supported or simply
		/// displayed as a warning if the client is supported. 
		/// </summary>
		/// 
		/// <returns>A string that is a eventual message from the BioSIM Web API</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static string IsClientSupported()
        {
			if (!IS_CLIENT_SUPPORTED.HasValue)
			{
				string query = $"crev={Revision}";
				string serverReply = GetStringFromConnection(BIOSIMSTATUS, query).ToString();   // is returned in JSON format
				OrderedDictionary statusMap = null;
				try
				{
					statusMap = JsonConvert.DeserializeObject<OrderedDictionary>(serverReply);
				} 
				catch (Exception e) 
				{
					throw new BioSimClientException("Something wrong happened while retrieving the server status: " + e.Message);
				}

				if (!(Boolean)statusMap["IsInitCompleted"])
					throw new BioSimClientException("The server initialization is not completed!");
				
				if (!statusMap.Contains("settings"))
					throw new BioSimClientException("The status map does not contain the entry settings!");
				
				try
				{
					OrderedDictionary settingsMap = JsonConvert.DeserializeObject<OrderedDictionary>(statusMap["settings"].ToString());
                    MAXIMUM_NB_LOCATIONS_PER_BATCH_NORMALS = Convert.ToInt32(settingsMap["NbMaxCoordinatesNormals"]);
                    MAXIMUM_NB_LOCATIONS_PER_BATCH_WEATHER_GENERATION = Convert.ToInt32(settingsMap["NbMaxCoordinatesWG"]);
                    IS_CLIENT_SUPPORTED = settingsMap.Contains("IsClientSupported") ? (bool)settingsMap["IsClientSupported"] : true;
                    CLIENT_MESSAGE = settingsMap.Contains("ClientMessage") ? (string)settingsMap["ClientMessage"] : "";
                }
				catch (Exception)
				{
					throw new BioSimClientException("The server status could not be parsed!");
				}
				if (IS_CLIENT_SUPPORTED.Value && CLIENT_MESSAGE.Length > 0)
					Console.WriteLine("WARNING: " + CLIENT_MESSAGE);
			}
			if (!IS_CLIENT_SUPPORTED.Value)
				throw new BioSimClientException(CLIENT_MESSAGE);
			return CLIENT_MESSAGE;
        }

		private static String ProcessElevationM(IBioSimPlot location)
		{
			if (double.IsNaN(location.GetElevationM()))
				return "NaN";
			else
				return "" + location.GetElevationM();
		}


		private static StringBuilder ConstructCoordinatesQuery(List<IBioSimPlot> locations)
		{
			StringBuilder latStr = new();
			StringBuilder longStr = new();
			StringBuilder elevStr = new();
			String latStrThisLoc, longStrThisLoc, elevStrThisLoc;
			foreach (IBioSimPlot location in locations)
			{
				latStrThisLoc = latStr.Length == 0 ? "" + location.GetLatitudeDeg() : SPACE_IN_REQUEST + location.GetLatitudeDeg();
				latStr.Append(latStrThisLoc);
				longStrThisLoc = longStr.Length == 0 ? "" + location.GetLongitudeDeg() : SPACE_IN_REQUEST + location.GetLongitudeDeg();
				longStr.Append(longStrThisLoc);
				elevStrThisLoc = elevStr.Length == 0 ? "" + ProcessElevationM(location) : SPACE_IN_REQUEST + ProcessElevationM(location);
				elevStr.Append(elevStrThisLoc);
			}

			StringBuilder query = new();
			query.Append("lat=" + latStr.ToString());
			query.Append("&long=" + longStr.ToString());
			if (elevStr.Length != 0)
			{
				query.Append("&elev=" + elevStr.ToString());
			}
			return query;
		}


		private static OrderedDictionary InternalCalculationForNormals(Period period,
				List<IBioSimPlot> locations,
				RCP? rcp,
				ClimateModel? climModel,
				List<Month> averageOverTheseMonths) // throws BioSimClientException, BioSimServerException 
		{
			//			LinkedHashMap<BioSimPlot, BioSimDataSet> outputMap = new LinkedHashMap<BioSimPlot, BioSimDataSet>();
			OrderedDictionary outputMap = new();
			StringBuilder query = ConstructCoordinatesQuery(locations);
			query.Append("&" + EnumUtilities.GetQueryPartForThisPeriod(period));

			if (rcp.HasValue)
				query.Append("&rcp=" + EnumUtilities.GetURLStringForThisRCP(rcp.Value));

			if (climModel.HasValue)
				query.Append("&climMod=" + climModel.Value.ToString());

			BioSimStringList serverReply = BioSimClient.GetStringFromConnection(NORMAL_API, query.ToString());

			ReadLines(serverReply, "month", locations, outputMap);

			if (averageOverTheseMonths == null || averageOverTheseMonths.Count == 0)
			{
				List<int> fieldsToBeRemoved = null;
				foreach (BioSimDataSet bioSimDataSet in outputMap.Values)
				{
					if (fieldsToBeRemoved == null)
					{
						fieldsToBeRemoved = new();
						for (int i = bioSimDataSet.GetFieldNames().Count - 1; i > 0; i--)
						{   // reverse order
							if (!EnumUtilities.GetFieldNamesForNormals().Contains(bioSimDataSet.GetFieldNames()[i]))
								fieldsToBeRemoved.Add(i);
						}
					}
					foreach (int fieldId in fieldsToBeRemoved)
						bioSimDataSet.RemoveField(fieldId);
				}
				return outputMap;
			}
			else
			{
//				LinkedHashMap<BioSimPlot, BioSimDataSet> formattedOutputMap = new LinkedHashMap<BioSimPlot, BioSimDataSet>();
				OrderedDictionary formattedOutputMap = new();
				foreach (IBioSimPlot location in outputMap.Keys)
				{
					BioSimDataSet ds = (BioSimDataSet)outputMap[location];
					BioSimMonthMap bsmm = new BioSimMonthMap(ds);
					formattedOutputMap.Add(location, bsmm.GetMeanForTheseMonths(averageOverTheseMonths));
				}
				return formattedOutputMap;
			}
		}


		/// <summary>
		/// Retrieve the normals and compile the mean or sum over several months
		/// </summary>
		/// <param name="period">A Period enum variable</param>
		/// <param name="locations">A List of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (if null the server takes the RCP 4.5 by default)</param>
		/// <param name="climModel">A ClimateModel enum variable (if null the server takes the RCM4 climate model)</param>
		/// <param name="averageOverTheseMonths">A list of Monte enum variables over which the mean or sum is to be calculated. If empty or null the method returns the montly averages</param>
		/// <returns>An OrderedDictionary instance with the normals</returns>
		public static OrderedDictionary GetNormals(
				Period period,
				List<IBioSimPlot> locations,
				RCP? rcp,
				ClimateModel? climModel,
				List<Month> averageOverTheseMonths) 
		{
			IsClientSupported();
			if (locations.Count > BioSimClient.MAXIMUM_NB_LOCATIONS_IN_A_SINGLE_REQUEST)
				throw new BioSimClientException($"The maximum number of locations for a single request is {MAXIMUM_NB_LOCATIONS_IN_A_SINGLE_REQUEST}");

			if (locations.Count > BioSimClient.getMaximumNbLocationsPerBatchNormals())
			{
				OrderedDictionary resultingMap = new();
				List<IBioSimPlot> copyList = new();
				copyList.AddRange(locations);
				List<IBioSimPlot> subList = new();
				while (copyList.Count > 0)
				{
					while (copyList.Count > 0 && subList.Count < BioSimClient.getMaximumNbLocationsPerBatchNormals())
                    {
						subList.Add(copyList[0]);
						copyList.RemoveAt(0);
					}
					foreach (DictionaryEntry newEntry in InternalCalculationForNormals(period, subList, rcp, climModel, averageOverTheseMonths))
						resultingMap.Add(newEntry.Key, newEntry.Value);
					subList.Clear();
				}
				return resultingMap;
			}
			else
				return InternalCalculationForNormals(period, locations, rcp, climModel, averageOverTheseMonths);
		}


		/// <summary>
		/// Provide the monthly normals.
		/// </summary>
		/// <param name="period">A Period enum variable</param>
		/// <param name="locations">A List of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (if null the server takes the RCP 4.5 by default)</param>
		/// <param name="climModel">A ClimateModel enum variable (if null the server takes the RCM4 climate model)</param>
		/// <returns>An OrderedDictionary instance with the normals</returns>
		public static OrderedDictionary GetMonthlyNormals(Period period,
			List<IBioSimPlot> locations,
			RCP? rcp,
			ClimateModel? climModel) // throws BioSimClientException, BioSimServerException {
		{
			return GetNormals(period, locations, rcp, climModel, null);
		}


		/// <summary>
		/// Provide the yearly normals.
		/// </summary>
		/// <param name="period">A Period enum variable</param>
		/// <param name="locations">A List of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (if null the server takes the RCP 4.5 by default)</param>
		/// <param name="climModel">A ClimateModel enum variable (if null the server takes the RCM4 climate model)</param>
		/// <returns>An OrderedDictionary instance with the normals</returns>
		public static OrderedDictionary GetAnnualNormals(
				Period period,
				List<IBioSimPlot> locations,
				RCP? rcp,
				ClimateModel? climModel) //throws BioSimClientException, BioSimServerException {
		{
			return GetNormals(period, locations, rcp, climModel, AllMonths);
		}



		/**
		 * Returns the names of the available models. This is a clone of the
		 * true list to avoid any intended changes in the model list.
		 * 
		 * @return a List of String instances
		 */


		/// <summary>
		/// Return the names of the available models. 
		/// <br></br>
		/// <br></br>
		/// This is a clone of the true list to avoid any unintended changes in the model list.
		/// </summary>
		/// <returns>a list of strings</returns>
		public static List<string> GetModelList() //throws BioSimClientException, BioSimServerException {
		{
			IsClientSupported();
			List<string> copy = new();
			copy.AddRange(GetReferenceModelList());
			return copy;
		}

		/// <summary>
		/// Provide help for a particular model.
		/// </summary>
		/// <param name="modelName">the model name</param>
		/// <returns>a description of the model</returns>
		public static string GetModelHelp(String modelName) 
		{
			IsClientSupported();
			if (modelName == null) 
				throw new ArgumentException("THe modelName parameter cannot be set to null!");

			String serverReply = GetStringFromConnection(BIOSIMMODELHELP, "model=" + modelName).ToString();
			return serverReply;
		}

		/// <summary>
		/// Provide the default parameters of a particular model.
		/// </summary>
		/// <param name="modelName">the model name</param>
		/// <returns>the model parameters nested into a BioSimParameterMap instance</returns>
		public static BioSimParameterMap GetModelDefaultParameters(String modelName) // throws BioSimClientException, BioSimServerException {
		{
			IsClientSupported();
			if (modelName == null) 
				throw new ArgumentException("THe modelName parameter cannot be set to null!");

			String serverReply = GetStringFromConnection(BIOSIMMODELDEFAULTPARAMETERS, "model=" + modelName).ToString();
			String[] parms = serverReply.Split("*");
			BioSimParameterMap parmMap = new BioSimParameterMap();
			foreach (String parm in parms)
			{
				String[] keyValue = parm.Split(":");
				if (keyValue.Length > 1)
					parmMap.AddParameter(keyValue[0], keyValue[1]);
				else
					parmMap.AddParameter(keyValue[0], "");
			}
			return parmMap;
		}


		private static List<string> GetReferenceModelList() 
		{
			if (ReferenceModelList == null)
			{
				List<string> myList = new();
				BioSimStringList modelList = BioSimClient.GetStringFromConnection(BioSimClient.MODEL_LIST_API, null);
				foreach (String model in modelList)
					myList.Add(model);

				ReferenceModelList = new();
				ReferenceModelList.AddRange(myList);
			}
			return ReferenceModelList;
		}


		private static void ReadLines(BioSimStringList serverReply,
			String fieldLineStarter,
			List<IBioSimPlot> refListForLocations,
			OrderedDictionary outputMap) 
		{
			//		long initTime;
			//		long totalTime = 0;
			BioSimDataSet dataSet = null;
			int locationId = 0;
			IBioSimPlot location = null;
			bool isDataSetProperlyInitialized = false;
			string modName = null;
			OrderedDictionary resultMap = new();
			foreach (String line in serverReply)
			{
				if (line.ToLowerInvariant().StartsWith("error"))		// TODO ask JF about tolowerinvariant?
				{
					throw new BioSimServerException(line);
				}
				else if (BioSimClient.GetModelList().Contains(line.Trim()))
				{
					resultMap = new();
					modName = line.Trim();
					outputMap.Add(modName, resultMap);
					locationId = 0;
					isDataSetProperlyInitialized = false;
				}
				else if (line.ToLowerInvariant().StartsWith(fieldLineStarter))
				{ // means it is a new location
					if (dataSet != null)    // must be indexed before instantiating a new DataSet
						dataSet.IndexFieldType();

					location = refListForLocations[locationId];
					string[] fields = line.Split(FieldSeparator);
					List<string> fieldNames = fields.ToList();
					dataSet = new BioSimDataSet(fieldNames);
					resultMap.Add(location, dataSet);
					locationId++;
					isDataSetProperlyInitialized = true;
				}
				else
				{
					if (!isDataSetProperlyInitialized)
					{
						if (modName != null)
							outputMap[modName] = new BioSimClientException(line);
						else 
							throw new BioSimClientException(serverReply.ToString());
					}
					else
					{
						Object[] fields = line.Split(FieldSeparator).ToList().ToArray<object>();
						dataSet.AddObservation(fields);
					}
				}
			}
			if (dataSet != null)
				dataSet.IndexFieldType();   // last DataSet has not been instantiated so it needs to be here.

			if (outputMap.Count == 0)
			{
				foreach (DictionaryEntry newEntry in resultMap) 
					outputMap.Add(newEntry.Key, newEntry.Value);
			}
		}

		private static OrderedDictionary InternalCalculationForClimateVariables(int fromYr,
			int toYr,
			List<IBioSimPlot> locations,
			RCP? rcp,
			ClimateModel? climMod,
			List<string> modelNames,
			int rep,
			int repModel,
			List<BioSimParameterMap> additionalParms) 
		{
			StringBuilder query = ConstructCoordinatesQuery(locations);
			query.Append("&from=" + fromYr);
			query.Append("&to=" + toYr);
			if (rcp.HasValue)
				query.Append("&rcp=" + EnumUtilities.GetURLStringForThisRCP(rcp.Value));
			if (climMod.HasValue)
				query.Append("&climMod=" + climMod.Value.ToString());
			if (ForceClimateGenerationEnabled)
			{
				Console.WriteLine("Warning: past climate is generated instead of being compiled from observations!");
				query.Append("&source=FromNormals");
			}
			if (NbNearestNeighbours != null)
				query.Append("&nb_nearest_neighbor=" + NbNearestNeighbours.ToString());
			if (rep > 1)
				query.Append("&rep=" + rep);
			for (int i = 0; i < modelNames.Count; i++)
			{
				if (i == 0)
					query.Append("&model=" + modelNames[i]);
				else
					query.Append(BioSimClient.SPACE_IN_REQUEST + modelNames[i]);
			}
			if (repModel > 1)
				query.Append("&repmodel=" + repModel);
			if (additionalParms != null)
			{
				StringBuilder sbParms = new();
				foreach (BioSimParameterMap oMap in additionalParms)
				{
					string strForThisMap = oMap == null || oMap.IsEmpty() ? "null" : oMap.ToString();
					if (sbParms.Length == 0)
						sbParms.Append(strForThisMap);
					else
						sbParms.Append(SPACE_IN_REQUEST + strForThisMap);
				}
				query.Append("&Parameters=" + sbParms.ToString());
			}
			//		System.out.println("Constructing request: " + (System.currentTimeMillis() - initTime) + " ms");
			BioSimStringList serverReply = GetStringFromConnection(BIOSIMWEATHER, query.ToString());		
			OrderedDictionary outputMap = new();
			//		long initTime = System.currentTimeMillis();
			ReadLines(serverReply, "rep", locations, outputMap);
			//		System.out.println("Total time to convert string into biosim dataset: " + (System.currentTimeMillis() - initTime) + " ms.");
			return outputMap;
		}


		/// <summary>
		/// Generate the meteorogical time series and apply one or many models on these series.
		/// <br></br>
		/// <br></br>
		/// The "modelnames" argument sets the models to be applied on the generated meteorological time series. 
		/// These model names should be contained in the list produced by the GetModelList method. 
		/// </summary>
		/// <param name="fromYr">The start date (yr) of the period (inclusive)</param>
		/// <param name="toYr">The end date (yr) of the period (inclusive)</param>
		/// <param name="locations">A list of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (by default RCP 4.5)</param>
		/// <param name="climMod">A ClimateModel enum variable (by default RCM 4)</param>
		/// <param name="modelNames">a list of strings representing the model names</param>
		/// <param name="rep">The number of replicates in climate generation if needed. Should be equal to or greater than 1. By default it is set to 1.</param>
		/// <param name="additionalParms">A list of BioSimParameterMap instances that contain the eventual additional parameters for the models</param>
		/// <returns>An OrderedDictionary instance with the model names as key</returns>
		public static OrderedDictionary GenerateWeather(int fromYr,
				int toYr,
				List<IBioSimPlot> locations,
				RCP? rcp,
				ClimateModel? climMod,
				List<String> modelNames,
				int rep,
				List<BioSimParameterMap> additionalParms)   // throws BioSimClientException, BioSimServerException {
		{
			return BioSimClient.GenerateWeather(fromYr, toYr, locations, rcp, climMod, modelNames, rep, 1, additionalParms);
		}



		/// <summary>
		/// Generate the meteorogical time series and apply one or many models on these series.
		/// <br></br>
		/// <br></br>
		/// The "modelnames" argument sets the models to be applied on the generated meteorological time series. 
		/// These model names should be contained in the list produced by the GetModelList method. 
		/// </summary>
		/// <param name="fromYr">The start date (yr) of the period (inclusive)</param>
		/// <param name="toYr">The end date (yr) of the period (inclusive)</param>
		/// <param name="locations">A list of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (by default RCP 4.5)</param>
		/// <param name="climMod">A ClimateModel enum variable (by default RCM 4)</param>
		/// <param name="modelNames">a list of strings representing the model names</param>
		/// <param name="additionalParms">A list of BioSimParameterMap instances that contain the eventual additional parameters for the models</param>
		/// <returns>An OrderedDictionary instance with the model names as key</returns>
		public static OrderedDictionary GenerateWeather(int fromYr,
			int toYr,
			List<IBioSimPlot> locations,
			RCP? rcp,
			ClimateModel? climMod,
			List<String> modelNames,
			List<BioSimParameterMap> additionalParms) // throws BioSimClientException, BioSimServerException {
		{ 
			return BioSimClient.GenerateWeather(fromYr, toYr, locations, rcp, climMod, modelNames, 1, 1, additionalParms);
		}


		/// <summary>
		/// Generate the meteorogical time series and apply one or many models on these series.
		/// <br></br>
		/// <br></br>
		/// The "modelnames" argument sets the models to be applied on the generated meteorological time series. 
		/// These model names should be contained in the list produced by the GetModelList method. 
		/// </summary>
		/// <param name="fromYr">The start date (yr) of the period (inclusive)</param>
		/// <param name="toYr">The end date (yr) of the period (inclusive)</param>
		/// <param name="locations">A list of IBioSimPlot instances</param>
		/// <param name="rcp">An RCP enum variable (by default RCP 4.5)</param>
		/// <param name="climMod">A ClimateModel enum variable (by default RCM 4)</param>
		/// <param name="modelNames">a list of strings representing the model names</param>
		/// <param name="rep">The number of replicates in climate generation if needed. Should be equal to or greater than 1. By default it is set to 1.</param>
		/// <param name="repModel">The number of replicates in the model. Should be equal to or greater than 1. By default it is set to 1.</param>
		/// <param name="additionalParms">A list of BioSimParameterMap instances that contain the eventual additional parameters for the models</param>
		/// <returns>An OrderedDictionary instance with the model names as key</returns>
		public static OrderedDictionary GenerateWeather(int fromYr,
				int toYr,
				List<IBioSimPlot> locations,
				RCP? rcp,
				ClimateModel? climMod,
				List<String> modelNames,
				int rep,
				int repModel,
				List<BioSimParameterMap> additionalParms) // throws BioSimClientException, BioSimServerException {
		{
			IsClientSupported();
			if (rep < 1 || repModel < 1)
				throw new ArgumentException("The rep and repModel parameters should be equal to or greater than 1!");

			if (locations.Count > MAXIMUM_NB_LOCATIONS_IN_A_SINGLE_REQUEST)
				throw new BioSimClientException("The maximum number of locations for a single request is " + MAXIMUM_NB_LOCATIONS_IN_A_SINGLE_REQUEST);

			totalServerRequestDuration = 0.0;

			if (locations.Count > BioSimClient.GetMaximumNbLocationsPerBatchWeatherGeneration())
			{
				OrderedDictionary resultingMap = null;
				List<IBioSimPlot> copyList = new();
				copyList.AddRange(locations);
				List<IBioSimPlot> subList = new();
				while (copyList.Count > 0)
				{
					while (copyList.Count > 0 && subList.Count < BioSimClient.GetMaximumNbLocationsPerBatchWeatherGeneration())
                    {
						subList.Add(copyList[0]);
						copyList.RemoveAt(0);
					}
					OrderedDictionary intermediateMap = InternalCalculationForClimateVariables(fromYr, toYr, subList, rcp, climMod, modelNames, rep, repModel, additionalParms);
					if (resultingMap == null)
						resultingMap = intermediateMap;
					else
					{
						foreach (String key in resultingMap.Keys)
							//							resultingMap[key].putAll(intermediateMap[key]);
							foreach (DictionaryEntry newEntry in (OrderedDictionary)intermediateMap[key])
								((OrderedDictionary)resultingMap[key]).Add(newEntry.Key, newEntry.Value);


					}
					subList.Clear();
				}
				return resultingMap;
			}
			else
				return InternalCalculationForClimateVariables(fromYr, toYr, locations, rcp, climMod, modelNames, rep, repModel, additionalParms);
		}


		private static int GetMaximumNbLocationsPerBatchWeatherGeneration() // throws BioSimClientException, BioSimServerException {
		{
			return MAXIMUM_NB_LOCATIONS_PER_BATCH_WEATHER_GENERATION;
		}


		/**
		 * 
		 * @param bool a boolean
		 */


		/// <summary>
		/// Force the climate generation for past dailies.
		/// <br></br>
		/// <br></br>
		/// By default the climate generation retrieves the observations for the dates prior to the current date. If this option is set to true, then
		/// the climate is generated from the disaggregation of normals even for dates prior to the current date.
		/// </summary>
		/// <param name="b">a boolean (true to force climate generation or false to use the observed dailies</param>
		public static void SetForceClimateGenerationEnabled(bool b)
		{
			ForceClimateGenerationEnabled = b;
		}


		/// <summary>
		/// Return true if the climate generation for past dailies is enabled or false otherwise.
		/// </summary>
		/// <returns>a boolean</returns>
		public static bool IsForceClimateGenerationEnabled()
		{
			return BioSimClient.ForceClimateGenerationEnabled;
		}


		/// <summary>
		/// Set the number of stations used for imputing the meteorological time series in space.
		/// </summary>
		/// <param name="nbNearestNeighbours">an integer between 1 and 35. By default, it is set to 4.</param>
		public static void SetNbNearestNeighbours(int nbNearestNeighbours)
		{
			if (nbNearestNeighbours < 1 || nbNearestNeighbours > 35)
			{
				throw new ArgumentException("The number of nearest neighbours must be an integer between 1 and 35!");
			}
			NbNearestNeighbours = nbNearestNeighbours;
		}


		/// <summary>
		/// Provide the number of climate stations used for imputing the meteorological time series in space.
		/// </summary>
		/// <returns>an integer</returns>
		public static int GetNbNearestNeighbours()
		{		
			if (NbNearestNeighbours.HasValue)
				return NbNearestNeighbours.Value;
			else
				return 4; // default value
		}

		/// <summary>
		/// Return true if the local connection is enabled (for test purpose only).
		/// </summary>
		/// <returns>a boolean</returns>
		public static bool IsLocalConnectionEnabled() { return IsLocal; }

		/// <summary>
		/// Enable/disable the local connection (for test purpose only).
		/// </summary>
		/// <param name="b">a boolean: true to enable/false to disable</param>
		public static void SetLocalConnectionEnabled(bool b) { IsLocal = b; }

		/// <summary>
		/// Return true if the test mode is enabled (for test purpose only).
		/// </summary>
		/// <returns>a boolean</returns>
		public static bool IsTestModeEnabled() { return IsTesting; }

		/// <summary>
		/// Enable/disable the test mode (for test purpose only).
		/// </summary>
		/// <param name="b">a boolean: true to enable/false to disable</param>
		public static void SetTestModeEnabled(bool b) { IsTesting = b; }

		/// <summary>
		/// Compute the time to produce a reply on the server end (for test purpose).
		/// </summary>
		/// <returns>a double</returns>
		public static double GetLastServerRequestDuration()
		{
			return totalServerRequestDuration;
		}
	
	}

}
