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
	internal sealed class BioSimMonthMap : OrderedDictionary  //LinkedHashMap<Month, Map<Variable, Double>>
	{
		internal BioSimMonthMap(BioSimDataSet dataSet) {
			Dictionary<Variable, int> fieldIndices = new();
			foreach (Variable v in  Enum.GetValues(typeof(Variable)))
			{
				fieldIndices.Add(v, dataSet.GetFieldNames().IndexOf(EnumUtilities.GetVariableDescriptorForThisVariable(v).FieldName));
			}
			int monthIndexInDataset = dataSet.GetFieldNames().IndexOf("Month");
			foreach (Observation obs in dataSet.GetObservations())
			{
				Object[] record = obs.ToArray();
				int monthValue = (int)record[monthIndexInDataset];
				Month m = (Month) Enum.GetValues(typeof(Month)).GetValue(monthValue - 1);
				Add(m, new Dictionary<Variable, double>());
				foreach (Variable v in Enum.GetValues(typeof(Variable)))
				{
					if (fieldIndices[v] != -1)
					{
						double value = (double)record[fieldIndices[v]];
						((Dictionary<Variable, double>)this[m]).Add(v, value);
					}
				}
			}
		}


		internal BioSimDataSet GetMeanForTheseMonths(List<Month> months) // should throws BioSimClientException 			
		{
			Dictionary<Variable, double> outputMap = new();
			int nbDays = 0;
			foreach (Month month in months)
			{
				if (Contains(month))
				{
					foreach (Variable var in EnumUtilities.GetVariablesForNormals())
					{
						if (((Dictionary<Variable, double>)this[month]).ContainsKey(var))
						{
							double value = ((Dictionary<Variable, double>)this[month])[var];
							if (!EnumUtilities.GetVariableDescriptorForThisVariable(var).Additive)
								value *= EnumUtilities.GetNumberOfDaysInThisMonth(month);
							if (!outputMap.ContainsKey(var))
								outputMap.Add(var, 0d);
							outputMap[var] = outputMap[var] + value;
						}
						else
							throw new BioSimClientException($"The variable {var.ToString()} is not in the MonthMap instance!");
					}
				}
				else
				{
					throw new BioSimClientException($"The )month {month.ToString()} is not in the MonthMap instance!");
				}
				nbDays += EnumUtilities.GetNumberOfDaysInThisMonth(month);
			}
			foreach (Variable var in EnumUtilities.GetVariablesForNormals())
			{
				if (!EnumUtilities.GetVariableDescriptorForThisVariable(var).Additive)
					outputMap[var] = outputMap[var] / nbDays;
			}

			List<string> fieldNames = new ();
			foreach (Variable v in outputMap.Keys)
				fieldNames.Add(v.ToString());
			BioSimDataSet ds = new BioSimDataSet(fieldNames);
			object[] rec = new object[outputMap.Count];
			int i = 0;
			foreach (Variable v in outputMap.Keys)
			{
				rec[i++] = outputMap[v];
			}
			ds.AddObservation(rec);
			ds.IndexFieldType();
			return ds;
		}

	}

}
