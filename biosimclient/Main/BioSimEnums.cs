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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclient.Main
{

	public enum ClimateModel
	{
		/**
		 * Hadley climate model
		 */
		Hadley,
		/**
		 * RCM 4 climate model (default climate model)
		 */
		RCM4,
		/**
		 * GCM 4 climate model
		 */
		GCM4
	}

	public enum Month
	{
		January,
		February,
		March,
		April,
		May,
		June,
		July,
		August,
		September,
		October,
		November,
		December
	}

	public enum RCP { RCP45, RCP85 }

	internal enum Variable { TN, T, TX, P, TD, H, WS, WD, R, Z, S, SD, SWE, WS2 }

	public enum Period
	{
		FromNormals1951_1980,
		FromNormals1961_1990,
		FromNormals1971_2000,
		FromNormals1981_2010,
		FromNormals1991_2020,
		FromNormals2001_2030,
		FromNormals2011_2040,
		FromNormals2021_2050,
		FromNormals2031_2060,
		FromNormals2041_2070,
		FromNormals2051_2080,
		FromNormals2061_2090,
		FromNormals2071_2100
	}


	class EnumUtilities
	{
		static Dictionary<Month, int> numberOfDays = new();
		static Dictionary<RCP, string> rcpURL = new();
		static Dictionary<Variable, VariableDescriptor> variableDescriptors = new();
		static List<Variable> variablesForNormals = new();
		static List<String> fieldNamesForNormals = new();
		static Dictionary<Period, string> queryPartForNormals = new();

		private readonly static EnumUtilities Initilizer = new();

		private EnumUtilities()
		{
			numberOfDays.Add(Month.January, 31);
			numberOfDays.Add(Month.February, 28);
			numberOfDays.Add(Month.March, 31);
			numberOfDays.Add(Month.April, 30);
			numberOfDays.Add(Month.May, 31);
			numberOfDays.Add(Month.June, 30);
			numberOfDays.Add(Month.July, 31);
			numberOfDays.Add(Month.August, 31);
			numberOfDays.Add(Month.September, 30);
			numberOfDays.Add(Month.October, 31);
			numberOfDays.Add(Month.November, 30);
			numberOfDays.Add(Month.December, 31);

			rcpURL.Add(RCP.RCP45, "4_5");
			rcpURL.Add(RCP.RCP85, "8_5");

			variableDescriptors.Add(Variable.TN, new VariableDescriptor("TMIN_MN", false, "min air temperature"));
			variableDescriptors.Add(Variable.T, new VariableDescriptor("", false, "air temperature"));
			variableDescriptors.Add(Variable.TX, new VariableDescriptor("TMAX_MN", false, "max air temperature"));
			variableDescriptors.Add(Variable.P, new VariableDescriptor("PRCP_TT", true, "precipitation"));
			variableDescriptors.Add(Variable.TD, new VariableDescriptor("TDEX_MN", false, "temperature dew point"));
			variableDescriptors.Add(Variable.H, new VariableDescriptor("", false, "humidity"));
			variableDescriptors.Add(Variable.WS, new VariableDescriptor("", false, "wind speed"));
			variableDescriptors.Add(Variable.WD, new VariableDescriptor("", false, "wind direction"));
			variableDescriptors.Add(Variable.R, new VariableDescriptor("", true, "solar radiation"));
			variableDescriptors.Add(Variable.Z, new VariableDescriptor("", false, "atmospheric pressure"));
			variableDescriptors.Add(Variable.S, new VariableDescriptor("", true, "snow precipitation"));
			variableDescriptors.Add(Variable.SD, new VariableDescriptor("", false, "snow depth accumulation"));
			variableDescriptors.Add(Variable.SWE, new VariableDescriptor("", true, "snow water equivalent"));
			variableDescriptors.Add(Variable.WS2, new VariableDescriptor("", false, "wind speed at 2 m"));

			variablesForNormals.Add(Variable.TN);
			variablesForNormals.Add(Variable.TX);
			variablesForNormals.Add(Variable.P);

			foreach(Variable v in variablesForNormals)
            {
				fieldNamesForNormals.Add(GetVariableDescriptorForThisVariable(v).FieldName);
            }

			queryPartForNormals.Add(Period.FromNormals1951_1980, "period=1951_1980");
			queryPartForNormals.Add(Period.FromNormals1961_1990, "period=1961_1990");
			queryPartForNormals.Add(Period.FromNormals1971_2000, "period=1971_2000");
			queryPartForNormals.Add(Period.FromNormals1981_2010, "period=1981_2010");
			queryPartForNormals.Add(Period.FromNormals1991_2020, "period=1991_2020");
			queryPartForNormals.Add(Period.FromNormals2001_2030, "period=2001_2030");
			queryPartForNormals.Add(Period.FromNormals2011_2040, "period=2011_2040");
			queryPartForNormals.Add(Period.FromNormals2021_2050, "period=2021_2050");
			queryPartForNormals.Add(Period.FromNormals2031_2060, "period=2031_2060");
			queryPartForNormals.Add(Period.FromNormals2041_2070, "period=2041_2070");
			queryPartForNormals.Add(Period.FromNormals2051_2080, "period=2051_2080");
			queryPartForNormals.Add(Period.FromNormals2061_2090, "period=2061_2090");
			queryPartForNormals.Add(Period.FromNormals2071_2100, "period=2071_2100");
		}


		public static int GetNumberOfDaysInThisMonth(Month m)
		{
			return numberOfDays[m];
		}

		public static string GetURLStringForThisRCP(RCP r)
		{
			return rcpURL[r];
		}

		public static VariableDescriptor GetVariableDescriptorForThisVariable(Variable v)
        {
			return variableDescriptors[v];
        }

		internal static List<Variable> GetVariablesForNormals()
        {
			return variablesForNormals;
        }

		internal static string GetQueryPartForThisPeriod(Period p)
		{
			return queryPartForNormals[p];
        }

		internal static List<string> GetFieldNamesForNormals()
        {
			return fieldNamesForNormals;
        }
	}


	public class VariableDescriptor
    {
		public string FieldName { get; private set; }
		public bool Additive { get; private set; }
		public string Description { get; private set; }

		internal VariableDescriptor(string fieldName, bool additive, string description)
        {
			FieldName = fieldName;
			Additive = additive;
			Description = description;
        }
    }

}





