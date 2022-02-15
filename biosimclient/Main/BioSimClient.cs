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
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
		//		private static readonly string BIOSIMMAXCOORDINATES = "BioSimMaxCoordinatesPerRequest";
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
			catch (IOException e)
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
//			SetMaxCapacities();
			return MAXIMUM_NB_LOCATIONS_PER_BATCH_NORMALS;
		}

		/**
		 * Reset the configuration to its initial values.
		*/
		public static void ResetClientConfiguration()
		{
			NbNearestNeighbours = null;
			ForceClimateGenerationEnabled = false;
			IsLocal = false;
			IsTesting = false;
		}


		private static BioSimStringList GetCompleteString(HttpResponseMessage connection, bool isError)
		{
		//		long initTime = System.currentTimeMillis();
			BioSimStringList stringList = new();
			try
			{
				Stream s = connection.Content.ReadAsStream();
				//InputStream is;
				//if (isError)
				//	is = connection.getErrorStream();
				//else
				//	is = connection.getInputStream();
				StreamReader br = new(s);
				String lineStr;
				while ((lineStr = br.ReadLine()) != null)
				{
					stringList.Add(lineStr);
				}
				//			System.out.println("Time to make the complete string: " + (System.currentTimeMillis() - initTime) + " ms.");
			}
			catch (IOException e)
			{
				stringList.Add(e.Message);
			}
			return stringList;
		}


		// TODO MF2021should be synchronized
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

		/**
		 * Retrieves the normals and compiles the mean or sum over some months.
		* @param period a Period enum variable
		* @param locations a List of BioSimPlot instances
		* @param rcp an RCP enum variable (if null the server takes the RCP 4.5 by default 
		* @param climModel a ClimateModel enum variable (if null the server takes the RCM4 climate model
		* @param averageOverTheseMonths the months over which the mean or sum is to be
		*                               calculated. If empty or null the method returns
		*                               the monthly averages.
		* @return a Map with the BioSimPlot instances as keys and BioSimDataSet instances as values.
		* @throws BioSimClientException if the client fails or a BioSimServerException if the server fails 
		*/
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



		/**
		 * Retrieves the monthly normals.
		 * @param period a Period enum variable
		 * @param locations a List of BioSimPlot instances
		 * @param rcp an RCP enum variable (if null the server takes the RCP 4.5 by default 
		 * @param climModel a ClimateModel enum variable (if null the server takes the RCM4 climate model
		 * @return a Map with the BioSimPlot instances as keys and BioSimDataSet instances as values.
		 * @throws BioSimClientException if the client fails or a BioSimServerException if the server fails 
		 */
		public static OrderedDictionary GetMonthlyNormals(Period period,
			List<IBioSimPlot> locations,
			RCP? rcp,
			ClimateModel? climModel) // throws BioSimClientException, BioSimServerException {
		{
			return GetNormals(period, locations, rcp, climModel, null);
		}


		/**
		 * Retrieves the yearly normals.
		 * @param period a Period enum variable
		 * @param locations a List of BioSimPlot instances
		 * @param rcp an RCP enum variable (if null the server takes the RCP 4.5 by default 
		 * @param climModel a ClimateModel enum variable (if null the server takes the RCM4 climate model
		 * @return a Map with the BioSimPlot instances as keys and BioSimDataSet instances as values.
		 * @throws BioSimClientException if the client fails or a BioSimServerException if the server fails 
		 */
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
		public static List<string> GetModelList() //throws BioSimClientException, BioSimServerException {
		{
			IsClientSupported();
			List<string> copy = new();
			copy.AddRange(GetReferenceModelList());
			return copy;
		}


		public static string GetModelHelp(String modelName) // throws BioSimClientException, BioSimServerException {
		{
			IsClientSupported();
			if (modelName == null) 
				throw new ArgumentException("THe modelName parameter cannot be set to null!");

			String serverReply = GetStringFromConnection(BIOSIMMODELHELP, "model=" + modelName).ToString();
			return serverReply;
		}

	
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


		private static List<string> GetReferenceModelList() // throws BioSimClientException, BioSimServerException {
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
			OrderedDictionary outputMap) // throws BioSimClientException, BioSimServerException {
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
					if (dataSet != null)
					{   // must be indexed before instantiating a new DataSet
						//					initTime = System.currentTimeMillis();
						dataSet.IndexFieldType();
						//					totalTime += System.currentTimeMillis() - initTime;
					}
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
						//					initTime = System.currentTimeMillis();
						dataSet.AddObservation(fields);
						//					totalTime += System.currentTimeMillis() - initTime;
					}
				}
			}
			if (dataSet != null)
			{
				//			initTime = System.currentTimeMillis();
				dataSet.IndexFieldType();   // last DataSet has not been instantiated so it needs to be here.
											//			totalTime += System.currentTimeMillis() - initTime;
			}
			if (outputMap.Count == 0)
			{
				foreach (DictionaryEntry newEntry in resultMap) 
					outputMap.Add(newEntry.Key, newEntry.Value);
			}
			//		System.out.println("Time to create observations: " + totalTime + " ms");
		}

		private static OrderedDictionary InternalCalculationForClimateVariables(int fromYr,
			int toYr,
			List<IBioSimPlot> locations,
			RCP? rcp,
			ClimateModel? climMod,
			List<String> modelNames,
			int rep,
			int repModel,
			List<BioSimParameterMap> additionalParms) // throws BioSimClientException, BioSimServerException {
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
				int i = 0;
				foreach (BioSimParameterMap oMap in additionalParms)
				{
					String strForThisMap = oMap == null || oMap.IsEmpty() ? "null" : oMap.ToString();
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

		/**
		 * Returns a model output for a particular time interval. 
		 * 
		 * The "modelnames" argument sets the models to be applied on
		 * the generated meteorological time series. It should be among the strings returned by the 
		 * getModelList static method. Generating the climate is time consuming. The 
		 * generated climate is stored on the server and it can be re used with some 
		 * other models. 
		 * 
		 * @param fromYr starting date (yr) of the period (inclusive)
		 * @param toYr ending date (yr) of the period (inclusive)
		 * @param locations the locations of the plots (BioSimPlot instances)
		 * @param modelNames a list of strings representing the model names
		 * @param rcp an RCP enum variable (by default RCP 4.5)
		 * @param climMod a ClimateModel enum variable (by default RCM 4)
		 * @param rep the number of replicates in climate generation if needed. Should be equal to or greater than 1. 
		 * @param additionalParms a list of BioSimParameterMap instances that contains the eventual additional parameters for the models
		 * @return a LinkedHashMap of BioSimPlot instances (keys) and climate variables (values)
		 * @throws BioSimClientException if the client fails or BioSimServerException if the server fails
		 */
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



		/**
		 * Returns a model output for a particular time interval. 
		 * 
		 * The "modelnames" parameter sets the models to be applied on
		 * the generated climate. It should be one of the strings returned by the 
		 * getModelList static method. Generating the climate is time consuming. The 
		 * generated climate is stored on the server and it can be re used with some 
		 * other models. The number of replicate is set to 1.
		 * 
		 * @param fromYr starting date (yr) of the period (inclusive)
		 * @param toYr ending date (yr) of the period (inclusive)
		 * @param locations the locations of the plots (BioSimPlot instances)
		 * @param modelNames a list of strings representing the model names
		 * @param rcp an RCP enum variable (by default RCP 4.5)
		 * @param climMod a ClimateModel enum variable (by default RCM 4)
		 * @param additionalParms a list of BioSimParameterMap instances that contains the eventual additional parameters for the models
		 * @return a LinkedHashMap of BioSimPlot instances (keys) and climate variables (values)
		 * @throws BioSimClientException if the client fails or BioSimServerException if the server fails
		 */
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




		/**
		 * Returns a model output for a particular period. In this method, the ephemeral 
		 * mode can be disabled by setting the argument isEphemeral to false. In such 
		 * a case, the generated climate is cached in memory. This involves
		 * two request to the server. The first aims at generating the climate, whereas
		 * the second applies a model on the generated climate in order to obtain the 
		 * desired variables. The ephemeral model should be preferred when a single model
		 * is to be applied to some locations. If several models are to be applied to the
		 * some locations, then the ephemeral mode should be disabled. The climate is then
		 * generated only once for all the models. This implies several calls to this method
		 * with exactly the same signature except for the argument "modelName". This "modelName"
		 * argument sets the model to be applied on the generated climate. It should be one 
		 * of the strings returned by the getModelList static method. 
		 * 
		 * @param fromYr starting date (yr) of the period (inclusive)
		 * @param toYr ending date (yr) of the period (inclusive)
		 * @param locations the locations of the plots (BioSimPlot instances)
		 * @param rcp an RCP enum variable (by default RCP 4.5)
		 * @param climMod a ClimateModel enum variable (by default RCM 4)
		 * @param modelNames a list of strings representing the model names
		 * @param rep the number of replicates in climate generation if needed. Should be equal to or greater than 1. 
		 * @param repModel the number of replicates in the model if needed. Should be equal to or greater than 1. 
		 * @param additionalParms a list of BioSimParameterMap instances that contains the eventual additional parameters for the models
		 * @return a LinkedHashMap of BioSimPlot instances (keys) and climate variables (values)
		 * @throws BioSimClientException if the client fails or BioSimServerException if the server fails
		 */
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
		 * By default the climate generation retrieves the observations for the
		 * dates prior to the current date. If this option is set to true, then 
		 * the climate is generated from the normals even for dates prior to
		 * the current date.
		 * 
		 * @param bool a boolean
		 */
		public static void SetForceClimateGenerationEnabled(bool b)
		{
			ForceClimateGenerationEnabled = b;
		}


		/**
		 * This option forces the client to generate weather for past dates instead
		 * of using the observations. By default, it is disabled
		 * @return a boolean
		 */
		public static bool IsForceClimateGenerationEnabled()
		{
			return BioSimClient.ForceClimateGenerationEnabled;
		}

		/**
		 * This option set the number of stations in the imputation of the climate variables
		 * @param nbNearestNeighbours an integer between 1 and 35. The default is 4 stations.
		 */
		public static void SetNbNearestNeighbours(int nbNearestNeighbours)
		{
			if (nbNearestNeighbours < 1 || nbNearestNeighbours > 35)
			{
				throw new ArgumentException("The number of nearest neighbours must be an integer between 1 and 35!");
			}
			NbNearestNeighbours = nbNearestNeighbours;
		}

		/**
		 * Returns the number of climate station used in the imputation of the climate variables.
		 * @return an integer
		 */
		public static int GetNbNearestNeighbours()
		{		
			if (NbNearestNeighbours.HasValue)
				return NbNearestNeighbours.Value;
			else
				return 4; // default value
		}

		public static bool IsLocalConnectionEnabled() { return IsLocal; }

		public static void SetLocalConnectionEnabled(bool b) { IsLocal = b; }

		public static bool IsTestModeEnabled() { return IsTesting; }

		public static void SetTestModeEnabled(bool b) { IsTesting = b; }

		public static double GetLastServerRequestDuration()
		{
			return totalServerRequestDuration;
		}
	
	}

}
