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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclient.Main
{
	public class BioSimDataSet 
	{

		protected List<String> fieldNames;
		protected List<Type> fieldTypes;
		protected List<Observation> observations;

		/// <summary>
		/// Only constructor with the field names.
		/// </summary>
		/// <param name="_fieldNames"></param>
		public BioSimDataSet(List<String> _fieldNames)
		{
			fieldNames = new();
			fieldTypes = new();
			observations = new();
			foreach (string fieldName in _fieldNames)
			{
				AddFieldName(fieldName);
			}
		}

		private void AddFieldName(String originalName)
		{
			int index = 0;
			string name = originalName;
			while (fieldNames.Contains(name))
			{
				name = originalName + index;
				index++;
			}
			fieldNames.Add(name);
		}

		/// <summary>
		/// This method returns the number of observations in the dataset.
		/// </summary>
		/// <returns></returns>
		public int GetNumberOfObservations()
		{
			return observations.Count;
		}

		/// <summary>
		/// Returns the field names in a list. The list is a new list so that changes will
		/// not affect the fieldNames member.
		/// </summary>
		/// <returns></returns>
		public List<string> GetFieldNames()
		{
			List<string> fieldNamesCopy = new();
			fieldNamesCopy.AddRange(fieldNames);
			return fieldNamesCopy;
		}

		/// <summary>
		/// Indexes the different field types. More specifically, it goes 
		/// through the columns and find the appropriate class for a particular
		/// field.This method should be called after adding all the observations.
		/// </summary>
		public void IndexFieldType()
		{
			fieldTypes.Clear();
			for (int j = 0; j < fieldNames.Count; j++)
				SetClassOfThisField(j);
		}

		public List<Observation> GetObservations()
		{
			return observations;
		}

		public void AddObservation(object[] observationFrame)
		{
			ParseDifferentFields(observationFrame);
			observations.Add(new Observation(observationFrame));
		}

		private void ParseDifferentFields(object[] lineRead)
		{
			for (int i = 0; i < fieldNames.Count; i++)
			{
				Type t = lineRead[i].GetType();
				if (t != typeof(double) && t != typeof(int))
                {
					String valueStr = lineRead[i].ToString();
					if (valueStr.Contains("."))
					{ // might be a double or a string
						try
						{
							lineRead[i] = double.Parse(valueStr);
						}
						catch (FormatException)
						{
							lineRead[i] = valueStr;
						}
					}
					else
					{   // might be an integer or a string
						try
						{
							lineRead[i] = int.Parse(valueStr);
						}
						catch (FormatException)
						{
							lineRead[i] = valueStr;
						}
					}
				}
			}
		}


		public object GetValueAt(int i, int j)
		{
			return observations[i].values[j];
		}

		private void SetValueAt(int i, int j, Object value)
		{
			if (value.GetType() == fieldTypes[j])
			{
				observations[i].values.RemoveAt(j);
				observations[i].values.Insert(j, value);
			}
		}


		private bool IsInteger(int j)
		{
			bool isInteger = true;
			for (int i = 0; i < GetNumberOfObservations(); i++)
			{
				if (GetValueAt(i, j).GetType() != typeof(int)) {
					isInteger = false;
					break;
				}
			}
			return isInteger;
		}

		private bool IsDouble(int indexJ)
		{
			bool isDouble = true;
			for (int i = 0; i < GetNumberOfObservations(); i++)
			{
				if (GetValueAt(i, indexJ).GetType() != typeof(double) && GetValueAt(i, indexJ).GetType() != typeof(int))
				{
					isDouble = false;
					break;
				}
			}
			return isDouble;
		}


		private void SetFieldType(int fieldIndex, Type clazz)
		{
			if (fieldIndex < fieldTypes.Count)
				fieldTypes[fieldIndex] = clazz;
			else if (fieldIndex == fieldTypes.Count)
				fieldTypes.Add(clazz);
			else
				throw new ArgumentException("The field type cannot be set!");
		}

		private void SetClassOfThisField(int fieldIndex)
		{
			if (IsInteger(fieldIndex))
				SetFieldType(fieldIndex, typeof(int));
			else if (IsDouble(fieldIndex))
			{
				SetFieldType(fieldIndex, typeof(double));
				reconvertToDoubleIfNeedsBe(fieldIndex);
			}
			else
			{
				SetFieldType(fieldIndex, typeof(string));
				ReconvertToStringIfNeedsBe(fieldIndex);
			}
		}


		private void reconvertToDoubleIfNeedsBe(int j)
		{
			for (int i = 0; i < GetNumberOfObservations(); i++)
			{
				object value = GetValueAt(i, j);
				if ((value.GetType() == typeof(int))) {
					SetValueAt(i, j, Convert.ToDouble((int)value)); // MF2020-04-30 Bug corrected here it was previously changed for a String
				}
			}
		}

		private void ReconvertToStringIfNeedsBe(int j)
		{
			for (int i = 0; i < GetNumberOfObservations(); i++)
			{
				object value = GetValueAt(i, j);
				if (value.GetType() == typeof(int) || value.GetType() == typeof(double)) {
					SetValueAt(i, j, value.ToString());
				}
			}
		}


		/**
		 * This method returns a list of the values in a particular field.
		 * @param i the field id
		 * @return a List of object instance
		 */
		public List<Object> getFieldValues(int i)
		{
			List<object> objs = new();
			foreach (Observation obs in observations)
			{
				objs.Add(obs.values[i]);
			}
			return objs;
		}


		internal void RemoveField(int fieldId)
		{
			foreach (Observation obs in GetObservations())
			{
				obs.values.RemoveAt(fieldId);
			}
			fieldTypes.RemoveAt(fieldId);
			fieldNames.RemoveAt(fieldId);
		}

		/// <summary>
		///  Converts the DataSet instance into a Map. There should not be any deplicate entry. 
		///  Otherwise the method returns an Exception.
		/// </summary>
		/// <returns></returns>
		public OrderedDictionary getMap()
		{
			OrderedDictionary outputMap = new();
			object[] rec;
			OrderedDictionary currentMap;
			foreach (Observation obs in GetObservations())
			{
				rec = obs.ToArray();
				currentMap = outputMap;
				for (int i = 0; i < rec.Length - 1; i++)
				{
					if (i == rec.Length - 2)
					{
						currentMap.Add(rec[i], rec[i + 1]);
					}
					else if (!currentMap.Contains(rec[i]))
					{
						currentMap.Add(rec[i], new OrderedDictionary());
						currentMap = (OrderedDictionary)currentMap[rec[i]];
					}
					else
					{
						throw new ArgumentException();
					}
				}
			}
			return outputMap;
		}

		internal BioSimDataSet GetMonthDataSet(List<Month> months) {		// TODO MF2022-01-27 should add a throw biosimclientexception here
			BioSimMonthMap monthMap = new(this);
			return monthMap.GetMeanForTheseMonths(months);
		}


		internal bool AreEqual(BioSimDataSet otherDataset)
		{
			if (fieldNames.Equals(otherDataset.fieldNames))
				if (fieldTypes.Equals(otherDataset.fieldTypes))
					if (observations.Count == otherDataset.observations.Count) 
					{ 
						for (int i = 0; i < observations.Count; i++)
						{
							if (!observations[i].IsEqualToThisObservation(otherDataset.observations[i]))
								return false;
						}
						return true;
					}
			return false;
		}

        public static BioSimDataSet ConvertLinkedHashMapToBioSimDataSet(OrderedDictionary map)     // TODO MF2022-01-27 should be ordereddictionary
        {
			BioSimDataSet outputDataSet = null;
			List<Object> observation = new();
			int plotId = 0;
			foreach (IBioSimPlot plot in map.Keys)
			{
				plotId++;
				BioSimDataSet dataSet = (BioSimDataSet)map[plot];
				if (outputDataSet == null)
				{
					List<string> fieldNames = new();
					fieldNames.Add("InnerKeyID");
					fieldNames.Add("Latitude");
					fieldNames.Add("Longitude");
					fieldNames.Add("Elevation");
					List<string> fieldNamesInInnerDataSet = dataSet.GetFieldNames();
					fieldNames.AddRange(fieldNamesInInnerDataSet);
					outputDataSet = new(fieldNames);
				}

				foreach (Observation innerObs in dataSet.GetObservations())
				{
					observation.Clear();
					observation.Add(plotId);
					observation.Add(plot.GetLatitudeDeg());
					observation.Add(plot.GetLongitudeDeg());
					observation.Add((double) plot.GetElevationM());
					observation.AddRange(innerObs.values);
					outputDataSet.AddObservation(observation.ToArray());
				}
			}
			outputDataSet.IndexFieldType();
			return outputDataSet;
		}




	}

}
